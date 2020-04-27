using System.Collections.Generic;

using Amazon.CDK;
using s3 = Amazon.CDK.AWS.S3;
using lambda = Amazon.CDK.AWS.Lambda;
using eventsources = Amazon.CDK.AWS.Lambda.EventSources;
using dynamo = Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Ecr.Assets;
using batch = Amazon.CDK.AWS.Batch;
using Amazon.CDK.AWS.Events.Targets;

using MyApp.Modules;
using Amazon.CDK.AWS.EC2;

namespace MyApp
{
    public class SrcStack : Stack
    {
        public SrcStack(Construct scope, string id, IStackProps props) : base(scope, id, props)
        {

            #region VPC, Subnets and Security Groups
            IVpc batchVpc = new Vpc(this, Constants.VPC_ID, new VpcProps{
                Cidr = Constants.CIDR_RANGE,
                MaxAzs = 4
            });

            var privateSubnetIds = new List<string>();
            foreach(Subnet subnet in batchVpc.PrivateSubnets){
                privateSubnetIds.Add(subnet.SubnetId);
            }

            var batchSecurityGroup = new SecurityGroup(this, Constants.BATCH_SECURITY_GROUP_ID,
                new SecurityGroupProps{
                    Vpc = batchVpc,
                    SecurityGroupName = Constants.BATCH_SECURITY_GROUP_NAME,
                    Description = Constants.BATCH_SECURITY_GROUP_DESCRIPTION,
                }
            );

            var batchSecurityGroupIds = new List<string>(){
                batchSecurityGroup.SecurityGroupId
            };

            #endregion

            #region S3, DynamoDB, Lambda (to trigger the batch on file drop)
            s3.Bucket bucket = new s3.Bucket(this, Constants.S3_BUCKET_ID, new s3.BucketProps{
                BucketName = Constants.S3_BUCKET_NAME + this.Account
            });
            var bucketName = bucket.BucketName;
            
            var gsi = new dynamo.GlobalSecondaryIndexProps(){
                IndexName = "GSI",
                PartitionKey = new dynamo.Attribute { Name = "CreatedTime", Type = dynamo.AttributeType.STRING },
                ProjectionType = dynamo.ProjectionType.KEYS_ONLY
            };

            dynamo.Table table =new dynamo.Table(this, Constants.DynamoDB_TABLE_ID, new dynamo.TableProps{
                TableName = Constants.DynamoDB_TABLE_ID,
                PartitionKey = new dynamo.Attribute { Name = "ProductId", Type = dynamo.AttributeType.STRING },
                BillingMode = dynamo.BillingMode.PAY_PER_REQUEST
            });
            table.AddGlobalSecondaryIndex(gsi);


            var lambdaBatchExecutionRoleProvider =  new LambdaBatchExecutionRole(this, Constants.LAMBDA_BATCH_EXECUTION_ROLE_ID);
            #endregion

            DockerImageAsset imageAsset = new DockerImageAsset(this, "BatchProcessorImage", new DockerImageAssetProps {
                Directory = Constants.BATCH_PROCESSOR_PYTHON_CODE_PATH,
                RepositoryName = Constants.ECR_REPOSITORY_NAME
            });

            #region Batch - ComputeEnvironment - Job Queue - Job Definition            
            var batchServiceRole =  new BatchServiceRole(this, Constants.BATCH_SERVICE_ROLE_ID);
            var ecsInstanceRole =  new EcsInstanceRole(this, Constants.ECS_INSTANCE_ROLE_ID);
            var batchInstanceProfile =  new InstanceProfile(this, Constants.BATCH_INSTANCE_PROFILE_ID, ecsInstanceRole);

            var computeEnvironment = new batch.CfnComputeEnvironment(this, Constants.BATCH_COMPUTE_ENVIRONMENT_ID, 
                new batch.CfnComputeEnvironmentProps{
                    ComputeEnvironmentName = Constants.BATCH_COMPUTE_ENVIRONMENT_NAME,
                    Type = Constants.BATCH_COMPUTE_TYPE,
                    ServiceRole = batchServiceRole.Role.RoleName,
                    ComputeResources = new batch.CfnComputeEnvironment.ComputeResourcesProperty{
                        Type = Constants.BATCH_COMPUTE_RESOURCE_TYPE,
                        MinvCpus = 0,
                        MaxvCpus = 32,
                        DesiredvCpus = 0,
                        InstanceRole = ecsInstanceRole.Role.RoleName,
                        InstanceTypes = new string[]{
                            Constants.BATCH_INSTANCE_TYPE
                        },
                        SecurityGroupIds = batchSecurityGroupIds.ToArray(),
                        Subnets = privateSubnetIds.ToArray()
                    }
                });
            
            var computeEnvironmentOrders = new List<batch.CfnJobQueue.ComputeEnvironmentOrderProperty>();
            var computeEnvironmentOrder = new batch.CfnJobQueue.ComputeEnvironmentOrderProperty(){
                    Order = 1,
                    ComputeEnvironment = computeEnvironment.Ref
            };
            computeEnvironmentOrders.Add(computeEnvironmentOrder);

            var batchProcessingJobQueue = new batch.CfnJobQueue(this, Constants.BATCH_JOB_QUEUE_ID, new batch.CfnJobQueueProps{
                JobQueueName = Constants.BATCH_PROCESSING_JOB_QUEUE,
                Priority = 1,
                ComputeEnvironmentOrder = computeEnvironmentOrders.ToArray()
            });

            var batchProcessingJobDefinition = new batch.CfnJobDefinition(this, Constants.BATCH_JOB_DEFINITION_ID, new batch.CfnJobDefinitionProps{
                Type = Constants.CONTAINER,
                JobDefinitionName = Constants.BATCH_JOB_DEFINITION_NAME,
                ContainerProperties = new batch.CfnJobDefinition.ContainerPropertiesProperty(){
                    Image = imageAsset.ImageUri,
                    Vcpus = Constants.BATCH_JOB_DEFINITION_VCPU,
                    Memory = Constants.BATCH_JOB_DEFINITION_MemoryLimitMiB,
                    Command = new string[]{
                        "python",
                        "batch_processor.py"
                    }
                }
            });
            #endregion
            

            #region lambda s3 event trigger

            BatchTriggerLambdaInput batchTriggerLambdaInput = new BatchTriggerLambdaInput{
                BucketName = bucketName,
                FileName = "sample.csv",
                Region = "us-east-1",
                BatchJobDefinition = batchProcessingJobDefinition.JobDefinitionName,
                BatchJobName = Constants.BATCH_JOB_NAME,
                BatchJobQueue = batchProcessingJobQueue.JobQueueName,
                DBTable = table.TableName
            };

            lambda.Function lambda = BatchTriggerLambda.BatchTriggerLambdaFunction(this, Constants.LAMBDA_NAME, 
                    batchTriggerLambdaInput, lambdaBatchExecutionRoleProvider);

            var batchLambdaFunction = new LambdaFunction(
               lambda
            );

             var eventPutSource = new eventsources.S3EventSource(bucket , new eventsources.S3EventSourceProps{
                Events = new s3.EventType[]{
                    s3.EventType.OBJECT_CREATED_PUT
                }
            });

            lambda.AddEventSource(eventPutSource);
            
            #endregion

            #region OUTPUTS
             var cfnComputeOutput = new CfnOutput(this, Constants.BATCH_COMPUTE_ENVIRONMENT_NAME_OUTPUT_ID,
                new CfnOutputProps{
                    Value = computeEnvironment.Ref
                }
            );

            var cfnS3Output = new CfnOutput(this, Constants.S3_BUCKET_OUTPUT_ID,
                new CfnOutputProps{
                    Value = bucket.BucketName
                }
            );

             var cfnEcrRepositoryOutput = new CfnOutput(this, Constants.ECR_REPOSITORY_OUTPUT_ID,
                new CfnOutputProps{
                    Value = imageAsset.Repository.RepositoryArn
                }
            );

             var cfnDynamoTableOutput = new CfnOutput(this, Constants.DYNAMO_TABLE_OUTPUT_ID,
                new CfnOutputProps{
                    Value = table.TableName
                }
            );

             var cfnLambdaRepositoryOutput = new CfnOutput(this, Constants.LAMBDA_OUTPUT_ID,
                new CfnOutputProps{
                    Value = lambda.FunctionArn
                }
            );
            
            #endregion

            Tag.Add(this, "Name", Constants.APP_NAME);
        }
    }
}

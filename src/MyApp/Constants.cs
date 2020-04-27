namespace MyApp
{
    class Constants
    {
        public static string STACK_PREFIX = "netcore";
        public static string APP_NAME = STACK_PREFIX + "-cdk-batch-app";
        public static string VPC_ID = STACK_PREFIX+ "vpc";
        public static string CIDR_RANGE = "10.0.0.0/16";
        public static string S3_BUCKET_ID = STACK_PREFIX+ "-bkt";

        // This function name should be the same inside "LambdaBatchBuilderApp" - ex: dotnet-lambda-batch-processing-function
        public static string S3_BUCKET_NAME = STACK_PREFIX+ "-batch-processing-job-";
        public static string S3_EVENTS_RULE_ID = STACK_PREFIX+ "-batch-s3-event-rule-id-";
        public static string S3_EVENTS_RULE_NAME = STACK_PREFIX+ "batch-s3-event-rule";

        public static string SNS_LAMBDA_TOPIC_ID = STACK_PREFIX+ "-sns-lambda-topic-id";
        public static string SNS_LAMBDA_TOPIC_NAME = STACK_PREFIX+ "-sns-lambda-topic";
        public static string LAMBDA_NAME = STACK_PREFIX+ "-lambda-batch-processing-function";
        public static string LAMBDA_BATCH_ROLE_SERVICE = "lambda.amazonaws.com";
        public static string LAMBDA_BATCH_POLICY_NAME = STACK_PREFIX+ "-lambda-processing-role";
        public static string LAMBDA_BATCH_EXECUTION_ROLE_ID = STACK_PREFIX+ "-lambda-processing-job-id";
        
        public static string BATCH_ROLE_SERVICE = "batch.amazonaws.com";
        public static string BATCH_SERVICE_ROLE_NAME = STACK_PREFIX+ "-batch-service-role";
        public static string BATCH_SERVICE_ROLE_ID = STACK_PREFIX+ "-batch-service-role-id";

        
        public static string EC2_INSTANCE_ROLE_SERVICE = "ec2.amazonaws.com";
        public static string ECS_INSTANCE_ROLE_SERVICE = "ecs.amazonaws.com";
        public static string ECS_INSTANCE_ROLE_NAME = STACK_PREFIX+ "-ecs-instance-role";
        public static string ECS_INSTANCE_ROLE_ID = STACK_PREFIX+ "-ecs-instance-role-id";

        public static string BATCH_INSTANCE_PROFILE_NAME = STACK_PREFIX+ "-batch-instance-profile";
        public static string BATCH_INSTANCE_PROFILE_ID = STACK_PREFIX+ "-batch-instance-profile-id";

        public static string BATCH_COMPUTE_ENVIRONMENT_ID = STACK_PREFIX+ "-batch-compute-environment-id";
        public static string BATCH_COMPUTE_ENVIRONMENT_NAME = STACK_PREFIX+ "-batch-compute-environment";

        public static string BATCH_SECURITY_GROUP_ID = STACK_PREFIX+ "-batch-security-group-id";
        public static string BATCH_SECURITY_GROUP_NAME = STACK_PREFIX+ "-batch-security-group";
        public static string BATCH_SECURITY_GROUP_DESCRIPTION = "Security group for the batch app";

        public static string BATCH_INSTANCE_TYPE =  "optimal";

        public static string BATCH_COMPUTE_RESOURCE_TYPE =  "EC2";
        public static string BATCH_COMPUTE_TYPE =  "MANAGED";
        public static string DynamoDB_TABLE_ID = APP_NAME + "-table";

        public static string BATCH_JOB_QUEUE_ID = STACK_PREFIX+ "-batch-job-queue-id";
        public static string BATCH_PROCESSING_JOB_QUEUE = STACK_PREFIX+ "-batch-job-queue";


        public static string BATCH_JOB_DEFINITION_ID = STACK_PREFIX+ "-batch-job-definition-id";
        public static string BATCH_JOB_DEFINITION_NAME = STACK_PREFIX+ "-batch-job-definition";

        public static string BATCH_JOB_NAME = STACK_PREFIX + "batch-job";
        public static int BATCH_JOB_DEFINITION_VCPU = 2;
        public static double BATCH_JOB_DEFINITION_MemoryLimitMiB = 2000;
        public static string CONTAINER = "CONTAINER";
        public static string BATCH_PROCESSOR_PYTHON_CODE_PATH = "./src/BatchProcessor";
    
        public static string ECR_REPOSITORY_NAME = APP_NAME + "-repository";
        public static string ECR_REPOSITORY_ID = APP_NAME + "-repository-id";

        public static string BATCH_COMPUTE_ENVIRONMENT_NAME_OUTPUT_ID = "Batch Compute Environment";
        public static string S3_BUCKET_OUTPUT_ID = "S3 Bucket";
        public static string ECR_REPOSITORY_OUTPUT_ID = "ECR Repository";
        public static string DYNAMO_TABLE_OUTPUT_ID = "Dynamo Table";

        public static string LAMBDA_OUTPUT_ID = "Lambda_ARN";

        public static string CLOUDWATCH_EVENTS_RULE_ID = APP_NAME + "-cloudwatch-events-s3triggerlambda-id";
    }
}    

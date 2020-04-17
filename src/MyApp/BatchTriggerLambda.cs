using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using MyApp.Modules;
using System.Collections.Generic;

namespace MyApp
{
    public static class BatchTriggerLambda 
    {
        public static Function BatchTriggerLambdaFunction(MyApp.SrcStack stack, string id, 
                BatchTriggerLambdaInput batchTriggerLambdaInput, 
                LambdaBatchExecutionRole lambdaBatchExecutionRole) 
        {
            return new Function(stack, Constants.LAMBDA_NAME, new FunctionProps
            {
                FunctionName = Constants.LAMBDA_NAME,
                Runtime = Runtime.PYTHON_3_7,
                Code = Code.FromAsset("resources"),
                Handler = "batch_trigger_lambda.lambda_handler",
                Role = lambdaBatchExecutionRole.Role,
                Environment = new Dictionary<string, string>
                {
                    ["BucketName"] = batchTriggerLambdaInput.BucketName,
                    ["FileName"] = batchTriggerLambdaInput.FileName,
                    ["Region"] = batchTriggerLambdaInput.Region,
                    ["BatchJobName"] = batchTriggerLambdaInput.BatchJobName,
                    ["BatchJobQueue"] = batchTriggerLambdaInput.BatchJobQueue,
                    ["BatchJobDefinition"] = batchTriggerLambdaInput.BatchJobDefinition,
                    ["DBTable"] = batchTriggerLambdaInput.DBTable
                }
                
            });
        }
    }
}
namespace MyApp.Utilities
{
    public static class ServiceBuilder
    {
        # region Lambda, Static helper
        public static string[] GetLambdaBatchExecutionRoleManagedPolicyARNs(){ 
            string[] lambdaBatchExecutionRoleManagedPolicyARNs = new string[]{
              "AWSLambdaExecute",
              "AmazonS3FullAccess",
              "AWSBatchFullAccess"
          };
          return lambdaBatchExecutionRoleManagedPolicyARNs;
        }

        public static string[] GetBatchServiceRoleManagedPolicyARNs(){ 
            string[] lambdaBatchExecutionRoleManagedPolicyARNs = new string[]{
              "AmazonS3FullAccess",
              "AmazonDynamoDBFullAccess",
              "service-role/AWSBatchServiceRole"
          };
          return lambdaBatchExecutionRoleManagedPolicyARNs;
        }

         public static string[] GetEcsInstanceRoleManagedPolicyARNs(){ 
            string[] lambdaBatchExecutionRoleManagedPolicyARNs = new string[]{
              "AmazonS3FullAccess",
              "AmazonDynamoDBFullAccess",
              "service-role/AmazonEC2ContainerServiceforEC2Role"
          };
          return lambdaBatchExecutionRoleManagedPolicyARNs;
        }

        #endregion
    }
}
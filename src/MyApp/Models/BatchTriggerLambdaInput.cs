namespace MyApp
{
   public class BatchTriggerLambdaInput{
       public string BucketName {get;set;}
       public string FileName {get;set;}
       public string Region {get;set;}

       public string BatchJobName {get;set;}
       public string BatchJobQueue {get;set;}
       public string BatchJobDefinition {get;set;}
       public string DBTable {get;set;}
   }
}
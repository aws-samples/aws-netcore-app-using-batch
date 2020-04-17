
# Orchestrating an Application Process with AWS Batch using AWS Cloud Development Kit (CDK)

In many real work applications, you can use custom Docker images with AWS Batch and AWS Cloud Development Kit (CDK) to execute complex jobs efficiently. 

The AWS CDK is an open source software development framework to model and provision your cloud application resources using familiar programming languages, including TypeScript, JavaScript, Python, C# and Java. For the solution in this blog, we will use C# for the infrastructure code. 

This is a sequel to an earlier published blog with same infrastructure and concept. In this blog, we will 
leverage the capabilities and features of AWS CDK (with Microsoft .NET using C#) instead of CloudFormation. If you want to try the previous solution using AWS CloudFormation please follow this link - https://aws.amazon.com/blogs/compute/orchestrating-an-application-process-with-aws-batch-using-aws-cloudformation/

This post provides a file processing implementation using Docker images and Amazon S3, AWS Lambda, Amazon DynamoDB, and AWS Batch. In this scenario, the user uploads a CSV file to into an Amazon S3 bucket, which is processed by AWS Batch as a job. These jobs can be packaged as Docker containers and are executed using Amazon EC2 and Amazon ECS. 

Let’s get started!

**As part of this blog we will do the following.**

1.	AWS CDK bootstraps and deploys a CloudFormation template. 

2.	The infrastructure launches the S3 bucket that stores the CSV files. Other AWS services required for this application orchestration is also spun up.

3.	The Amazon S3 file event notification executes an AWS Lambda function that starts an AWS Batch job.

4.	AWS Batch executes the job as a Docker container. 

5. A Python-based program reads the contents of the S3 bucket, parses each row, and updates an Amazon DynamoDB table.

6. Amazon DynamoDB stores each processed row from the CSV. 

![Alt text](Orchestrating%20an%20Application%20using%20Batch.png?raw=true "Title")

### Design Considerations

1. The provided sample uses Microsoft .NET C# AWS CDK instead of CloudFormation. Any other supported programming language CDK can be used. Please refer AWS CDK Documentation.

2. The sample provided has a Python code that is packaged for Lambda Function. This can be coded in any other available programming language.

3. For the AWS Batch Processing, a python code & Dockerfile is provided using which ImageAssets are built by the CDK. Container Image is packaged and pushed as part of CDK commands execution. This python code is run from the ECR which eventually parses S3 file and pushes it to Dynamo. Provided sample is making using of python. This can be coded in any other available programming language which allows code to be containerized.

4. To handle a higher volume of CSV file contents, you can do multithreaded or multiprocessing programming to complement the AWS Batch performance scale.

5. .NET Core 3.1 along with AWS CDK version 1.32.2, developer preview for .NET is used (available at the time of writing). Implementation, other functionalities may be available with upcoming newer versions Please watch out for newer release which may remove any obsolete or newer implementation

### Major components 

Refer to corresponding downloaded path on the provided below

1. Microsoft .NET AWS CDK to build the infrastructure - aws-netcore-app-using-batch/code/src/MyApp

2. Python Lambda code as part of resources for CDK to build - aws-netcore-app-using-batch/code/resources

3. Python Batch processor code and Dockerfile for the AWS Batch to execute - aws-netcore-app-using-batch/code/src/BatchProcessor

### Steps

1. Download this repository - 

```
  $ git clone https://github.com/aws-samples/aws-netcore-app-using-batch
```

2. Execute the below commands to spin up the infrastructure cloudformation stack. This stack spins up all the necessary AWS infrastructure needed for this exercise

```
$ cd aws-netcore-app-using-batch
$ dotnet build src
$ cdk bootstrap
$ cdk deploy –-require-approval never

```

### Testing

Make sure to complete the above step. You can review the image in AWS Console > ECR - "batch-processing-job-repository" repository

1. AWS S3 bucket - netcore-batch-processing-job-<YOUR_ACCOUNT_NUMBER> is created as part of the stack.

2. Drop the provided Sample.CSV into the S3 bucket. This will trigger the Lambda to trigger the AWS Batch

3. In AWS Console > Batch, Notice the Job runs and performs the operation based on the pushed container image. The job parses the CSV file and adds each row into DynamoDB.

4. In AWS Console > DynamoDB, look for "netcore-cdk-batch-app-table" table. Note sample products provided as part of the CSV is added by the batch

### Code Cleanup

1. AWS Console > S3 bucket - netcore-batch-processing-job-<YOUR_ACCOUNT_NUMBER> - Delete the contents of the file and S3 bucket

2. AWS Console > ECR - netcore-batch-processing-job - delete the image(s) that are pushed to the repository and image

3. AWS Console > DynamoDB - netcore-cdk-batch-app-table - delete the table and items

4. AWS Console > CloudFormation - select "CDKToolkit" - delete the stack. You may have to delete the staging S3 bucket. This is an optional step especially if you have other CDK Stacks deployed.

4. Optionally run the below commands to delete the stack.

# CLI Commands to delete the S3, Dynamo and ECR repository 
    ```
    $ aws s3 rb s3://netcore-batch-processing-job-<your-account-number> --force
    $ aws ecr delete-repository --repository-name netcore-cdk-batch-app-repository --force
    $ aws dynamodb delete-table --table-name netcore-cdk-batch-app-table

    ```
## License

This library is licensed under the MIT-0 License. See the LICENSE file.

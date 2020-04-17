import json
import boto3
import os

def lambda_handler(event, context):
    inputFileName = os.environ.get("FileName")
    bucketName = os.environ.get("BucketName")
    BatchProcessingJobName =  os.environ.get("BatchJobName")
    BatchProcessingJobQueue =  os.environ.get("BatchJobQueue")
    BatchJobDefinition =  os.environ.get("BatchJobDefinition")
    DBTable =  os.environ.get("DBTable")

    # print("Input Event - " + str(event))
    # print("Input Context - " + str(context))

    for record in event['Records']:
        bucketName = record['s3']['bucket']['name']
        inputFileName = record['s3']['object']['key'] 
        print("Received BucketName - " + bucketName + " & InputFileName - " + inputFileName + " from S3 file drop event")
    response = {
        'statusCode': 200,
        'body': json.dumps('Input Received - ' + json.dumps(event))
    }

    batch = boto3.client('batch')
    region = batch.meta.region_name
    
    #batchCommand = "--bucketName " + bucketName  + " --fileName " + inputFileName + " --region " + region + " --dbtable " + DBTable
    out = "BatchProcessingJobName - " + BatchProcessingJobName + " BatchProcessingJobQueue-" + BatchProcessingJobQueue + " BatchJobDefinition- " + BatchJobDefinition
    #out = out + "  " + batchCommand
    print(out)
    
    response = batch.submit_job(jobName=BatchProcessingJobName, 
                                jobQueue=BatchProcessingJobQueue, 
                                jobDefinition=BatchJobDefinition, 
                                containerOverrides={
                                    "command": [ "python", "batch_processor.py"  ],
                                    "environment": [ 
                                        {"name": "InputBucket", "value": bucketName},
                                        {"name": "FileName", "value": inputFileName},
                                        {"name": "Region", "value": region},
                                        {"name": "DBTable", "value": DBTable}
                                    ]
                                })
    print("Job ID is {}.".format(response['jobId']))
    return response  
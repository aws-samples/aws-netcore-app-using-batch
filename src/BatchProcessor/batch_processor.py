import boto3
from boto3.dynamodb.conditions import Key, Attr
import csv, sys, time, argparse
from datetime import datetime
import json
import os
import sys
from operator import itemgetter, attrgetter
from time import sleep
import urllib3
import json

LOGTYPE_ERROR = 'ERROR'
LOGTYPE_INFO = 'INFO'
LOGTYPE_DEBUG = 'DEBUG'

# table - batch_processing_job
def batch_processing_job_update(productId, productName, productDescription, fileName, region, dbTable):
    try:
        now = datetime.now()
        date_time = now.strftime("%m-%d-%Y %H:%M:%S.%f")[:-3]
        
        dynamodb = boto3.resource('dynamodb', region_name = region)
        table = dynamodb.Table(dbTable)
        
        table.put_item(
            Item={
                'ProductId': productId, 
                'Filename': fileName, 
                'CreatedTime': date_time, 
                'LastModified': date_time, 
                'ProductName': productName,
                'ProductDescription': productDescription
            }
        )
        
    except Exception as ex:
        logMessage(fileName, "Error Updating DynamoDB:" + str(ex), LOGTYPE_ERROR)

def read_file(fileName, inputBucket, inputFile, s3):
    input_products = []
    logMessage(fileName, 'Reading file - ' + inputBucket + "/" + inputFile, LOGTYPE_INFO)
    productId = ""
    productName = ""
    productDescription = ""
    try:
        s3_object = s3.Object(inputBucket, inputFile).get()[u'Body'].read().decode('utf-8')
        input_lines = s3_object.splitlines()
        
        productIndex = 0
        for row in csv.DictReader(input_lines, delimiter='\t'):
            try:
                print('row - ' + str(row))
                eachRow = row['ProductId,ProductName,ProductDescription'].split(',')
                productId = eachRow[0]
                productName = eachRow[1]
                productDescription = eachRow[2]
                
            except Exception as ex:
                logMessage(fileName, "Error retrieving Product " + str(ex), LOGTYPE_DEBUG)

            productIndex = productIndex + 1
            input_products.append({'productIndex': productIndex, 'productId': productId, 'productName': productName, 'productDescription': productDescription})
            input_products = sorted(input_products, key=itemgetter('productIndex')) 
            
    except Exception as ex:
        logMessage(fileName, "Error parsing excel " + str(ex), LOGTYPE_ERROR)

    return input_products

def batch_process(input_products, fileName, region, dbTable):
    print("inside batch_process - filename: " + fileName + "  REGION: " + region)
    try:
        for source_products in input_products:
            productIndex = source_products['productIndex']
            productId = str(source_products['productId'])
            productName = str(source_products['productName'])
            productDescription = str(source_products['productDescription'])
            
            print(str(productIndex) + " " + productId + "  " + productName + "  " + productDescription + "  " + fileName) 
            batch_processing_job_update(productId, productName, productDescription, fileName, region, dbTable) 
            logMessage(fileName, "Product updated for " + productId + " and " + productName + " with "+ productDescription, LOGTYPE_INFO)

    except Exception as ex:
        logMessage(fileName, "Error batch processing files" + str(ex), LOGTYPE_ERROR)


def process_files(inputBucket, fileName, region, dbTable):
    try:
        urllib3.disable_warnings()
        s3 = boto3.resource('s3', verify=False)
        
        prefix = fileName
        print("region - " + region)
        bucket = s3.Bucket(name=inputBucket)
        FilesNotFound = True
        startTime = datetime.now()
        
        
        for files in bucket.objects.filter(Prefix=prefix):
            logMessage(fileName, 'files.key-' + files.key, LOGTYPE_DEBUG)
            isCSVFile = files.key.endswith(".csv")
            
            if isCSVFile:
                FilesNotFound = False
                input_products = read_file(fileName, inputBucket, files.key, s3)
                
                logMessage(fileName, "Calling batch_process for input_products " + str(input_products), LOGTYPE_INFO) 
                if len(input_products) > 0:
                    batch_process(input_products, fileName, region, dbTable)
                else:
                    logMessage(fileName, "No products could be found in bucket {}/{}".format(inputBucket, prefix), LOGTYPE_INFO)

        if FilesNotFound:
            logMessage(fileName, "No file in {0}/{1}".format(bucket, prefix), LOGTYPE_INFO)
        
        endTime = datetime.now()
        diffTime = endTime - startTime
        logMessage(fileName, "Total processing time - " + str(diffTime.seconds), LOGTYPE_INFO) 

    except Exception as ex:
        logMessage(fileName, "Error processing files:" + str(ex), LOGTYPE_ERROR)  

def main():
    startTime = datetime.now()
    inputBucket = ""
    fileName = ""
    region = ""
    dbTable = ""

    try:
        # Triggering Lambda sets the environment variables for the batch to pick up
        inputBucket = os.environ.get("InputBucket")
        fileName = os.environ.get("FileName")
        region = os.environ.get("Region")
        dbTable = os.environ.get("DBTable")
        
        logMessage(fileName, 'received ' + inputBucket + "  " + fileName +  "  " + region + "  " +  dbTable + " from environment", LOGTYPE_INFO)
    except:
        error = ''
        print("Error getting environment variables")
        

    try:        
        if not inputBucket and not fileName:
            parser = argparse.ArgumentParser()
            parser.add_argument("--bucketName", "-js", type=str, required=True)
            parser.add_argument("--fileName")
            parser.add_argument("--region")
            parser.add_argument("--dbtable")
            args = parser.parse_args()
            inputBucket = args.bucketName
            fileName = args.fileName
            region = args.region
            dbTable = args.dbtable
            
            logMessage(fileName, 'received ' + inputBucket + "  " + fileName +  "  " + region + "  " +  dbTable + " from params", LOGTYPE_INFO)
       
    except Exception as ex:
        logMessage(fileName, "Unexpected error:" + str(ex), LOGTYPE_ERROR)
        
    if inputBucket and fileName and region and dbTable:
        process_files(inputBucket, fileName, region, dbTable)
    else:
        logMessage(fileName, "No Inputs received or parsed", LOGTYPE_ERROR)

    endTime = datetime.now()
    diffTime = endTime - startTime

    logMessage(fileName, "Total processing time - " + str(diffTime.seconds), LOGTYPE_INFO)

def logMessage(productId, message, logType):
    try:
        logMessageDetails = constructMessageFormat(productId, message, "", logType)
        
        if logType == "INFO" or logType == "ERROR":
            print(logMessageDetails)
        elif logType == "DEBUG":
            try:
                if os.environ.get('DEBUG') == "LOGTYPE":
                   print(logMessageDetails) 
            except KeyError:
                pass
    except Exception as ex:
        logMessageDetails = constructMessageFormat(productId, message, "Error occurred at Batch_processor.logMessage" + str(ex), logType)
        print(logMessageDetails)


def constructMessageFormat(productId, message, additionalErrorDetails, logType):
    if additionalErrorDetails != "":
        return "ProductId: " + productId + " " + logType + ": " + message + " Additional Details -  " + additionalErrorDetails
    else:
        return "ProductId: " + productId + " " + logType + ": " + message

def test():
    print("just a test message")

# COMMENT below for local/development testing
# main()

print("Python file invoked")
if __name__ == '__main__':
   main()
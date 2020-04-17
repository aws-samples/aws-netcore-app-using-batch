#!/bin/bash


dotnet build src
cdk bootstrap
cdk deploy --require-approval never

pwd
cd src/LambdaBatchBuilderApp/LambdaBatchBuilderApp/src/LambdaBatchBuilderApp
dotnet lambda deploy-function dotnet-lambda-batch-processing-function --region us-east-1 --function-role dotnet-lambda-batch-processing-role1



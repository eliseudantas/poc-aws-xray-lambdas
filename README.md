# X-Ray and Lambda Functions

It's a simple divide-and-conquer strategy. The `EntryService` gets a request to process a list of Entities. Then it divides this list in parts, for each part it adds a message into a queue. There's a Lambda that looks to this queue and processes the requests. So, because the Lambda can scale horizontally, the requests are processed asynchronously. This results in expressive reduction of the overall processing  time.

This project is a demonstration of how to integrate a .net Lambda project with the AWS x-Ray service. 

# CDK 

The `cdk.json` file tells the CDK Toolkit how to execute your app.

It uses the [.NET Core CLI](https://docs.microsoft.com/dotnet/articles/core/) to compile and execute your project.

## Useful commands

* `dotnet build src` compile this app
* `cdk deploy`       deploy this stack to your default AWS account/region
* `cdk diff`         compare deployed stack with current state
* `cdk synth`        emits the synthesized CloudFormation template


# X-Ray abd Lambdas
Using Lambdas with Aws xRay

https://docs.aws.amazon.com/lambda/latest/dg/services-xray.html


### Xray + SQS

* https://docs.aws.amazon.com/xray/latest/devguide/xray-services-sqs.html

* https://docs.aws.amazon.com/xray/latest/devguide/xray-concepts.html#xray-concepts-tracingheader

* https://github.com/aws/aws-xray-sdk-dotnet


### ISSUE: Link Lambda ---> SQS (trace_id)

* https://github.com/aws/aws-xray-sdk-node/issues/208
* https://github.com/aws/aws-xray-sdk-java/issues/246
* https://github.com/aws/aws-xray-sdk-java/issues/138




### Things to talk about

- SQS to Lambda caveats
	- https://docs.aws.amazon.com/xray/latest/devguide/xray-concepts.html

- Every cloudwatch log has a @xrayTraceId, it's something like `1-5febd332-60016e2f13dfdf2146f54e55`, you can get this value and search by it in the XRay dashboard to understand the whole context of the operation that created that log. 

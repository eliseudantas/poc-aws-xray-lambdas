using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SQS;
using System.Collections.Generic;

namespace Poc.CDK
{
    public class PocStack : Stack
    {
        internal PocStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // The code that defines your stack goes here

            var queue = new Queue(this, "poc_xray_process_queue", new QueueProps
            {
                VisibilityTimeout = Duration.Hours(1)
            });

            var bucket = new Bucket(this, "poc_xray_bucket", new BucketProps { 
                BucketName = "poc-xray-bucket"
            });



            var handler = new Function(this, "poc_xray_EntryServiceProxyFunction", new FunctionProps
            {
                FunctionName = "poc-xray-api-function",
                Runtime = Runtime.DOTNET_CORE_3_1,
                Code = Code.FromAsset("./src/Poc.Lambda.Api/bin/Release/netcoreapp3.1/Poc.Lambda.Api.zip"),
                Handler = "Poc.Lambda.Api::Poc.Lambda.Api.LambdaEntryPoint::FunctionHandlerAsync",
                Environment = new Dictionary<string, string>{
                     { "BUCKET", bucket.buck },
                     { "SQS", queue.QueueUrl }
                },
                Tracing = Tracing.ACTIVE,
                Timeout = Duration.Minutes(3)
            });

            queue.Grant(handler, "sqs:SendMessage");

            bucket.GrantReadWrite(handler);

            var api = new RestApi(this, "poc_xray_ApiGateway", new RestApiProps
            {
                RestApiName = "Entry Service API"
            });

            var apiIntegration = new LambdaIntegration(handler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string> {
                    { "application/json", "{ 'statusCode', '200'}" }
                }
            });

            api.Root
                .AddResource("{proxy+}")
                .AddMethod("ANY", apiIntegration);


            var function = new Function(this, "poc_xray_SingleEntryProcessorFunction", new FunctionProps
            {
                Runtime = Runtime.DOTNET_CORE_3_1,
                Code = Code.FromAsset("./src/Poc.Lambda.SingleEntryProcessor/bin/Release/netcoreapp3.1/Poc.Lambda.SingleEntryProcessor.zip"),
                Handler = "Poc.Lambda.SingleEntryProcessor::Poc.Lambda.SingleEntryProcessor.Function::FunctionHandler",
                Environment = new Dictionary<string, string>{
                    { "BUCKET", bucket.BucketName },
                    { "SQS", queue.QueueUrl }
                },
                Tracing = Tracing.PASS_THROUGH,
                Timeout = Duration.Minutes(3)
            });

            function.AddEventSource(new SqsEventSource(queue, new SqsEventSourceProps { }));
        }
    }
}

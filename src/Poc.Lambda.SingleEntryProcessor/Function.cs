using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Context;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using Amazon.XRay.Recorder.Core.Strategies;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Poc.Core;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Poc.Lambda.SingleEntryProcessor
{
    public class Function
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var recorder = new AWSXRayRecorderBuilder()
                .WithStreamingStrategy(new DefaultStreamingStrategy())
                .Build();

            AWSXRayRecorder.InitializeInstance(configuration, recorder);

           

        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            AWSSDKHandler.RegisterXRayForAllServices();

            context.Logger.LogLine("sqs-event: " + JsonConvert.SerializeObject(evnt, Formatting.Indented));


            //// Receive the message from the queue, specifying the "AWSTraceHeader"
            //ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest()
            //        .WithQueueUrl("")
            //        .WithAttributeNames("AWSTraceHeader");

            //List<Message> messages = sqs.receiveMessage(receiveMessageRequest).getMessages();

            //if (!messages.isEmpty())
            //{
            //    Message message = messages.get(0);

            //    // Retrieve the trace header from the AWSTraceHeader message system attribute
            //    String traceHeaderStr = message.getAttributes().get("AWSTraceHeader");
            //    if (traceHeaderStr != null)
            //    {
            //        TraceHeader traceHeader = TraceHeader.fromString(traceHeaderStr);

            //        // Recover the trace context from the trace header
            //        Segment segment = AWSXRay.getCurrentSegment();
            //        segment.setTraceId(traceHeader.getRootTraceId());
            //        segment.setParentId(traceHeader.getParentId());
            //        segment.setSampled(traceHeader.getSampled().equals(TraceHeader.SampleDecision.SAMPLED));
            //    }
            //}
           

            foreach (var message in evnt.Records)
            {
                

                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {

            var traceId = message.Attributes["AWSTraceHeader"].Split(";").First(o => o.Contains("Root")).Split("=").Last();
            var operation = JsonConvert.DeserializeObject<Operation>(message.Body);

            context.Logger.LogLine($"Processed message {message.Body}");

            var recorder = AWSXRayRecorder.Instance;
            

            AWSXRayRecorder.Instance.BeginSubsegment("SingleEntryProcessor Function");
            var segment = AWSXRayRecorder.Instance.GetEntity();

            AWSXRayRecorder.Instance.AddAnnotation("operation_id", operation.OperationId);
            AWSXRayRecorder.Instance.AddAnnotation("root_trace_id", traceId);
            AWSXRayRecorder.Instance.AddMetadata("request_object", operation);

            // TODO: Do interesting work based on the new message

            Thread.Sleep(300);

            AWSXRayRecorder.Instance.EndSubsegment();

            await Task.CompletedTask;
        }
    }
}

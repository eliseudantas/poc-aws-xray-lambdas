using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Core;
using System.Threading;
using Amazon.S3;
using Amazon;
using System.Text;
using System.IO;
using Amazon.SQS;
using Amazon.Runtime;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Poc.Core;

namespace Poc.Lambda.Api.Controllers
{
    [Route("api/[controller]")]
    public class ProcessController : ControllerBase
    {

        // Instantiate random number generator.  
        private readonly Random _random = new Random();
        private string bucketName;
        private string queueUrl;
        private AmazonS3Client s3Client;
        private AmazonSQSClient sqsClient;

        public ProcessController()
        {
            bucketName = Environment.GetEnvironmentVariable("BUCKET");
            queueUrl = Environment.GetEnvironmentVariable("SQS");

            s3Client = new AmazonS3Client(RegionEndpoint.USEast1);

            sqsClient = new AmazonSQSClient(RegionEndpoint.USEast1);

        }

        [HttpGet("{id}")]
        public async Task<string> Get(int id)
        {


            AWSXRayRecorder.Instance.AddAnnotation("operation_id", id);

            AWSXRayRecorder.Instance.BeginSubsegment("Operation to divide the load");
            try
            {
                var s3file = await s3Client.GetObjectAsync(bucketName, "records.csv");

                using (var responseStream = s3file.ResponseStream)
                using (var reader = new StreamReader(responseStream))
                {

                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var segment = AWSXRayRecorder.Instance.GetEntity();


                        AWSXRayRecorder.Instance.BeginSubsegment($"Creates a Request (ID={line}).");

                        AWSXRayRecorder.Instance.AddAnnotation("root_trace_id", segment.RootSegment.TraceId);
                        AWSXRayRecorder.Instance.AddAnnotation("operation_id", id);

                        var messageBody = JsonConvert.SerializeObject(new Operation
                        {
                            Description = "Do something!",
                            RecordId = line,
                        }, Formatting.Indented);


                        var sqsMessage = new SendMessageRequest
                        {
                            MessageBody = messageBody,
                            QueueUrl = queueUrl
                        };


                        await sqsClient.SendMessageAsync(sqsMessage);

                        AWSXRayRecorder.Instance.AddMetadata("message-body", messageBody);

                        AWSXRayRecorder.Instance.EndSubsegment();
                    }

                }

            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }

            return "success";
        }


    }
}

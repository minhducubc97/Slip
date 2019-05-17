using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading;

namespace Slip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SQSController : ControllerBase
    {
	    private string MySlackQueueUrl;
	    private string MyGlipQueueUrl;
	    private IAmazonSQS Sqs;
        private static readonly HttpClient client = new HttpClient();

        public SQSController()
	    {
            MySlackQueueUrl = "https://sqs.ca-central-1.amazonaws.com/608040905965/Slack";
            MyGlipQueueUrl = "https://sqs.ca-central-1.amazonaws.com/608040905965/Glip";
			Sqs = new AmazonSQSClient("AKIAI3DBF35TO2NJOZGA", "FFFp9svfoiDk62L8RavGFiP1/n00NfhKfkymjaD4", RegionEndpoint.CACentral1);
        }

	    [Route("createtable")]
	    public IActionResult CreateDynamoDBTable([FromQuery] string queueName)
	    {
			Console.WriteLine("********************************************");
		    Console.WriteLine("Amazon SQS");
		    Console.WriteLine("********************************************");

		    var sqsRequest = new CreateQueueRequest
		    {
			    QueueName = queueName
			};

		    var createQueueResponse = Sqs.CreateQueueAsync(sqsRequest).Result;

		    var newQueueUrl = createQueueResponse.QueueUrl;

		    var listQueuesRequest = new ListQueuesRequest();
		    var listQueuesResponse = Sqs.ListQueuesAsync(listQueuesRequest);
		    foreach (var queueUrl in listQueuesResponse.Result.QueueUrls)
		    {
			    Console.WriteLine($"QueueUrl: {queueUrl}");
		    }
			return Ok();
	    }

        [HttpPost]
	    [Route("slackmessage")]
	    public IActionResult InsertSlackMessage([FromBody]string body)
	    {
            var sqsMessageRequest = new SendMessageRequest
		    {
			    QueueUrl = MySlackQueueUrl,
				MessageBody = body
		    };

		    Sqs.SendMessageAsync(sqsMessageRequest);
            Thread.Sleep(1000);
            try
            {
                ReadSQS("Slack");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            return Ok(body);
	    }

        [HttpPost]
        [Route("glipmessage")]
	    public IActionResult InsertGlipMessage([FromBody]string body)
        {
            var sqsMessageRequest = new SendMessageRequest
		    {
			    QueueUrl = MyGlipQueueUrl,
			    MessageBody = body
		    };

		    Sqs.SendMessageAsync(sqsMessageRequest);
            Thread.Sleep(1000);
            try
            {
                ReadSQS("Glip");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            
            return Ok();
		}

        public async void ReadSQS(string Source)
        {
            var sqs = new AmazonSQSClient(RegionEndpoint.CACentral1);

            var queueUrl = sqs.GetQueueUrlAsync(Source).Result.QueueUrl;

            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl
            };

            var receiveMessageResponse = sqs.ReceiveMessageAsync(receiveMessageRequest).Result;
            while (true)
            {
                if (receiveMessageResponse.Messages.Count != 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            foreach (var message in receiveMessageResponse.Messages)
            {
                Console.WriteLine($"	Body: {message.Body}");

                var messageReceptHandle = receiveMessageResponse.Messages.FirstOrDefault()?.ReceiptHandle;

                var deleteRequest = new DeleteMessageRequest
                {
                    QueueUrl = queueUrl,
                    ReceiptHandle = messageReceptHandle
                };

                if (Source == "Slack")
                {
                    PostMessageToGlipAsync(message.Body);
                }
                else
                {
                    PostMessageToSlackbotAsync(message.Body);
                }

                sqs.DeleteMessageAsync(deleteRequest);
            }
        }

        public async Task PostMessageToSlackbotAsync(string messageBody)
        {
            string myJson = "{\"text\":\"" + messageBody +  "\"}";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://hooks.slack.com/services/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",
                    new StringContent(myJson, Encoding.UTF8, "application/json"));
            }
        }

        public async Task PostMessageToGlipAsync(string messageBody)
        {
            string myJson = "{\"message\":\"" + messageBody + "\"}";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://54.188.121.149:8443/api",
                    new StringContent(myJson, Encoding.UTF8, "application/json"));
            }
        }
    }
}
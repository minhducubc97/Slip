using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Slip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SQSController : ControllerBase
    {
	    private string MySlackQueueUrl;
	    private string MyGlipQueueUrl;
	    private IAmazonSQS Sqs;

	    public SQSController()
	    {
		    MySlackQueueUrl = "https://sqs.ca-central-1.amazonaws.com/997928571690/Slack";
		    MyGlipQueueUrl = "https://sqs.ca-central-1.amazonaws.com/997928571690/Glip";
			Sqs = new AmazonSQSClient(RegionEndpoint.CACentral1);
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

	    [Route("slackmessage")]
	    public IActionResult InsertSlackMessage()
	    {
		    var sqsMessageRequest = new SendMessageRequest
		    {
			    QueueUrl = MySlackQueueUrl,
				MessageBody = "Email information"
		    };

		    Sqs.SendMessageAsync(sqsMessageRequest);

			Console.WriteLine("Finished sending message to our SQS queue.\n");

			return Ok();
	    }

	    [Route("glipmessage")]
	    public IActionResult InsertGlipMessage()
	    {
			var sqsMessageRequest = new SendMessageRequest
		    {
			    QueueUrl = MyGlipQueueUrl,
			    MessageBody = "Email information"
		    };

		    Sqs.SendMessageAsync(sqsMessageRequest);

		    Console.WriteLine("Finished sending message to our SQS queue.\n");

		    return Ok();
		}
	}
}
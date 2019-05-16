using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Slip
{
	public class Program
	{
		public static void Main(string[] args)
		{
			ReadSQS();
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();

		public static void ReadSQS()
		{
			var sqs = new AmazonSQSClient(RegionEndpoint.CACentral1);

			var queueUrl = sqs.GetQueueUrlAsync("EmailQueue").Result.QueueUrl;

			var receiveMessageRequest = new ReceiveMessageRequest
			{
				QueueUrl = queueUrl
			};

			var receiveMessageResponse = sqs.ReceiveMessageAsync(receiveMessageRequest).Result;

			foreach (var message in receiveMessageResponse.Messages)
			{
				Console.WriteLine("Message \n");
				Console.WriteLine($"	MessageId: {message.MessageId} \n");
				Console.WriteLine($"	ReceiptHandle: {message.ReceiptHandle} \n");
				Console.WriteLine($"	MSD5Body: {message.MD5OfBody}");
				Console.WriteLine($"	Body: {message.Body}");

				foreach (var attribute in message.Attributes)
				{
					Console.WriteLine("Attribute \n");
					Console.WriteLine($"	Name: {attribute.Key} \n");
					Console.WriteLine($"	Value: {attribute.Value} \n");
				}

				var messageReceptHandle = receiveMessageResponse.Messages.FirstOrDefault()?.ReceiptHandle;
				
				var deleteRequest = new DeleteMessageRequest
				{
					QueueUrl = queueUrl,
					ReceiptHandle = messageReceptHandle
				};

				sqs.DeleteMessageAsync(deleteRequest);
			}
		}
	}
}

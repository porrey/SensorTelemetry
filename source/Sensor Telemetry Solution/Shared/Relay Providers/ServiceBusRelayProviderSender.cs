using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Porrey.SensorTelemetry.Interfaces;
using ppatierno.AzureSBLite.Messaging;

namespace Porrey.SensorTelemetry.Relays
{
	public class ServiceBusRelayProviderSender<T> : IRelayProviderSender<T>
	{
		[Dependency]
		protected IServiceBusConfiguration ServiceBusConfiguration { get; set; }
		protected MessagingFactory Factory { get; set; }
		protected QueueClient Client { get; set; }

		public Task Initialize()
		{
			// ***
			// *** Create the client.
			// ***
			this.Factory = MessagingFactory.CreateFromConnectionString(this.ServiceBusConfiguration.ConnectionString);
			this.Client = this.Factory.CreateQueueClient(this.ServiceBusConfiguration.QueueName);

			return Task.FromResult(0);
		}

		public Task<bool> Send(string eventName, T message)
		{
			bool returnValue = false;

			try
			{
				if (this.Client != null)
				{
					// ***
					// *** Serial the message to JSON.
					// ***
					string json = JsonConvert.SerializeObject(message);

					// ***
					// *** Convert the JSON string to a BrokeredMessage.
					// ***
					MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
					BrokeredMessage brokeredMessage = new BrokeredMessage(stream);
					brokeredMessage.Properties["time"] = DateTime.UtcNow;

					// ***
					// *** Send the message.
					// ***
					this.Client.Send(brokeredMessage);
					returnValue = true;
				}
			}
			catch
			{
				returnValue = false;
			}

			return Task.FromResult(returnValue);
		}

		public void Dispose()
		{
			// ***
			// *** Release the QueueClient.
			// ***
			if (this.Client != null)
			{
				this.Client.Close();
				this.Client = null;
			}

			// ***
			// *** Release the MessagingFactory.
			// ***
			if (this.Factory != null)
			{
				this.Factory.Close();
				this.Factory = null;
			}
		}
	}
}

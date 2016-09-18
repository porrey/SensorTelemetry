using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Porrey.SensorTelemetry.Interfaces;
using ppatierno.AzureSBLite.Messaging;

namespace Porrey.SensorTelemetry.Relays
{
	public class ServiceBusRelayProviderReceiver<T> : IRelayProviderReceiver<T>
	{
		[Dependency]
		protected IServiceBusConfiguration ServiceBusConfiguration { get; set; }
		protected MessagingFactory Factory { get; set; }
		protected QueueClient Client { get; set; }
		protected IRelayProviderCallbackDelegate<T> Callback { get; set; }
		protected CancellationTokenSource CancellationTokenSource { get; set; }

		public Task Initialize()
		{
			// ***
			// *** Create the cancellation token source.
			// ***
			this.CancellationTokenSource = new CancellationTokenSource();

			// ***
			// *** Create the client.
			// ***
			this.Factory = MessagingFactory.CreateFromConnectionString(this.ServiceBusConfiguration.ConnectionString);
			this.Client = this.Factory.CreateQueueClient(this.ServiceBusConfiguration.QueueName);

			// ***
			// *** Start receiving messages...
			// ***
			this.ReceiveMessages(this.Client, this.CancellationTokenSource.Token);

			return Task.FromResult(0);
		}

		public void SetCallback(string eventName, IRelayProviderCallbackDelegate<T> callback)
		{
			this.Callback = callback;
		}

		protected Task ReceiveMessages(QueueClient client, CancellationToken token)
		{
			return Task.Factory.StartNew(() =>
			{
				// ***
				// *** Loop until canceled.
				// ***
				while (!token.IsCancellationRequested)
				{
					try
					{
						// ***
						// *** Wait for a message.
						// ***
						BrokeredMessage brokeredMessage = client.Receive();

						if (brokeredMessage != null)
						{
							// ***
							// *** Complete the receive.
							// ***
							brokeredMessage.Complete();

							// ***
							// *** Get the JSON body of the message.
							// ***
							var json = Encoding.UTF8.GetString(brokeredMessage.GetBytes());

							// ***
							// *** Convert the JSON to an instance of the object it represents.
							// ***
							T message = JsonConvert.DeserializeObject<T>(json);

							// ***
							// *** Invoke the callback.
							// ***
							this.Callback?.Invoke(message);
						}
					}
					catch
					{
					}
				}
			});
		}

		public void Dispose()
		{
			// ***
			// *** Cancel the background task that monitors 
			// *** for incoming messages.
			// ***
			this.CancellationTokenSource.Cancel();

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

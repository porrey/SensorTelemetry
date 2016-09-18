using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Porrey.SensorTelemetry.Interfaces;
using Microsoft.Devices.Tpm;

namespace Porrey.SensorTelemetry.Relays
{
	public class IotHubRelayProviderReceiverTpm<T> : IRelayProviderReceiver<T>
	{
		[Dependency]
		protected IIotHubConfiguration IotHubConfiguration { get; set; }
		protected DeviceClient DeviceClient { get; set; }
		protected IRelayProviderCallbackDelegate<T> Callback { get; set; }
		protected CancellationTokenSource CancellationTokenSource { get; set; }

		public Task Initialize()
		{
			this.CancellationTokenSource = new CancellationTokenSource();

			// ***
			// *** Create a TpmDevice to access the credential information. Specify
			// *** a logical device ID of 0 (this is the slot we used to store the
			// *** credentials).
			// ***
			uint logicalDeviceId = 0;
			TpmDevice tpm = new TpmDevice(logicalDeviceId);

			// ***
			// *** Get the connection properties from the TPM.
			// ***
			string uri = tpm.GetHostName();
			string deviceId = tpm.GetDeviceId();
			string sasToken = tpm.GetSASToken();

			// ***
			// *** Create the device connection.
			// ***
			this.DeviceClient = DeviceClient.Create(uri, AuthenticationMethodFactory.CreateAuthenticationWithToken(deviceId, sasToken));

			this.ReceiveMessages(this.DeviceClient, this.CancellationTokenSource.Token);
			return Task.FromResult(0);
		}

		public void SetCallback(string eventName, IRelayProviderCallbackDelegate<T> callback)
		{
			this.Callback = callback;
		}

		protected Task ReceiveMessages(DeviceClient client,  CancellationToken token)
		{
			return Task.Factory.StartNew(async () =>
			{
				// ***
				// *** Loop until canceled.
				// ***
				while (!token.IsCancellationRequested)
				{
					// ***
					// *** Wait to receive the next message.
					// ***
					var item = await client.ReceiveAsync();

					// ***
					// *** The item is null if a timeout occurs while waiting.
					// ***
					if (item != null)
					{
						// ***
						// *** Convert item to a Message instance.
						// ***
						Message hubMessage = item as Message;

						// ***
						// *** Check to ensure we have a Message instance.
						// ***
						if (hubMessage != null)
						{
							// ***
							// *** Get the data from the message and convert it to a string.
							// ***
							string json = Encoding.UTF8.GetString(hubMessage.GetBytes());

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

					await Task.Delay(1000);
				}
			});
		}

		public async void Dispose()
		{
			// ***
			// *** Cancel the background task that monitors 
			// *** for incoming messages.
			// ***
			CancellationTokenSource.Cancel();

			// ***
			// *** Release the Device Client.
			// ***
			if (this.DeviceClient != null)
			{
				await this.DeviceClient.CloseAsync();
				this.DeviceClient = null;
			}

			// ***
			// *** Release the Callback.
			// ***
			this.Callback = null;
		}
	}
}

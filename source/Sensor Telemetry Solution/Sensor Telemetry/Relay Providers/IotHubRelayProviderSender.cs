using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Porrey.SensorTelemetry.Interfaces;

namespace Porrey.SensorTelemetry.Relays
{
	public class IotHubRelayProviderSender<T> : IRelayProviderSender<T>
	{
		[Dependency]
		protected IIotHubConfiguration IotHubConfiguration { get; set; }
		protected ServiceClient ServiceClient { get; set; }

		public Task Initialize()
		{
			this.ServiceClient = ServiceClient.CreateFromConnectionString(this.IotHubConfiguration.ConnectionString);
			return Task.FromResult(0);
		}

		public Task<bool> Send(string eventName, T message)
		{
			bool returnValue = false;

			try
			{
				var messageString = JsonConvert.SerializeObject(message);
				var hubMessage = new Message(Encoding.UTF8.GetBytes(messageString));
				this.ServiceClient.SendAsync(this.IotHubConfiguration.DeviceId, hubMessage);

				returnValue = true;
			}
			catch
			{
				returnValue = false;
			}

			return Task.FromResult(returnValue);
		}

		public async void Dispose()
		{
			// ***
			// *** Release the Service Client.
			// ***
			if (this.ServiceClient != null)
			{
				await this.ServiceClient.CloseAsync();
				this.ServiceClient = null;
			}
		}
	}
}

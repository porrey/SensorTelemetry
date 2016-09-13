using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Interfaces;

namespace Porrey.SensorTelemetry.Relays
{
	public class SignalrRelayProviderSender<T> : IRelayProviderSender<T>
	{
		[Dependency]
		protected IMobileServicesConfiguration MobileServicesConfiguration { get; set; }

		/// <summary>
		/// Gets/sets a reference to the SingalR hub connection object.
		/// </summary>
		protected HubConnection HubConnection { get; set; }

		/// <summary>
		/// Get/sets a reference to the subscription token instance
		/// created when the event is subscribed to. This object is used to 
		/// unsubscribe from the event.
		/// </summary>
		protected IHubProxy Proxy { get; set; }

		public async Task Initialize()
		{
			// ***
			// *** Configure and connect to the SignalR hub
			// ***
			this.HubConnection = new HubConnection(this.MobileServicesConfiguration.Url, true);
			this.Proxy = this.HubConnection.CreateHubProxy("RelayHub");
			await this.HubConnection.Start();
		}

		public Task<bool> Send(string eventName, T message)
		{
			bool returnValue = false;

			try
			{
				this.Proxy.Invoke(eventName, message);
				returnValue = true;
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
			// *** Release the proxy
			// ***
			this.Proxy = null;

			// ***
			// *** Release the hub
			// ***
			if (this.HubConnection != null)
			{
				this.HubConnection.Dispose();
				this.HubConnection = null;
			}
		}
	}
}

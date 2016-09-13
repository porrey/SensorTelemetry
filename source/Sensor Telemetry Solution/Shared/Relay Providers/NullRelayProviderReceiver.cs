using System.Threading.Tasks;
using Porrey.SensorTelemetry.Interfaces;

namespace Porrey.SensorTelemetry.Relays
{
	public class NullRelayProviderReceiver<T> : IRelayProviderReceiver<T>
	{
		public Task Initialize()
		{
			return Task.FromResult(0);
		}

		public void SetCallback(string eventName, IRelayProviderCallbackDelegate<T> callback)
		{
		}

		public void Dispose()
		{
		}
	}
}

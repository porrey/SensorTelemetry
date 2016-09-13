using System.Threading.Tasks;
using Porrey.SensorTelemetry.Interfaces;

namespace Porrey.SensorTelemetry.Relays
{
	public class NullRelayProviderSender<T> : IRelayProviderSender<T>
	{
		public Task Initialize()
		{
			return Task.FromResult(0);
		}

		public Task<bool> Send(string eventName, T message)
		{
			return Task.FromResult(true);
		}

		public void Dispose()
		{
		}
	}
}

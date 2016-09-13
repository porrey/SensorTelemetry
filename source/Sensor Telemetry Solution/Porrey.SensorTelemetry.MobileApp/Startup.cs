using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Porrey.SensorTelemetry.MobileApp.Startup))]

namespace Porrey.SensorTelemetry.MobileApp
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			ConfigureMobileApp(app);
			app.MapSignalR();
		}
	}
}
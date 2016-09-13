// Copyright © 2015-2016 Daniel Porrey
//
// This file is part of the Sensor Telemetry solution.
// 
// Sensor Telemetry is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Sensor Telemetry is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Sensor Telemetry. If not, see http://www.gnu.org/licenses/.
//
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Models;
using Porrey.SensorTelemetry.Relays;
using Porrey.SensorTelemetry.Repositories;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Events;
using Prism.Unity.Windows;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace Porrey.SensorTelemetry
{
	public sealed partial class App : PrismUnityApplication
	{
		public App()
		{
			this.InitializeComponent();
		}

		protected override Task OnInitializeAsync(IActivatedEventArgs args)
		{
			// ***
			// *** The temperature data will come from the emote IoT device, 
			// *** so use a NULL device.
			// ***
			this.Container.RegisterType<ITemperatureRepository, NullTemperatureRepository>(new ContainerControlledLifetimeManager());

			// ***
			// *** Set the hub configuration values
			// ***
			IIotHubConfiguration iotHubConfiguration = new IotHubConfiguration()
			{
				ConnectionString = "{ YOUR CONNECTION STRING HERE}"
				DeviceId = "{YOUR DEVICE ID HERE}",
			};
			this.Container.RegisterInstance<IIotHubConfiguration>(iotHubConfiguration, new ContainerControlledLifetimeManager());

			// ***
			// *** All device commands will pass through an IoT Hub. This version of the application is
			// *** loaded to the IoT device and will only be SENDING commands.
			// ***
			this.Container.RegisterType<IRelayProviderSender<DeviceCommandEventArgs>, IotHubRelayProviderSender<DeviceCommandEventArgs>>(new ContainerControlledLifetimeManager());
			this.Container.RegisterType<IRelayProviderReceiver<DeviceCommandEventArgs>, NullRelayProviderReceiver<DeviceCommandEventArgs>>(new ContainerControlledLifetimeManager());

			// ***
			// *** Initialize the container with registrations used by both versions of the application.
			// ***
			UnityConfiguration.InitializeContainer(this.Container, this.NavigationService);

			return base.OnInitializeAsync(args);
		}

		protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
		{
			// ***
			// *** Navigate to the main page
			// ***
			this.NavigationService.Navigate(MagicValue.Views.StartPage, null);
			Window.Current.Activate();

			return Task.FromResult<object>(null);
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
			eventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(e.Exception));
			e.Handled = true;
		}
	}
}

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
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Relays;
using Porrey.SensorTelemetry.Repositories;
using Porrey.SensorTelemetry.Services;
using Porrey.SensorTelemetry.Shared.Models;
using Porrey.SensorTelemetry.ViewModels;
using Prism.Events;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using Windows.ApplicationModel.Resources;
using ppatierno.AzureSBLite.Messaging;

namespace Porrey.SensorTelemetry
{
	public static class UnityConfiguration
	{
		public static Task InitializeContainer(IUnityContainer container, INavigationService navigationService)
		{
			// ***
			// *** Add general application objects
			// ***
			container.RegisterInstance<IApplicationInstanceIdentity>(new ApplicationInstanceIdentity(), new ContainerControlledLifetimeManager());
			container.RegisterInstance<IEventAggregator>(new EventAggregator(), new ContainerControlledLifetimeManager());
			container.RegisterInstance<INavigationService>(navigationService);
			container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));
			container.RegisterType<IApplicationSettingsRepository, ApplicationSettingsRepository>(new ContainerControlledLifetimeManager());

			// ***
			// *** Set the mobile services configuration
			// ***
			IMobileServicesConfiguration mobileServicesConfiguration = new MobileServicesConfiguration()
			{
				Url = "{YOUR MOBILE SERVICES URL HERE}"
			};
			container.RegisterInstance<IMobileServicesConfiguration>(mobileServicesConfiguration, new ContainerControlledLifetimeManager());

			// ***
			// *** Set the mobile services configuration
			// ***
			IServiceBusConfiguration serviceBusConfiguration = new ServiceBusConfiguration()
			{
				ConnectionString = "{YOUR MOBILE SERVICES URL HERE}",
				QueueName = "{YOUR EVENT HUB NAME HERE}",
				DefaultTransportType = TransportType.Amqp
			};
			container.RegisterInstance<IServiceBusConfiguration>(serviceBusConfiguration, new ContainerControlledLifetimeManager());

			// ***
			// *** All temperature readings will pass through SignalR.
			// ***
			container.RegisterType<IRelayProviderSender<TemperatureChangedEventArgs>, SignalrRelayProviderSender<TemperatureChangedEventArgs>>(new ContainerControlledLifetimeManager());
			container.RegisterType<IRelayProviderReceiver<TemperatureChangedEventArgs>, SignalrRelayProviderReceiver<TemperatureChangedEventArgs>>(new ContainerControlledLifetimeManager());

			// ***
			// *** Background Services
			// ***
			container.RegisterType<IBackgroundService, DebugConsoleService>(MagicValue.BackgroundService.Debug, new ContainerControlledLifetimeManager());
			container.RegisterType<IBackgroundService, NotificationRelayService>(MagicValue.BackgroundService.Relay, new ContainerControlledLifetimeManager());
			container.RegisterType<IBackgroundService, TimerService>(MagicValue.BackgroundService.Timer, new ContainerControlledLifetimeManager());

			// ***
			// *** The Debug Console Service also doubles as the IDebugConsoleRepository.
			// ***
			container.RegisterType<IDebugConsoleRepository>(new InjectionFactory((c) =>
			{
				return (IDebugConsoleRepository)c.Resolve<IBackgroundService>(MagicValue.BackgroundService.Debug);
			}));

			// ***
			// *** View Models
			// ***
			container.RegisterType<MainPageViewModel, MainPageViewModel>(new ContainerControlledLifetimeManager());
			container.RegisterType<StartPageViewModel, StartPageViewModel>(new ContainerControlledLifetimeManager());

			return Task.FromResult(0);
		}
	}
}

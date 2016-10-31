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
using System;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Interfaces;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Events;

namespace Porrey.SensorTelemetry.Services
{
	/// <summary>
	/// This service relays commands between the internal Event Aggregator and external
	/// services so that messages sent are relayed to and from other clients.
	/// </summary>
	public class NotificationRelayService : IBackgroundService
	{
		private EventRelayMap<TemperatureChangedEventArgs> _temperatureChangedEventMap = null;
		private EventRelayMap<DeviceCommandEventArgs> _deviceCommandEventMap = null;

		public string Name => "Notification Relay";

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		[Dependency]
		protected IApplicationInstanceIdentity ApplicationInstanceIdentity { get; set; }

		[Dependency]
		protected IRelayProviderSender<TemperatureChangedEventArgs> TemperatureChangedRelayProviderSender { get; set; }

		[Dependency]
		protected IRelayProviderReceiver<TemperatureChangedEventArgs> TemperatureChangedRelayProviderReceiver { get; set; }

		[Dependency]
		protected IRelayProviderSender<DeviceCommandEventArgs> DeviceCommandRelayProviderSender { get; set; }

		[Dependency]
		protected IRelayProviderReceiver<DeviceCommandEventArgs> DeviceCommandRelayProviderReceiver { get; set; }

		public async Task<bool> Start()
		{
			bool returnValue = false;

			try
			{
				// ***
				// *** Initialize the relay providers.
				// ***
				await this.TemperatureChangedRelayProviderSender?.Initialize();
				await this.TemperatureChangedRelayProviderReceiver?.Initialize();
				await this.DeviceCommandRelayProviderSender?.Initialize();
				await this.DeviceCommandRelayProviderReceiver?.Initialize();

				// ***
				// *** Relay the TemperatureChangedEventArgs via SignalR.
				// ***
				_temperatureChangedEventMap = new EventRelayMap<TemperatureChangedEventArgs>(this.ApplicationInstanceIdentity,
												this.TemperatureChangedRelayProviderSender,
												this.TemperatureChangedRelayProviderReceiver,
												this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>(),
												(e) => { e.SensorReading.Source = ApplicationSensorReadingSource.Cloud; return e; },
												(e) => { return e.SensorReading.Source == ApplicationSensorReadingSource.Device; },
												(e) => { return e; },
												(e) => { return e.SensorReading.Source == ApplicationSensorReadingSource.Device; });

				// ***
				// *** Relay the DeviceCommandEventArgs via an IoT Hub
				// ***
				_deviceCommandEventMap = new EventRelayMap<DeviceCommandEventArgs>(this.ApplicationInstanceIdentity,
												 this.DeviceCommandRelayProviderSender,
												 this.DeviceCommandRelayProviderReceiver,
												this.EventAggregator.GetEvent<Events.DeviceCommandEvent>(),
												(e) => { return e; },
												(e) => { return true; },
												(e) => { return e; },
												(e) => { return true; });

				returnValue = true;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return returnValue;
		}

		public Task<bool> Stop()
		{
			bool returnValue = false;

			if (_temperatureChangedEventMap != null)
			{
				_temperatureChangedEventMap.Dispose();
				_temperatureChangedEventMap = null;
			}

			if (_deviceCommandEventMap != null)
			{
				_deviceCommandEventMap.Dispose();
				_deviceCommandEventMap = null;
			}


			return Task<bool>.FromResult(returnValue);
		}
	}
}

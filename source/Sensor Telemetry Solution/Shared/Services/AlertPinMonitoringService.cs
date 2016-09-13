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
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Events;
using Windows.Devices.Gpio;

namespace Porrey.SensorTelemetry.Services
{
	public class AlertPinMonitoringService : IBackgroundService
	{
		private GpioPin _alertPin = null;
		private bool _previouseStatus = false;

		public string Name => "Alert Pin Monitoring";

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		[Dependency]
		protected IGpioPinConfiguration GpioPinConfiguration { get; set; }

		[Dependency]
		protected ITemperatureRepository TemperatureRepository { get; set; }

		protected GpioPin AlertPin
		{
			get
			{
				return _alertPin;
			}
			set
			{
				_alertPin = value;
			}
		}

		protected bool PreviouseStatus
		{
			get
			{
				return _previouseStatus;
			}

			set
			{
				_previouseStatus = value;
			}
		}

		public Task<bool> Start()
		{
			bool returnValue = false;

			try
			{
				GpioController gpio = GpioController.GetDefault();

				if (gpio != null)
				{
					// ***
					// *** Configure the pin to read from
					// ***
					this.AlertPin = gpio.OpenPin(this.GpioPinConfiguration.AlertPinNumber);
					this.AlertPin.SetDriveMode(GpioPinDriveMode.Input);

					// ***
					// *** Capture changes on the alert pin
					// ***
					if (this.AlertPin != null)
					{
						this.AlertPin.ValueChanged += AlertPin_ValueChanged;
					}

					returnValue = true;
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
				returnValue = false;
			}

			return Task<bool>.FromResult(returnValue);
		}

		public Task<bool> Stop()
		{
			bool returnValue = false;

			try
			{
				if (this.AlertPin != null)
				{
					this.AlertPin.Dispose();
					this.AlertPin = null;
				}

				returnValue = true;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
				returnValue = false;
			}

			return Task<bool>.FromResult(returnValue);
		}

		private async void AlertPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
		{
			try
			{
				// ***
				// *** Since the previous state is being compared before firing the event, it 
				// *** does not matter if we check rising or falling edge. The event will only
				// *** be fired once for each change.
				// ***
				if (this.TemperatureRepository.AlertIsActive != this.PreviouseStatus)
				{
					this.PreviouseStatus = this.TemperatureRepository.AlertIsActive;
					await this.OnTemperatureAlertChangedEvent();

					// ***
					// *** Log this action
					// ***
					this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(DebugEventType.Information, this.Name, "Alert Pin value changed [Is Active = {0}]", this.TemperatureRepository.AlertIsActive));
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		protected async Task OnTemperatureAlertChangedEvent()
		{
			try
			{
				if (this.TemperatureRepository != null)
				{
					this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>().Publish(new TemperatureChangedEventArgs()
					{
						SensorReading = ApplicationSensorReading.FromIApplicationSensorReading(await this.TemperatureRepository.GetSensorReading())
					});
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}
	}
}

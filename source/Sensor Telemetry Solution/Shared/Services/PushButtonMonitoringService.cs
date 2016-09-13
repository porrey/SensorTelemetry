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
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Events;
using Windows.Devices.Gpio;

namespace Porrey.SensorTelemetry.Services
{
	public class PushButtonMonitoringService : IBackgroundService
	{
		private bool _buttonIsDown = false;

		public string Name => "Push Button Monitoring";

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		[Dependency]
		protected IGpioPinConfiguration GpioPinConfiguration { get; set; }

		[Dependency]
		protected IDebugConsoleRepository DebugConsoleProvider { get; set; }

		protected GpioPinValue PreviousValue { get; set; } = GpioPinValue.High;
		protected GpioPin PushButtonPin { get; set; }

		public Task<bool> Start()
		{
			bool returnValue = false;

			try
			{
				GpioController gpio = GpioController.GetDefault();

				if (gpio != null)
				{
					this.PushButtonPin = gpio.OpenPin(this.GpioPinConfiguration.PushButtonPinNumber);
					this.PushButtonPin.SetDriveMode(GpioPinDriveMode.Input);
					this.PushButtonPin.ValueChanged += PushButtonPin_ValueChanged;
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
				if (this.PushButtonPin != null)
				{
					// ***
					// *** Remove the event handler
					// ***
					this.PushButtonPin.ValueChanged -= PushButtonPin_ValueChanged;

					// ***
					// *** Release the pin
					// ***

					this.PushButtonPin.Dispose();
					this.PushButtonPin = null;
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

		private void PushButtonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
		{
			GpioPinValue value = this.PushButtonPin.Read();

			try
			{
				if (args.Edge == GpioPinEdge.FallingEdge && value == GpioPinValue.Low && this.PreviousValue == GpioPinValue.High)
				{
					// ***
					// *** The button was pressed
					// ***
					_buttonIsDown = true;

					// ***
					// *** Log this event
					// ***
					this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(
							DebugEventType.Information,
							this.Name,
							"The button was pressed."));
				}
				else if (args.Edge == GpioPinEdge.RisingEdge && _buttonIsDown) 
				{
					// ***
					// *** The button was released
					// ***
					_buttonIsDown = false;

					// ***
					// *** Log this event
					// ***
					this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(
							DebugEventType.Information,
							this.Name,
							"The button was released."));

					// ***
					// *** Send the device command event
					// ***
					this.EventAggregator.GetEvent<Events.DeviceCommandEvent>().Publish(new DeviceCommandEventArgs(DeviceCommand.ResetAlert));
				}
				else
				{
					// ***
					// *** Log this event
					// ***
					this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(
							DebugEventType.Information,
							this.Name,
							"The button was reset."));

					// ***
					// *** Reset to default state
					// ***
					_buttonIsDown = false;
					this.PreviousValue = GpioPinValue.High;
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
			finally
			{
				this.PreviousValue = value;
			}
		}		
	}
}

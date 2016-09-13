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
	public class LedService : IBackgroundService
	{
		private SubscriptionToken _temperatureAlertChangedEventToken = null;
		private SubscriptionToken _deviceCommandEventToken = null;
		private bool _runningLedTest = false;
		private ApplicationSensorReading _previousSensorReading = null;

		public string Name => "LED";

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		[Dependency]
		protected IGpioPinConfiguration GpioPinConfiguration { get; set; }

		protected GpioPin GreenPin { get; set; }
		protected GpioPin RedPin { get; set; }
		protected GpioPin BluePin { get; set; }
		protected GpioPin YellowPin { get; set; }

		public Task<bool> Start()
		{
			bool returnValue = false;

			try
			{
				GpioController gpio = GpioController.GetDefault();

				if (gpio != null)
				{
					this.GreenPin = gpio.OpenPin(this.GpioPinConfiguration.GreenLedPinNumber);
					this.GreenPin.SetDriveMode(GpioPinDriveMode.Output);

					this.RedPin = gpio.OpenPin(this.GpioPinConfiguration.RedLedPinNumber);
					this.RedPin.SetDriveMode(GpioPinDriveMode.Output);

					this.BluePin = gpio.OpenPin(this.GpioPinConfiguration.BluePinNumber);
					this.BluePin.SetDriveMode(GpioPinDriveMode.Output);

					this.YellowPin = gpio.OpenPin(this.GpioPinConfiguration.YellowPinNumber);
					this.YellowPin.SetDriveMode(GpioPinDriveMode.Output);

					// ***
					// *** Initialize all to off
					// ***
					this.GreenPin.Write(GpioPinValue.High);
					this.RedPin.Write(GpioPinValue.High);
					this.BluePin.Write(GpioPinValue.High);
					this.YellowPin.Write(GpioPinValue.High);

					this._temperatureAlertChangedEventToken = this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>().Subscribe((args) =>
					{
						this.UpdateLeds(args.SensorReading);
					}, ThreadOption.BackgroundThread);


					this.EventAggregator.GetEvent<Events.DeviceCommandEvent>().Subscribe(async (args) =>
					{
						// ***
						// *** This service will handle the LED test command
						// ***
						if (!_runningLedTest && args.DeviceCommand == DeviceCommand.RunLedTest)
						{
							this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(
								DebugEventType.Information,
								nameof(LedService),
								"A Run LED Test command was received."));

							await this.RunLedTest();
						}
					});

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
				if (_temperatureAlertChangedEventToken != null)
				{
					this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>().Unsubscribe(_temperatureAlertChangedEventToken);
					_temperatureAlertChangedEventToken.Dispose();
					_temperatureAlertChangedEventToken = null;
				}

				if (_deviceCommandEventToken != null)
				{
					this.EventAggregator.GetEvent<Events.DeviceCommandEvent>().Unsubscribe(_deviceCommandEventToken);
					_deviceCommandEventToken.Dispose();
					_deviceCommandEventToken = null;
				}

				if (this.GreenPin != null)
				{
					this.GreenPin.Dispose();
					this.GreenPin = null;
				}

				if (this.RedPin != null)
				{
					this.RedPin.Dispose();
					this.RedPin = null;
				}

				if (this.BluePin != null)
				{
					this.BluePin.Dispose();
					this.BluePin = null;
				}

				if (this.YellowPin != null)
				{
					this.YellowPin.Dispose();
					this.YellowPin = null;
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

		private Task UpdateLeds(ApplicationSensorReading sensorReading)
		{
			if (!_runningLedTest)
			{
				// ***
				// *** Green is "all clear"
				// ***
				bool allClear = !sensorReading.IsCritical && !sensorReading.IsAboveUpperThreshold && !sensorReading.IsBelowLowerThreshold;
				this.GreenPin.Write(allClear ? GpioPinValue.Low : GpioPinValue.High);

				// ***
				// *** Red is > upper threshold temperature
				// ***
				this.RedPin.Write(sensorReading.IsAboveUpperThreshold ? GpioPinValue.Low : GpioPinValue.High);

				// ***
				// *** Blue is < upper threshold temperature
				// ***
				this.BluePin.Write(sensorReading.IsBelowLowerThreshold ? GpioPinValue.Low : GpioPinValue.High);

				// ***
				// *** Yellow is >= critical temperature
				// ***
				this.YellowPin.Write(sensorReading.IsCritical ? GpioPinValue.Low : GpioPinValue.High);

				// ***
				// *** Cache the last sensor reading
				// ***
				_previousSensorReading = sensorReading;
            }

			return Task.FromResult(0);
		}

		/// <summary>
		/// This is a test of the LED's. It runs through a pattern to ensure the
		/// LED's are all working. They also run in order (from left to right)  
		/// to ensure the correct wiring.
		/// </summary>
		/// <returns></returns>
		private async Task RunLedTest()
		{
			try
			{
				// ***
				// *** Flag that we are testing so the temperature updates
				// *** do not interfere
				// ***
				_runningLedTest = true;

				// ***
				// *** Delay in milliseconds between each LED
				// ***
				int delay = 300;

				// ***
				// *** All LED's off
				// ***
				this.BluePin.Write(GpioPinValue.High);
				this.GreenPin.Write(GpioPinValue.High);
				this.RedPin.Write(GpioPinValue.High);
				this.YellowPin.Write(GpioPinValue.High);
				await Task.Delay(delay);

				// ***
				// *** Run through each color 3 times
				// ***
				for (int i = 0; i < 3; i++)
				{
					// ***
					// *** Blue
					// ***
					this.BluePin.Write(GpioPinValue.Low);
					await Task.Delay(delay);
					this.BluePin.Write(GpioPinValue.High);
					await Task.Delay(delay);

					// ***
					// *** Green
					// ***
					this.GreenPin.Write(GpioPinValue.Low);
					await Task.Delay(delay);
					this.GreenPin.Write(GpioPinValue.High);
					await Task.Delay(delay);

					// ***
					// *** Red
					// ***
					this.RedPin.Write(GpioPinValue.Low);
					await Task.Delay(delay);
					this.RedPin.Write(GpioPinValue.High);
					await Task.Delay(delay);

					// ***
					// *** Yellow
					// ***
					this.YellowPin.Write(GpioPinValue.Low);
					await Task.Delay(delay);
					this.YellowPin.Write(GpioPinValue.High);
					await Task.Delay((int)(delay * 1.5));
				}

				// ***
				// *** Now flash them all at the same time (x3)
				// ***
				for (int i = 0; i < 3; i++)
				{
					this.BluePin.Write(GpioPinValue.Low);
					this.GreenPin.Write(GpioPinValue.Low);
					this.RedPin.Write(GpioPinValue.Low);
					this.YellowPin.Write(GpioPinValue.Low);
					await Task.Delay(delay);

					this.BluePin.Write(GpioPinValue.High);
					this.GreenPin.Write(GpioPinValue.High);
					this.RedPin.Write(GpioPinValue.High);
					this.YellowPin.Write(GpioPinValue.High);
					await Task.Delay(delay);
				}

			}
			finally
			{
				_runningLedTest = false;

				// ***
				// *** Restore the LED's
				// ***
				if (_previousSensorReading != null)
				{
					await this.UpdateLeds(_previousSensorReading);
				}
			}
		}
	}
}

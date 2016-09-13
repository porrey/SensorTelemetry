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
using Windows.Devices.I2c;
using Windows.Devices.Sensors;
using Windows.Devices.Sensors.Interfaces;

namespace Porrey.SensorTelemetry.Repositories
{
	public class Mcp9808TemperatureRepository : ITemperatureRepository, IDisposable
	{
		private IApplicationSensorReading _previousReading = null;
		private SubscriptionToken _deviceCommandEventToken = null;
		private SubscriptionToken _timerEventToken = null;

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		[Dependency]
		protected IApplicationSettingsRepository ApplicationSettingsRepository { get; set; }

		public async Task<bool> Connect()
		{
			bool returnValue = false;

			try
			{
				// ***
				// *** Get the device at the default address
				// ***
				this.Device = await SensorController.GetI2CDevice<IMcp9808>(Mcp9808Address.Default, I2cBusSpeed.FastMode);

				if (this.Device != null)
				{
					if (await this.Device.Initialize() == InitializationResult.Successful)
					{
						this.Device.AlertEnabled = true;
						this.Device.AlertOutputPolarity = Mcp9808AlertOutputPolarity.ActiveHigh;
						this.Device.AlertOutputSelect = Mcp9808AlertOutputSelect.All;
						this.Device.AlertOutputMode = Mcp9808AlertOutputMode.ComparatorMode;
						this.Device.LowerTemperatureThreshold = this.ApplicationSettingsRepository.LowerTemperatureThreshold;
						this.Device.UpperTemperatureThreshold = this.ApplicationSettingsRepository.UpperTemperatureThreshold;
						this.Device.CriticalTemperatureThreshold = this.ApplicationSettingsRepository.CriticalTemperatureThreshold;
						this.Device.ClearInterrupt();

						returnValue = true;
					}
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return returnValue;
		}

		public Task Start()
		{
			try
			{
				// ***
				// *** Subscribe to the device command event
				// ***
				_deviceCommandEventToken = this.EventAggregator.GetEvent<Events.DeviceCommandEvent>().Subscribe((args) =>
				{
					this.OnDeviceCommand(args);
				}, ThreadOption.BackgroundThread);

				// ***
				// *** Start the timer
				// ***
				_timerEventToken = this.EventAggregator.GetEvent<Events.TimerEvent>().Subscribe((args) =>
				{
					this.OnTimerEvent(args);
				});
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return Task.FromResult(0);
		}

		public Task Stop()
		{
			try
			{
				// ***
				// *** Unsubscribe from timer event
				// ***
				if (_timerEventToken != null)
				{
					this.EventAggregator.GetEvent<Events.TimerEvent>().Unsubscribe(_timerEventToken);
					_timerEventToken.Dispose();
					_timerEventToken = null;
				}

				// ***
				// *** Unsubscribe from device command event
				// ***
				if (_deviceCommandEventToken != null)
				{
					this.EventAggregator.GetEvent<Events.DeviceCommandEvent>().Unsubscribe(_deviceCommandEventToken);
					_deviceCommandEventToken.Dispose();
					_deviceCommandEventToken = null;
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return Task.FromResult(0);
		}

		public async Task<IApplicationSensorReading> GetSensorReading()
		{
			IApplicationSensorReading returnValue = null;

			try
			{
				if (this.Device != null && this.Device.IsInitialized)
				{
					IDeviceSensorReading sensorReading = (await this.Device.ReadSensor());
					returnValue = sensorReading.ConvertToApplicationSensorReading(ApplicationSensorReadingSource.Device);
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return returnValue;
		}

		public bool AlertIsActive
		{
			get
			{
				bool returnValue = false;

				if (this.Device != null)
				{
					returnValue = this.Device.AlertOutputStatus == Mcp9808AlertOutputStatus.Asserted;
				}

				return returnValue;
			}
		}

		public IMcp9808 Device { get; set; }

		private async Task SendSensorReading(bool force)
		{
			try
			{
				IApplicationSensorReading sensorReading = await this.GetSensorReading();

				// ***
				// *** Add the temperature thresholds
				// ***
				sensorReading.CriticalThreshold = this.Device.CriticalTemperatureThreshold;
				sensorReading.LowerThreshold = this.Device.LowerTemperatureThreshold;
				sensorReading.UpperThreshold = this.Device.UpperTemperatureThreshold;

				// ***
				// *** Only send the event when the reading has changed
				// ***
				if (force ||
					_previousReading == null ||
					_previousReading.Temperature != sensorReading.Temperature ||
					_previousReading.IsCritical != sensorReading.IsCritical ||
					_previousReading.IsAboveUpperThreshold != sensorReading.IsAboveUpperThreshold ||
					_previousReading.IsBelowLowerThreshold != sensorReading.IsBelowLowerThreshold ||
					_previousReading.CriticalThreshold != sensorReading.CriticalThreshold ||
					_previousReading.LowerThreshold != sensorReading.LowerThreshold ||
					_previousReading.UpperThreshold != sensorReading.UpperThreshold)
				{
					this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>().Publish(new TemperatureChangedEventArgs()
					{
						SensorReading = ApplicationSensorReading.FromIApplicationSensorReading(sensorReading)
					});
					_previousReading = sensorReading;
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		private async void OnTimerEvent(TimerEventArgs e)
		{
			try
			{
				// ***
				// *** Read the sensor once every second
				// ***
				if (e.IsMyInterval(TimeSpan.FromSeconds(1)))
				{
					await this.SendSensorReading(false);
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		protected async void OnDeviceCommand(DeviceCommandEventArgs e)
		{
			try
			{
				if (e.DeviceCommand == DeviceCommand.UpdateTemperature)
				{
					// ***
					// *** Log this event
					// ***
					this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(
							DebugEventType.Information,
							nameof(Mcp9808TemperatureRepository),
							"An Update Temperature command was received."));

					// ***
					// *** Send out a sensor reading to all devices
					// ***
					await this.SendSensorReading(true);
				}
				else if (e.DeviceCommand == DeviceCommand.ResetAlert)
				{
					// ***
					// *** Log this event
					// ***
					this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(
							DebugEventType.Information,
							nameof(Mcp9808TemperatureRepository),
							"A Reset Alert command was received."));

					// ***
					// *** Check if we have a device
					// ***
					if (this.Device != null)
					{
						// ***
						// *** Reset the interrupt
						// ***
						this.Device.ClearInterrupt();
					}
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		public void Dispose()
		{
			// ***
			// *** Dispose the MCP9808 device
			// ***
			this.Device.Dispose();
			this.Device = null;

			this.Stop().Wait();
		}
	}
}

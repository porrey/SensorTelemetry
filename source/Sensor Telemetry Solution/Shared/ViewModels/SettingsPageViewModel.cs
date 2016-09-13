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
using System.Collections.Generic;
using System.Threading.Tasks;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Commands;
using Prism.Windows.Navigation;
using Windows.Devices.Sensors.Interfaces;

namespace Porrey.SensorTelemetry.ViewModels
{
	public class SettingsPageViewModel : SensorTelemetryViewModel
	{
		public SettingsPageViewModel()
		{
			// ***
			// *** Set up the commands
			// ***
			this.AutoAlertModeCommand = DelegateCommand.FromAsyncHandler(this.OnAutoAlertModeCommand, this.OnCanAutoAlertModeCommand);
			this.ManualAlertModeCommand = DelegateCommand.FromAsyncHandler(this.OnManualAlertModeCommand, this.OnCanManualAlertModeCommand);
		}

		protected override string OnGetPageName() => "MCP9808 Settings";

		public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
		{
			base.OnNavigatedTo(e, viewModelState);

			// ***
			// *** Get the device at the default address
			// ***
			this.LowerTemperatureThreshold = this.ApplicationSettingsRepository.LowerTemperatureThreshold;
			this.UpperTemperatureThreshold = this.ApplicationSettingsRepository.UpperTemperatureThreshold;
			this.CriticalTemperatureThreshold = this.ApplicationSettingsRepository.CriticalTemperatureThreshold;
		}

		public bool IsEnabled => this.TemperatureRepository.Device != null;

		public float CriticalTemperatureThreshold
		{
			get
			{
				return this.TemperatureRepository.Device != null ? this.TemperatureRepository.Device.CriticalTemperatureThreshold : 0f;
			}
			set
			{
				// ***
				// *** value is ALWAYS Celsius
				// ***
				if (this.TemperatureRepository.Device != null)
				{
					this.TemperatureRepository.Device.CriticalTemperatureThreshold = value;
					this.ApplicationSettingsRepository.CriticalTemperatureThreshold = value;
					this.OnPropertyChanged(nameof(CriticalTemperatureThreshold));
				}
			}
		}

		public float LowerTemperatureThreshold
		{
			get
			{
				return this.TemperatureRepository.Device != null ? this.TemperatureRepository.Device.LowerTemperatureThreshold : 0f;
			}
			set
			{
				if (this.TemperatureRepository.Device != null)
				{
					// ***
					// *** value is ALWAYS Celsius
					// ***
					this.TemperatureRepository.Device.LowerTemperatureThreshold = value;
					this.ApplicationSettingsRepository.LowerTemperatureThreshold = value;
					this.OnPropertyChanged(nameof(LowerTemperatureThreshold));
				}
			}
		}

		public float UpperTemperatureThreshold
		{
			get
			{
				return this.TemperatureRepository.Device != null ? this.TemperatureRepository.Device.UpperTemperatureThreshold : 0f;
			}
			set
			{
				// ***
				// *** value is ALWAYS Celsius
				// ***
				if (this.TemperatureRepository.Device != null)
				{
					this.TemperatureRepository.Device.UpperTemperatureThreshold = value;
					this.ApplicationSettingsRepository.UpperTemperatureThreshold = value;
					this.OnPropertyChanged(nameof(UpperTemperatureThreshold));
				}
			}
		}

		public bool AutoAlertResetMode
		{
			get
			{
				return this.TemperatureRepository.Device != null ? this.TemperatureRepository.Device.AlertOutputMode == Mcp9808AlertOutputMode.ComparatorMode : false;
			}
			set
			{
				if (this.TemperatureRepository.Device != null)
				{
					// ***
					// *** Update the device and the application settings
					// ***
					if (value)
					{
						this.TemperatureRepository.Device.AlertOutputMode = Mcp9808AlertOutputMode.ComparatorMode;
						this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(DebugEventType.Information, "MCP9808 Settings", "Change alert mode to 'Comparator'."));
					}
					else
					{
						this.TemperatureRepository.Device.AlertOutputMode = Mcp9808AlertOutputMode.InterruptMode;
						this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(DebugEventType.Information, "MCP9808 Settings", "Change alert mode to 'Interrupt'."));
					}

					// ***
					// *** Update bindings
					// ***
					this.AutoAlertModeCommand.RaiseCanExecuteChanged();
					this.ManualAlertModeCommand.RaiseCanExecuteChanged();
					this.OnPropertyChanged(nameof(AutoAlertResetMode));
				}
			}
		}

		public DelegateCommand AutoAlertModeCommand { get; set; }

		protected bool OnCanAutoAlertModeCommand() => this.OnCanCommandWrapper(() =>
														{
															bool returnValue = false;

															if (this.TemperatureRepository.Device != null)
															{
																returnValue = this.TemperatureRepository.Device.AlertOutputMode == Mcp9808AlertOutputMode.ComparatorMode;
															}

															return returnValue;
														});

		protected Task OnAutoAlertModeCommand()
		{
			try
			{
				this.AutoAlertResetMode = true;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return Task.FromResult(0);
		}

		public DelegateCommand ManualAlertModeCommand { get; set; }

		protected bool OnCanManualAlertModeCommand() => this.OnCanCommandWrapper(() =>
															{
																bool returnValue = false;

																if (this.TemperatureRepository.Device != null)
																{
																	returnValue = this.TemperatureRepository.Device.AlertOutputMode == Mcp9808AlertOutputMode.ComparatorMode;
																}

																return returnValue;
															});

		protected Task OnManualAlertModeCommand()
		{
			try
			{
				this.AutoAlertResetMode = false;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return Task.FromResult(0);
		}
	}
}
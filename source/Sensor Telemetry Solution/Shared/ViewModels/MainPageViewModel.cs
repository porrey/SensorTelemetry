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
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Interfaces;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Windows.Navigation;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace Porrey.SensorTelemetry.ViewModels
{
	public class MainPageViewModel : SensorTelemetryViewModel
	{
		private SubscriptionToken _temperatureChangedEventToken = null;

		public MainPageViewModel()
		{
			// ***
			// *** Set up the commands
			// ***
			this.ViewHistoryCommand = DelegateCommand.FromAsyncHandler(this.OnViewHistory, this.OnCanViewHistory);
			this.FahrenheitCommand = DelegateCommand.FromAsyncHandler(this.OnFahrenheit, this.OnCanFahrenheit);
			this.CelsiusCommand = DelegateCommand.FromAsyncHandler(this.OnCelsius, this.OnCanCelsius);
			this.ResetAlertCommand = DelegateCommand.FromAsyncHandler(this.OnResetAlert, this.OnCanResetAlert);			

			// ***
			// *** Create a default value for the Sensor Reading
			// ***
			this.SensorReading = new ApplicationSensorReading()
			{
				Temperature = float.NaN,
				IsCritical = false,
				IsAboveUpperThreshold = false,
				IsBelowLowerThreshold = false,
				CriticalThreshold = float.NaN,
				UpperThreshold = float.NaN,
				LowerThreshold = float.NaN,
				Source = ApplicationSensorReadingSource.None,
				TimestampUtc = DateTimeOffset.MinValue
			};
		}

		protected override string OnGetPageName() => "Sensor Monitor";

		protected override bool OnIsHome() => true;

		public async override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
		{
			try
			{
				base.OnNavigatedTo(e, viewModelState);

				// ***
				// *** Get the first reading
				// ***
				await this.TriggerTemperatureUpdate();

				// ***
				// *** Subscribe to temperature change event
				// ***
				this._temperatureChangedEventToken = this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>().Subscribe((args) =>
				{
					this.OnTemperatureChangedEvent(args);
				}, ThreadOption.UIThread);

				// ***
				// *** Subscribe to the telemetry status changed event
				// ***
				this.EventAggregator.GetEvent<Events.TelemetryStatusChangedEvent>().Subscribe((args) =>
				{
					this.OnTelemetryStatusChangedEvent(args);
				}, ThreadOption.UIThread);

				// ***
				// *** Initialize the temperature unit
				// ***
				this.TemperatureUnit = this.ApplicationSettingsRepository.TemperatureUnit;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
		{
			try
			{
				if (this._temperatureChangedEventToken != null)
				{
					// ***
					// *** Unsubscribe from the events
					// ***
					if (this._temperatureChangedEventToken != null)
					{
						this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>().Unsubscribe(this._temperatureChangedEventToken);
						this._temperatureChangedEventToken.Dispose();
						this._temperatureChangedEventToken = null;
					}
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
			finally
			{
				base.OnNavigatingFrom(e, viewModelState, suspending);
			}
		}

		private async void OnTemperatureChangedEvent(TemperatureChangedEventArgs e)
		{
			try
			{
				this.SensorReading = e.SensorReading;
				this.ResetAlertCommand.RaiseCanExecuteChanged();

				if (e.SensorReading.Source == ApplicationSensorReadingSource.Device)
				{
					this.DeviceName = "MCP9808";
					this.ShowTelemetryStatus = true;
				}
				else if (e.SensorReading.Source == ApplicationSensorReadingSource.Cloud)
				{
					this.DeviceName = "Cloud";
					this.ShowTelemetryStatus = false;
				}
				else
				{
					this.DeviceName = "Not Detected";
					this.ShowTelemetryStatus = false;
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			await Task.FromResult(0);
		}

		private async void OnTelemetryStatusChangedEvent(TelemetryStatusChangedEventArgs args)
		{
			try
			{
				this.TelemetryIsSending = args.Status == TelemetryChangedStatus.Sending;
				this.TotalTelemetrySent = args.TotalSent;
				this.TotalTelemetryFailed = args.TotalFailed;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			await Task.FromResult(0);
		}

		private Task TriggerTemperatureUpdate()
		{
			try
			{
				// ***
				// *** Send a command to the device to trigger it to send
				// *** out a new sensor reading. This command will get
				// *** relayed to the device through the Notification
				// *** Service.
				// ***
				this.EventAggregator.GetEvent<Events.DeviceCommandEvent>().Publish(new DeviceCommandEventArgs(DeviceCommand.UpdateTemperature));
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return Task.FromResult(0);
		}

		private string _deviceName = "Unknown";
		public string DeviceName
		{
			get
			{
				return _deviceName;
			}
			set
			{
				this.SetProperty(ref _deviceName, value);
			}
		}

		private ApplicationSensorReading _sensorReading = null;
		public ApplicationSensorReading SensorReading
		{
			get
			{
				return _sensorReading;
			}
			set
			{
				this.SetProperty(ref _sensorReading, value);
			}
		}

		private string _temperatureUnit = string.Empty;
		public string TemperatureUnit
		{
			get
			{
				return _temperatureUnit;
			}
			set
			{
				this.SetProperty(ref _temperatureUnit, value);
				this.ApplicationSettingsRepository.TemperatureUnit = value;

				// ***
				// *** Notify bindings
				// ***
				this.CelsiusCommand.RaiseCanExecuteChanged();
				this.FahrenheitCommand.RaiseCanExecuteChanged();
			}
		}

		private bool _telemetryIsSending = false;
		public bool TelemetryIsSending
		{
			get
			{
				return _telemetryIsSending;
			}
			set
			{
				this.SetProperty(ref _telemetryIsSending, value);
			}
		}

		private long _totalTelemetrySent = 0;
		public long TotalTelemetrySent
		{
			get
			{
				return _totalTelemetrySent;
			}
			set
			{
				this.SetProperty(ref _totalTelemetrySent, value);
			}
		}

		private long _totalTelemetryFailed = 0;
		public long TotalTelemetryFailed
		{
			get
			{
				return _totalTelemetryFailed;
			}
			set
			{
				this.SetProperty(ref _totalTelemetryFailed, value);
			}
		}

		private bool _showTelemetryStatus = false;
		public bool ShowTelemetryStatus
		{
			get
			{
				return _showTelemetryStatus;
			}
			set
			{
				this.SetProperty(ref _showTelemetryStatus, value);
			}
		}

		#region Commands
		public DelegateCommand ViewHistoryCommand { get; set; }

		protected bool OnCanViewHistory() => true;

		protected async Task OnViewHistory()
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				if (!this.NavigationService.Navigate(MagicValue.Views.HistoryPage, null))
				{
					MessageDialog md = new MessageDialog("Could not navigate to page.", "Navigation Failed");
					await md.ShowAsync();
				}
			});
		}

		public DelegateCommand FahrenheitCommand { get; set; }

		protected bool OnCanFahrenheit() => this.OnCanCommandWrapper(() => this.TemperatureUnit != MagicValue.TemperatureUnit.Fahrenheit);

		protected async Task OnFahrenheit()
		{
			try
			{
				this.TemperatureUnit = MagicValue.TemperatureUnit.Fahrenheit;
				await this.TriggerTemperatureUpdate();
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		public DelegateCommand CelsiusCommand { get; set; }

		protected bool OnCanCelsius() => this.OnCanCommandWrapper(() => this.TemperatureUnit != MagicValue.TemperatureUnit.Celcius);

		protected async Task OnCelsius()
		{
			try
			{
				this.TemperatureUnit = MagicValue.TemperatureUnit.Celcius;
				await this.TriggerTemperatureUpdate();
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		public DelegateCommand ResetAlertCommand { get; set; }

		protected bool OnCanResetAlert() => this.OnCanCommandWrapper(() => this.SensorReading != null ? this.SensorReading.AlertActive : false);

		protected Task OnResetAlert()
		{
			try
			{
				// ***
				// *** Send the reset command to the device
				// ***
				this.EventAggregator.GetEvent<Events.DeviceCommandEvent>().Publish(new DeviceCommandEventArgs(DeviceCommand.ResetAlert));
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return Task.FromResult(0);
		}
		#endregion
	}
}

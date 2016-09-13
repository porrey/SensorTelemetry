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
using Newtonsoft.Json;
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Events;
using Prism.Mvvm;
using Windows.Storage;

namespace Porrey.SensorTelemetry.Repositories
{
	public class ApplicationSettingsRepository : BindableBase, IApplicationSettingsRepository
	{
		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		#region Settings
		public string TemperatureUnit
		{
			get
			{
				return this.GetSetting<string>(MagicValue.Property.TemperatureUnit, MagicValue.Defaults.TemperatureUnit);
			}
			set
			{
				this.SaveSetting<string>(MagicValue.Property.TemperatureUnit, value);
			}
		}

		public float CriticalTemperatureThreshold
		{
			get
			{
				return this.GetSetting<float>(MagicValue.Property.CriticalTemperatureThreshold, MagicValue.Defaults.CriticalTemperatureThreshold);
			}
			set
			{
				this.SaveSetting<float>(MagicValue.Property.CriticalTemperatureThreshold, value);
			}
		}

		public float UpperTemperatureThreshold
		{
			get
			{
				return this.GetSetting<float>(MagicValue.Property.UpperTemperatureThreshold, MagicValue.Defaults.UpperTemperatureThreshold);
			}
			set
			{
				this.SaveSetting<float>(MagicValue.Property.UpperTemperatureThreshold, value);
			}
		}

		public float LowerTemperatureThreshold
		{
			get
			{
				return this.GetSetting<float>(MagicValue.Property.LowerTemperatureThreshold, MagicValue.Defaults.LowerTemperatureThreshold);
			}
			set
			{
				this.SaveSetting<float>(MagicValue.Property.LowerTemperatureThreshold, value);
			}
		}
		#endregion

		public T GetSetting<T>(string name, T defaultValue)
		{
			T returnValue = default(T);

			try
			{
				if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(name))
				{
					// ***
					// *** WintRT will not serialize all objects, so use Newtonsoft.Json
					// ***
					string json = (string)ApplicationData.Current.RoamingSettings.Values[name];
					returnValue = JsonConvert.DeserializeObject<T>(json);
				}
				else
				{
					returnValue = defaultValue;
				}
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return returnValue;
		}

		public void SaveSetting<T>(string name, T value)
		{
			try
			{
				// ***
				// *** Not all objects will serialize so use Newtonsoft.Json for everything
				// ***
				string json = JsonConvert.SerializeObject(value);
				ApplicationData.Current.RoamingSettings.Values[name] = json;
				this.OnPropertyChanged(name);
				this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(name, value));
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		public Task ResetToDefaults()
		{
			try
			{
				ApplicationData.Current.RoamingSettings.Values.Clear();
				this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.TemperatureUnit, this.TemperatureUnit));
				this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.CriticalTemperatureThreshold, this.CriticalTemperatureThreshold));
				this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.LowerTemperatureThreshold, this.LowerTemperatureThreshold));
				this.EventAggregator.GetEvent<Events.ApplicationSettingChangedEvent>().Publish(new ApplicationSettingChangedEventArgs(MagicValue.Property.UpperTemperatureThreshold, this.UpperTemperatureThreshold));
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return Task.FromResult(0);
		}
	}
}

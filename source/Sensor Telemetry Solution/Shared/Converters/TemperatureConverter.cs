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
using Microsoft.Practices.ServiceLocation;
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Interfaces;
using Windows.UI.Xaml.Data;

namespace Porrey.SensorTelemetry.Converters
{
	public sealed class TemperatureConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			string returnValue = "--.-";

			// ***
			// *** Get the current display unit from the settings
			// ***
			IApplicationSettingsRepository settings = ServiceLocator.Current.GetInstance<IApplicationSettingsRepository>();

			// ***
			// *** Convert the value to a float
			// ***
			float temperature = System.Convert.ToSingle(value);

			if (!float.IsNaN(temperature))
			{
				// ***
				// *** Convert the temperature to Fahrenheit if the
				// *** current display unit is Fahrenheit (otherwise
				// *** it is already Celsius).
				// ***
				if (settings.TemperatureUnit == MagicValue.TemperatureUnit.Fahrenheit)
				{
					temperature = Temperature.ConvertToFahrenheit(temperature);
				}

				// ***
				// *** Format the output
				// ***
				returnValue = string.Format("{0:0.0°}{1}", temperature, settings.TemperatureUnit);
			}

			return returnValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}
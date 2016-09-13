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
namespace System
{
	/// <summary>
	/// Helper methods for temperature conversion
	/// </summary>
	public class Temperature
	{
		/// <summary>
		/// Converts a temperature in Celsius to Fahrenheit.
		/// </summary>
		/// <param name="fahrenheit">The temperature in Celsius.</param>
		/// <returns>Returns the temperature in Fahrenheit.</returns>
		public static float ConvertToCelsius(float fahrenheit) => (fahrenheit - 32f) * 5f / 9f;

		/// <summary>
		/// Converts a temperature in Fahrenheit to Celsius.
		/// </summary>
		/// <param name="celsius">The temperature in Fahrenheit.</param>
		/// <returns>Returns the temperature in Celsius.</returns>
		public static float ConvertToFahrenheit(float celsius) => (celsius * 9f / 5f) + 32f;
	}
}

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
using Porrey.SensorTelemetry.Interfaces;

namespace Porrey.SensorTelemetry.Models
{
	/// <summary>
	/// Specifies the GPIO Pin Numbers for the various pins used.
	/// </summary>
	public class GpioPinConfiguration : IGpioPinConfiguration
	{
		/// <summary>
		/// Get/sets the pin used for the green LED.
		/// </summary>
		public int GreenLedPinNumber { get; set; }

		/// <summary>
		/// Get/sets the pin used for the red LED.
		/// </summary>
		public int RedLedPinNumber { get; set; }

		/// <summary>
		/// Get/sets the pin used for the blue LED.
		/// </summary>
		public int BluePinNumber { get; set; }

		/// <summary>
		/// Get/sets the pin used for the yellow LED.
		/// </summary>
		public int YellowPinNumber { get; set; }

		/// <summary>
		/// Get/sets the pin used for monitoring the push button.
		/// </summary>
		public int PushButtonPinNumber { get; set; }

		/// <summary>
		/// Get/sets the pin used for monitoring the alert
		/// pin on the MCP9808.
		/// </summary>
		public int AlertPinNumber { get; set; }
	}
}

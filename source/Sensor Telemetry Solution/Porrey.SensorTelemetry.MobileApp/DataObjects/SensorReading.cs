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
using Microsoft.Azure.Mobile.Server;

namespace Porrey.SensorTelemetry.MobileApp
{
	/// <summary>
	/// This is the Entity definition for the Azure SQL table
	/// that the stream analytics process will write to and the 
	/// Mobile service will read from.
	/// </summary>
	public class SensorReading : EntityData
	{
		/// <summary>
		/// Gets/sets the time that the reading was taken 
		/// from the sensor (device). NOTE: Stream Analytics
		/// does not work with DatTimeOffset (it will not give 
		/// any error but it will not write the data to the SQL
		/// database).
		/// </summary>
		public DateTime TimestampUtc { get; set; }

		/// <summary>
		/// Gets/sets the source. This value should always
		/// be 1 indicating the sensor reading came directly
		/// from the device.
		/// </summary>
		public int Source { get; set; }

		/// <summary>
		/// Gets/sets the temperature reading from the sensor.
		/// </summary>
        public float Temperature { get; set; }

		/// <summary>
		/// Indicates if the temperature was above the critical 
		/// threshold. A value of 1 is True and a value of 0 is 
		/// False.
		/// </summary>
		public int IsCritical { get; set; }

		/// <summary>
		/// Indicates if the temperature was above the upper 
		/// threshold. A value of 1 is True and a value of 0 is 
		/// False.
		/// </summary>
		public int IsAboveUpperThreshold { get; set; }

		/// <summary>
		/// Indicates if the temperature was below the lower 
		/// threshold. A value of 1 is True and a value of 0 is 
		/// False.
		/// </summary>
		public int IsBelowLowerThreshold { get; set; }
	}
}
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
using Newtonsoft.Json;
using Windows.Devices.Sensors.Interfaces;

namespace Windows.Devices.Sensors.Models
{
	/// <summary>
	/// This class is used to get data from mobile services
	/// </summary>
	public class SensorReading
	{
		public string Id { get; set; }

		/// <summary>
		/// Gets/sets the date and time the sensor reading was taken from the device
		/// in UTC time.
		/// </summary>
		public DateTimeOffset TimestampUtc { get; set; }

		public int Source { get; set; }
		public float Temperature { get; set; }
		public int IsCritical { get; set; }
		public int IsAboveUpperThreshold { get; set; }
		public int IsBelowLowerThreshold { get; set; }

		public DateTimeOffset? __CreatedAt { get; set; }
		public bool __Deleted { get; set; }
		public DateTimeOffset? __UpdatedAt { get; set; }
		public byte[] __Version { get; set; }

		public DateTimeOffset CreatedAtLocal => __CreatedAt.HasValue ? __CreatedAt.Value.ToLocalTime() : DateTimeOffset.MinValue;

		public DateTimeOffset TimestampLocal => this.TimestampUtc.ToLocalTime();

		[JsonIgnore]
		public int Delay
		{
			get
			{
				int value = (int)this.CreatedAtLocal.Subtract(this.TimestampLocal).TotalSeconds;
				return value;
			}
		}
	}
}

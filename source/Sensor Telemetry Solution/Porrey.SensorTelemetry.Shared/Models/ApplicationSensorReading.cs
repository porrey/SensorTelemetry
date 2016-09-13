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
using Porrey.SensorTelemetry.Shared.Interfaces;

namespace Porrey.SensorTelemetry.Shared.Models
{
	public class ApplicationSensorReading : IApplicationSensorReading
	{
		public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.Now.UtcDateTime;
		public ApplicationSensorReadingSource Source { get; set; } = ApplicationSensorReadingSource.None;
		public float Temperature { get; set; } = float.NaN;
		public bool IsCritical { get; set; } = false;
		public bool IsAboveUpperThreshold { get; set; } = false;
		public bool IsBelowLowerThreshold { get; set; } = false;
		public float CriticalThreshold { get; set; }
		public float UpperThreshold { get; set; }
		public float LowerThreshold { get; set; }

		public bool AlertActive => this.IsCritical ||
						this.IsBelowLowerThreshold ||
						this.IsAboveUpperThreshold;

		public DateTimeOffset TimestampLocal => this.TimestampUtc.LocalDateTime;

		public static ApplicationSensorReading FromIApplicationSensorReading(IApplicationSensorReading sensorReading) => new ApplicationSensorReading()
		{
			Temperature = sensorReading.Temperature,
			IsCritical = sensorReading.IsCritical,
			IsBelowLowerThreshold = sensorReading.IsBelowLowerThreshold,
			IsAboveUpperThreshold = sensorReading.IsAboveUpperThreshold,
			CriticalThreshold = sensorReading.CriticalThreshold,
			UpperThreshold = sensorReading.UpperThreshold,
			LowerThreshold = sensorReading.LowerThreshold,
			Source = sensorReading.Source,
			TimestampUtc = sensorReading.TimestampUtc
		};
	}
}

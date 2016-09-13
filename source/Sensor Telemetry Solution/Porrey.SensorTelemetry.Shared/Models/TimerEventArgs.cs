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

namespace Porrey.SensorTelemetry.Shared.Models
{
	/// <summary>
	/// Defines the structure for a timer event argument.
	/// </summary>
	public class TimerEventArgs : EventArgs
	{
		public TimerEventArgs(long eventCounter)
		{
			this.EventCounter = eventCounter;
		}

		/// <summary>
		/// Gets a counter that is incremented for every event. When the value
		/// reaches the maximum value for a long the value is reset to zero.
		/// </summary>
		public long EventCounter { get; }

		public bool IsMyInterval(TimeSpan interval) => this.EventCounter % (int)(interval.TotalMilliseconds / 500d) == 0;
	}
}

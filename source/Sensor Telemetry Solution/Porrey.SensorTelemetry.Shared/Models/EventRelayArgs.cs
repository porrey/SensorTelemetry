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
	/// This is the base object for event arguments.
	/// </summary>
	public class EventRelayArgs : EventArgs
	{
		public EventRelayArgs()
		{
			this.EventCreatedDateTimeUtc = DateTimeOffset.Now.UtcDateTime;
		}

		/// <summary>
		/// A unique key that identifies the sender of the event.
		/// </summary>
		public string SenderKey { get; set; }

		/// <summary>
		/// This value is incremented each time a message is
		/// relayed to help detect relay echoing. If this value 
		/// is more than 1 then the message is most likely
		/// echoing.
		/// </summary>
		public int RelayCount { get; set; }

		/// <summary>
		/// Gets/sets the date and time this event argument
		/// was created.
		/// </summary>
		public DateTimeOffset EventCreatedDateTimeUtc { get; set; }
	}
}
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
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Events;

namespace Porrey.SensorTelemetry.Shared.Events
{
	/// <summary>
	/// Defines events for the event aggregator.
	/// </summary>
	public class Events
	{
		/// <summary>
		/// This event can be used to push any type of internal message for debugging purposes.
		/// </summary>
		public class DebugEvent : PubSubEvent<DebugEventArgs> { }

		/// <summary>
		/// This event is fired when any application settings value has changed.
		/// </summary>
		public class ApplicationSettingChangedEvent : PubSubEvent<ApplicationSettingChangedEventArgs> { }

		/// <summary>
		/// This event is fired when the temperature reading changes on the device.
		/// </summary>
		public class TemperatureChangedEvent : PubSubEvent<TemperatureChangedEventArgs> { }

		/// <summary>
		/// This event is fired when a telemetry send is initiated or completed.
		/// </summary>
		public class TelemetryStatusChangedEvent : PubSubEvent<TelemetryStatusChangedEventArgs> { }

		/// <summary>
		/// This event represents a device command and is used to send commands from any
		/// application instance to the device.
		/// </summary>
		public class DeviceCommandEvent : PubSubEvent<DeviceCommandEventArgs> { }

		/// <summary>
		/// This event is fired periodically by a timer service and can be monitored
		/// if something needs to be periodically updated or refreshed.
		/// </summary>
		public class TimerEvent : PubSubEvent<TimerEventArgs> { }
	}
}
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
namespace Porrey.SensorTelemetry.Shared.Models
{
	public enum DeviceCommand
	{
		UpdateTemperature,
		RunLedTest,
		ResetAlert
	}

	public class DeviceCommandEventArgs : EventRelayArgs
	{
		/// <summary>
		/// Creates a default instance of DeviceCommandEventArgs.
		/// </summary>
		public DeviceCommandEventArgs()
		{
		}

		/// <summary>
		/// Creates an instance of DeviceCommandEventArgs fo the given type.
		/// </summary>
		/// <param name="deviceCommand"></param>
		public DeviceCommandEventArgs(DeviceCommand deviceCommand)
		{
			this.DeviceCommand = deviceCommand;
			this.Parameters = new object[0];
		}

		/// <summary>
		/// Creates a new instance of DeviceCommandEventArgs of the
		/// given type with the given parameters.
		/// </summary>
		/// <param name="deviceCommand"></param>
		/// <param name="parameters"></param>
		public DeviceCommandEventArgs(DeviceCommand deviceCommand, params object[] parameters)
		{
			this.DeviceCommand = deviceCommand;
			this.Parameters = parameters;
		}

		/// <summary>
		/// 
		/// </summary>
		public DeviceCommand DeviceCommand { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public object[] Parameters { get; set; }
	}
}

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

namespace Windows.Devices.Sensors
{
	/// <summary>
	/// Abstract definition for a MCP9808 exception.
	/// </summary>
	public abstract class Mcp9808Exception : Exception
	{
		/// <summary>
		/// Creates a default instance of Mcp9808Exception
		/// </summary>
		public Mcp9808Exception()
		{
		}

		/// <summary>
		/// Creates an instance of MCP9808 with the given message.
		/// </summary>
		/// <param name="message">The message of the exception.</param>
		public Mcp9808Exception(string message) 
			: base(message)
		{
		}

		/// <summary>
		/// Creates an instance of MCP9808 with the given message
		/// and internal exception.
		/// </summary>
		/// <param name="message">The message of the exception.</param>
		/// <param name="innerException">An inner exception.</param>
		public Mcp9808Exception(string message, Exception innerException) 
			: base(message, innerException)
		{
		}
	}
}

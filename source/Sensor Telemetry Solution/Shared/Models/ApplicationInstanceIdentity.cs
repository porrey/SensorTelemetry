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

namespace Porrey.SensorTelemetry.Interfaces
{
	/// <summary>
	/// Defines an interface to uniquely identify an instance of an application.
	/// </summary>
	public class ApplicationInstanceIdentity : IApplicationInstanceIdentity
	{
		/// <summary>
		/// Creates an instance default of ApplicationInstanceIdentity.
		/// </summary>
		public ApplicationInstanceIdentity()
		{
			this.Key = string.Format("{0}{1}", Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
							 .Replace("-", "");
		}

		/// <summary>
		/// A unique key value that differs from any other instance
		/// running regardless of the computer it is on.
		/// </summary>
		public string Key { get; set; }
	}
}

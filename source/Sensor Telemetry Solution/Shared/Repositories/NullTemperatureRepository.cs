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
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Interfaces;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Events;
using Windows.Devices.Sensors.Interfaces;

namespace Porrey.SensorTelemetry.Repositories
{
	/// <summary>
	/// Follows the Null Pattern. This implements the ITemperatureRepository
	/// when no MCP9808 device is detected.
	/// </summary>
	public class NullTemperatureRepository : ITemperatureRepository
	{
		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		public Task<bool> Connect() => Task<bool>.FromResult(true);

		public Task Start() => Task.FromResult(0);

		public Task Stop() => Task.FromResult(0);

		public Task<IApplicationSensorReading> GetSensorReading() => Task<IApplicationSensorReading>.FromResult((IApplicationSensorReading)new ApplicationSensorReading());

		public bool AlertIsActive => false;

		public IMcp9808 Device => null;
	}
}

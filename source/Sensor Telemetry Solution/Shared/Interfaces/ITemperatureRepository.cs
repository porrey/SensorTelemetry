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
using Porrey.SensorTelemetry.Shared.Interfaces;
using Windows.Devices.Sensors.Interfaces;

namespace Porrey.SensorTelemetry.Interfaces
{
	public interface ITemperatureRepository
	{
		Task<bool> Connect();
		Task Start();
		Task Stop();
		Task<IApplicationSensorReading> GetSensorReading();
		bool AlertIsActive { get; }
		IMcp9808 Device { get; }
    }
}

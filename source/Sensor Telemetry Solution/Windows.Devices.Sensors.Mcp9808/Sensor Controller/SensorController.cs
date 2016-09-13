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
using Windows.Devices.I2c;
using Windows.Devices.Sensors.Interfaces;

namespace Windows.Devices.Sensors
{
	/// <summary>
	/// Creates a device of the given type as defined by T.
	/// </summary>
	public static class SensorController
	{
		/// <summary>
		/// Gets the device from the I2C bus at the specified address.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="deviceAddress">The address of the device.</param>
		/// <param name="busSpeed">The desired bus speed.</param>
		/// <returns>Returns a device that implements II2CSensor.</returns>
		public static Task<T> GetI2CDevice<T>(byte deviceAddress, I2cBusSpeed busSpeed) where T : II2CSensor
		{
			T returnValue = default(T);

			if (typeof(T) == typeof(IMcp9808))
			{
				IMcp9808 device = new Mcp9808(deviceAddress, busSpeed);
                returnValue = (T)device;
			}

			return Task<T>.FromResult(returnValue);
		}
	}
}

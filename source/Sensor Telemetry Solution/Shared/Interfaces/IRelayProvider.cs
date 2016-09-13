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
using System.Threading.Tasks;

namespace Porrey.SensorTelemetry.Interfaces
{
	/// <summary>
	/// Defines the callback delegate for the relay provider.
	/// </summary>
	/// <typeparam name="T">Represents the type of the message to be sent or received. The
	/// relay provider will handle the details of encoding the message during transport.</typeparam>
	/// <param name="message">The message object that is transported through the IRelayProvider.</param>
	/// <returns>True if successful; false otherwise.</returns>
	public delegate Task<bool> IRelayProviderCallbackDelegate<T>(T message);

	public interface IRelayProviderSender<T> : IDisposable
	{
		/// <summary>
		/// Initializes the relay provider.
		/// </summary>
		Task Initialize();

		/// <summary>
		/// Sends a message externally via the IRelayProvider.
		/// </summary>
		/// <param name="eventName">The name or ID of the event.</returns>
		/// <param name="message">The message object that is transported through the IRelayProvider.</param>
		/// <returns>True if successful; false otherwise.</returns>
		Task<bool> Send(string eventName, T message);
	}

	public interface IRelayProviderReceiver<T> : IDisposable
	{
		/// <summary>
		/// Initializes the relay provider.
		/// </summary>
		Task Initialize();

		/// <summary>
		/// Sets a callback to receive incoming messages from
		/// the IRelayProvider.
		/// </summary>
		/// <param name="eventName">The name or ID of the event.</returns>
		/// <param name="callback">A IRelayProviderCallbackDelegate delegate 
		/// method to receive the message.</param>
		void SetCallback(string eventName, IRelayProviderCallbackDelegate<T> callback);
	}
}

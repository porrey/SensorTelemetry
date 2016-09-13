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
using System.Diagnostics;
using System.Threading.Tasks;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Events;

namespace Porrey.SensorTelemetry.Common
{
	/// <summary>
	/// Defines a method for transforming an
	/// </summary>
	/// <typeparam name="T">The type of an object that inherits from EventRelayArgs.</typeparam>
	/// <param name="e">An instance of the event arguments of type T.</param>
	/// <returns></returns>
	public delegate T TransformArgumentDelegate<T>(T e) where T : EventRelayArgs;

	/// <summary>
	/// Defines a method for determining if an event with event arguments of type T
	/// should be routed.
	/// </summary>
	/// <typeparam name="T">The type of an object that inherits from EventRelayArgs.</typeparam>
	/// <param name="e">An instance of the event arguments of type T.</param>
	/// <returns></returns>
	public delegate bool ShouldRouteEventDelegate<T>(T e) where T : EventRelayArgs;

	/// <summary>
	/// Creates a map between a PubSub event and a IRelayprovider allowing
	/// event messages to flow back and forth between these two services.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EventRelayMap<T> : IDisposable where T : EventRelayArgs
	{
		private SubscriptionToken _subscriptionToken = null;

		/// <summary>
		/// Creates an instance of EventMap.
		/// </summary>
		/// <param name="applicationInstanceIdentity">An instance of IApplicationInstanceIdentity
		/// uniquely identifying the current application instance.</param>
		/// <param name="relayProvider">The underlying transport for the incoming and
		/// outgoing messages. This could be SignalR, and IoT Hub or some type of notification framework.</param>
		/// <param name="eventTarget">A PubSubEvent instance representing the event to be mapped.</param>
		/// <param name="inboundTransformAction">A TransformArgumentDelegate method used to transform the 
		/// event arguments before being routed inbound.</param>
		/// <param name="shouldRouteInbound">A ShouldRouteEventDelegate method that determines if the event
		/// should be routed. Note this method is called prior to the transformation.</param>
		/// <param name="outboundTransformAction">A TransformArgumentDelegate method used to transform the 
		/// event arguments before being routed outbound.</param>
		/// <param name="shouldRouteOutbound">A ShouldRouteEventDelegate method that determines if the event
		/// should be routed. Note this method is called prior to the transformation.</param>
		public EventRelayMap(IApplicationInstanceIdentity applicationInstanceIdentity, 
						IRelayProviderSender<T> relayProviderSender,
						IRelayProviderReceiver<T> relayProviderReceiver,
						PubSubEvent<T> eventTarget,
						TransformArgumentDelegate<T> inboundTransformAction, ShouldRouteEventDelegate<T> shouldRouteInbound,
						TransformArgumentDelegate<T> outboundTransformAction, ShouldRouteEventDelegate<T> shouldRouteOutbound)
		{
			this.ApplicationInstanceIdentity = applicationInstanceIdentity;
			this.Event = eventTarget;
			this.InboundTransformAction = inboundTransformAction;
			this.ShouldRouteInbound = shouldRouteInbound;
			this.OutboundTransformAction = outboundTransformAction;
			this.ShouldRouteOutbound = shouldRouteOutbound;
			this.RelayProviderSender = relayProviderSender;
			this.RelayProviderReceiver = relayProviderReceiver;

			// ***
			// *** Links an inbound event from the IRelayProvider
			// *** to the external Publish/Subscribe event system.
			// ***
			string eventName = string.Format("On{0}", this.Event.GetType().Name);
			this.RelayProviderReceiver?.SetCallback(eventName, this.OnProcessInboundMessage);

			// ***
			// *** Setup to receive inbound messages and processes them.
			// ***
			_subscriptionToken = this.Event.Subscribe((e) => { this.OnProcessOutboundMessage(e); });
		}

		/// <summary>
		/// Gets/sets an instance of IApplicationInstanceIdentity
		/// uniquely identifying the current application instance.
		/// </summary>
		public IApplicationInstanceIdentity ApplicationInstanceIdentity { get; set; }

		/// <summary>
		/// Gets/sets a PubSubEvent instance representing the event to be mapped.
		/// </summary>
		public PubSubEvent<T> Event { get; set; }

		/// <summary>
		/// Gets/sets the TransformArgumentDelegate method used to transform the 
		/// event arguments before being routed inbound.
		/// </summary>
		public TransformArgumentDelegate<T> InboundTransformAction { get; set; }

		/// <summary>
		/// Gets/sets the ShouldRouteEventDelegate method that determines if the event
		/// should be routed. Note this method is called prior to the transformation.
		/// </summary>
		public ShouldRouteEventDelegate<T> ShouldRouteInbound { get; set; }

		/// <summary>
		/// Gets/sets the TransformArgumentDelegate method used to transform the 
		/// event arguments before being routed outbound.
		/// </summary>
		public TransformArgumentDelegate<T> OutboundTransformAction { get; set; }

		/// <summary>
		/// Gets/sets the ShouldRouteEventDelegate method that determines if the event
		/// should be routed. Note this method is called prior to the transformation.
		/// </summary>
		public ShouldRouteEventDelegate<T> ShouldRouteOutbound { get; set; }

		/// <summary>
		/// The underlying transport for the outgoing messages. This
		/// could be SignalR, and IoT Hub or some type of notification framework.
		/// </summary>
		public IRelayProviderSender<T> RelayProviderSender { get; set; }

		/// <summary>
		/// The underlying transport for the outgoing messages. This
		/// could be SignalR, and IoT Hub or some type of notification framework.
		/// </summary>
		public IRelayProviderReceiver<T> RelayProviderReceiver { get; set; }

		/// <summary>
		/// Processes an inbound message.
		/// </summary>
		/// <param name="e">The data received from the event.</param>
		protected Task<bool> OnProcessInboundMessage(T e)
		{
			bool returnValue = false;
			Debug.WriteLine("Received event of type '{0}'.", typeof(T).Name);

			try
			{
				// ***
				// *** An inbound message will have a relay count of 0
				// *** 
				if (e.RelayCount == 1)
				{
					// ***
					// *** Only forward events that came from another
					// *** application instance.
					// ***
					if (e.SenderKey != this.ApplicationInstanceIdentity.Key)
					{
						if (this.ShouldRouteInbound(e))
						{
							T newE = this.InboundTransformAction(e);

							// ***
							// *** Publish internally on the Event Aggregator
							// ***
							this.Event.Publish(newE);
						}
					}
				}

				returnValue = true;
			}
			catch
			{
				// ***
				// *** Discard this event
				// ***
				returnValue = false;
			}

			return Task.FromResult(returnValue);
		}

		/// <summary>
		/// Processes an outbound event.
		/// </summary>
		/// <param name="e">The data received from the event.</param>
		protected void OnProcessOutboundMessage(T e)
		{
			try
			{
				// ***
				// *** Any message originating from this application will
				// *** have an empty SenderKey and a relay count of 0
				// ***
				if (string.IsNullOrWhiteSpace(e.SenderKey) && e.RelayCount == 0)
				{
					if (this.ShouldRouteOutbound(e))
					{
						// ***
						// *** Transform the item
						// ***
						T newE = this.OutboundTransformAction(e);

						// ***
						// *** Create the message object to
						// *** wrap the actual item being sent.
						// ***
						newE.SenderKey = this.ApplicationInstanceIdentity.Key;
						newE.RelayCount++;

						// ***
						// *** Send to the SignalR hub
						// ***
						Debug.WriteLine("Sending event of type '{0}'.", typeof(T).Name);
						string eventName = string.Format("Send{0}", this.Event.GetType().Name);
						this.RelayProviderSender?.Send(eventName, newE);
					}
				}
			}
			catch
			{
				// ***
				// *** Discard this event
				// ***
			}
		}

		/// <summary>
		/// CLeans up the internal objects.
		/// </summary>
		public void Dispose()
		{
			// ***
			// *** Unsubscribe from the event.
			// ***
			if (_subscriptionToken != null)
			{
				this.Event.Unsubscribe(this._subscriptionToken);
				_subscriptionToken.Dispose();
				_subscriptionToken = null;
			}

			// ***
			// *** Release the RelayProviderSender object.
			// ***
			if (this.RelayProviderSender != null)
			{
				this.RelayProviderSender.Dispose();
				this.RelayProviderSender = null;
			}

			// ***
			// *** Release the RelayProviderReceiver object.
			// ***
			if (this.RelayProviderReceiver != null)
			{
				this.RelayProviderReceiver.Dispose();
				this.RelayProviderReceiver = null;
			}
		}
	}
}

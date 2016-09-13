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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Events;
using Prism.Mvvm;

namespace Porrey.SensorTelemetry.Services
{
	public class DebugConsoleService : BindableBase, IBackgroundService, IDebugConsoleRepository
	{
		private SubscriptionToken _exceptionEventToken = null;
		private readonly ObservableCollection<DebugEventArgs> _items = new ObservableCollection<DebugEventArgs>();

		public string Name => "Debug Console";

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		public Task<bool> Start()
		{
			bool returnValue = false;

			try
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Subscribe(async (args) =>
				{
					await this.OnDebugEvent(args);
				}, ThreadOption.UIThread);

				returnValue = true;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
				returnValue = false;
			}

			return Task<bool>.FromResult(returnValue);
		}

		public Task<bool> Stop()
		{
			bool returnValue = false;

			try
			{
				// ***
				// *** Unsubscribe from the event
				// ***
				if (_exceptionEventToken != null)
				{
					this.EventAggregator.GetEvent<Events.DebugEvent>().Unsubscribe(_exceptionEventToken);
					_exceptionEventToken.Dispose();
					_exceptionEventToken = null;
				}

				// ***
				// *** Release the collection items
				// ***
				this.Items.Clear();

				returnValue = true;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
				returnValue = false;
			}

			return Task<bool>.FromResult(returnValue);
		}

		public ObservableCollection<DebugEventArgs> Items => _items;

		protected Task OnDebugEvent(DebugEventArgs e)
		{
			try
			{
				lock (this.Items)
				{
					this.Items.Add(e);
					this.OnPropertyChanged("Items");
				}
			}
			catch
			{
				// ****
				// *** Discard
				// ***
			}

			return Task.FromResult(0);
		}

		public Task Refresh()
		{
			try
			{
				this.OnPropertyChanged("Items");
			}
			catch
			{
				// ****
				// *** Discard
				// ***
			}

			return Task.FromResult(0);
		}

		public Task Clear()
		{
			try
			{
				lock (this.Items)
				{
					this.Items.Clear();
					this.OnPropertyChanged("Items");
				}
			}
			catch
			{
				// ****
				// *** Discard
				// ***
			}

			return Task.FromResult(0);
		}
	}
}

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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.WindowsAzure.MobileServices;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Commands;
using Prism.Windows.Navigation;
using Windows.Devices.Sensors.Models;

namespace Porrey.SensorTelemetry.ViewModels
{
	public class HistoryPageViewModel : SensorTelemetryViewModel
	{
		private MobileServiceIncrementalLoadingCollection<SensorReading, SensorReading> _items = null;
		private MobileServiceClient _mobileService = null;
		private IMobileServiceTable<SensorReading> _table = null;

		public HistoryPageViewModel()
		{
			// ***
			// *** Set up the commands
			// ***
			this.RefreshCommand = DelegateCommand.FromAsyncHandler(this.OnRefresh, this.OnCanRefresh);
		}

		protected override string OnGetPageName() => "Telemetry History";

		[Dependency]
		protected IMobileServicesConfiguration MobileServicesConfiguration { get; set; }

		public async override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
		{
			try
			{
				base.OnNavigatedTo(e, viewModelState);

				// ***
				// *** Connect to mobile services
				// ***
				_mobileService = new MobileServiceClient(this.MobileServicesConfiguration.Url);
				_table = _mobileService.GetTable<SensorReading>();

				// ***
				// *** Load the data
				// ***
				await this.LoadData();
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
		{
			try
			{
				// ***
				// *** Disconnect the collections events
				// ***
				if (this.Items != null)
				{
					this.Items.LoadingComplete -= Items_LoadingComplete;
					this.Items.LoadingItems -= Items_LoadingItems;
				}

				// ***
				// *** Dispose the mobile services
				// ***
				_mobileService.Dispose();
				_mobileService = null;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
			finally
			{
				base.OnNavigatingFrom(e, viewModelState, suspending);
			}
		}

		public MobileServiceIncrementalLoadingCollection<SensorReading, SensorReading> Items
		{
			get
			{
				return _items;
			}
			set
			{
				this.SetProperty(ref _items, value);
			}
		}

		private bool _isActive = false;
		public bool IsActive
		{
			get
			{
				return _isActive;
			}
			set
			{
				this.SetProperty(ref _isActive, value);
			}
		}

		private Task LoadData()
		{
			try
			{
				// ***
				// *** Disconnect the collections events
				// ***
				if (this.Items != null)
				{
					this.Items.LoadingComplete -= Items_LoadingComplete;
					this.Items.LoadingItems -= Items_LoadingItems;
				}

				// ***
				// *** Load the items
				// ***
				this.Items = _table.IncludeTotalCount().OrderByDescending(t => t.TimestampUtc).ToIncrementalLoadingCollection();

				// ***
				// *** Connect up to the collection loading to enable and disable the marquee
				// ***
				this.Items.LoadingComplete += Items_LoadingComplete;
				this.Items.LoadingItems += Items_LoadingItems;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return Task.FromResult(0);
		}

		private void Items_LoadingItems(object sender, EventArgs e)
		{
			this.IsActive = true;
		}

		private void Items_LoadingComplete(object sender, LoadingCompleteEventArgs e)
		{
			this.IsActive = false;
		}

		#region Commands
		public DelegateCommand RefreshCommand { get; set; }

		protected bool OnCanRefresh() => true;

		protected async Task OnRefresh()
		{
			await this.LoadData();
		}
		#endregion
	}
}

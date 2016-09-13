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
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Commands;
using Prism.Windows.Navigation;

namespace Porrey.SensorTelemetry.ViewModels
{
	public class DebugConsolePageViewModel : SensorTelemetryViewModel
	{
		protected override string OnGetPageName() => "Debug Console";

		public DebugConsolePageViewModel()
		{
			// ***
			// *** Set up the commands
			// ***
			this.ClearCommand = DelegateCommand.FromAsyncHandler(this.OnClear, this.OnCanClear);
			this.RefreshCommand = DelegateCommand.FromAsyncHandler(this.OnRefresh, this.OnCanRefresh);
			this.SendTestCommand = DelegateCommand.FromAsyncHandler(this.OnSendTest, this.OnCanSendTest);
		}

		[Dependency]
		public IDebugConsoleRepository DebugConsoleProvider { get; set; }

		public async override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
		{
			try
			{
				base.OnNavigatedTo(e, viewModelState);

				// ***
				// *** Watch for collection changes
				// ***
				this.DebugConsoleProvider.Items.CollectionChanged += Items_CollectionChanged;

				// ***
				// *** Load the data
				// ***
				await this.RefreshView();
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		private async void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			await this.RefreshView();
		}

		public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
		{

			try
			{
				// ***
				// *** Unhook the collection event handler
				// ***
				this.DebugConsoleProvider.Items.CollectionChanged -= Items_CollectionChanged;
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

		private async Task RefreshView()
		{
			try
			{
				this.OnPropertyChanged("DebugConsoleProvider");
				await this.DebugConsoleProvider.Refresh();
				this.ClearCommand.RaiseCanExecuteChanged();
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
		}

		#region Commands
		public DelegateCommand RefreshCommand { get; set; }

		protected bool OnCanRefresh() => true;

		protected async Task OnRefresh()
		{
			await this.RefreshView();			
		}

		public DelegateCommand ClearCommand { get; set; }

		protected bool OnCanClear() => this.DebugConsoleProvider.Items.Count() > 0;

		protected async Task OnClear()
		{
			await this.DebugConsoleProvider.Clear();
			this.ClearCommand.RaiseCanExecuteChanged();
		}

		public DelegateCommand SendTestCommand { get; set; }

		protected bool OnCanSendTest() => true;

		protected Task OnSendTest()
		{
			// ***
			// *** Log this action
			// ***
			this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(DebugEventType.Information, "Test Message", "This is a test message sent via the event aggregator and collected by the Debug Console Service."));
			this.ClearCommand.RaiseCanExecuteChanged();
			return Task.FromResult(0);
        }		
		#endregion
	}
}

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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Porrey.SensorTelemetry.ViewModels
{
	public abstract class SensorTelemetryViewModel : ViewModelBase
	{
		private SubscriptionToken _timerEventSubscriptionToken = null;

		public SensorTelemetryViewModel()
		{
			// ***
			// *** Set up the commands
			// ***
			this.GoHomeCommand = DelegateCommand.FromAsyncHandler(this.OnGoHome, this.OnCanGoHome);
			this.DebugConsoleCommand = DelegateCommand.FromAsyncHandler(this.OnDebugConsole, this.OnCanDebugConsole);
			this.ViewSettingsCommand = DelegateCommand.FromAsyncHandler(this.OnViewSettings, this.OnCanViewSettings);
			this.RunLedTestCommand= DelegateCommand.FromAsyncHandler(this.OnRunLedTest, this.OnCanRunLedTest);
			this.ExitCommand = DelegateCommand.FromAsyncHandler(this.OnExit, this.OnCanExit);

			this.PageName = this.OnGetPageName();
		}

		protected CoreDispatcher Dispatcher { get; set; }

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		[Dependency]
		protected IApplicationSettingsRepository ApplicationSettingsRepository { get; set; }

		[Dependency]
		protected INavigationService NavigationService { get; set; }

		[Dependency]
		protected ITemperatureRepository TemperatureRepository { get; set; }

		public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
		{
			// ***
			// *** Get the current dispatcher and cache it for the view model
			// ***
			this.Dispatcher = Window.Current.Dispatcher;

			// ***
			// *** Subscribe to timer events
			// ***
			_timerEventSubscriptionToken = this.EventAggregator.GetEvent<Events.TimerEvent>().Subscribe((args) =>
			{
				this.OnPropertyChanged(nameof(CurrentTime));
            }, ThreadOption.UIThread);

			base.OnNavigatedTo(e, viewModelState);
		}

		public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
		{
			// ***
			// *** Unsubscribe from the timer event
			// ***
			if (_timerEventSubscriptionToken != null)
			{
				this.EventAggregator.GetEvent<Events.TimerEvent>().Unsubscribe(_timerEventSubscriptionToken);
                _timerEventSubscriptionToken.Dispose();
				_timerEventSubscriptionToken = null;
            }

			// ***
			// *** Call the base OnNavigatedFrom
			// ***
			base.OnNavigatingFrom(e, viewModelState, suspending);

			// ***
			// *** Release the cached dispatcher instance
			// ***
			this.Dispatcher = null;
		}

		protected delegate bool OnCanCommandWarpperDelegate();
		protected bool OnCanCommandWrapper(OnCanCommandWarpperDelegate action, [CallerMemberName]string callerName = null)
		{
			bool returnValue = false;

			try
			{
				returnValue = action.Invoke();
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex, callerName));
			}

			return returnValue;
		}

		private string _pageName = string.Empty;
		public string PageName
		{
			get
			{
				return _pageName;
			}
			set
			{
				this.SetProperty(ref _pageName, value);
			}
		}

		protected virtual string OnGetPageName() => "No Page Name Set";

		protected virtual bool OnIsHome() => false;

		/// <summary>
		/// Gets the current time (and date) for the view
		/// </summary>
		public DateTimeOffset CurrentTime => DateTimeOffset.Now;

		#region Command
		public DelegateCommand GoHomeCommand { get; set; }

		protected bool OnCanGoHome() => !this.OnIsHome();

		protected async Task OnGoHome()
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				if (!this.NavigationService.Navigate(MagicValue.Views.MainPage, null))
				{
					MessageDialog md = new MessageDialog("Could not navigate to history page.", "Navigation Failed");
					await md.ShowAsync();
				}
			});
		}

		public DelegateCommand DebugConsoleCommand { get; set; }

		protected bool OnCanDebugConsole() => true;

		protected async Task OnDebugConsole()
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				if (!this.NavigationService.Navigate(MagicValue.Views.DebugConsole, null))
				{
					MessageDialog md = new MessageDialog("Could not navigate to page.", "Navigation Failed");
					await md.ShowAsync();
				}
			});
		}

		public DelegateCommand ViewSettingsCommand { get; set; }

		protected bool OnCanViewSettings() => this.TemperatureRepository.Device != null;

		protected async Task OnViewSettings()
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				if (!this.NavigationService.Navigate(MagicValue.Views.SettingsPage, null))
				{
					MessageDialog md = new MessageDialog("Could not navigate to page.", "Navigation Failed");
					await md.ShowAsync();
				}
			});
		}

		public DelegateCommand RunLedTestCommand { get; set; }

		protected bool OnCanRunLedTest() => true;

		protected Task OnRunLedTest()
		{
			this.EventAggregator.GetEvent<Events.DeviceCommandEvent>().Publish(new DeviceCommandEventArgs(DeviceCommand.RunLedTest));
			return Task.FromResult(0);
		}

		public DelegateCommand ExitCommand { get; set; }

		protected bool OnCanExit() => true;

		protected Task OnExit()
		{
			App.Current.Exit();
			return Task.FromResult(0);
		}
		#endregion
	}
}

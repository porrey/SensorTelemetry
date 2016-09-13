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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Porrey.SensorTelemetry.Views
{
	public sealed partial class MainPage : SensorTelemetryPage
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			menuSplitView.IsPaneOpen = false;
			base.OnNavigatedTo(e);
			
		}

		public override bool CanGoBack => false;

		private void MenuButton_Click(object sender, RoutedEventArgs e)
		{
			menuSplitView.IsPaneOpen = !menuSplitView.IsPaneOpen;
		}

		private async void AboutButton_Click(object sender, RoutedEventArgs e)
		{
			menuSplitView.IsPaneOpen = false;
			AboutDialog dialog = new AboutDialog();
			await dialog.ShowAsync();
		}

		private async void PrivacyButton_Click(object sender, RoutedEventArgs e)
		{
			menuSplitView.IsPaneOpen = false;
			PrivacyDialog dialog = new PrivacyDialog();
			await dialog.ShowAsync();
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			menuSplitView.IsPaneOpen = false;
		}
	}
}

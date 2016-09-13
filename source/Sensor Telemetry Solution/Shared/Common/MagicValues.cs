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
namespace Porrey.SensorTelemetry.Common
{
	public static class MagicValue
	{
		public static class Views
		{
			public const string StartPage = "Start";
			public const string MainPage = "Main";
			public const string SettingsPage = "Settings";
			public const string HistoryPage = "History";
			public const string DebugConsole = "DebugConsole";
		}

		public static class Property
		{
			public const string TemperatureUnit = "TemperatureUnit";
			public const string CriticalTemperatureThreshold = "CriticalTemperatureThreshold";
			public const string UpperTemperatureThreshold = "UpperTemperatureThreshold";
			public const string LowerTemperatureThreshold = "LowerTemperatureThreshold";
			public const string AutoAlertResetMode = "AutoAlertResetMode";
        }

		public static class TemperatureUnit
		{
			public const string Celcius = "C";
			public const string Fahrenheit = "F";
		}

		public static class Defaults
		{
			public const string TemperatureUnit = "C";
			public const float CriticalTemperatureThreshold = 32f;
			public const float UpperTemperatureThreshold = 28f;
			public const float LowerTemperatureThreshold = 24f;
			public const bool AutoAlertResetMode = true;
        }

		public static class Limits
		{
			public const float SliderMinimumValue = -20;
			public const float SliderMaximumValue = 100;
		}

		public static class BackgroundService
		{
			public const string Debug = "Debug";
			public const string Led = "LED";
			public const string AlertPinMonitor = "AlertPinMonitor";
			public const string PushButtonMonitor = "PushButtonMonitor";
			public const string Telemetry = "Telemetry";
			public const string Relay = "Relay";
			public const string Timer = "Timer";
		}
	}
}
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
namespace System.Threading.Tasks
{
	public static class TaskExtensions
	{
		/// <summary>
		/// Waits for a result on a Task and returns the
		/// result as type T. This call causes a thread to
		/// block so be careful calling this from a UI thread.
		/// </summary>
		/// <typeparam name="T">The return type of the result.</typeparam>
		/// <param name="task">The task to execute.</param>
		/// <returns>The result of the Task.</returns>
		public static T WaitForResult<T>(this Task<T> task)
		{
			T returnValue = default(T);

			try
			{
				task.Wait();
				returnValue = task.Result;
			}
			catch (AggregateException agex)
			{
				if (agex.InnerException != null)
				{
					throw agex.InnerException;
				}
			}

			return returnValue;
		}
	}
}

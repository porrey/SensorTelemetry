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
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;

namespace Porrey.SensorTelemetry.MobileApp
{
	public class SensorReadingController : TableController<SensorReading>
	{
		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			SensorReadingContext context = new SensorReadingContext();
			DomainManager = new EntityDomainManager<SensorReading>(context, Request);
		}

		/// <summary>
		/// Returns all items as an IQueryable. The URL is in the form
		/// GET tables/SensorReading
		/// </summary>
		/// <returns>Returns an IQueryable of SensorReading.</returns>
		public IQueryable<SensorReading> GetAllSensorReadings() => this.Query();

		/// <summary>
		/// Gets a single item. The URL is in the form 
		/// GET tables/SensorReading/48D68C86-6EA6-4C25-AA33-223FC9A27959
		/// </summary>
		/// <param name="id">The ID of the item.</param>
		/// <returns>Returns a single SensorReading.</returns> 
		public SingleResult<SensorReading> GetSensorReading(string id) => this.Lookup(id);

		/// <summary>
		/// Updates a single item. The URL is in the form 
		/// PATCH tables/SensorReading/48D68C86-6EA6-4C25-AA33-223FC9A27959
		/// </summary>
		/// <param name="id">The ID of the item.</param>
		/// <param name="patch">A SensorReading item containing the updates.</param>
		/// <returns>Returns the updated item.</returns>
		[HttpPost]
		public Task<SensorReading> PatchSensorReading(string id, Delta<SensorReading> patch) => UpdateAsync(id, patch);

		/// <summary>
		/// Creates a new item. The URL is in the form
		/// POST tables/SensorReading
		/// </summary>
		/// <param name="item">A SensorReading to be created.</param>
		/// <returns>Returns the result of the action.</returns>
		[HttpPost]
		public async Task<IHttpActionResult> PostSensorReading(SensorReading item)
		{
			SensorReading current = await InsertAsync(item);
			return CreatedAtRoute("Tables", new { id = current.Id }, current);
		}

		/// <summary>
		/// Deletes a single item. The URL is in the form
		/// DELETE tables/SensorReading/48D68C86-6EA6-4C25-AA33-223FC9A27959
		/// </summary>
		/// <param name="id">The ID of the item.</param>
		public Task DeleteSensorReading(string id) => DeleteAsync(id);
	}
}
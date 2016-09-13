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
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Porrey.SensorTelemetry.MobileApp
{
	public class SensorReadingContext : DbContext
	{
		// You can add custom code to this file. Changes will not be overwritten.
		// 
		// If you want Entity Framework to alter your database
		// automatically whenever you change your model schema, please use data migrations.
		// For more information refer to the documentation:
		// http://msdn.microsoft.com/en-us/data/jj591621.aspx
		//
		// To enable Entity Framework migrations in the cloud, please ensure that the 
		// service name, set by the 'MS_MobileServiceName' AppSettings in the local 
		// Web.config, is the same as the service name when hosted in Azure.

		private const string connectionStringName = "Name=MS_TableConnectionString";

		public SensorReadingContext() : base(connectionStringName)
		{
		}

		public DbSet<SensorReading> SensorReading { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Add(
				new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
					"ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
		}
	}
}

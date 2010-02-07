//-----------------------------------------------------------------------
// <copyright file="JobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Tasty.Configuration;

    /// <summary>
    /// Provides conviencence methods and a way to access the <see cref="IJobStore"/> 
    /// currently in use.
    /// </summary>
    public static class JobStore
    {
        private static readonly object locker = new object();
        private static IJobStore current;

        /// <summary>
        /// Gets or sets the current <see cref="IJobStore"/> implementation in use.
        /// The setter on this property is primarily meant for testing purposes.
        /// </summary>
        /// <remarks>
        /// It is not recommended to set this property during runtime. You should instead
        /// set it during static initialization if you would rather not infer it from
        /// the configuration. Setting it later could cause persistence errors if any
        /// currently-executing jobs try to persist their update data to the new store.
        /// </remarks>
        public static IJobStore Current
        {
            get
            {
                lock (locker)
                {
                    if (current == null)
                    {
                        current = (IJobStore)Activator.CreateInstance(Type.GetType(TastySettings.Section.Jobs.Store.JobStoreType));
                    }

                    return current;
                }
            }

            set
            {
                lock (locker)
                {
                    current = value;
                }
            }
        }

        /// <summary>
        /// Gets the given result set as a collection of <see cref="JobRecord"/>s.
        /// Assumes the result set has the expected schema definition.
        /// </summary>
        /// <param name="resultSet">The result set to convert into a collection of <see cref="JobRecord"/>s.</param>
        /// <returns>A collection of <see cref="JobRecord"/>s.</returns>
        public static IEnumerable<JobRecord> CreateRecordCollection(DataTable resultSet)
        {
            return from DataRow row in resultSet.Rows
                   select new JobRecord()
                   {
                       Id = (int)row["Id"],
                       Name = (string)row["Name"],
                       JobType = Type.GetType((string)row["Type"]),
                       Data = (string)row["Data"],
                       Status = (JobStatus)Enum.Parse(typeof(JobStatus), (string)row["Status"]),
                       Exception = (row["Exception"] != DBNull.Value) ? (string)row["Exception"] : null,
                       QueueDate = new DateTime(((DateTime)row["QueueDate"]).Ticks, DateTimeKind.Utc),
                       StartDate = (DateTime?)(row["StartDate"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["StartDate"]).Ticks, DateTimeKind.Utc) : null),
                       FinishDate = (DateTime?)(row["FinishDate"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["FinishDate"]).Ticks, DateTimeKind.Utc) : null),
                       ScheduleName = (row["ScheduleName"] != DBNull.Value) ? (string)row["ScheduleName"] : null
                   };
        }

        /// <summary>
        /// Parameterizes the given <see cref="JobRecord"/> into the given <see cref="DbCommand"/> object.
        /// </summary>
        /// <typeparam name="TCommand">The type of <see cref="DbCommand"/> to parameterize the record for.</typeparam>
        /// <typeparam name="TParameter">The type of <see cref="DbParameter"/> to use with the given <see cref="DbCommand"/> type.</typeparam>
        /// <param name="record">The <see cref="JobRecord"/> to parameterize.</param>
        /// <param name="command">The <see cref="DbCommand"/> to add <see cref="DbParameter"/>s to.</param>
        /// <returns>The parameterized <see cref="DbCommand"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "I'm both too lazy and too unfamiliar with ADO.NET to figure out how to infer the parameter type from the command type right now. TODO. Happy?")]
        public static TCommand ParameterizeRecord<TCommand, TParameter>(JobRecord record, TCommand command)
            where TCommand : DbCommand
            where TParameter : DbParameter, new()
        {
            command.Parameters.Add(new TParameter() { ParameterName = "@Name", Value = record.Name });
            command.Parameters.Add(new TParameter() { ParameterName = "@Type", Value = record.JobType.AssemblyQualifiedName });
            command.Parameters.Add(new TParameter() { ParameterName = "@Data", Value = record.Data });
            command.Parameters.Add(new TParameter() { ParameterName = "@Status", Value = record.Status.ToString() });

            var exception = new TParameter() { ParameterName = "@Exception", Value = record.Exception };

            if (String.IsNullOrEmpty(record.Exception))
            {
                exception.Value = DBNull.Value;
            }

            command.Parameters.Add(exception);
            command.Parameters.Add(new TParameter() { ParameterName = "@QueueDate", Value = record.QueueDate });

            var startDate = new TParameter() { ParameterName = "@StartDate", Value = record.StartDate };

            if (record.StartDate == null)
            {
                startDate.Value = DBNull.Value;
            }

            command.Parameters.Add(startDate);

            var finishDate = new TParameter() { ParameterName = "@FinishDate", Value = record.FinishDate };
            
            if (record.FinishDate == null)
            {
                finishDate.Value = DBNull.Value;
            }

            command.Parameters.Add(finishDate);

            var scheduleName = new TParameter() { ParameterName = "@ScheduleName", Value = record.ScheduleName };
            
            if (String.IsNullOrEmpty(record.ScheduleName))
            {
                scheduleName.Value = DBNull.Value;
            }

            command.Parameters.Add(scheduleName);

            return command;
        }
    }
}

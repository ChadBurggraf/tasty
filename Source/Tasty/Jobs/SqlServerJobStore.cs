//-----------------------------------------------------------------------
// <copyright file="SqlJobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Implements <see cref="IJobStore"/> for Microsoft SQL Server.
    /// </summary>
    public class SqlServerJobStore : SqlJobStore
    {
        /// <summary>
        /// Initializes a new instance of the SqlServerJobStore class.
        /// </summary>
        public SqlServerJobStore()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlServerJobStore class.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        public SqlServerJobStore(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Creates a data adapter.
        /// </summary>
        /// <param name="command">The command to create the adapter with.</param>
        /// <returns>The created data adapter.</returns>
        public override DataAdapter CreateAdapter(DbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command);
        }

        /// <summary>
        /// Creates a connection.
        /// </summary>
        /// <returns>The created connection.</returns>
        public override DbConnection CreateConnection()
        {
            EnsureConnectionString();
            return new SqlConnection(ConnectionString);
        }

        /// <summary>
        /// Creates a delete command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="transaction">The transaction to create the command with, if applicable.</param>
        /// <param name="id">The ID of the record to delete.</param>
        /// <returns>A delete command.</returns>
        public override DbCommand CreateDeleteCommand(DbConnection connection, DbTransaction transaction, int id)
        {
            const string sql = "DELETE FROM [TastyJob] WHERE [Id] = @Id";

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.Transaction = transaction as SqlTransaction;
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("@Id", id));

            return command;
        }

        /// <summary>
        /// Creates a select command that can be used to fetch a collection of each scheduled job's latest record.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <returns>A select command.</returns>
        public override DbCommand CreateLatestScheduledJobsSelectCommand(DbConnection connection)
        {
            const string sql =
                @"SELECT * FROM (
	                SELECT *, RANK() OVER (PARTITION BY [Type],[ScheduleName] ORDER BY [QueueDate] DESC) AS [Rank]
	                FROM [TastyJob] WHERE [ScheduleName] IS NOT NULL
                ) t WHERE [Rank] = 1";

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;

            return command;
        }

        /// <summary>
        /// Creates an insert or update command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="transaction">The transaction to create the command with, if applicable.</param>
        /// <param name="record">The record to insert or update.</param>
        /// <returns>An insert or update command.</returns>
        public override DbCommand CreateSaveCommand(DbConnection connection, DbTransaction transaction, JobRecord record)
        {
            const string insert = "INSERT INTO [TastyJob]([Name],[Type],[Data],[Status],[Exception],[QueueDate],[StartDate],[FinishDate],[ScheduleName]) VALUES(@Name,@Type,@Data,@Status,@Exception,@QueueDate,@StartDate,@FinishDate,@ScheduleName); SELECT SCOPE_IDENTITY()";
            const string update = "UPDATE [TastyJob] SET [Name]=@Name,[Type]=@Type,[Data]=@Data,[Status]=@Status,[Exception]=@Exception,[QueueDate]=@QueueDate,[StartDate]=@StartDate,[FinishDate]=@FinishDate,[ScheduleName]=@ScheduleName WHERE [Id]=@Id";

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.Transaction = transaction as SqlTransaction;

            if (record.Id == null)
            {
                command.CommandText = insert;
            }
            else
            {
                command.CommandText = update;
                command.Parameters.Add(new SqlParameter("@Id", record.Id.Value));
            }

            command.Parameters.Add(new SqlParameter("@Name", record.Name));
            command.Parameters.Add(new SqlParameter("@Type", record.JobType));
            command.Parameters.Add(new SqlParameter("@Data", record.Data));
            command.Parameters.Add(new SqlParameter("@Status", record.Status.ToString()));

            var exception = new SqlParameter("@Exception", record.Exception);

            if (String.IsNullOrEmpty(record.Exception))
            {
                exception.Value = DBNull.Value;
            }

            command.Parameters.Add(exception);
            command.Parameters.Add(new SqlParameter("@QueueDate", record.QueueDate));

            var startDate = new SqlParameter("@StartDate", record.StartDate);

            if (record.StartDate == null)
            {
                startDate.Value = DBNull.Value;
            }

            command.Parameters.Add(startDate);

            var finishDate = new SqlParameter("@FinishDate", record.FinishDate);

            if (record.FinishDate == null)
            {
                finishDate.Value = DBNull.Value;
            }

            command.Parameters.Add(finishDate);

            var scheduleName = new SqlParameter("@ScheduleName", record.ScheduleName);

            if (String.IsNullOrEmpty(record.ScheduleName))
            {
                scheduleName.Value = DBNull.Value;
            }

            command.Parameters.Add(scheduleName);

            return command;
        }

        /// <summary>
        /// Creates a collection of <see cref="JobRecord"/>s from the given <see cref="DataSet"/>.
        /// </summary>
        /// <param name="resultSet">The <see cref="DataSet"/> to create the records from.</param>
        /// <returns>A collection of <see cref="JobRecord"/>s</returns>
        public override IEnumerable<JobRecord> CreateRecordCollection(DataSet resultSet)
        {
            List<JobRecord> records = new List<JobRecord>();

            if (resultSet != null && resultSet.Tables.Count > 0)
            {
                foreach (DataRow row in resultSet.Tables[0].Rows)
                {
                    records.Add(
                        new JobRecord()
                        {
                            Id = (int)row["Id"],
                            Name = (string)row["Name"],
                            JobType = (string)row["Type"],
                            Data = (string)row["Data"],
                            Status = (JobStatus)Enum.Parse(typeof(JobStatus), (string)row["Status"]),
                            Exception = (row["Exception"] != DBNull.Value) ? (string)row["Exception"] : null,
                            QueueDate = new DateTime(((DateTime)row["QueueDate"]).Ticks, DateTimeKind.Utc),
                            StartDate = (DateTime?)(row["StartDate"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["StartDate"]).Ticks, DateTimeKind.Utc) : null),
                            FinishDate = (DateTime?)(row["FinishDate"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["FinishDate"]).Ticks, DateTimeKind.Utc) : null),
                            ScheduleName = (row["ScheduleName"] != DBNull.Value) ? (string)row["ScheduleName"] : null
                        });
                    }
            }

            return records;
        }

        /// <summary>
        /// Creates a select command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="id">The ID of the result to fetch.</param>
        /// <returns>A select command.</returns>
        public override DbCommand CreateSelectCommand(DbConnection connection, int id)
        {
            const string sql = "SELECT * FROM [TastyJob] WHERE [Id] = @Id";

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("@Id", id));

            return command;
        }

        /// <summary>
        /// Creates a select command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="ids">The IDs to restrict the result set to.</param>
        /// <returns>A select command.</returns>
        public override DbCommand CreateSelectCommand(DbConnection connection, IEnumerable<int> ids)
        {
            const string sql = "SELECT * FROM [TastyJob] WHERE [Id] IN ({0}) ORDER BY [QueueDate]";

            string[] idStrings = ids != null ? ids.Select(i => i.ToString(CultureInfo.InvariantCulture)).ToArray() : new string[0];

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = String.Format(CultureInfo.InvariantCulture, sql, String.Join(",", idStrings));

            return command;
        }

        /// <summary>
        /// Creates a select command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="status">The job status to filter results on.</param>
        /// <param name="count">The maximum number of results to select.</param>
        /// <returns>A select command.</returns>
        public override DbCommand CreateSelectCommand(DbConnection connection, JobStatus status, int count)
        {
            const string sql = " * FROM [TastyJob] WHERE [Status]=@Status ORDER BY [QueueDate]";

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;

            if (count > 0)
            {
                command.CommandText = String.Format(CultureInfo.InvariantCulture, "SELECT TOP {0}{1}", count, sql);
            }
            else
            {
                command.CommandText = String.Concat("SELECT", sql);
            }

            command.Parameters.Add(new SqlParameter("@Status", status.ToString()));

            return command;
        }
    }
}

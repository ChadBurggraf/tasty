//-----------------------------------------------------------------------
// <copyright file="PostgresJobStore.cs" company="Tasty Codes">
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
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Npgsql;

    /// <summary>
    /// Implements <see cref="IJobStore"/> for Postgres.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class PostgresJobStore : SqlJobStore
    {
        /// <summary>
        /// Initializes a new instance of the PostgresJobStore class.
        /// </summary>
        public PostgresJobStore()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PostgresJobStore class.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        public PostgresJobStore(string connectionString)
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
            return new NpgsqlDataAdapter((NpgsqlCommand)command);
        }

        /// <summary>
        /// Creates a command that can be used to fetch the number of records matching the given filter parameters.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="likeName">A string representing a full or partial job name to filter on.</param>
        /// <param name="withStatus">A <see cref="JobStatus"/> to filter on, or null if not applicable.</param>
        /// <param name="inSchedule">A schedule name to filter on, if applicable.</param>
        /// <returns>A select command.</returns>
        public override DbCommand CreateCountCommand(DbConnection connection, string likeName, JobStatus? withStatus, string inSchedule)
        {
            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;

            StringBuilder sb = new StringBuilder(@"SELECT COUNT(""id"") FROM ""tasty_job"" WHERE ""name"" LIKE :name");

            if (withStatus != null)
            {
                sb.Append(@" AND ""status""=:status");
                command.Parameters.Add(new NpgsqlParameter(":status", withStatus.Value.ToString()));
            }

            if (!String.IsNullOrEmpty(inSchedule))
            {
                sb.Append(@" AND ""schedule_name""=:schedule_name");
                command.Parameters.Add(new NpgsqlParameter(":schedule_name", inSchedule));
            }

            command.CommandText = sb.ToString();
            command.Parameters.Add(new NpgsqlParameter(":name", String.Concat("%", (likeName ?? String.Empty).Trim(), "%")));

            return command;
        }

        /// <summary>
        /// Creates a connection.
        /// </summary>
        /// <returns>The created connection.</returns>
        public override DbConnection CreateConnection()
        {
            EnsureConnectionString();
            return new NpgsqlConnection(ConnectionString);
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
            const string sql = @"DELETE FROM ""tasty_job"" WHERE ""id"" = :id";

            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.Transaction = transaction as NpgsqlTransaction;
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter(":id", id));

            return command;
        }

        /// <summary>
        /// Creates a select command that can be used to fetch a collection of each scheduled job's latest record.
        /// </summary>
        /// <param name="scheduleNames">A collection of schedule names to get the latest persisted jobs for.</param>
        /// <param name="connection">The connection to create the command with.</param>
        /// <returns>A select command.</returns>
        public override DbCommand CreateLatestScheduledJobsSelectCommand(IEnumerable<string> scheduleNames, DbConnection connection)
        {
            StringBuilder sb = new StringBuilder(
                @"SELECT * FROM (
	                SELECT *, rank() OVER (PARTITION BY ""type"", ""schedule_name"" ORDER BY ""queue_date"" DESC)
	                FROM ""tasty_job"" WHERE ""schedule_name"" IN (");

            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;

            int i = 0;

            foreach (string scheduleName in scheduleNames)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }

                string paramName = String.Concat(":sn", i++);
                sb.Append(paramName);

                command.Parameters.Add(new NpgsqlParameter(paramName, scheduleName));
            }

            sb.Append(@")) AS t WHERE ""rank"" = 1");
            command.CommandText = sb.ToString();

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
            const string insert = @"INSERT INTO ""tasty_job""(""name"",""type"",""data"",""status"",""exception"",""queue_date"",""start_date"",""finish_date"",""schedule_name"") VALUES(:name,:type,:data,:status,:exception,:queue_date,:start_date,:finish_date,:schedule_name); SELECT currval('tasty_job_id_seq') AS ""id"";";
            const string update = @"UPDATE ""tasty_job"" SET ""name""=:name,""type""=:type,""data""=:data,""status""=:status,""exception""=:exception,""queue_date""=:queue_date,""start_date""=:start_date,""finish_date""=:finish_date,""schedule_name""=:schedule_name WHERE ""id""=:id";

            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.Transaction = transaction as NpgsqlTransaction;

            if (record.Id == null)
            {
                command.CommandText = insert;
            }
            else
            {
                command.CommandText = update;
                command.Parameters.Add(new NpgsqlParameter(":id", record.Id.Value));
            }

            command.Parameters.Add(new NpgsqlParameter(":name", record.Name));
            command.Parameters.Add(new NpgsqlParameter(":type", record.JobType));
            command.Parameters.Add(new NpgsqlParameter(":data", record.Data));
            command.Parameters.Add(new NpgsqlParameter(":status", record.Status.ToString()));

            var exception = new NpgsqlParameter(":exception", record.Exception);

            if (String.IsNullOrEmpty(record.Exception))
            {
                exception.Value = DBNull.Value;
            }

            command.Parameters.Add(exception);
            command.Parameters.Add(new NpgsqlParameter(":queue_date", record.QueueDate));

            var startDate = new NpgsqlParameter(":start_date", record.StartDate);

            if (record.StartDate == null)
            {
                startDate.Value = DBNull.Value;
            }

            command.Parameters.Add(startDate);

            var finishDate = new NpgsqlParameter(":finish_date", record.FinishDate);

            if (record.FinishDate == null)
            {
                finishDate.Value = DBNull.Value;
            }

            command.Parameters.Add(finishDate);

            var scheduleName = new NpgsqlParameter(":schedule_name", record.ScheduleName);

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
                            Id = (int)row["id"],
                            Name = (string)row["name"],
                            JobType = (string)row["type"],
                            Data = (string)row["data"],
                            Status = (JobStatus)Enum.Parse(typeof(JobStatus), (string)row["status"]),
                            Exception = (row["exception"] != DBNull.Value) ? (string)row["exception"] : null,
                            QueueDate = new DateTime(((DateTime)row["queue_date"]).Ticks, DateTimeKind.Utc),
                            StartDate = (DateTime?)(row["start_date"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["start_date"]).Ticks, DateTimeKind.Utc) : null),
                            FinishDate = (DateTime?)(row["finish_date"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["finish_date"]).Ticks, DateTimeKind.Utc) : null),
                            ScheduleName = (row["schedule_name"] != DBNull.Value) ? (string)row["schedule_name"] : null
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
            const string sql = @"SELECT * FROM ""tasty_job"" WHERE ""id"" = :id";

            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter(":id", id));

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
            const string sql = @"SELECT * FROM ""tasty_job"" WHERE ""id"" IN ({0}) ORDER BY ""queue_date""";

            string[] idStrings = ids != null ? ids.Select(i => i.ToString(CultureInfo.InvariantCulture)).ToArray() : new string[0];

            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();
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
            const string sql = @"SELECT * FROM ""tasty_job"" WHERE ""status""=:status ORDER BY ""queue_date""";

            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;

            if (count > 0)
            {
                command.CommandText += String.Concat(" LIMIT ", count);
            }

            command.Parameters.Add(new NpgsqlParameter(":status", status.ToString()));

            return command;
        }

        /// <summary>
        /// Creates a select command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="likeName">A string representing a full or partial job name to filter on.</param>
        /// <param name="withStatus">A <see cref="JobStatus"/> to filter on, or null if not applicable.</param>
        /// <param name="inSchedule">A schedule name to filter on, if applicable.</param>
        /// <param name="orderBy">A field to order the resultset by.</param>
        /// <param name="sortDescending">A value indicating whether to order the resultset in descending order.</param>
        /// <param name="pageNumber">The page number to get.</param>
        /// <param name="pageSize">The size of the pages to get.</param>
        /// <returns></returns>
        public override DbCommand CreateSelectCommand(DbConnection connection, string likeName, JobStatus? withStatus, string inSchedule, JobRecordResultsOrderBy orderBy, bool sortDescending, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 0)
            {
                pageSize = 0;
            }

            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;

            StringBuilder sb = new StringBuilder();
            string orderByColumn = null;

            switch (orderBy)
            {
                case JobRecordResultsOrderBy.FinishDate:
                    orderByColumn = @"""finish_date""";
                    break;
                case JobRecordResultsOrderBy.JobType:
                    orderByColumn = @"""type""";
                    break;
                case JobRecordResultsOrderBy.Name:
                    orderByColumn = @"""name""";
                    break;
                case JobRecordResultsOrderBy.QueueDate:
                    orderByColumn = @"""queue_date""";
                    break;
                case JobRecordResultsOrderBy.ScheduleName:
                    orderByColumn = @"""schedule_name""";
                    break;
                case JobRecordResultsOrderBy.StartDate:
                    orderByColumn = @"""start_date""";
                    break;
                case JobRecordResultsOrderBy.Status:
                    orderByColumn = @"""status""";
                    break;
                default:
                    throw new NotImplementedException();
            }

            sb.Append(@"SELECT * FROM ""tasty_job"" WHERE ""name"" LIKE :name");

            if (withStatus != null)
            {
                sb.Append(@" AND ""status""=:status");
                command.Parameters.Add(new NpgsqlParameter(":status", withStatus.Value.ToString()));
            }

            if (!String.IsNullOrEmpty(inSchedule))
            {
                sb.Append(@" AND ""schedule_name""=:schedule_name");
                command.Parameters.Add(new NpgsqlParameter(":schedule_name", inSchedule));
            }

            sb.AppendFormat(
                CultureInfo.InvariantCulture,
                @" ORDER BY {0} {1} LIMIT :take OFFSET :skip",
                orderByColumn,
                sortDescending ? "DESC" : "ASC");

            command.CommandText = sb.ToString();

            command.Parameters.Add(new NpgsqlParameter(":name", String.Concat("%", (likeName ?? String.Empty).Trim(), "%")));
            command.Parameters.Add(new NpgsqlParameter(":skip", (pageNumber - 1) * pageSize));
            command.Parameters.Add(new NpgsqlParameter(":take", pageSize));

            return command;
        }
    }
}

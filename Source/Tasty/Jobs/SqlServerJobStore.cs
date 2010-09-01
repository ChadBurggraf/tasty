//-----------------------------------------------------------------------
// <copyright file="SqlServerJobStore.cs" company="Tasty Codes">
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
    using System.Text;

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
        /// Creates a command that can be used to fetch the number of records matching the given filter parameters.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="likeName">A string representing a full or partial job name to filter on.</param>
        /// <param name="withStatus">A <see cref="JobStatus"/> to filter on, or null if not applicable.</param>
        /// <param name="inSchedule">A schedule name to filter on, if applicable.</param>
        /// <returns>A select command.</returns>
        public override DbCommand CreateCountCommand(DbConnection connection, string likeName, JobStatus? withStatus, string inSchedule)
        {
            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;

            StringBuilder sb = new StringBuilder(@"SELECT COUNT([Id]) FROM [TastyJob] WHERE [Name] LIKE @Name");

            if (withStatus != null)
            {
                sb.Append(@" AND [Status]=@Status");
                command.Parameters.Add(new SqlParameter("@Status", withStatus.Value.ToString()));
            }

            if (!String.IsNullOrEmpty(inSchedule))
            {
                sb.Append(" AND [ScheduleName]=@ScheduleName");
                command.Parameters.Add(new SqlParameter("@ScheduleName", inSchedule));
            }

            command.CommandText = sb.ToString();
            command.Parameters.Add(new SqlParameter("@Name", String.Concat("%", (likeName ?? String.Empty).Trim(), "%")));

            return command;
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
            const string Sql = "DELETE FROM [TastyJob] WHERE [Id] = @Id";

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.Transaction = transaction as SqlTransaction;
            command.CommandText = Sql;
            command.Parameters.Add(new SqlParameter("@Id", id));

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
	                SELECT *, RANK() OVER (PARTITION BY [Type],[ScheduleName] ORDER BY [QueueDate] DESC) AS [Rank]
	                FROM [TastyJob] WHERE [ScheduleName] IN (");

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;

            int i = 0;

            foreach (string scheduleName in scheduleNames)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }

                string paramName = String.Concat("@SN", i++);
                sb.Append(paramName);

                command.Parameters.Add(new SqlParameter(paramName, scheduleName));
            }

            sb.Append(@")) t WHERE [Rank] = 1");
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
            const string Insert = "INSERT INTO [TastyJob]([Name],[Type],[Data],[Status],[Exception],[QueueDate],[StartDate],[FinishDate],[ScheduleName]) VALUES(@Name,@Type,@Data,@Status,@Exception,@QueueDate,@StartDate,@FinishDate,@ScheduleName); SELECT SCOPE_IDENTITY()";
            const string Update = "UPDATE [TastyJob] SET [Name]=@Name,[Type]=@Type,[Data]=@Data,[Status]=@Status,[Exception]=@Exception,[QueueDate]=@QueueDate,[StartDate]=@StartDate,[FinishDate]=@FinishDate,[ScheduleName]=@ScheduleName WHERE [Id]=@Id";

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.Transaction = transaction as SqlTransaction;

            if (record.Id == null)
            {
                command.CommandText = Insert;
            }
            else
            {
                command.CommandText = Update;
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
            const string Sql = "SELECT * FROM [TastyJob] WHERE [Id] = @Id";

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = Sql;
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
            const string Sql = "SELECT * FROM [TastyJob] WHERE [Id] IN ({0}) ORDER BY [QueueDate]";

            string[] idStrings = ids != null ? ids.Select(i => i.ToString(CultureInfo.InvariantCulture)).ToArray() : new string[0];

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = String.Format(CultureInfo.InvariantCulture, Sql, String.Join(",", idStrings));

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
            const string Sql = " * FROM [TastyJob] WHERE [Status]=@Status ORDER BY [QueueDate]";

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;

            if (count > 0)
            {
                command.CommandText = String.Format(CultureInfo.InvariantCulture, "SELECT TOP {0}{1}", count, Sql);
            }
            else
            {
                command.CommandText = String.Concat("SELECT", Sql);
            }

            command.Parameters.Add(new SqlParameter("@Status", status.ToString()));

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
        /// <returns>A select command.</returns>
        public override DbCommand CreateSelectCommand(DbConnection connection, string likeName, JobStatus? withStatus, string inSchedule, JobRecordResultsOrderBy orderBy, bool sortDescending, int pageNumber, int pageSize)
        {
            const string Sql = @"SELECT * FROM (
                SELECT *, ROW_NUMBER() OVER(ORDER BY {0} {1}) AS [RowNumber]
                FROM [TastyJob] WHERE [Name] LIKE @Name";

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 0)
            {
                pageSize = 0;
            }

            SqlCommand command = ((SqlConnection)connection).CreateCommand();
            command.CommandType = CommandType.Text;

            StringBuilder sb = new StringBuilder();
            string orderByColumn = null;

            switch (orderBy)
            {
                case JobRecordResultsOrderBy.FinishDate:
                    orderByColumn = "[FinishDate]";
                    break;
                case JobRecordResultsOrderBy.JobType:
                    orderByColumn = "[Type]";
                    break;
                case JobRecordResultsOrderBy.Name:
                    orderByColumn = "[Name]";
                    break;
                case JobRecordResultsOrderBy.QueueDate:
                    orderByColumn = "[QueueDate]";
                    break;
                case JobRecordResultsOrderBy.ScheduleName:
                    orderByColumn = "[ScheduleName]";
                    break;
                case JobRecordResultsOrderBy.StartDate:
                    orderByColumn = "[StartDate]";
                    break;
                case JobRecordResultsOrderBy.Status:
                    orderByColumn = "[Status]";
                    break;
                default:
                    throw new NotImplementedException();
            }

            sb.AppendFormat(
                CultureInfo.InvariantCulture,
                Sql,
                orderByColumn,
                sortDescending ? "DESC" : "ASC");

            if (withStatus != null)
            {
                sb.Append(" AND [Status]=@Status");
                command.Parameters.Add(new SqlParameter("@Status", withStatus.Value.ToString()));
            }

            if (!String.IsNullOrEmpty(inSchedule))
            {
                sb.Append(" AND [ScheduleName]=@ScheduleName");
                command.Parameters.Add(new SqlParameter("@ScheduleName", inSchedule));
            }

            sb.Append(@") t WHERE [RowNumber] > @SkipFrom AND [RowNumber] <= @SkipTo");
            command.CommandText = sb.ToString();

            int skipFrom = (pageNumber - 1) * pageSize;

            command.Parameters.Add(new SqlParameter("@Name", String.Concat("%", (likeName ?? String.Empty).Trim(), "%")));
            command.Parameters.Add(new SqlParameter("@SkipFrom", skipFrom));
            command.Parameters.Add(new SqlParameter("@SkipTo", skipFrom + pageSize));

            return command;
        }
    }
}

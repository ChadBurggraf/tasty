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
    using System.Data.SqlClient;
    using System.Linq;         

    /// <summary>
    /// Implements <see cref="IJobStore"/> for SQL Server.
    /// </summary>
    public class SqlServerJobStore : IJobStore
    {
        /// <summary>
        /// Gets a collection of jobs that have been marked as <see cref="JobStatus.Canceling"/>.
        /// Opens a new transaction, then calls the delegate to perform any work. The transaction
        /// is committed when the delegate returns.
        /// </summary>
        /// <param name="cancelling">The function to call with the cancelling job collection.</param>
        public void CancellingJobs(Action<IEnumerable<JobRecord>> cancelling)
        {
            const string Sql = "SELECT * FROM [TastyJob] WHERE [Status]='Cancelling' ORDER BY [QueueDate]";

            using (SqlConnection connection = new SqlConnection())
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted, "TastyCancellingJobs");

                try
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable results = new DataTable();
                        adapter.Fill(results);


                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates a new job record.
        /// </summary>
        /// <param name="record">The record to create.</param>
        /// <returns>The created record.</returns>
        public JobRecord CreateJob(JobRecord record)
        {
            using (SqlConnection connection = new SqlConnection())
            {
                connection.Open();

                using (SqlCommand command = this.InsertCommand(record, connection))
                {
                    SqlParameter id = new SqlParameter("@Id", SqlDbType.Int);
                    id.Direction = ParameterDirection.Output;
                    command.Parameters.Add(id);

                    command.ExecuteNonQuery();
                    record.Id = (int)id.Value;
                }
            }

            return record;
        }

        /// <summary>
        /// Gets a collection of queued jobs that can be dequeued right now.
        /// Opens a new transaction, then calls the delegate to perform any work. The transaction
        /// is committed when the delegate returns.
        /// </summary>
        /// <param name="dequeueing">The function to call with the dequeued job collection.</param>
        public void DequeueJobs(Action<IEnumerable<JobRecord>> dequeueing)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates a collection of jobs. Opens a new transaction, then calls the delegate to perform
        /// any work on each record. The transaction is committed when all of the records have been iterated through.
        /// </summary>
        /// <param name="records">The records to update.</param>
        /// <param name="updating">The function to call for each iteration, which should perform any updates necessary on the job record.</param>
        public void UpdateJobs(IEnumerable<JobRecord> records, Action<JobRecord> updating)
        {
            const string Sql = "UPDATE [TastyJob] SET [Name]=@Name,[Type]=@Type,[Data]=@Data,[Status]=@Status,[Exception]=@Exception,[QueueDate]=@QueueDate,[StartDate]=@StartDate,[FinishDate]=@FinishDate,[ScheduleName]=@ScheduleName WHERE [Id]=@Id";
            
            using (SqlConnection connection = new SqlConnection())
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted, "TastyUpdateJobs");

                try
                {
                    foreach (var record in records)
                    {
                        if (updating != null)
                        {
                            updating(record);
                        }

                        SqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = Sql;

                        command.Parameters.Add(new SqlParameter("@Id", record.Id.Value));
                        Parameterize(record, command).ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="SqlCommand"/> for inserting the given <see cref="JobRecord"/> into the database.
        /// </summary>
        /// <param name="record">The record to create the command for.</param>
        /// <param name="connection">The connection to use when creating the command.</param>
        /// <returns>A new INSERT <see cref="SqlCommand"/>.</returns>
        private SqlCommand InsertCommand(JobRecord record, SqlConnection connection)
        {
            const string Sql = "INSERT INTO [TastyJob]([Name],[Type],[Data],[Status],[Exception],[QueueDate],[StartDate],[FinishDate],[ScheduleName]) VALUES(@Name,@Type,@Data,@Status,@Exception,@QueueDate,@StartDate,@FinishDate,@ScheduleName); SET @Id=SCOPE_IDENTITY()";
            
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = Sql;
            
            return Parameterize(record, command);
        }

        /// <summary>
        /// Parameterizes the given <see cref="JobRecord"/> into the given <see cref="SqlCommand"/>.
        /// Does not include the record's ID.
        /// </summary>
        /// <param name="record">The record to parameterize.</param>
        /// <param name="command">The command to parameterize the record into.</param>
        /// <returns>The command with the record's values added as parameters.</returns>
        private static SqlCommand Parameterize(JobRecord record, SqlCommand command)
        {
            command.Parameters.Add(new SqlParameter("@Name", record.Name));
            command.Parameters.Add(new SqlParameter("@Type", record.JobType.AssemblyQualifiedName));
            command.Parameters.Add(new SqlParameter("@Data", record.Data));
            command.Parameters.Add(new SqlParameter("@Status", record.Status.ToString()));

            SqlParameter exception = new SqlParameter("@Exception", record.Exception);

            if (String.IsNullOrEmpty(record.Exception))
            {
                exception.Value = DBNull.Value;
            }

            command.Parameters.Add(exception);
            command.Parameters.Add(new SqlParameter("@QueueDate", record.QueueDate));

            SqlParameter startDate = new SqlParameter("@StartDate", record.StartDate);

            if (record.StartDate == null)
            {
                startDate.Value = DBNull.Value;
            }

            command.Parameters.Add(startDate);

            SqlParameter finishDate = new SqlParameter("@FinishDate", record.FinishDate);

            if (record.FinishDate == null)
            {
                finishDate.Value = DBNull.Value;
            }

            command.Parameters.Add(finishDate);

            SqlParameter scheduleName = new SqlParameter("@ScheduleName", record.ScheduleName);

            if (String.IsNullOrEmpty(record.ScheduleName))
            {
                scheduleName.Value = DBNull.Value;
            }

            command.Parameters.Add(scheduleName);

            return command;
        }

        private static IList<JobRecord> RecordCollection(DataTable resultSet)
        {
            return (from row in resultSet.Rows
                    select new JobRecord()
                {
                    Id = (int)row["Id"],
                    Name = (string)row["Name"],
                    JobType = Type.GetType((string)row["Type"]),
                    Data = (string)row["Data"],
                    Status = (JobStatus)Enum.Parse(typeof(JobStatus), (string)row["Status"]),
                    Exception = (row != DBNull.Value) ? (string)row["Exception"] : null,
                    QueueDate = (DateTime)row["QueueDate"],
                    StartDate = (DateTime?)(row != DBNull.Value ? row["StartDate"] : null),
                    FinishDate = (DateTime?)(row != DBNull.Value ? row["FinishDate"] : null),
                    ScheduleName = (row != DBNull.Value) ? (string)row["ScheduleName"] : null
                })
            /*List<JobRecord> records = new List<JobRecord>(resultSet.Rows.Count);

            foreach (var row in resultSet)
            {
                JobRecord record = new JobRecord()
                {
                    Id = (int)row["Id"],
                    Name = (string)row["Name"],
                    JobType = Type.GetType((string)row["Type"]),
                    Data = (string)row["Data"],
                    Status = (JobStatus)Enum.Parse(typeof(JobStatus), (string)row["Status"]),
                    Exception = (row != DBNull.Value) ? (string)row["Exception"] : null,
                    QueueDate = (DateTime)row["QueueDate"],
                    StartDate = (DateTime?)(row != DBNull.Value ? row["StartDate"] : null),
                    FinishDate = (DateTime?)(row != DBNull.Value ? row["FinishDate"] : null),
                    ScheduleName = (row != DBNull.Value) ? (string)row["ScheduleName"] : null
                };
            }

            return records;*/
        }
    }
}
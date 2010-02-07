//-----------------------------------------------------------------------
// <copyright file="UrlTokenStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Tasty.Configuration;

    /// <summary>
    /// Provides global persistence functionality and a way to access the <see cref="IUrlTokenStore"/>
    /// currently in use.
    /// </summary>
    public static class UrlTokenStore
    {
        private static readonly object currentLocker = new object();
        private static IUrlTokenStore current;

        /// <summary>
        /// Gets or sets the current <see cref="IUrlTokenStore"/> implementation in use.
        /// The setter on this property is primarily meant for testing purposes.
        /// </summary>
        public static IUrlTokenStore Current
        {
            get
            {
                lock (currentLocker)
                {
                    if (current == null)
                    {
                        current = (IUrlTokenStore)Activator.CreateInstance(Type.GetType(TastySettings.Section.UrlTokens.UrlTokenStoreType));
                    }

                    return current;
                }
            }

            set
            {
                lock (currentLocker)
                {
                    current = value;
                }
            }
        }

        /// <summary>
        /// Gets the given result set as a collection of <see cref="UrlTokenRecord"/>s.
        /// Assumes the result set has the expected schema definition.
        /// </summary>
        /// <param name="resultSet">The result set to convert into a collection of <see cref="UrlTokenRecord"/>s.</param>
        /// <returns>A collection of <see cref="UrlTokenRecord"/>s.</returns>
        public static IEnumerable<UrlTokenRecord> CreateRecordCollection(DataTable resultSet)
        {
            return from DataRow row in resultSet.Rows
                   select new UrlTokenRecord()
                   {
                       Key = (string)row["Key"],
                       TokenType = Type.GetType((string)row["Type"]),
                       Data = (string)row["Data"],
                       Created = new DateTime(((DateTime)row["Created"]).Ticks, DateTimeKind.Utc),
                       Expires = new DateTime(((DateTime)row["Expires"]).Ticks, DateTimeKind.Utc)
                   };
        }

        /// <summary>
        /// Parameterizes the given <see cref="UrlTokenRecord"/> into the given <see cref="DbCommand"/> object.
        /// </summary>
        /// <typeparam name="TCommand">The type of <see cref="DbCommand"/> to parameterize the record for.</typeparam>
        /// <typeparam name="TParameter">The type of <see cref="DbParameter"/> to use with the given <see cref="DbCommand"/> type.</typeparam>
        /// <param name="record">The <see cref="UrlTokenRecord"/> to parameterize.</param>
        /// <param name="command">The <see cref="DbCommand"/> to add <see cref="DbParameter"/>s to.</param>
        /// <returns>The parameterized <see cref="DbCommand"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "I'm both too lazy and too unfamiliar with ADO.NET to figure out how to infer the parameter type from the command type right now. TODO. Happy?")]
        public static TCommand ParameterizeRecord<TCommand, TParameter>(UrlTokenRecord record, TCommand command)
            where TCommand : DbCommand
            where TParameter : DbParameter, new()
        {
            command.Parameters.Add(new TParameter() { ParameterName = "@Key", Value = record.Key });
            command.Parameters.Add(new TParameter() { ParameterName = "@Type", Value = record.TokenType.AssemblyQualifiedName });
            command.Parameters.Add(new TParameter() { ParameterName = "@Data", Value = record.Data });
            command.Parameters.Add(new TParameter() { ParameterName = "@Created", Value = record.Created });
            command.Parameters.Add(new TParameter() { ParameterName = "@Expires", Value = record.Expires });

            return command;
        }
    }
}

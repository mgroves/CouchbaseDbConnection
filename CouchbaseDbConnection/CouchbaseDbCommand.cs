using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Query;
using Dynamitey;
using Dynamitey.DynamicObjects;

namespace CouchbaseDbConnection
{
    public class CouchbaseDbCommand : DbCommand
    {
        public CouchbaseDbCommand()
        {
            DbParameterCollection = new CouchbaseDbParameterCollection();
        }

        // Property to hold the reference to the connection
        public CouchbaseDbConnection Connection { get; set; }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection is not open.");
            }

            // Execute your Couchbase-specific command here and get the results
            // For example, you might use the Couchbase SDK to execute a query
            // and obtain the result set.
            var numMutations= await Connection.SqlPlusPlusCommandAsync(CommandText, DbParameterCollection);

            return (int)numMutations;
        }

        [Obsolete("Don't use sync")]
        public override int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var query = await Connection.SqlPlusPlusAsync(CommandText, DbParameterCollection);
            var firstResult = await query.FirstOrDefaultAsync(cancellationToken);

            // no results, return null
            if (firstResult == null)
                return null;

            // if its a primitive (e.g. SELECT VALUE or SELECT RAW), return that
            var obj = (object)firstResult;
            if (obj.GetType().IsPrimitive)
                return obj;

            // otherwise, scalar means return the "first value" from the first result
            // there really isn't a "first" or "ordinal" value in JSON, so let's just go alphabetically
            // hopefully developers are only selecting one field anyway for ExecuteScalar
            var memberNames = Dynamic.GetMemberNames(firstResult, dynamicOnly: true);
            List<string> memberNamesList = memberNames;
            var firstPropNameAlphabetically = memberNamesList
                .OrderBy(n => n)
                .FirstOrDefault();

            var firstValue = Dynamic.InvokeGet(firstResult, firstPropNameAlphabetically);

            return firstValue;
        }

        [Obsolete("Don't use sync")]
        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override string CommandText { get; set; }
        public override CommandType CommandType { get; set; }

        public override int CommandTimeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override UpdateRowSource UpdatedRowSource
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        protected override DbConnection DbConnection
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        protected override DbParameterCollection DbParameterCollection { get; }

        protected override DbTransaction DbTransaction
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override bool DesignTimeVisible
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        protected override DbParameter CreateDbParameter()
        {
            return new CouchbaseParameter();
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection is not open.");
            }

            var query = await Connection.SqlPlusPlusAsync(CommandText, DbParameterCollection);
            var enumerator = query.GetAsyncEnumerator(cancellationToken);
            await enumerator.MoveNextAsync(); // "preload" the first row, since Couchbase has no schema data

            var couchbaseDataReader = new CouchbaseDbDataReader(enumerator, behavior);

            return couchbaseDataReader;
        }

        [Obsolete("Don't use sync with Couchbase")]
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }
    }
}
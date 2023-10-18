using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Query;

namespace CouchbaseDbConnection
{
    public class CouchbaseDbConnection : DbConnection
    {
        private readonly ICluster _cluster;

        public CouchbaseDbConnection(ICluster cluster)
        {
            _cluster = cluster;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            // no closing, cluster should stay open for the life of the application
        }

        public override void Open()
        {
            // cluster should be ready to go, no opening needed
        }

        public override string ConnectionString { get; set; }
        public override string Database { get { throw new NotImplementedException();} }

        public override ConnectionState State => ConnectionState.Open;

        public override string DataSource { get { throw new NotImplementedException(); } }
        public override string ServerVersion { get { throw new NotImplementedException(); } }

        protected override DbCommand CreateDbCommand()
        {
            // Create a new CouchbaseDbCommand and associate it with this connection
            return new CouchbaseDbCommand
            {
                Connection = this // Set the connection property of the command
            };
        }

        public async Task<uint> SqlPlusPlusCommandAsync(string commandText,
            DbParameterCollection dbParameterCollection)
        {
            var options = new QueryOptions();
            if (dbParameterCollection != null)
            {
                foreach (DbParameter dbParameter in dbParameterCollection)
                {
                    options.Parameter(dbParameter.ParameterName, dbParameter.Value);
                }
            }

            var results = await _cluster.QueryAsync<object>(commandText, options);
            return results.MetaData.Metrics.MutationCount;
        }

        public async Task<IAsyncEnumerable<dynamic>> SqlPlusPlusAsync(string commandText,
            DbParameterCollection dbParameterCollection)
        {
            var options = new QueryOptions();
            if (dbParameterCollection != null)
            {
                foreach (DbParameter dbParameter in dbParameterCollection)
                {
                    options.Parameter(dbParameter.ParameterName, dbParameter.Value);
                }
            }

            var results = await _cluster.QueryAsync<object>(commandText, options);
            return results.Rows;
        }
    }
}

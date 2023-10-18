using System;
using System.Data;
using System.Data.Common;

namespace CouchbaseDbConnection
{
    public class CouchbaseParameter : DbParameter
    {
        public override void ResetDbType()
        {
            throw new NotImplementedException();
        }

        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override string ParameterName { get; set; }
        public override string SourceColumn
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override object Value { get; set; }
        public override bool SourceColumnNullMapping
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override int Size { get; set; }
    }
}
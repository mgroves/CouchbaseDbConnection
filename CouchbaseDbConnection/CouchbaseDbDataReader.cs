using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dynamitey;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchbaseDbConnection
{
    public class CouchbaseDbDataReader : DbDataReader
    {
        //private readonly IAsyncEnumerable<dynamic> _data;
        private readonly CommandBehavior _behavior;
        private readonly IAsyncEnumerator<object> _dataEnumerator;
        private bool _preloadProcessed;
        private List<string>? _names;

        public CouchbaseDbDataReader(IAsyncEnumerator<dynamic> dataEnumerator, CommandBehavior behavior)
        {
            _dataEnumerator = dataEnumerator;
            _behavior = behavior;
            _preloadProcessed = false;
        }

        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int ordinal)
        {
            var get = Dynamic.InvokeGet(_dataEnumerator.Current, _names[ordinal]);
            if (get is JArray)
            {
                JArray array = get;
                var arrayType = array[0].Type;
                if(arrayType == JTokenType.Integer)
                    return typeof(List<int>);
                if (arrayType == JTokenType.String)
                    return typeof(List<string>);
                if (arrayType == JTokenType.Boolean)
                    return typeof(List<bool>);
                if (arrayType == JTokenType.Object)
                    return typeof(List<JObject>);
            }
            var value = get?.Value;
            if (value == null)
                return typeof(object);
            var type = value.GetType();
            return type;
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override string GetName(int ordinal)
        {
            if (_names != null)
                return _names[ordinal];

            PopulateNames();
            return _names[ordinal];
        }

        private void PopulateNames()
        {
            if (_names != null)
                return;
            var obj = _dataEnumerator.Current;
            IEnumerable<string> names = Dynamic.GetMemberNames(obj, true);
            _names = names.ToList();
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public override string GetString(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(int ordinal)
        {
            var obj = _dataEnumerator.Current;
            var val = Dynamic.InvokeGet(obj, _names[ordinal]);

            if (val is JArray)
            {
                JArray array = val;
                var arrayType = array[0].Type;
                if (arrayType == JTokenType.Integer)
                    return array.ToObject<List<int>>();
                if (arrayType == JTokenType.String)
                    return array.ToObject<List<string>>();
                if (arrayType == JTokenType.Boolean)
                    return array.ToObject<List<bool>>();
                if (arrayType == JTokenType.Object)
                    return array.ToObject<List<JObject>>();
                return "";
            }

            var value = val.Value;
            if (value == null)
            {
                // TODO (Dapper will attempt to cast this to whatever class later, but I don't know that class here)
                return val;
            }
            return val.Value; // "val" is a JToken, so use Value to get the underlying value
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int FieldCount
        {
            get
            {
                PopulateNames();
                return _names.Count;
            }
        }

        public override object this[int ordinal] => GetValue(ordinal);

        public override object this[string name] => throw new NotImplementedException();

        public override int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasRows
        {
            get { throw new NotImplementedException(); }
        }
        public override bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public override bool NextResult()
        {
            // only ever one "result set" in Couchbase, so this does nothing
            return false;
        }

        public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            // only one "result set" in Couchbase, so this does nothing
            return false;
        }


        public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            try
            {
                // the first result is always preloaded, to get meta data (field names, count of fields)
                // so no need to "move next" if that result is already loaded
                if (!_preloadProcessed)
                {
                    _preloadProcessed = true;
                    return true;
                }

                // Attempt to move to the next item asynchronously
                return await _dataEnumerator.MoveNextAsync(cancellationToken);
            }
            catch (Exception)
            {
                // Handle exceptions if necessary
                return false;
            }
        }

        [Obsolete("Don't use sync with Couchbase")]
        public override bool Read()
        {
            throw new NotImplementedException();
        }

        public override int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
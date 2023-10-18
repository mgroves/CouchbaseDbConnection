using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace CouchbaseDbConnection
{
    public class CouchbaseDbParameterCollection : DbParameterCollection
    {
        private readonly List<DbParameter> _parameters = new List<DbParameter>();
        public override object SyncRoot { get; }

        public override int Add(object value)
        {
            if (value is DbParameter dbParameter)
            {
                _parameters.Add(dbParameter);
                return _parameters.Count - 1;
            }
            throw new ArgumentException("Value must be a DbParameter", nameof(value));
        }

        public override void AddRange(Array values)
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override bool Contains(object value)
        {
            if (value is DbParameter dbParameter)
            {
                return _parameters.Contains(dbParameter);
            }
            throw new ArgumentException("Value must be a DbParameter", nameof(value));
        }

        public override int IndexOf(object value)
        {
            if (value is DbParameter dbParameter)
            {
                return _parameters.IndexOf(dbParameter);
            }
            throw new ArgumentException("Value must be a DbParameter", nameof(value));
        }

        public override void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public override void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAt(string parameterName)
        {
            throw new NotImplementedException();
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            if (index < 0 || index >= _parameters.Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            _parameters[index] = value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            int index = _parameters.FindIndex(p => p.ParameterName == parameterName);

            if (index >= 0)
            {
                _parameters[index] = value;
            }
            else
            {
                throw new ArgumentException($"No parameter found with name {parameterName}", nameof(parameterName));
            }
        }

        public override int Count
        {
            get { throw new NotImplementedException(); }
        }

        public override int IndexOf(string parameterName)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(string value)
        {
            throw new NotImplementedException();
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        protected override DbParameter GetParameter(int index)
        {
            if (index < 0 || index >= _parameters.Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            return _parameters[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            DbParameter parameter = _parameters.Find(p => p.ParameterName == parameterName);
            if (parameter != null)
            {
                return parameter;
            }
            throw new ArgumentException($"No parameter found with name {parameterName}", nameof(parameterName));
        }
    }
}
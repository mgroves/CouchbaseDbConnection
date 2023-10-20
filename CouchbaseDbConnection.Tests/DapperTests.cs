using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Couchbase;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CouchbaseDbConnection.Tests
{
    [TestFixture]
    public class DapperTests
    {
        private ICluster _cluster;
        private IDbConnection _db;
        private Random _rand;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _cluster = await Cluster.ConnectAsync("couchbase://localhost", options =>
            {
                options.UserName = "Administrator";
                options.Password = "password";
            });
            _db = new CouchbaseDbConnection(_cluster);
            _rand = new Random();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _cluster.DisposeAsync();
        }

        [Test]
        public async Task DapperSelect()
        {
            var result = await _db.QueryAsync<UserProfile>("SELECT 'Matt' AS name, 13 AS shoeSize");

            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].Name, Is.EqualTo("Matt"));
        }

        [Test]
        public async Task DapperSelectWithParameters()
        {
            // TODO: must be data in collection for this test to work

            var result = await _db.QueryAsync<UserProfile>(
                "SELECT f.* FROM userprofile._default._default f WHERE f.name == $name",
                new { name = "Matt"});

            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].Name, Is.EqualTo("Matt"));
        }

        [Test]
        public async Task DapperMutation()
        {
            var key = Path.GetRandomFileName();
            var user = new UserProfile
            {
                Name = $"{Path.GetRandomFileName()} {Path.GetRandomFileName()}",
                ShoeSize = _rand.Next(1, 17)
            };

            var numMutations = await _db.ExecuteAsync(@"INSERT INTO userprofile._default._default (KEY,VALUE) VALUES ($key, {
                                   ""name"" : $name, 
                                   ""shoeSize"" : $shoeSize })",
                new { key = key, name = user.Name, shoeSize = user.ShoeSize});

            Assert.That(numMutations, Is.EqualTo(1));
        }

        [Test]
        public async Task DapperExecuteScalar()
        {
            // TODO: must be data in collection for this test to work

            var countUsers = await _db.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM userprofile._default._default");

            Assert.That(countUsers, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task DapperExecuteScalarValue()
        {
            // TODO: must be data in collection for this test to work

            var countUsers = await _db.ExecuteScalarAsync<int>(@"SELECT VALUE COUNT(*) FROM userprofile._default._default");

            Assert.That(countUsers, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task DapperExecuteScalarRaw()
        {
            // TODO: must be data in collection for this test to work

            var countUsers = await _db.ExecuteScalarAsync<int>(@"SELECT RAW COUNT(*) FROM userprofile._default._default");

            Assert.That(countUsers, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task DapperWithNestedJObject()
        {
            //SqlMapper.AddTypeHandler(new NestedObjectTypeHandler());
            var result = await _db.QueryAsync<UserProfileWithJObjectNested>("SELECT 'Matt' AS name, 13 AS shoeSize, { \"foo\" : \"bar\", \"baz\" : \"qux\"} AS MyNested");

            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].Name, Is.EqualTo("Matt"));
            Assert.That(list[0].MyNested["foo"].Value<string>(), Is.EqualTo("bar"));
            Assert.That(list[0].MyNestedCsharp.Foo, Is.EqualTo("bar"));
        }

        [Test]
        public async Task DapperWithNoResults()
        {
            var result = await _db.QueryAsync<UserProfile>("SELECT f.* FROM userprofile._default._default f WHERE 1 == 2");

            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task DapperWithPrimitiveArrayInt()
        {
            var result = await _db.QueryAsync<UserProfileWithPrimitiveArrayInt>("SELECT 'Matt' AS name, 13 AS shoeSize, [1,2,3,4,5] AS AnArray");
        
            var list = result.ToList();
        
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].Name, Is.EqualTo("Matt"));
            Assert.That(list[0].AnArray, Is.Not.EqualTo(null));
            Assert.That(list[0].AnArray[0], Is.EqualTo(1));
        }

        [Test]
        public async Task DapperWithPrimitiveArrayString()
        {
            var result = await _db.QueryAsync<UserProfileWithPrimitiveArrayString>(@"SELECT 'Matt' AS name, 13 AS shoeSize, [""a"",""b"",""c""] AS AnArray");

            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].Name, Is.EqualTo("Matt"));
            Assert.That(list[0].AnArray, Is.Not.EqualTo(null));
            Assert.That(list[0].AnArray[0], Is.EqualTo("a"));
        }

        [Test]
        public async Task DapperWithArrayOfObjects()
        {
            var result = await _db.QueryAsync<UserProfileWithArrayObjects>(@"SELECT 'Matt' AS name, 13 AS shoeSize, [{""foo"" : ""bar1"", ""baz"" : ""qux1""}, {""foo"" : ""bar2"", ""baz"" : ""qux2""}] AS AnArray");

            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].Name, Is.EqualTo("Matt"));
            Assert.That(list[0].AnArray, Is.Not.EqualTo(null));
            Assert.That(list[0].AnArray.Count, Is.EqualTo(2));
            Assert.That(list[0].AnArrayCsharp[1].Foo, Is.EqualTo("bar2"));
        }
    }

    public class UserProfile
    {
        public string Name { get; set; }
        public int ShoeSize { get; set; }
    }

    public class UserProfileWithPrimitiveArrayInt
    {
        public string Name { get; set; }
        public int ShoeSize { get; set; }
        public List<int> AnArray { get; set; }
    }

    public class UserProfileWithPrimitiveArrayString
    {
        public string Name { get; set; }
        public int ShoeSize { get; set; }
        public List<string> AnArray { get; set; }
    }
    
    public class UserProfileWithArrayObjects
    {
        public string Name { get; set; }
        public int ShoeSize { get; set; }
        public List<JObject> AnArray { get; set; }
        public List<NestedObject> AnArrayCsharp => AnArray.Select(a => a.ToObject<NestedObject>()).ToList();
    }

    public class UserProfileWithJObjectNested
    {
        public string Name { get; set; }
        public int ShoeSize { get; set; }
        public JObject MyNested { get; set; }

        public NestedObject MyNestedCsharp => MyNested.ToObject<NestedObject>();
    }

    public class NestedObject
    {
        public string Foo { get; set; }
        public string Baz { get; set; }
    }

    // This approach can be used instead of JObject for nested objects
    // public class NestedObjectTypeHandler : SqlMapper.TypeHandler<NestedObject>
    // {
    //     public override NestedObject Parse(object value)
    //     {
    //         return JsonConvert.DeserializeObject<NestedObject>(value.ToString());
    //     }
    //
    //     public override void SetValue(IDbDataParameter parameter, NestedObject value)
    //     {
    //         parameter.Value = JsonConvert.SerializeObject(value);
    //     }
    // }
}

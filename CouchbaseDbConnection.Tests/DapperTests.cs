using System;
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
            var result = await _db.QueryAsync<UserProfile>("SELECT f.* FROM userprofile._default._default f WHERE f.name == 'Matt' LIMIT 2");

            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].Name, Is.EqualTo("Matt"));
        }

        [Test]
        public async Task DapperSelectWithParameters()
        {
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
            var countUsers = await _db.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM userprofile._default._default");

            Assert.That(countUsers, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task DapperExecuteScalarValue()
        {
            var countUsers = await _db.ExecuteScalarAsync<int>(@"SELECT VALUE COUNT(*) FROM userprofile._default._default");

            Assert.That(countUsers, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task DapperExecuteScalarRaw()
        {
            var countUsers = await _db.ExecuteScalarAsync<int>(@"SELECT RAW COUNT(*) FROM userprofile._default._default");

            Assert.That(countUsers, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task DapperWithNestedObject()
        {
            //SqlMapper.AddTypeHandler(new NestedObjectTypeHandler());
            var result = await _db.QueryAsync<UserProfileWithNested>("SELECT f.*, { \"foo\" : \"bar\", \"baz\" : \"qux\"} AS MyNested FROM userprofile._default._default f WHERE f.name == 'Matt'");

            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].Name, Is.EqualTo("Matt"));
            Assert.That(list[0].MyNested["foo"].Value<string>(), Is.EqualTo("bar"));
            Assert.That(list[0].MyNestedCsharp.Foo, Is.EqualTo("bar"));
        }
    }

    public class UserProfile
    {
        public string Name { get; set; }
        public int ShoeSize { get; set; }
    }

    public class UserProfileWithNested
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

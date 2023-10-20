# CouchbaseDbConnection

This is a [DbConnection](https://learn.microsoft.com/en-us/dotnet/api/system.data.common.dbconnection) implementation for use with Couchbase (Server or Capella).

## Why?

My primary goal was to see how well I could get [Dapper](https://github.com/DapperLib/Dapper) working with [Couchbase](https://couchbase.com/). Why? Because a) I like Dapper a lot, b) I'd like to help other people who also like Dapper to try Couchbase with minimal effort.

## What works

When in doubt, check out the tests to see what works.

QueryAsync, ExecuteAsync, ScalarAsync, they all work perfectly fine with primitive values.

## What doesn't work

* ACID Transactions. Couchbase has ACID transactions, but the API doesn't line up very well with the way transactions are implemented.

* Some date/time stuff. Using C#'s DateTime with JSON strings that contain date/time works fine. DateTimeOffset doesn't seem to work. UNIX timestamp doesn't seem to work.

* Anything with multiple result sets. Couchbase doesn't support this yet, so it doesn't work.

* Nested Objects and Arrays of Objects. I can't quite crack this nut. There is a workaround: use JObject and a "shadow" property in C#. It's a little clunky, but it works. You could also just use JObject directly.

* Non-async. Couchbase .NET SDK is an almost entirely asynchronous library. So, you can't use Query<T> with Dapper, you have to use QueryAsync<T>

I haven't given up hope on these things, but it might be more trouble that it's worth. Let me know in an issue or contact me if you have ideas or really need one of these to work.

## What's next

There's a bunch of stuff that's not implemented in CouchbaseDbConnection and the various supporting classes: mainly because my tests with Dapper haven't run into those areas. If you plan to use this with anything except Dapper, please let me know.

I might keep tinkering with this as I get more ideas, or if anyone out there expresses any interest.

After that: writing a better set of tests, creating CI, and publishing a NuGet package.
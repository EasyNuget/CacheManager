namespace CacheManagerLoadTest;

internal static class StaticData
{
	internal const int NumberOfTasks = 1000;
	internal const string Api = "http://mock-api.com/data";
	internal const string SqlImage = "mcr.microsoft.com/mssql/server:2019-latest";
	internal const string RedisImage = "redis:latest";
	internal const string DockerEndPoint = "npipe://./pipe/docker_engine";
	internal const string DatabaseName = "master";
	internal const string DbUser = "sa";
	internal const string DbPassword = "YourStrong!Passw0rd";
	internal const int SqlPort = 1433;
	internal const int RedisPort = 6379;
	internal const string Key = "test_key";
	internal const string Value = "test_value";
	internal const string QueryToSelect = "SELECT TOP 1 [value] FROM [cache_table] WHERE [key] = @key";
	internal const string QueryToInsert = $"INSERT INTO cache_table ([key], [value]) VALUES ('{Key}', '{Value}');";
	internal const string QueryToDelete = $"DELETE FROM cache_table WHERE [key] = '{Key}'";

	internal const string SqlConnectionOnRunCi =
		$"Server=localhost;Database={DatabaseName};Persist Security Info=True;User ID={DbUser};Password={DbPassword};MultipleActiveResultSets=True;TrustServerCertificate=True;Max Pool Size=500;Application Name=Test";

	internal const string RedisConnectionOnRunCi = "localhost:6379";

	internal const string QueryToCreateTable = """
	                                           IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'cache_table')
	                                           BEGIN
	                                           CREATE TABLE cache_table (
	                                           [key] NVARCHAR(100) PRIMARY KEY,
	                                           [value] NVARCHAR(MAX)
	                                           );
	                                           END
	                                           """;
}

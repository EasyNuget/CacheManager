using CacheManager;
using CacheManager.Config;
using CacheManager.Database;
using CacheManager.Database.Config;
using CacheManager.Redis;
using CacheManager.Redis.Config;
using Dapper;
using DotNet.Testcontainers.Builders;
using Flurl.Http.Testing;
using Microsoft.Data.SqlClient;
using StackExchange.Redis;
using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace CacheManagerIntegrationTest;

public class EasyCacheManagerTests : IAsyncLifetime
{
	private MsSqlContainer _sqlContainer = null!;
	private RedisContainer _redisContainer = null!;
	private IConnectionMultiplexer _redisConnection = null!;
	private SqlConnection _sqlConnection = null!;
	private EasyCacheManager _easyCacheManager = null!;

	public async Task InitializeAsync()
	{
		string sqlConnectionString;
		string redisConnectionString;

		// Check if we're in GitHub Actions or local development
		var isCi = Environment.GetEnvironmentVariable("CI") == "true";

		if (isCi)
		{
			// GitHub Actions: Use Docker containers defined in the workflow
			sqlConnectionString = StaticData.SqlConnectionOnRunCi;
			redisConnectionString = StaticData.RedisConnectionOnRunCi;
		}
		else
		{
			var sqlTask = Task.Run(async () =>
			{
				_sqlContainer = new MsSqlBuilder()
					.WithImage(StaticData.SqlImage)
					.WithDockerEndpoint(StaticData.DockerEndPoint)
					.WithPassword(StaticData.DbPassword)
					.WithAutoRemove(true)
					.WithCleanUp(true)
					.WithEnvironment("ACCEPT_EULA", "Y")
					.WithEnvironment("SA_PASSWORD", StaticData.DbPassword)
					.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(StaticData.SqlPort))
					.Build();

				await _sqlContainer.StartAsync().ConfigureAwait(false);
			});

			var redisTask = Task.Run(async () =>
			{
				_redisContainer = new RedisBuilder()
					.WithImage(StaticData.RedisImage)
					.WithDockerEndpoint(StaticData.DockerEndPoint)
					.WithAutoRemove(true)
					.WithCleanUp(true)
					.WithPortBinding(StaticData.RedisPort, true)
					.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(StaticData.RedisPort))
					.Build();

				await _redisContainer.StartAsync().ConfigureAwait(false);
			});

			await Task.WhenAll(sqlTask, redisTask).ConfigureAwait(false);

			sqlConnectionString = _sqlContainer.GetConnectionString();
			redisConnectionString = _redisContainer.GetConnectionString();
		}

		// Initialize Redis Connection
		_redisConnection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString).ConfigureAwait(false);

		// Initialize SQL Connection
		_sqlConnection = new SqlConnection(sqlConnectionString);
		await _sqlConnection.OpenAsync().ConfigureAwait(false);

		// Ensure database setup
		await _sqlConnection.ExecuteAsync(StaticData.QueryToCreateTable).ConfigureAwait(false);

		// Initialize Cache Manager with all sources
		_easyCacheManager = new CacheBuilder()
			.AddApi(new ApiConfig { Url = StaticData.Api })
			.AddRedis(new RedisConfig { ConnectionString = redisConnectionString })
			.AddDb(new DbConfig { ConnectionString = sqlConnectionString, Query = StaticData.QueryToSelect })
			.AddMemory(new MemoryConfig())
			.Build(new LockConfig());
	}

	public async Task DisposeAsync()
	{
		if (_sqlContainer is not null)
		{
			_sqlConnection.Dispose();

			await _sqlContainer.StopAsync().ConfigureAwait(false);
			await _sqlContainer.DisposeAsync().ConfigureAwait(false);
		}

		if (_redisContainer is not null)
		{
			_redisConnection.Dispose();

			await _redisContainer.StopAsync().ConfigureAwait(false);
			await _redisContainer.DisposeAsync().ConfigureAwait(false);
		}

		await _easyCacheManager.DisposeAsync().ConfigureAwait(false);
	}

	[Fact]
	public async Task GetAsync_ShouldReturnData_FromDbAndSaveToMemoryAndRedis()
	{
		// Arrange
		_ = await _sqlConnection.ExecuteAsync(StaticData.QueryToInsert).ConfigureAwait(true);

		// Act
		var result = await _easyCacheManager.GetAsync<string>(StaticData.Key).ConfigureAwait(true);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(StaticData.Value, result);

		// Clear
		_ = await _sqlConnection.ExecuteAsync(StaticData.QueryToDelete).ConfigureAwait(true);
	}

	[Fact]
	public async Task GetAsync_ShouldReturnData_FromApiAndSaveToMemoryAndRedis()
	{
		// Arrange
		using var httpTest = new HttpTest();

		_ = httpTest.ForCallsTo(StaticData.Api)
			.WithVerb(HttpMethod.Get)
			.RespondWithJson(StaticData.Value);

		// Act
		var result = await _easyCacheManager.GetAsync<string>(StaticData.Key).ConfigureAwait(true);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(StaticData.Value, result);
	}

	[Fact]
	public async Task SetAsync_ShouldStoreData_InAllCacheSources()
	{
		// Arrange

		// Act
		await _easyCacheManager.SetAsync(StaticData.Key, StaticData.Value).ConfigureAwait(true);

		var resultFromMemory = await _easyCacheManager.GetAsync<string>(StaticData.Key).ConfigureAwait(true);

		// Assert
		Assert.Equal(StaticData.Value, resultFromMemory);
	}
}

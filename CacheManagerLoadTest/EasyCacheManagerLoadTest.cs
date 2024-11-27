using System.Collections.Concurrent;
using CacheManager;
using CacheManager.Config;
using CacheManager.Database;
using CacheManager.Database.Config;
using CacheManager.Redis;
using CacheManager.Redis.Config;
using CacheManagerApi;
using CacheManagerApi.Config;
using Dapper;
using DotNet.Testcontainers.Builders;
using Flurl.Http.Testing;
using Microsoft.Data.SqlClient;
using StackExchange.Redis;
using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace CacheManagerLoadTest;

public class EasyCacheManagerLoadTest : IAsyncLifetime
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
		_ = await _sqlConnection.ExecuteAsync(StaticData.QueryToCreateTable).ConfigureAwait(false);

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
	}

	[Fact]
	public async Task LoadTest_GetAndSet_AllShouldBeValid()
	{
		const int NumberOfTasks = StaticData.NumberOfTasks;

		var tasks = new List<Task>();

		for (var i = 0; i < NumberOfTasks; i++)
		{
			tasks.Add(Task.Run(async () =>
			{
				var randomKey = Guid.NewGuid().ToString();

				await _easyCacheManager.SetAsync(randomKey, randomKey).ConfigureAwait(true);

				var response = await _easyCacheManager.GetAsync<string>(randomKey).ConfigureAwait(true);

				if (response != randomKey)
				{
					throw new InvalidOperationException("not valid response");
				}
			}));
		}

		await Task.WhenAll(tasks).ConfigureAwait(true);

		Assert.True(true, "All tasks completed successfully.");
	}

	[Fact]
	public async Task LoadTest_GetFromApi_AllShouldBeValid()
	{
		const int NumberOfTasks = StaticData.NumberOfTasks;

		var tasks = new List<Task>();

		for (var i = 0; i < NumberOfTasks; i++)
		{
			tasks.Add(Task.Run(async () =>
			{
				using var httpTest = new HttpTest();

				var randomKey = Guid.NewGuid().ToString();

				_ = httpTest.ForCallsTo(StaticData.Api)
					.WithVerb(HttpMethod.Get)
					.RespondWithJson(randomKey);

				var response = await _easyCacheManager.GetAsync<string>(randomKey).ConfigureAwait(true);

				if (response != randomKey)
				{
					throw new InvalidOperationException("not valid response");
				}
			}));
		}

		await Task.WhenAll(tasks).ConfigureAwait(true);

		Assert.True(true, "All tasks completed successfully.");
	}

	[Fact]
	public async Task LoadTest_GetFromApiJust50Percent_AllShouldBeValid()
	{
		const int NumberOfTasks = StaticData.NumberOfTasks;
		var taskAdd = new List<Task>();
		var taskGet = new List<Task>();
		var keys = new ConcurrentBag<string>();

		for (var i = 0; i < NumberOfTasks / 2; i++)
		{
			taskAdd.Add(Task.Run(async () =>
			{
				using var httpTest = new HttpTest();

				var randomKey = Guid.NewGuid().ToString();

				_ = httpTest.ForCallsTo(StaticData.Api)
					.WithVerb(HttpMethod.Get)
					.RespondWithJson(randomKey);

				var response = await _easyCacheManager.GetAsync<string>(randomKey).ConfigureAwait(true);

				if (response != randomKey)
				{
					throw new InvalidOperationException("not valid response");
				}

				keys.Add(randomKey);
			}));
		}

		await Task.WhenAll(taskAdd).ConfigureAwait(true);

		for (var i = 0; i < NumberOfTasks / 2; i++)
		{
			taskGet.Add(Task.Run(async () =>
			{
				using var httpTest = new HttpTest();

				if (keys.TryTake(out var randomKey))
				{
					_ = httpTest.ForCallsTo(StaticData.Api)
						.WithVerb(HttpMethod.Get)
						.RespondWithJson(null);

					var response = await _easyCacheManager.GetAsync<string>(randomKey).ConfigureAwait(true);

					if (response != randomKey)
					{
						throw new InvalidOperationException("not valid response");
					}
				}
				else
				{
					throw new InvalidOperationException("not valid key");
				}
			}));
		}

		await Task.WhenAll(taskGet).ConfigureAwait(true);

		Assert.True(true, "All tasks completed successfully.");
	}
}

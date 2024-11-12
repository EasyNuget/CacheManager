using CacheManager;
using CacheManager.Config;
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
    private EasyCacheManager<string> _easyCacheManager = null!;


    public async Task InitializeAsync()
    {
        // Set up SQL Server container
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

        // Set up Redis container
        _redisContainer = new RedisBuilder()
            .WithImage(StaticData.RedisImage)
            .WithDockerEndpoint(StaticData.DockerEndPoint)
            .WithAutoRemove(true)
            .WithCleanUp(true)
            .WithPortBinding(StaticData.RedisPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(StaticData.RedisPort))
            .Build();

        await _sqlContainer.StartAsync();
        await _redisContainer.StartAsync();

        // Build connection strings
        var sqlConnectionString = _sqlContainer.GetConnectionString();
        var redisConnectionString = _redisContainer.GetConnectionString();

        // Initialize Redis Connection
        _redisConnection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);

        // Initialize SQL Connection
        _sqlConnection = new SqlConnection(sqlConnectionString);
        await _sqlConnection.OpenAsync();

        await _sqlConnection.ExecuteAsync(StaticData.QueryToCreateTable);

        _easyCacheManager = new CacheBuilder<string>()
            .AddApi(new ApiConfig
            {
                Url = StaticData.Api
            })
            .AddRedis(new RedisConfig
            {
                ConnectionString = redisConnectionString
            })
            .AddDb(new DbConfig
            {
                ConnectionString = sqlConnectionString,
                Query = StaticData.QueryToSelect
            })
            .AddMemory(new MemoryConfig())
            .Build(new LockConfig());
    }

    public async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
        await _sqlContainer.DisposeAsync();

        await _redisContainer.StopAsync();
        await _redisContainer.DisposeAsync();

        _redisConnection.Dispose();
        _sqlConnection.Dispose();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnData_FromDbAndSaveToMemoryAndRedis()
    {
        // Arrange
        await _sqlConnection.ExecuteAsync(StaticData.QueryToInsert);

        // Act
        var result = await _easyCacheManager.GetAsync(StaticData.Key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StaticData.Value, result);

        // Clear
        await _sqlConnection.ExecuteAsync(StaticData.QueryToDelete);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnData_FromApiAndSaveToMemoryAndRedis()
    {
        // Arrange
        using var httpTest = new HttpTest();

        httpTest.ForCallsTo(StaticData.Api)
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(StaticData.Value);

        // Act
        var result = await _easyCacheManager.GetAsync(StaticData.Key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StaticData.Value, result);
    }

    [Fact]
    public async Task SetAsync_ShouldStoreData_InAllCacheSources()
    {
        // Arrange

        // Act
        await _easyCacheManager.SetAsync(StaticData.Key, StaticData.Value);

        var resultFromMemory = await _easyCacheManager.GetAsync(StaticData.Key);

        // Assert
        Assert.Equal(StaticData.Value, resultFromMemory);
    }
}
using APIAlquilerVehiculos.Cache;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace APIAlquilerVehiculos.Test.Cache;

[TestFixture]
[TestOf(typeof(RedisCache))]
public class RedisCacheTest
{
    private RedisContainer _container = null!;
    private IConnectionMultiplexer _redis = null!;
    private RedisCache _cache = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
        await _container.StartAsync();

        _redis = await ConnectionMultiplexer.ConnectAsync(
            _container.GetConnectionString()
        );
    }

    [SetUp]
    public async Task Setup()
    {
        _cache = new RedisCache(_redis);
        await _cache.ClearAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _redis.DisposeAsync();
        await _container.DisposeAsync();
    }

    public class CacheTestDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public CacheTestDto CreateCacheTestDto() => new CacheTestDto
    {
        Id = 1,
        Nombre = "Prueba"
    };
    
    //GET
    [Test]
    public async Task GetAsyncTest_ExistingKey_ValueReturned()
    {
        //Arrange
        string key = "TEST:GET";
        await _cache.SetAsync(key, "valor guardado");
        
        //Act
        var response = await _cache.GetAsync<string>(key);
        
        //Assert
        Assert.That(response, Is.EqualTo("valor guardado"));

    }
    [Test]
    public async Task GetTAsync_NonExistingKey_NullReturned()
    {
        // Arrange
        string key = "TEST:NON-EXISTING";

        // Act
        var response = await _cache.GetAsync<string>(key);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task GetTAsync_ExpiredKey_NullReturned()
    {
        // Arrange
        string key = "TEST:EXPIRED";

        await _cache.SetAsync(
            key,
            "Valor temporal",
            TimeSpan.FromMilliseconds(100)
        );

        // Act
        await Task.Delay(300);
        var response = await _cache.GetAsync<string>(key);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task GetTAsync_InvalidJson_JsonException()
    {
        // Arrange
        string key = "TEST:INVALID-JSON";

        await _redis.GetDatabase()
            .StringSetAsync(key, "esto no es json");

        // Act
        Assert.ThrowsAsync<System.Text.Json.JsonException>(async () =>
            await _cache.GetAsync<CacheTestDto>(key)
        );

        // Assert
    }

    
    // REMOVE
    [Test]
    public async Task RemoveAsync_ExistingKey_KeyRemoved()
    {
        // Arrange
        string key = "TEST:REMOVE";

        await _cache.SetAsync(key, "Valor");

        // Act
        await _cache.RemoveAync(key);
        

        var response = await _cache.GetAsync<string>(key);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task RemoveAsync_NonExistingKey_NoException()
    {
        // Arrange
        string key = "TEST:REMOVE-NON-EXISTING";

        // Act
        await _cache.RemoveAync(key);

        // Assert
        var response = await _cache.GetAsync<string>(key);

        Assert.That(response, Is.Null);
    }


    // CLEAR
    [Test]
    public async Task ClearAsync_ExistingKeys_AllKeysRemoved()
    {
        // Arrange
        await _cache.SetAsync("TEST:KEY1", "Valor 1");
        await _cache.SetAsync("TEST:KEY2", "Valor 2");
        await _cache.SetAsync("TEST:KEY3", "Valor 3");

        // Act
        await _cache.ClearAsync();

        // Assert
        Assert.That(
            await _cache.GetAsync<string>("TEST:KEY1"),
            Is.Null
        );

        Assert.That(
            await _cache.GetAsync<string>("TEST:KEY2"),
            Is.Null
        );

        Assert.That(
            await _cache.GetAsync<string>("TEST:KEY3"),
            Is.Null
        );
    }

    [Test]
    public async Task ClearAsync_EmptyCache_NoException()
    {
        // Arrange
        // Act
        await _cache.ClearAsync();

        // Assert
        var keys = _redis
            .GetServer(_redis.GetEndPoints().First())
            .Keys(pattern: "*")
            .ToList();

        Assert.That(keys, Is.Empty);
    }
    


}
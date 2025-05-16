using Newtonsoft.Json;
using StackExchange.Redis;

public interface IRedisRepository
{
    Task StoreAsync(string key, IEnumerable<FileRecord> records);
}

public class RedisRepository : IRedisRepository
{
    private readonly IDatabase _db;
    public RedisRepository(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    public async Task StoreAsync(string key, IEnumerable<FileRecord> records)
    {
        var json = JsonConvert.SerializeObject(records);
        await _db.StringSetAsync(key, json);
    }
}

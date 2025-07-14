using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Caching.RedisWorker
{
    /// <summary>
    /// Create By: CuongLv
    /// Guide: https://tutexchange.com/using-redis-cache-with-asp-net-core-3-1-using-stackexchange-redis-extensions-core-extensions/
    /// https://stackexchange.github.io/StackExchange.Redis/Configuration.html
    /// </summary>
    public class RedisConn
    {
        private readonly string _redisHost;
        private readonly int _redisPort;
        // private readonly int _db_index;        

        private ConnectionMultiplexer _redis;
        public RedisConn(IConfiguration config)
        {
            _redisHost = config["Redis:Host"];
            _redisPort = Convert.ToInt32(config["Redis:Port"]);
            // _db_index = Convert.ToInt32(config["Redis:Database:db_product"]);            
        }

        public void Connect()
        {
            try
            {
                var configString = $"{_redisHost}:{_redisPort},connectRetry=5,allowAdmin=true";
                _redis = ConnectionMultiplexer.Connect(configString);
            }
            catch (RedisConnectionException err)
            {

               // throw err;
            }
            // Log.Debug("Connected to Redis");
        }

        public void Set(string key, string value, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            db.StringSet(key, value);
        }
        public void Set(string key, string value, DateTime expires, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            var expiryTimeSpan = expires.Subtract(DateTime.Now);

            db.StringSet(key, value, expiryTimeSpan);
        }

        public async Task<string> GetAsync(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return await db.StringGetAsync(key);
        }
        public string Get(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return db.StringGet(key);
        }

        public string GetNoAsync(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return db.StringGet(key);
        }

        public async void clear(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            await db.KeyDeleteAsync(key);
        }
        public async void FlushDatabaseByIndex( int db_index)
        {
            await _redis.GetServer(_redisHost,_redisPort).FlushDatabaseAsync(db_index);
        }
       
        public async Task DeleteCacheByKeyword(string keyword, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            var server = _redis.GetServer(_redisHost, _redisPort);
            var keys = server.Keys(db_index, pattern: "*" + keyword + "*").ToList();
            foreach (var key in keys)
            {
                try
                {
                    await db.KeyDeleteAsync(key);
                }
                catch { }
            }
        }
    }
}

using StackExchange.Redis;
using System.Configuration;
namespace HuloToys_Service.RedisWorker
{
    public class RedisConn
    {
        private readonly string _redisHost;
        private readonly int _redisPort;
        // private readonly int _db_index;        
        //Redis_Host
        private ConnectionMultiplexer _redis;
        public RedisConn()
        {
            _redisHost = ConfigurationManager.AppSettings["Redis_Host"];
            _redisPort = Convert.ToInt32(ConfigurationManager.AppSettings["Redis_Port"]);
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

                throw err;
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
    }
}

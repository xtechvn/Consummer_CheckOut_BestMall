using APP_CHECKOUT.Model.Orders;
using APP_CHECKOUT.Models.Orders;
using MongoDB.Driver;
using System.Configuration;
using System.Reflection;

namespace APP_CHECKOUT.MongoDb
{
    public class CartMongodbService
    {
        private IMongoCollection<CartItemMongoDbModel> bookingCollection;

        public CartMongodbService() {

            //      "connection_string": "mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=Adavigo"
            string _connection = "mongodb://" + ConfigurationManager.AppSettings["Mongo_usr"]
                 + ":" + ConfigurationManager.AppSettings["Mongo_pwd"]
                 + "@" + ConfigurationManager.AppSettings["Mongo_Host"]
                 + ":" + ConfigurationManager.AppSettings["Mongo_Port"]
                 + "/?authSource=" + ConfigurationManager.AppSettings["Mongo_catalog"];
            var booking = new MongoClient(_connection);
            IMongoDatabase db = booking.GetDatabase(ConfigurationManager.AppSettings["Mongo_catalog"]);
            bookingCollection = db.GetCollection<CartItemMongoDbModel>("Cart");
        }
        public async Task<string> Insert(CartItemMongoDbModel item)
        {
            try
            {
                item.GenID();
                await bookingCollection.InsertOneAsync(item);
                return item._id;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                //LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;

        }
        public async Task<string> UpdateCartQuanity(CartItemMongoDbModel data)
        {
            try
            {
                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x._id, data._id);
                var update  = Builders<CartItemMongoDbModel>.Update
                    .Set(x => x.created_date, data.created_date)
                    .Set(x => x.quanity, data.quanity);
                var updated_item =await bookingCollection.UpdateOneAsync(filterDefinition, update);

                return data._id;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                //LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;

        }
        public async Task<CartItemMongoDbModel> FindById(string id)
        {
            try
            {
                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x._id, id);

                var model = await bookingCollection.Find(filterDefinition).ToListAsync();
                if (model != null)
                {
                    return model.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                //LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;

        }
        public async Task<CartItemMongoDbModel> FindByProductId(string product_id,long account_client_id)
        {
            try
            {
                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x.account_client_id, account_client_id);
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x.product._id, product_id);

                var model = await bookingCollection.Find(filterDefinition).ToListAsync();
                if (model != null)
                {
                    return model.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                //LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;

        }
        public async Task<int> Count(long client_id)
        {
            try
            {

                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x.account_client_id, client_id);

                var model = await bookingCollection.Find(filterDefinition).ToListAsync();
                if (model != null)
                {
                    return model.Count;
                }
                   
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                //LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return 0;
        }
        public async Task<List<CartItemMongoDbModel>> GetList(long client_id)
        {
            try
            {

                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x.account_client_id, client_id);

                var model = await bookingCollection.Find(filterDefinition).ToListAsync();
                if (model != null)
                {
                    return model;
                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                //LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public async Task<bool> Delete(string id)
        {
            try
            {

                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x._id, id);

                var model = await bookingCollection.DeleteOneAsync(filterDefinition);
                return model.DeletedCount > 0;

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                //LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return false;
        }
    }
}

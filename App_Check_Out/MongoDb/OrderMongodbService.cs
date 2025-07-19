using APP_CHECKOUT.Models.Orders;
using MongoDB.Driver;
using Nest;
using System.Configuration;

namespace APP_CHECKOUT.MongoDb
{
    public class OrderMongodbService
    {
        private IMongoCollection<OrderDetailMongoDbModel> bookingCollection;

        public OrderMongodbService() {

            //      "connection_string": "mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=Adavigo"
            string _connection = "mongodb://" + ConfigurationManager.AppSettings["Mongo_usr"]
                 + ":" + ConfigurationManager.AppSettings["Mongo_pwd"]
                 + "@" + ConfigurationManager.AppSettings["Mongo_Host"]
                 + ":" + ConfigurationManager.AppSettings["Mongo_Port"]
                 + "/?authSource=" + ConfigurationManager.AppSettings["Mongo_catalog"];
            var booking = new MongoClient(_connection);
            IMongoDatabase db = booking.GetDatabase(ConfigurationManager.AppSettings["Mongo_catalog"]);
            bookingCollection = db.GetCollection<OrderDetailMongoDbModel>("Orders");
        }
        public async Task<string> Insert(OrderDetailMongoDbModel item)
        {
            try
            {
                item.GenID();
                await bookingCollection.InsertOneAsync(item);
                return item._id;
            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;

        }
      
        public async Task<OrderDetailMongoDbModel> FindById(string id)
        {
            try
            {
                var filter = Builders<OrderDetailMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<OrderDetailMongoDbModel>.Filter.Eq(x => x._id, id);

                var model = await bookingCollection.Find(filterDefinition).ToListAsync();
                if (model != null)
                {
                    return model.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("rderMongodbService - FindById[" + id + "][" + ex.ToString() + "]");

            }
            return null;

        }
        public async Task<string> Update(OrderDetailMongoDbModel model)
        {
            try
            {
                var filter = Builders<OrderDetailMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<OrderDetailMongoDbModel>.Filter.Eq(x => x._id, model._id);

                await bookingCollection.ReplaceOneAsync(filterDefinition, model);
                return model._id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("rderMongodbService - Update[" + model._id + "][" + ex.ToString() + "]");
            }
            return null;

        }
        public async Task<bool> Delete(string id)
        {
            try
            {

                var filter = Builders<OrderDetailMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<OrderDetailMongoDbModel>.Filter.Eq(x => x._id, id);

                var model = await bookingCollection.DeleteOneAsync(filterDefinition);
                return model.DeletedCount > 0;

            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return false;
        }
    }
}

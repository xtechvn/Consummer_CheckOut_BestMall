using Elasticsearch.Net;
using Nest;
using System.Reflection;
using Entities.Models;
using APP_CHECKOUT.Elasticsearch;

namespace Caching.Elasticsearch
{
    public class AddressClientESService : ESRepository<AddressClientESModel>
    {
        public string index = "hulotoys_sp_getaddressclient";
        private static string _ElasticHost;

        public AddressClientESService(string Host) : base(Host)
        {
            _ElasticHost = Host;


        }
        public List<AddressClientESModel> GetByClientID(long client_id)
        {
            List<AddressClientESModel> result = new List<AddressClientESModel>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<AddressClientESModel>(sd => sd
                            .Index(index)
                            .Query(q => q.Term(m => m.ClientId,client_id)
                                )
                            .Size(100)

                            );
                if (query.IsValid)
                {
                    var data = query.Documents as List<AddressClientESModel>;
                    //LogHelper.InsertLogTelegram( "GetByClientID - AddressClientESService [" + client_id + "][" + JsonConvert.SerializeObject(result) + "]");

                    return data;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
            }
            return null;
        }
        public AddressClientESModel GetById(long id,long client_id)
        {
            AddressClientESModel result = new AddressClientESModel();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<AddressClientESModel>(sd => sd
                            .Index(index)
                            .Query(q => q.Term(m => m.Id, id) 
                                &&
                                q.Term(m => m.ClientId, client_id)
                                )
                            .Size(100)

                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {

                    var data = query.Documents as List<AddressClientESModel>;
                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = "AddressClientESService" + "->" + "InsertOrUpdateAddress" + "=>" + ex.ToString();
            }
            return null;
        }
    }
}

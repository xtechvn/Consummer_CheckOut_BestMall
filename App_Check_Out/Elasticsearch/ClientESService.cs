using Elasticsearch.Net;
using Nest;
using System.Reflection;
using Newtonsoft.Json;
using APP_CHECKOUT.Elasticsearch;
using APP_CHECKOUT.Models.Client;

namespace Caching.Elasticsearch
{
    public class ClientESService : ESRepository<ClientESModel>
    {
        public string index = "hulotoys_sp_getclient";
        private static string _ElasticHost;

        public ClientESService(string Host) : base(Host) {
            _ElasticHost = Host;

        }
        public ClientESModel GetById(long id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<ClientESModel>(sd => sd
                               .Index(index)
                               .Query(q => q
                                   .Term("Id", id)
                               ));

                if (query.IsValid)
                {
                    var result = query.Documents as List<ClientESModel>;
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
            }
            return null;
        }
        public List<ClientESModel> GetByEmail(string email)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<ClientESModel>(sd => sd
                               .Index(index)
                                .Query(q => q
                                .MatchPhrase(m => m
                                .Field(f => f.Email)
                                .Query(email))));
               
                if (query.IsValid)
                {
                    var result = query.Documents as List<ClientESModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
            }
            return null;
        }
    }
}

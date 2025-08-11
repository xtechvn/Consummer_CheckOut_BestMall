using Elasticsearch.Net;
using Nest;
using System.Reflection;
using APP_CHECKOUT.Models.Account;
using APP_CHECKOUT.Elasticsearch;
using Microsoft.Extensions.Configuration;

namespace Caching.Elasticsearch
{
    public class AccountClientESService : ESRepository<AccountESModel>
    {
        public string index = "hulotoys_sp_getaccountclient";
        private static string _ElasticHost;

        public AccountClientESService(string Host) : base(Host)
        {
            _ElasticHost = Host;

        }
        public AccountESModel GetByUsername(string user_name)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                               .Query(q => q
                                   .Match(m => m.Field(y=>y.UserName).Query(user_name)
                               )));

                if (query.IsValid)
                {
                    var result = query.Documents as List<AccountESModel>;
                   // LogHelper.InsertLogTelegram( "GetByUsername - AccountClientESService ["+user_name+"]["+ JsonConvert.SerializeObject(result) + "]" );
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
            }
            return null;
        }
      
        public AccountESModel GetById(long id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                               .Query(q => q
                                   .Term(m => m.Id, id)
                               ));

                if (query.IsValid)
                {
                    var result = query.Documents as List<AccountESModel>;
                    //LogHelper.InsertLogTelegram( "GetById - AccountClientESService [" + id + "][" + JsonConvert.SerializeObject(result) + "]");
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
            }
            return null;
        }
        public AccountESModel GetByUsernameAndPassword(string user_name, string password)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                             .Query(q =>
                               q.Bool(
                                   qb => qb.Must( q=>
                                      q.Match(qs => qs
                                        .Field(s => s.UserName)
                                        .Query(user_name)
                                       )
                                     && q.Match(qs => qs
                                        .Field(s => s.Password)
                                        .Query(password)
                                       )

                                    )
                               )
                            ));


                if (query.IsValid)
                {
                    var data = query.Documents as List<AccountESModel>;

                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
            }
            return null;
        }
        public AccountESModel GetByUsernameAndGoogleToken(string user_name, string token)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                             .Query(q =>
                               q.Bool(
                                   qb => qb.Must(
                                      qb => qb.Match(qs => qs
                                        .Field(s => s.UserName)
                                        .Query(user_name)
                                       )

                                    )
                               )
                            ));


                if (query.IsValid)
                {
                    var data = query.Documents as List<AccountESModel>;

                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
            }
            return null;
        }
        public AccountESModel GetByClientIdAndPassword(long client_id, string password)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                             .Query(q =>
                               q.Bool(
                                   qb => qb.Must(
                                      qb => qb.Term(x=>x.ClientId, client_id) 
                                      &&
                                       qb.Match(qs => qs
                                        .Field(s => s.Password)
                                        .Query(password)
                                       )

                                    )
                               )
                            ));


                if (query.IsValid)
                {
                    var data = query.Documents as List<AccountESModel>;

                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
            }
            return null;
        }
        public AccountESModel GetByClientID(long client_id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);


                var query = elasticClient.Search<AccountESModel>(sd => sd
                             .Index(index)
                           .Query(q =>
                             q.Bool(
                                 qb => qb.Must(
                                    qb => qb.Term(x=>x.ClientId, client_id)


                                  )
                             )
                          ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<AccountESModel>;

                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
            }
            return null;
        }
    }
}

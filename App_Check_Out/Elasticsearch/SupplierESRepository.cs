using Entities.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;

namespace Caching.Elasticsearch.FlashSale
{
    
    public class SupplierESRepository
    {
        public string index = "hulotoys_sp_getsupplier";
        private static IConfiguration configuration;
        private readonly ElasticClient _client;
        public SupplierESRepository(string Host)
        {
            var settings = new ConnectionSettings(new Uri(configuration["DataBaseConfig:Elastic:Host"]))
               .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }

        public async Task<SupplierESModel> GetByIdAsync(int flashsaleId)
        {
            var response = await _client.SearchAsync<SupplierESModel>(s => s
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.supplierid)
                        .Value(flashsaleId)
                    )
                )
            );

            return response.Documents.FirstOrDefault();
        }
       
    }



}

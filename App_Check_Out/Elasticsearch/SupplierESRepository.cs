using Entities.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;
using System.Configuration;

namespace Caching.Elasticsearch.FlashSale
{
    
    public class SupplierESRepository
    {
        public string index = "hulotoys_sp_getsupplier";
        private readonly ElasticClient _client;
        public SupplierESRepository(string host)
        {
            var settings = new ConnectionSettings(new Uri(host))
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

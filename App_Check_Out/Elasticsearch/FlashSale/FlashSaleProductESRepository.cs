using APP_CHECKOUT.Elasticsearch;
using Nest;

namespace Caching.Elasticsearch.FlashSale
{

    public class FlashSaleProductESRepository : ESRepository<FlashSaleProductESModel>
    {
        public string index = "hulotoys_sp_getflashsaleproduct";
        private static string _ElasticHost;
        private readonly ElasticClient _client;

        public FlashSaleProductESRepository(string Host) : base(Host)
        {
            _ElasticHost = Host;
            var settings = new ConnectionSettings(new Uri(_ElasticHost))
                .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }

        // 1. Function tìm kiếm data theo flashsale_id
        public async Task<FlashSaleProductESModel> GetByIdAsync(long flashsale_productid)
        {
            var response = await _client.SearchAsync<FlashSaleProductESModel>(s => s
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.flashsale_productid)
                        .Value(flashsale_productid)
                    )
                )
            );

            return response.Documents.FirstOrDefault();
        }

        // 2. Function xóa theo flashsale_id
        public async Task<bool> DeleteByProductIdAsync(long flashsale_productid)
        {
            var response = await _client.DeleteByQueryAsync<FlashSaleProductESModel>(q => q
                .Query(rq => rq
                    .Term(t => t
                        .Field(f => f.flashsale_productid)
                        .Value(flashsale_productid)
                    )
                )
            );

            return response.IsValid && response.Deleted > 0;
        }

        // 3. Function insert vào index
        public async Task<bool> InsertAsync(FlashSaleProductESModel product)
        {
            var response = await _client.IndexDocumentAsync(product);
            return response.IsValid;
        }
        public async Task<List<FlashSaleProductESModel>> GetByFlashsaleId(int flashsale_id)
        {
            var now = DateTime.Now;

            var response = await _client.SearchAsync<FlashSaleProductESModel>(s => s
                 .Query(q => q
                     .Bool(b => b // Sử dụng Bool query để kết hợp nhiều điều kiện
                         .Must(
                             m => m.Term(t => t
                                 .Field(f => f.flashsale_id)
                                 .Value(flashsale_id)
                             ),
                             m => m.Term(t => t // Thêm điều kiện status = 1
                                 .Field(f => f.status) // Giả sử tên trường là "status"
                                 .Value(1)
                             )
                         )
                     )
                 ).Take(4000)
                .Size(4000)
             );

            if (response.IsValid)
            {
                return response.Documents.ToList();
            }
            else
            {
                return new List<FlashSaleProductESModel>();
            }
        }
        public async Task<List<FlashSaleProductESModel>> GetByListFlashsaleId(List<int> flashsale_ids)
        {
            var now = DateTime.Now;

            var response = await _client.SearchAsync<FlashSaleProductESModel>(s => s

                .Query(q => q
                    .Terms(t => t
                        .Field(f => f.flashsale_id)
                        .Terms(flashsale_ids)
                    )
                )
                .Take(4000)
                .Size(4000)

            );

            if (response.IsValid)
            {
                return response.Documents.ToList();
            }
            else
            {
                return new List<FlashSaleProductESModel>();
            }
        }
        public async Task<bool> DeleteByIds(List<long> flashsale_productids)
        {
            if (flashsale_productids == null || flashsale_productids.Count == 0)
            {
                return false;
            }

            var response = await _client.DeleteByQueryAsync<FlashSaleProductESModel>(q => q
                .Query(rq => rq
                    .Terms(t => t // Use .Terms for multiple values
                        .Field(f => f.flashsale_productid)
                        .Terms(flashsale_productids)
                    )
                )
            );

            if (response.IsValid)
            {
                return true;
            }
            return false;

        }
        public async Task<bool> IndexMany(List<FlashSaleProductESModel> flashSales)
        {
            if (flashSales == null || flashSales.Count == 0)
            {
                return false;
            }
            foreach (var item in flashSales)
            {
                await _client.IndexDocumentAsync(item);
            }
            return true;

        }
        public async Task<List<FlashSaleProductESModel>> GetListSuperSale(List<int> flashsale_ids)
        {
            var now = DateTime.Now;

            var response = await _client.SearchAsync<FlashSaleProductESModel>(s => s
                 .Query(q => q
                     .Bool(b => b // Sử dụng Bool query để kết hợp nhiều điều kiện
                         .Must(
                            m => m.Term(t => t // Điều kiện supersale = true
                                .Field(f => f.supersale)
                                .Value(true)
                            ),

                            m => m.Term(t => t // Điều kiện status = 1
                                .Field(f => f.status)
                                .Value(1)
                            ),

                            m => m.Terms(t => t // Sử dụng Terms query để tìm kiếm nhiều flashsale_id
                                .Field(f => f.flashsale_id)
                                .Terms(flashsale_ids) // Truyền danh sách ID vào đây
                            )

                         )
                     )
                 )
                .Size(20)
             );

            if (response.IsValid)
            {
                return response.Documents.ToList();
            }
            else
            {
                return new List<FlashSaleProductESModel>();
            }
        }
    }



}

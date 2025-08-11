using APP_CHECKOUT.Models.Models;
using APP_CHECKOUT.MongoDb;
using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Entities.ViewModels.Products;
using HuloToys_Service.Models.Flashsale;
using HuloToys_Service.Utilities.lib;
using Newtonsoft.Json;
using System.Configuration;
using System.Reflection;
using Utilities;

namespace HuloToys_Service.Controllers.Product.Bussiness
{
    public class ProductDetailService
    {

        private readonly ClientESService _clientESService;
        private readonly FlashSaleESRepository flashSaleESRepository;
        private readonly FlashSaleProductESRepository flashSaleProductESRepository;
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        public ProductDetailService(ClientESService clientESService, FlashSaleESRepository _flashSaleESRepository, FlashSaleProductESRepository _flashSaleProductESRepository, ProductDetailMongoAccess productDetailMongoAccess)
        {
            _clientESService = clientESService;
            flashSaleESRepository = _flashSaleESRepository;
            flashSaleProductESRepository = _flashSaleProductESRepository;
            _productDetailMongoAccess = productDetailMongoAccess;
        }
        public async Task<ProductListResponseFEModel> ProductListing(ProductListRequestModel request)
        {
            ProductListResponseFEModel result = new ProductListResponseFEModel();
            try
            {
                // Chuẩn hóa từ khóa tìm kiếm
                request.keyword = StringHelper.ValidateTextForSearch(request.keyword);


                var data = await _productDetailMongoAccess.ResponseListing(request.keyword, request.group_id, request.page_index, request.page_size, request.price_from, request.price_to, request.rating);
                if (data != null && data.items != null && data.items.Count > 0)
                {
                    result = JsonConvert.DeserializeObject<ProductListResponseFEModel>(JsonConvert.SerializeObject(data));
                    result.items= await UpdateProductDetail(data.items);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return result;
        }
        public async Task<ProductListResponseFEModel> ProductListingByLabelAndSupplier(ProductListByIdRequestModel request)
        {
            ProductListResponseFEModel result = new ProductListResponseFEModel();
            try
            {
                // Chuẩn hóa từ khóa tìm kiếm
                request.keyword = StringHelper.ValidateTextForSearch(request.keyword);


                var data = await _productDetailMongoAccess.ResponseListing(request.keyword, request.group_id, request.page_index, request.page_size, request.price_from, request.price_to, request.rating, request.supplier_id, request.label_id);
                result = JsonConvert.DeserializeObject<ProductListResponseFEModel>(JsonConvert.SerializeObject(data));
                if (result != null && result.items != null && result.items.Count > 0)
                {
                    result.items = await UpdateProductDetail(data.items);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return result;
        }
        public async Task<ProductMongoDbModelFEResponse> GetByID(string id)
        {
            ProductMongoDbModelFEResponse result = new ProductMongoDbModelFEResponse();
            try
            {
                var detail = await _productDetailMongoAccess.GetByID(id);
                if (detail != null)
                {
                    result = await UpdateProductDetail(detail);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return result;
        }
        public async Task<ProductDetailResponseModel> GetFullProductById(string id)
        {
            ProductDetailResponseModel result = new ProductDetailResponseModel();
            try
            {
                var data = await _productDetailMongoAccess.GetFullProductById(id);
                if (data != null && data.product_main != null && data.product_main._id.Trim() != "")
                {
                    result = JsonConvert.DeserializeObject<ProductDetailResponseModel>(JsonConvert.SerializeObject(data));
                    result.product_main=await UpdateProductDetail(result.product_main);
                    if(data.product_sub!=null && data.product_sub.Count > 0)
                    {
                        result.product_sub = await UpdateProductDetail(data.product_sub);
                    }
                    result.product_main.amount_min = result.product_sub.Min(x => (x.amount_after_flashsale != null && x.amount_after_flashsale > 0 ? x.amount_after_flashsale : x.amount));
                    result.product_main.amount_max = result.product_sub.Max(x => (x.amount_after_flashsale != null && x.amount_after_flashsale > 0 ? x.amount_after_flashsale : x.amount));

                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return result;
        }
        public async Task<ProductDetailResponseModel> UpdateFullProductById(ProductDetailResponseModel result)
        {
            try
            {
                result.product_main = await UpdateProductDetail(result.product_main);
                if (result.product_sub != null && result.product_sub.Count > 0)
                {
                    result.product_sub = await UpdateProductDetail(result.product_sub);
                }
                result.product_main.amount_min = result.product_sub.Min(x => (x.amount_after_flashsale != null && x.amount_after_flashsale > 0 ? x.amount_after_flashsale : x.amount));
                result.product_main.amount_max = result.product_sub.Max(x => (x.amount_after_flashsale != null && x.amount_after_flashsale > 0 ? x.amount_after_flashsale : x.amount));
              
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return result;
        }
        public async Task<List<ProductMongoDbModelFEResponse>> ListByProducts(List<string> ids)
        {
            List<ProductMongoDbModelFEResponse> result = new List<ProductMongoDbModelFEResponse>();
            try
            {
                var list_product_mongo = await _productDetailMongoAccess.ListByProducts(ids);
                if (list_product_mongo != null && list_product_mongo.Count > 0)
                {
                    result =  await UpdateProductDetail(list_product_mongo);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return result;
        }
        public async Task<ProductMongoDbModelFEResponse> UpdateProductDetail(ProductMongoDbModel product)
        {
            ProductMongoDbModelFEResponse item = new ProductMongoDbModelFEResponse();
            try
            {

                item = JsonConvert.DeserializeObject<ProductMongoDbModelFEResponse>(JsonConvert.SerializeObject(product));
                var active_flashsale = await flashSaleESRepository.SearchActiveFlashSales();
                List<FlashSaleProductESModel> list_item = new List<FlashSaleProductESModel>();
                if (active_flashsale != null && active_flashsale.Count > 0)
                {
                    list_item = await flashSaleProductESRepository.GetByListFlashsaleId(active_flashsale.Select(x => x.flashsale_id).ToList());
                    UpdateProductItem(item, active_flashsale, list_item);
                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return item;
        }

        public async Task<List<ProductMongoDbModelFEResponse>> UpdateProductDetail(List<ProductMongoDbModel> products_original)
        {
            List<ProductMongoDbModelFEResponse> products=new List<ProductMongoDbModelFEResponse> ();
            try
            {
                products=JsonConvert.DeserializeObject<List<ProductMongoDbModelFEResponse>>(JsonConvert.SerializeObject(products_original));
                if (products == null || products.Count <= 0) return products;
                products=await UpdateProductDetail(products);
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return products;
        }
        public async Task<List<ProductMongoDbModelFEResponse>> UpdateProductDetail(List<ProductMongoDbModelFEResponse> products)
        {
            try
            {
                if (products == null || products.Count <= 0) return products;
                var active_flashsale = await flashSaleESRepository.SearchActiveFlashSales();
                List<FlashSaleProductESModel> list_item = new List<FlashSaleProductESModel>();
                if (active_flashsale != null && active_flashsale.Count > 0)
                {
                    list_item = await flashSaleProductESRepository.GetByListFlashsaleId(active_flashsale.Select(x => x.flashsale_id).ToList());
                }
                List<ProductMongoDbModelFEResponse> output = new List<ProductMongoDbModelFEResponse>();
                foreach (var item in products)
                {
                    UpdateProductItem(item, active_flashsale, list_item);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return products;
        }

        public async Task<List<ProductMongoDbModel>> SubListing(string parent_id)
        {
            return await _productDetailMongoAccess.SubListing(parent_id);
        }
        public async Task<ProductListResponseModel> GlobalSearch(string keyword = "", int? stars = 0, string? group_product_id = "", string? brands = "", int page_index = 1, int page_size = 12)
        {
            return await _productDetailMongoAccess.GlobalSearch(keyword,stars,group_product_id,brands,page_index,page_size);
        }
        public async Task<List<ProductMongoDbModel>> ListByProductNoExtend(List<string> ids)
        {
            return await _productDetailMongoAccess.ListByProducts(ids);
        }
        public async Task<List<FlashSaleProductResposeModel>> GetFlashSaleProductByProductIds(List<FlashSaleProductESModel> list_item)
        {
            List<FlashSaleProductResposeModel> result = new List<FlashSaleProductResposeModel>();
            try
            {
                if (list_item != null && list_item.Count > 0)
                {
                    var list_product_mongo = await _productDetailMongoAccess.ListByProducts(list_item.Select(x => x.productid).ToList());
                    foreach (var item in list_item)
                    {
                        var selected = list_product_mongo.FirstOrDefault(x => x._id == item.productid);
                        if (selected == null || selected._id == null) continue;
                        var amount_product = selected.amount;
                        if (selected.amount <= 0 && selected.amount_min != null && selected.amount_min > 0)
                        {
                            amount_product = (double)selected.amount_min;

                        }
                        double old_price = selected.old_price == null || selected.old_price <= 0 ? amount_product : (double)selected.old_price;
                        if (old_price <= 0)
                        {
                            old_price = amount_product;
                        }
                        double total_discount = 0;
                        double percent = Convert.ToDouble(item.discountvalue);
                        switch (item.valuetype)
                        {
                            case 1:
                                total_discount += (amount_product * Convert.ToDouble(percent / 100));
                                break;
                            case 0:
                                total_discount += percent;
                                break;

                            default: break;
                        }
                        var discount_percent = Math.Round(total_discount / old_price * 100, 0);
                        discount_percent = discount_percent <= 0 ? 0 : discount_percent;
                        result.Add(new FlashSaleProductResposeModel()
                        {
                            amount = old_price,
                            amount_after_flashsale = NumberHelpers.RoundUpToHundredsDouble(amount_product - total_discount),
                            discountvalue = discount_percent,
                            position = item.position??0,
                            total_discount = total_discount,
                            _id = selected._id,
                            avatar = selected.avatar,
                            name = selected.name,
                            code = selected.code,
                            rating = selected.rating,
                            review_count = selected.review_count,
                            total_sold = selected.total_sold
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
            }
            return result;
        }
        private bool UpdateProductItem(ProductMongoDbModelFEResponse item, List<FlashSaleESModel> active_flashsale, List<FlashSaleProductESModel> list_item)
        {
            try
            {
                if (item == null || item._id == null) return false;
               
                if (active_flashsale != null && active_flashsale.Count > 0 && list_item != null && list_item.Count > 0)
                {
                    UpdateProductFlashsale(item, active_flashsale, list_item);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
                return false;
            }
            return true;
        }

       
     
        private bool UpdateProductFlashsale(ProductMongoDbModelFEResponse item, List<FlashSaleESModel> active_flashsale, List<FlashSaleProductESModel> list_item)
        {
            try
            {
                if (item == null || item._id == null) return false;
                if (active_flashsale != null && active_flashsale.Count > 0 && list_item != null && list_item.Count > 0)
                {
                    var exists_flash_sale_product = list_item.FirstOrDefault(x => x.productid == (item.parent_product_id != null && item.parent_product_id.Trim() != "" ? item.parent_product_id : item._id));
                    if (exists_flash_sale_product != null && exists_flash_sale_product.flashsale_id != null)
                    {
                        var exists_flash_sale = active_flashsale.First(x => x.flashsale_id == exists_flash_sale_product.flashsale_id);
                        double total_discount = 0;

                        double percent = Convert.ToDouble(exists_flash_sale_product.discountvalue);
                        var amount_product = item.amount;
                        if (item.amount <= 0 && item.amount_min != null && item.amount_min > 0)
                        {
                            amount_product = (double)item.amount_min;

                        }
                        double old_price = item.old_price == null || item.old_price <= 0 ? amount_product : (double)item.old_price;
                        if (old_price <= 0)
                        {
                            old_price = amount_product;
                        }
                        switch (exists_flash_sale_product.valuetype)
                        {
                            case 1:
                                total_discount += (amount_product * Convert.ToDouble(percent / 100));
                                break;
                            case 0:
                                total_discount += percent;
                                break;

                            default: break;
                        }
                        total_discount = Math.Round(total_discount, 0);
                        item.exists_flashsale_id = exists_flash_sale.flashsale_id;
                        item.flash_sale_fromdate = exists_flash_sale.fromdate;
                        item.flash_sale_todate = exists_flash_sale.todate;
                        item.exists_flashsale_name = exists_flash_sale.name;
                        item.amount_after_flashsale = amount_product - total_discount;
                        item.profit -= total_discount;
                        if (item.amount <= 0 && item.amount_min != null && item.amount_min > 0)
                        {
                            item.amount_min -= total_discount;
                            item.amount_min = NumberHelpers.RoundUpToHundredsDouble((double)item.amount_min);

                        }
                        if (item.amount <= 0 && item.amount_max != null && item.amount_max > 0)
                        {
                            item.amount_max -= total_discount;
                            item.amount_max = NumberHelpers.RoundUpToHundredsDouble((double)item.amount_max);

                        }
                        item.discount = Math.Round(((old_price - (double)item.amount_after_flashsale) / old_price * 100), 0);
                        item.discount = item.discount <= 0 ? 0 : item.discount;
                        //item.price = amount_product- item.profit;
                        item.old_price = old_price;
                        item.amount_after_flashsale = NumberHelpers.RoundUpToHundredsDouble((double)item.amount_after_flashsale);
                        item.profit = NumberHelpers.RoundUpToHundredsDouble((double)item.profit);
                    }
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex;
                
                return false;
            }
            return true;
        }


    }
}

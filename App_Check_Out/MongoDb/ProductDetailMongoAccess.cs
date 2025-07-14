using APP_CHECKOUT.Models.Models;
using Entities.ViewModels.Products;
using HAPP_CHECKOUT.Utilities.Product;
using HuloToys_Service.Utilities.lib;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Configuration;

namespace APP_CHECKOUT.MongoDb
{
    public class ProductDetailMongoAccess
    {
        private IMongoCollection<ProductMongoDbModel> _productDetailCollection;

        public ProductDetailMongoAccess()
        {

            //      "connection_string": "mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=Adavigo"
            string _connection = "mongodb://" + ConfigurationManager.AppSettings["Mongo_usr"]
                 + ":" + ConfigurationManager.AppSettings["Mongo_pwd"]
                 + "@" + ConfigurationManager.AppSettings["Mongo_Host"]
                 + ":" + ConfigurationManager.AppSettings["Mongo_Port"]
                 + "/?authSource=" + ConfigurationManager.AppSettings["Mongo_catalog"];
            var booking = new MongoClient(_connection);
            IMongoDatabase db = booking.GetDatabase(ConfigurationManager.AppSettings["Mongo_catalog"]);
            _productDetailCollection = db.GetCollection<ProductMongoDbModel>("ProductDetail");
        }
        public async Task<string> AddNewAsync(ProductMongoDbModel model)
        {
            try
            {
                model.GenID();
                await _productDetailCollection.InsertOneAsync(model);
                return model._id;
            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }
        public async Task<string> UpdateAsync(ProductMongoDbModel model)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id));
                await _productDetailCollection.FindOneAndReplaceAsync(filterDefinition, model);
                return model._id;
            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }


        public async Task<ProductMongoDbModel> GetByID(string id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x._id, id); ;
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                return model;
            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }
        public async Task<ProductDetailResponseDbModel> GetFullProductById(string id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x._id, id); ;
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                var result = new ProductDetailResponseDbModel()
                {
                    product_main=model,
                    product_sub=await SubListing(id)
                };
                return result;
            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }

        public async Task<List<ProductMongoDbModel>> Listing(string keyword = "", int group_id = -1, int page_index = 1, int page_size = 10)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                if(keyword!=null && keyword.Trim() != "")
                {
                    filterDefinition &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.name, keyword);

                }
                if (group_id > 0)
                {
                    filterDefinition &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, group_id.ToString());
                }
                var sort_filter = Builders<ProductMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filterDefinition).Sort(sort_filter_definition);
                model.Options.Skip = page_index < 1 ? 0 : (page_index - 1) * page_size;
                model.Options.Limit = page_size;
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }
        //public async Task<ProductListResponseModel> ResponseListing(string keyword = "", int group_id = -1)
        //{
        //    try
        //    {
        //        var filter = Builders<ProductMongoDbModel>.Filter;
        //        var filterDefinition = filter.Empty;
        //        if (keyword != null && keyword.Trim() != "")
        //        {
        //            filterDefinition |= Builders<ProductMongoDbModel>.Filter.Regex(x => x.name, keyword);
        //        }
        //        if (group_id > 0)
        //        {
        //            filterDefinition &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, group_id.ToString());
        //        }
        //        filterDefinition &= Builders<ProductMongoDbModel>.Filter.Or(
        //                                           Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
        //                                           Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
        //                                       );
        //        filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.status, (int)ProductStatus.ACTIVE);
        //        var sort_filter = Builders<ProductMongoDbModel>.Sort;
        //        var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
        //        var model = _productDetailCollection.Find(filterDefinition).Sort(sort_filter_definition);
        //        long count = await model.CountDocumentsAsync();
        //        var items = await model.ToListAsync();
        //        return new ProductListResponseModel()
        //        {
        //            items = items,
        //            count = count
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        public async Task<ProductListResponseModel> ResponseListing(string keyword = "", int group_id = -1,
            int page_index=1,int page_size=10, double? price_from = null, double? price_to = null , float? rating = null
            , int? supplier=-1, int? label_id=-1)
        {
            try
            {
                var keyword_nonunicode = StringHelper.RemoveUnicode(keyword);

                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                // filterDefinition &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.name, new Regex(Regex.Escape(keyword), RegexOptions.IgnoreCase));
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Or(
                  //Unicode
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.name, new BsonRegularExpression($"{keyword}", "i")),
                  // Builders<ProductMongoDbModel>.Filter.Regex(x => x.description, new BsonRegularExpression($"{keyword}", "i")),
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.sku, new BsonRegularExpression($"{keyword}", "i")),
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.code, new BsonRegularExpression($"{keyword}", "i")),
                  // Builders<ProductMongoDbModel>.Filter.ElemMatch(
                  //       x => x.specification, Builders<ProductSpecificationDetailMongoDbModel>.Filter.Regex(x => x.value, new BsonRegularExpression($"{keyword}", "i"))
                  // ),
                  // Builders<ProductMongoDbModel>.Filter.ElemMatch(
                  //       x => x.attributes_detail, Builders<ProductAttributeMongoDbModelItem>.Filter.Regex(x => x.name, new BsonRegularExpression($"{keyword}", "i"))
                  //),
                  //Non-unicode
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.name, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                   //Builders<ProductMongoDbModel>.Filter.Regex(x => x.description, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                   Builders<ProductMongoDbModel>.Filter.Regex(x => x.sku, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.code, new BsonRegularExpression($"{keyword_nonunicode}", "i"))
                // Builders<ProductMongoDbModel>.Filter.ElemMatch(
                //       x => x.specification, Builders<ProductSpecificationDetailMongoDbModel>.Filter.Regex(x => x.value, new BsonRegularExpression($"{keyword_nonunicode}", "i"))
                // ),
                // Builders<ProductMongoDbModel>.Filter.ElemMatch(
                //       x => x.attributes_detail, Builders<ProductAttributeMongoDbModelItem>.Filter.Regex(x => x.name, new BsonRegularExpression($"{keyword_nonunicode}", "i"))
                //)

                );
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.status, (int)ProductStatus.ACTIVE);

                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Or(
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                );
                if (group_id > 0)
                {
                    filterDefinition &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, group_id.ToString());
                }
                if (label_id!=null && label_id > 0)
                {
                    filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.label_id, (int)label_id);
                }
                if (supplier != null && supplier > 0)
                {
                    filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.supplier_id, (int)supplier);
                }
                //// Lọc theo khoảng giá
                //if (price_from.HasValue)
                //{
                //    filterDefinition &= Builders<ProductMongoDbModel>.Filter.Gte(x => x.price, price_from.Value);
                //}
                //if (price_to.HasValue)
                //{
                //    filterDefinition &= Builders<ProductMongoDbModel>.Filter.Lte(x => x.price, price_to.Value);
                //}
                // Lọc theo khoảng giá dựa trên amount_min và amount_max
                // ✅ Lọc theo khoảng giá giao nhau
                //if (price_from > 0 || price_to > 0)
                //{
                //    var fromVal = price_from ?? 0;
                //    var toVal = price_to ?? double.MaxValue;

                //    var priceFilter = filter.And(
                //        filter.Gte(x => x.amount_max, fromVal),
                //        filter.Lte(x => x.amount_min, toVal)
                //    );
                //    filterDefinition &= priceFilter;
                //}
                // Lọc theo rating nếu có
                //if (rating != null)
                //{
                //    filterDefinition &= Builders<ProductMongoDbModel>.Filter.Gte(x => x.star, rating.Value);
                //}


                var sort_filter = Builders<ProductMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filterDefinition).Sort(sort_filter_definition);
                model.Options.Skip = page_index < 1 ? 0 : (page_index - 1) * page_size;
                model.Options.Limit = page_size;
                long count = await model.CountDocumentsAsync();
                var items = await model.ToListAsync();
                return new ProductListResponseModel()
                {
                    items = items,
                    count = count
                };
            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }
        public async Task<ProductListResponseModel> Search(string keyword = "")
        {
            try
            {

                //string regex_keyword_pattern = keyword;
                //var keyword_split = keyword.Split(" ");
                //if (keyword_split.Length > 0) {
                //    regex_keyword_pattern = "";

                //    foreach (var word  in keyword_split)
                //    {
                //        string w=word.Trim();
                //        if (StringHelper.HasSpecialCharacterExceptVietnameseCharacter(word)) {
                //            w = StringHelper.RemoveSpecialCharacterExceptVietnameseCharacter(word);
                //        }
                //        regex_keyword_pattern += "(?=.*"+w+".*)";

                //    }
                //}
                //regex_keyword_pattern = "^" + regex_keyword_pattern + ".*$";
                // var regex = new BsonRegularExpression(keyword.Trim().ToLower(), "i");

                // var filter = Builders<ProductMongoDbModel>.Filter.Or(
                //    Builders<ProductMongoDbModel>.Filter.Regex(x => x.name, regex), // Case-insensitive regex
                //    Builders<ProductMongoDbModel>.Filter.Regex(x => x.sku, regex), // Case-insensitive regex
                //    Builders<ProductMongoDbModel>.Filter.Regex(x => x.code, regex)  // Case-insensitive regex
                //)
                var keyword_nonunicode = StringHelper.RemoveUnicode(keyword);

                var filter = Builders<ProductMongoDbModel>.Filter.Or(
                  //Unicode
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.name, new BsonRegularExpression($"{keyword}", "i")),
                 // Builders<ProductMongoDbModel>.Filter.Regex(x => x.description, new BsonRegularExpression($"{keyword}", "i")),
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.sku, new BsonRegularExpression($"{keyword}", "i")),
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.code, new BsonRegularExpression($"{keyword}", "i")),
                 // Builders<ProductMongoDbModel>.Filter.ElemMatch(
                 //       x => x.specification, Builders<ProductSpecificationDetailMongoDbModel>.Filter.Regex(x => x.value, new BsonRegularExpression($"{keyword}", "i"))
                 // ),
                 // Builders<ProductMongoDbModel>.Filter.ElemMatch(
                 //       x => x.attributes_detail, Builders<ProductAttributeMongoDbModelItem>.Filter.Regex(x => x.name, new BsonRegularExpression($"{keyword}", "i"))
                 //),
                  //Non-unicode
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.name, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                  //Builders<ProductMongoDbModel>.Filter.Regex(x => x.description, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                   Builders<ProductMongoDbModel>.Filter.Regex(x => x.sku, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                  Builders<ProductMongoDbModel>.Filter.Regex(x => x.code, new BsonRegularExpression($"{keyword_nonunicode}", "i"))
                 // Builders<ProductMongoDbModel>.Filter.ElemMatch(
                 //       x => x.specification, Builders<ProductSpecificationDetailMongoDbModel>.Filter.Regex(x => x.value, new BsonRegularExpression($"{keyword_nonunicode}", "i"))
                 // ),
                 // Builders<ProductMongoDbModel>.Filter.ElemMatch(
                 //       x => x.attributes_detail, Builders<ProductAttributeMongoDbModelItem>.Filter.Regex(x => x.name, new BsonRegularExpression($"{keyword_nonunicode}", "i"))
                 //)
                    )
                & Builders<ProductMongoDbModel>.Filter.Eq(x => x.status, (int)ProductStatus.ACTIVE);
                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                               );
                var sort_filter = Builders<ProductMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filter).Sort(sort_filter_definition); var items = await model.ToListAsync();
                long count = await model.CountDocumentsAsync();
                return new ProductListResponseModel()
                {
                    items = items,
                    count = count
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<ProductMongoDbModel>> SubListing(string parent_id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.parent_product_id, parent_id);
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.status, (int)ProductStatus.ACTIVE); ;

                var sort_filter = Builders<ProductMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filterDefinition).Sort(sort_filter_definition); var items = await model.ToListAsync(); var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }

        public async Task<string> DeactiveByParentId(string id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.parent_product_id, id);
                var update = Builders<ProductMongoDbModel>.Update.Set(x => x.status, (int)ProductStatus.DEACTIVE);

                var updated_item = await _productDetailCollection.UpdateManyAsync(filterDefinition, update);
                return id;
            }
            catch (Exception ex)
            {
                //string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                //LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;

        }
        public async Task<ProductListResponseModel> GlobalSearch(string keyword = "",int? stars=0,string? group_product_id="",string? brands="",int page_index=1,int page_size=12)
        {
            try
            {

                // string regex_keyword_pattern = keyword;
                // var keyword_split = keyword.Split(" ");
                // if (keyword_split.Length > 0)
                // {
                //     regex_keyword_pattern = "";

                //     foreach (var word in keyword_split)
                //     {
                //         string w = word.Trim();
                //         if (StringHelper.HasSpecialCharacterExceptVietnameseCharacter(word))
                //         {
                //             w = StringHelper.RemoveSpecialCharacterExceptVietnameseCharacter(word);
                //         }
                //         regex_keyword_pattern += "(?=.*" + w + ".*)";

                //     }
                // }
                // regex_keyword_pattern = "^" + regex_keyword_pattern + ".*$";
                // var regex = new BsonRegularExpression(regex_keyword_pattern.Trim().ToLower(), "i");

                // var filter = Builders<ProductMongoDbModel>.Filter.Or(
                //    Builders<ProductMongoDbModel>.Filter.Regex(x => x.name, regex), // Case-insensitive regex
                //    Builders<ProductMongoDbModel>.Filter.Regex(x => x.sku, regex), // Case-insensitive regex
                //    Builders<ProductMongoDbModel>.Filter.Regex(x => x.code, regex)  // Case-insensitive regex
                //)
                var filter = Builders<ProductMongoDbModel>.Filter.Or(
                     Builders<ProductMongoDbModel>.Filter.Regex(p => p.name, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i")),
                     Builders<ProductMongoDbModel>.Filter.Regex(p => p.sku, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i")),
                     Builders<ProductMongoDbModel>.Filter.Regex(p => p.code, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i"))

                     )
                & Builders<ProductMongoDbModel>.Filter.Eq(x => x.status, (int)ProductStatus.ACTIVE);
                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                                  Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                                  Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                              );
                if (stars!=null && stars>0)
                {
                    filter &= Builders<ProductMongoDbModel>.Filter.Gte(x => x.star, (int)stars);
                }
                if (group_product_id != null && group_product_id.Trim()!="")
                {
                    string filter_regex = group_product_id.Replace(",", "|");
                    filter &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, filter_regex);
                }
                if (brands != null && brands.Trim() != "")
                {
                    string filter_regex = brands.Replace(",", "|");
                    filter &= Builders<ProductMongoDbModel>.Filter.ElemMatch(
                                        p => p.specification,
                                        attr => brands.Contains(attr.value)
                                    );
                }
                var sort_filter = Builders<ProductMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filter).Sort(sort_filter_definition); 

                long count = await model.CountDocumentsAsync();
                model.Options.Skip = page_index < 1 ? 0 : (page_index - 1) * page_size;
                model.Options.Limit = page_size;
                var items = await model.ToListAsync();
                return new ProductListResponseModel()
                {
                    items = items,
                    count = count
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<ProductMongoDbModel>> ListByProducts(List<string> ids)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.In(x => x._id, ids);

                var model = _productDetailCollection.Find(filterDefinition);
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new List<ProductMongoDbModel>();
            }
        }


    }
}

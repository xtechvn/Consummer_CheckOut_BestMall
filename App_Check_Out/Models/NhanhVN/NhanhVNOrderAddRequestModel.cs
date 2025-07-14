using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_CHECKOUT.Models.NhanhVN
{
    public class NhanhVNOrderAddRequestModel
    {
        public string id { get;set; }
        public int? depotId { get;set; }
        public string? type { get; set; }
        public string customerName { get; set; }
        public string customerMobile { get; set; }
        public string? customerEmail { get; set; }
        public string? customerAddress { get; set; }
        public string? customerCityName { get; set; }
        public string? customerDistrictName { get; set; }
        public string? customerWardLocationName { get; set; }
        public double? moneyDiscount { get; set; }
        public double? moneyTransfer { get; set; }
        public int? moneyTransferAccountId { get; set; }
        public double? moneyDeposit { get; set; }
        public int? moneyDepositAccountId { get; set; }
        public string? paymentMethod { get; set; }
        public string? paymentCode { get; set; }
        public string? paymentGateway { get; set; }
        public int? carrierId { get; set; }
        public int? carrierServiceId { get; set; }
        public int? customerShipFee { get; set; }
        public string? deliveryDate { get; set; }
        public string? status { get; set; }
        public string? description { get; set; }
        public string? privateDescription { get; set; }

        public string? trafficSource { get; set; }
        public string? couponCode { get; set; }
        public int? allowTest { get; set; }
        public int? saleId { get; set; }
        public int? autoSend { get; set; }
        public int? sendCarrierType { get; set; }
        public int? carrierAccountId { get; set; }
        public int? carrierShopId { get; set; }
        public string? carrierServiceCode { get; set; }
        public string? utmCampaign { get; set; }
        public string? utmSource { get; set; }
        public string? utmMedium { get; set; }
        public int? usedPoint { get; set; }
        public int? isPartDelivery { get; set; }
        public List<NhanhVNOrderProductRequestModel> productList { get; set; }
        public List<NhanhVNOrderAffiliateRequestModel> affiliate { get; set; }



}
public class NhanhVNOrderProductRequestModel
    {
        public string? id { get; set; }
        public string? idNhanh { get; set; }
        public int? quantity { get; set; }
        public string? name { get; set; }
        public string? code { get; set; }
        public string? imei { get; set; }
        public string? type { get; set; }
        public int? price { get; set; }
        public int? weight { get; set; }
        public int? importPrice { get; set; }
        public string? description { get; set; }
        public List<NhanhVNOrderProductGiftRequestModel>? gifts { get; set; }

    }
    public class NhanhVNOrderAffiliateRequestModel
    {
        public string? code { get; set; }
        public double? discount { get; set; }
        public double? bonus { get; set; }
    }
    public class NhanhVNOrderProductGiftRequestModel
    {
        public string? Id { get; set; }
        public string? productStoreId { get; set; }
        public int? quantity { get; set; }
        public int? value { get; set; }
    }
}

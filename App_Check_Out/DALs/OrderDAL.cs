using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace APP_CHECKOUT.DAL
{
    public class OrderDAL : GenericService<Order>
    {
        private static DbWorker _DbWorker;
        public OrderDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<long> CreateOrder(Order request)
        {
            try
            {
                
                SqlParameter[] objParam = new SqlParameter[] {
                     new SqlParameter("@ClientId", request.ClientId),
                     new SqlParameter("@OrderNo", request.OrderNo),
                     new SqlParameter("@Price", request.Price),
                     new SqlParameter("@Profit", request.Profit),
                     new SqlParameter("@Discount", request.Discount),
                     new SqlParameter("@Amount", request.Amount),
                     new SqlParameter("@Status", request.OrderStatus),
                     new SqlParameter("@PaymentType", request.PaymentType),
                     new SqlParameter("@PaymentStatus", request.PaymentStatus),
                     new SqlParameter("@UtmSource", request.UtmSource),
                     new SqlParameter("@UtmMedium", request.UtmMedium),
                     new SqlParameter("@Note", request.Note),
                     new SqlParameter("@VoucherId", request.VoucherId),
                     new SqlParameter("@IsDelete", request.IsDelete),
                     new SqlParameter("@UserId", request.UserId),
                     new SqlParameter("@UserGroupIds", request.UserGroupIds),
                     new SqlParameter("@CreatedBy", request.CreatedBy),
                     new SqlParameter("@ProvinceId", request.ProvinceId),
                     new SqlParameter("@DistrictId", request.DistrictId),
                     new SqlParameter("@WardId", request.WardId),
                     new SqlParameter("@Address", request.Address),
                     new SqlParameter("@Phone", request.Phone),
                     new SqlParameter("@ReceiverName", request.ReceiverName),
                     new SqlParameter("@ShippingFee", request.ShippingFee),
                     new SqlParameter("@CarrierId", request.CarrierId),
                     new SqlParameter("@ShippingType", request.ShippingType),
                     new SqlParameter("@ShippingCode", request.ShippingCode),  
                     new SqlParameter("@PackageWeight", request.PackageWeight),
                     new SqlParameter("@ShippingStatus", request.ShippingStatus),
                     new SqlParameter("@ShippingTypeCode", request.ShippingTypeCode),
                     new SqlParameter("@ShippingToken", request.ShippingToken),
                    new SqlParameter("@SupplierId", request.SupplierId??(object)DBNull.Value),
                    new SqlParameter("@OrderMergeId", request.OrderMergeId??(object)DBNull.Value),

                };
                request.OrderId = _DbWorker.ExecuteNonQuery(SPName.CREATE_ORDER, objParam);
                return request.OrderId;

            }
            catch 
            {

            }
            return -1;
        }
        public async Task<long> UpdateOrder(Order request)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[] {
                     new SqlParameter("@OrderId", request.OrderId),
                     new SqlParameter("@ClientId", request.ClientId),
                     new SqlParameter("@OrderNo", request.OrderNo),
                     new SqlParameter("@Price", request.Price),
                     new SqlParameter("@Profit", request.Profit),
                     new SqlParameter("@Discount", request.Discount),
                     new SqlParameter("@Amount", request.Amount),
                     new SqlParameter("@Status", request.OrderStatus),
                     new SqlParameter("@PaymentType", request.PaymentType),
                     new SqlParameter("@PaymentStatus", request.PaymentStatus),
                     new SqlParameter("@UtmSource", request.UtmSource),
                     new SqlParameter("@UtmMedium", request.UtmMedium),
                     new SqlParameter("@Note", request.Note),
                     new SqlParameter("@VoucherId", request.VoucherId),
                     new SqlParameter("@IsDelete", request.IsDelete),
                     new SqlParameter("@UserId", request.UserId),
                     new SqlParameter("@UserGroupIds", request.UserGroupIds),
                     new SqlParameter("@UserUpdateId", request.UserUpdateId),

                };
                return _DbWorker.ExecuteNonQuery(SPName.UPDATE_ORDER, objParam);
            }
            catch
            {

            }
            return -1;
        }
    }
}

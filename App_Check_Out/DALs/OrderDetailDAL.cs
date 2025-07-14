using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace APP_CHECKOUT.DAL
{
    public class OrderDetailDAL : GenericService<OrderDetail>
    {
        private static DbWorker _DbWorker;
        public OrderDetailDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<long> CreateOrderDetail(OrderDetail request)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[] {
                     new SqlParameter("@OrderId", request.OrderId),
                     new SqlParameter("@ProductId", request.ProductId),
                     new SqlParameter("@ProductCode", request.ProductCode),
                     new SqlParameter("@Amount", request.Amount),
                     new SqlParameter("@Price", request.Price),
                     new SqlParameter("@Profit", request.Profit),
                     new SqlParameter("@Discount", request.Discount),
                     new SqlParameter("@Quantity", request.Quantity),
                     new SqlParameter("@TotalAmount", request.TotalAmount),
                     new SqlParameter("@TotalPrice", request.TotalPrice),
                     new SqlParameter("@TotalProfit", request.TotalProfit),
                     new SqlParameter("@TotalDiscount", request.TotalDiscount),
                     new SqlParameter("@ProductLink", request.ProductLink),
                     new SqlParameter("@UserCreate", request.UserCreate),
                     new SqlParameter("@UserUpdated", request.UserUpdated),
                     new SqlParameter("@ParentProductId", request.ParentProductId),

                };
                request.OrderDetailId = _DbWorker.ExecuteNonQuery(SPName.CREATE_ORDER_DEATIL, objParam);

                return request.OrderDetailId;
            }
            catch 
            {

            }
            return -1;
        }
        public async Task<long> UpdateOrderDetail(OrderDetail request)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[] {
                   new SqlParameter("@OrderDetailId", request.OrderDetailId),
                   new SqlParameter("@OrderId", request.OrderId),
                     new SqlParameter("@ProductId", request.ProductId),
                     new SqlParameter("@ProductCode", request.ProductCode ==null || request.ProductCode.Trim() =="" ? "":request.ProductCode),
                     new SqlParameter("@Amount", request.Amount),
                     new SqlParameter("@Price", request.Price),
                     new SqlParameter("@Profit", request.Profit),
                     new SqlParameter("@Discount", request.Discount),
                     new SqlParameter("@Quantity", request.Quantity),
                     new SqlParameter("@TotalAmount", request.TotalAmount),
                     new SqlParameter("@TotalPrice", request.TotalPrice),
                     new SqlParameter("@TotalProfit", request.TotalProfit),
                     new SqlParameter("@ProductLink", request.ProductLink),
                     new SqlParameter("@UserCreate", request.UserCreate),
                     new SqlParameter("@UserUpdated", request.UserUpdated),
                    new SqlParameter("@ParentProductId", request.ParentProductId),


                };
                return _DbWorker.ExecuteNonQuery(SPName.UPDATE_ORDER_DEATIL, objParam);
            }
            catch
            {

            }
            return -1;
        }
    }
}

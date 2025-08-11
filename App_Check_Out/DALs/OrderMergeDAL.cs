using APP_CHECKOUT.Utilities.Lib;
using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    public class OrderMergeDAL : GenericService<OrderMerge>
    {
        private static DbWorker _DbWorker;

        public OrderMergeDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        /// <summary>
        /// Thực thi stored procedure để chèn một bản ghi OrderMerge mới.
        /// </summary>
        /// <param name="model">Đối tượng OrderMerge chứa dữ liệu cần chèn.</param>
        /// <returns>Trả về Id của bản ghi được chèn.</returns>
        public async Task<long> InsertOrderMerge(OrderMerge model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                new SqlParameter("@ClientId", model.ClientId <= 0 ? (object)DBNull.Value : model.ClientId),
                new SqlParameter("@OrderNo", model.OrderNo ?? (object)DBNull.Value),
                new SqlParameter("@CreatedDate", model.CreatedDate ),
                new SqlParameter("@CreatedBy", model.CreatedBy <= 0 ? (object)DBNull.Value : model.CreatedBy),
                new SqlParameter("@UpdateLast", model.UpdateLast ?? (object)DBNull.Value),
                new SqlParameter("@UserUpdateId", model.UserUpdateId <= 0 ? (object)DBNull.Value : model.UserUpdateId),
                new SqlParameter("@Price", model.Price ?? (object)DBNull.Value),
                new SqlParameter("@Profit", model.Profit ?? (object)DBNull.Value),
                new SqlParameter("@Discount", model.Discount ?? (object)DBNull.Value),
                new SqlParameter("@Amount", model.Amount ?? (object)DBNull.Value),
                new SqlParameter("@OrderStatus", model.OrderStatus <0 ? (object)DBNull.Value : model.OrderStatus),
                new SqlParameter("@PaymentType", model.PaymentType < 0 ? (object)DBNull.Value : model.PaymentType),
                new SqlParameter("@PaymentStatus", model.PaymentStatus < 0 ? (object)DBNull.Value : model.PaymentStatus),
                new SqlParameter("@UtmSource", model.UtmSource ?? (object)DBNull.Value),
                new SqlParameter("@UtmMedium", model.UtmMedium ?? (object)DBNull.Value),
                new SqlParameter("@Note", model.Note ?? (object)DBNull.Value),
                new SqlParameter("@VoucherId", model.VoucherId==null ? (object)DBNull.Value : model.VoucherId),
                new SqlParameter("@IsDelete", model.IsDelete ?? (object)DBNull.Value),
                new SqlParameter("@UserId", model.UserId <= 0 ? (object)DBNull.Value : model.UserId),
                new SqlParameter("@UserGroupIds", model.UserGroupIds ?? (object)DBNull.Value),
                new SqlParameter("@ReceiverName", model.ReceiverName ?? (object)DBNull.Value),
                new SqlParameter("@Phone", model.Phone ?? (object)DBNull.Value),
                new SqlParameter("@ProvinceId", model.ProvinceId <= 0 ? (object)DBNull.Value : model.ProvinceId),
                new SqlParameter("@DistrictId", model.DistrictId <= 0 ? (object)DBNull.Value : model.DistrictId),
                new SqlParameter("@WardId", model.WardId <= 0 ? (object)DBNull.Value : model.WardId),
                new SqlParameter("@Address", model.Address ?? (object)DBNull.Value),
                new SqlParameter("@RefundStatus", model.RefundStatus < 0 ? (object)DBNull.Value : model.RefundStatus),
                new SqlParameter("@RefundReason", model.RefundReason ?? (object)DBNull.Value),
                new SqlParameter("@RefundDate", model.RefundDate ?? (object)DBNull.Value),
                new SqlParameter("@ShippingFee", model.ShippingFee ?? (object)DBNull.Value)

                };

                // Giả sử SP_InsertOrderMerge trả về Id của bản ghi mới chèn
                return _DbWorker.ExecuteNonQuery("sp_InsertOrderMerge", objParam);
            }
            catch (Exception ex)
            {
                 LogHelper.InsertLogTelegram("InsertOrderMerge - OrderMergeDAL: " + ex); // Bỏ comment nếu có LogHelper
                return -2;
            }
        }

        /// <summary>
        /// Thực thi stored procedure để cập nhật một bản ghi OrderMerge.
        /// </summary>
        /// <param name="model">Đối tượng OrderMerge chứa dữ liệu cập nhật.</param>
        /// <returns>Trả về số dòng bị ảnh hưởng.</returns>
        public async Task<long> UpdateOrderMerge(OrderMerge model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@ClientId", model.ClientId <= 0 ? (object)DBNull.Value : model.ClientId),
                new SqlParameter("@OrderNo", model.OrderNo ?? (object)DBNull.Value),
                new SqlParameter("@CreatedDate", model.CreatedDate),
                new SqlParameter("@CreatedBy", model.CreatedBy <= 0 ? (object)DBNull.Value : model.CreatedBy),
                new SqlParameter("@UpdateLast", model.UpdateLast ?? (object)DBNull.Value),
                new SqlParameter("@UserUpdateId", model.UserUpdateId <= 0 ? (object)DBNull.Value : model.UserUpdateId),
                new SqlParameter("@Price", model.Price ?? (object)DBNull.Value),
                new SqlParameter("@Profit", model.Profit ?? (object)DBNull.Value),
                new SqlParameter("@Discount", model.Discount ?? (object)DBNull.Value),
                new SqlParameter("@Amount", model.Amount ?? (object)DBNull.Value),
                new SqlParameter("@OrderStatus", model.OrderStatus <= 0 ? (object)DBNull.Value : model.OrderStatus),
                new SqlParameter("@PaymentType", model.PaymentType <= 0 ? (object)DBNull.Value : model.PaymentType),
                new SqlParameter("@PaymentStatus", model.PaymentStatus <= 0 ? (object)DBNull.Value : model.PaymentStatus),
                new SqlParameter("@UtmSource", model.UtmSource ?? (object)DBNull.Value),
                new SqlParameter("@UtmMedium", model.UtmMedium ?? (object)DBNull.Value),
                new SqlParameter("@Note", model.Note ?? (object)DBNull.Value),
                new SqlParameter("@VoucherId", model.VoucherId==null ? (object)DBNull.Value : model.VoucherId),
                new SqlParameter("@IsDelete", model.IsDelete ?? (object)DBNull.Value),
                new SqlParameter("@UserId", model.UserId <= 0 ? (object)DBNull.Value : model.UserId),
                new SqlParameter("@UserGroupIds", model.UserGroupIds ?? (object)DBNull.Value),
                new SqlParameter("@ReceiverName", model.ReceiverName ?? (object)DBNull.Value),
                new SqlParameter("@Phone", model.Phone ?? (object)DBNull.Value),
                new SqlParameter("@ProvinceId", model.ProvinceId <= 0 ? (object)DBNull.Value : model.ProvinceId),
                new SqlParameter("@DistrictId", model.DistrictId <= 0 ? (object)DBNull.Value : model.DistrictId),
                new SqlParameter("@WardId", model.WardId <= 0 ? (object)DBNull.Value : model.WardId),
                new SqlParameter("@Address", model.Address ?? (object)DBNull.Value),
                new SqlParameter("@RefundStatus", model.RefundStatus <= 0 ? (object)DBNull.Value : model.RefundStatus),
                new SqlParameter("@RefundReason", model.RefundReason ?? (object)DBNull.Value),
                new SqlParameter("@RefundDate", model.RefundDate ?? (object)DBNull.Value),
                new SqlParameter("@ShippingFee", model.ShippingFee ?? (object)DBNull.Value)
                };

                return _DbWorker.ExecuteNonQuery("sp_UpdateOrderMerge", objParam);
            }
            catch (Exception ex)
            {
                 LogHelper.InsertLogTelegram("UpdateOrderMerge - OrderMergeDAL: " + ex); // Bỏ comment nếu có LogHelper
                return -2;
            }
        }

        /// <summary>
        /// Thực thi stored procedure để lấy danh sách OrderMerge có phân trang và tìm kiếm.
        /// </summary>
        /// <param name="pageIndex">Chỉ mục trang.</param>
        /// <param name="pageSize">Kích thước trang.</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo OrderNo.</param>
        /// <returns>Trả về DataTable chứa kết quả.</returns>
        public async Task<DataTable> GetOrderMergePaging(int pageIndex, int pageSize, string keyword)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[3];
                objParam[0] = new SqlParameter("@page_index", pageIndex);
                objParam[1] = new SqlParameter("@page_size", pageSize);
                objParam[2] = new SqlParameter("@keyword", string.IsNullOrEmpty(keyword) ? (object)DBNull.Value : keyword);

                return _DbWorker.GetDataTable("sp_GetOrderMergePaging", objParam);
            }
            catch (Exception ex)
            {
                 LogHelper.InsertLogTelegram("GetOrderMergePaging - OrderMergeDAL: " + ex); // Bỏ comment nếu có LogHelper
                return null;
            }
        }
    }
}

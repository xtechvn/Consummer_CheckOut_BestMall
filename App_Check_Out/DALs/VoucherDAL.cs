using DAL.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data;
using DAL.StoreProcedure;
using HuloToys_Service.Models.Models;
using Microsoft.Data.SqlClient;
using APP_CHECKOUT.Models.SQL;
using APP_CHECKOUT.Utilities.Lib;

namespace DAL
{
    public class VoucherDAL : GenericService<Voucher>
    {
        private static DbWorker _DbWorker;

        public VoucherDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);

        }


        public async Task<Voucher> FindByVoucherCode(string voucherCode)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Code.ToUpper() == voucherCode.ToUpper());
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL: " + ex);
                return null;
            }
        }
        public async Task<Voucher> FindByVoucherId(long id)
        {
            try
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL [" + id + "][" + _connection + "]: ");

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Id == id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL ["+id+"]: " + ex);
                return null;
            }
        }

        public async Task<Voucher> FindByVoucherCode(string voucherCode, bool is_public = false)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Code.ToUpper() == voucherCode.ToUpper() && s.IsPublic == is_public);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL: " + ex);
                return null;
            }
        }
       
        public int UpdateVoucher(Voucher model)
        {
            try
            {
                var objParam = new SqlParameter[]
                {
                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@code", model.Code ?? (object)DBNull.Value),
                    new SqlParameter("@cdate", model.Cdate ?? (object)DBNull.Value),
                    new SqlParameter("@udate", model.Udate ?? (object)DBNull.Value),
                    new SqlParameter("@eDate", model.EDate ?? (object)DBNull.Value),
                    new SqlParameter("@limitUse", model.LimitUse),
                    new SqlParameter("@price_sales", model.PriceSales ?? (object)DBNull.Value),
                    new SqlParameter("@unit", model.Unit ?? (object)DBNull.Value),
                    new SqlParameter("@rule_type", model.RuleType ?? (object)DBNull.Value),
                    new SqlParameter("@group_user_priority", model.GroupUserPriority ?? (object)DBNull.Value),
                    new SqlParameter("@is_public", model.IsPublic ?? (object)DBNull.Value),
                    new SqlParameter("@description", model.Description ?? (object)DBNull.Value),
                    new SqlParameter("@is_limit_voucher", model.IsLimitVoucher ?? (object)DBNull.Value),
                    new SqlParameter("@limit_total_discount", model.LimitTotalDiscount ?? (object)DBNull.Value),
                    new SqlParameter("@store_apply", model.StoreApply ?? (object)DBNull.Value),
                    new SqlParameter("@is_max_price_product", model.IsMaxPriceProduct ?? (object)DBNull.Value),
                    new SqlParameter("@min_total_amount", model.MinTotalAmount ?? (object)DBNull.Value),
                    new SqlParameter("@campaign_id", model.CampaignId ?? (object)DBNull.Value),
                    new SqlParameter("@name", model.Name ?? (object)DBNull.Value),
                };

                 return  _DbWorker.ExecuteNonQuery("SP_UpdateVoucher", objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateVoucher - VoucherDAL: " + ex);
                return -1;
            }
        }
    }
}

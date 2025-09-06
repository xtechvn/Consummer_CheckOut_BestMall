using APP_CHECKOUT.Common;
using APP_CHECKOUT.Utilities.Lib;
using DAL.Generic;
using DAL.StoreProcedure;
using HuloToys_Service.Models.Models;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class AllotmentUseDAL : GenericService<AllotmentUse>
    {
        private static DbWorker _DbWorker;

        public AllotmentUseDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public int Insert(AllotmentUse model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@DataId", model.DataId),
                    new SqlParameter("@CreateDate", model.CreateDate==null||model.CreateDate<=DateTime.MinValue ? DBNull.Value:model.CreateDate),
                    new SqlParameter("@AmountUse", model.AmountUse),
                    new SqlParameter("@AllomentFundId", model.AllotmentFundId),
                    new SqlParameter("@AccountClientId", model.AccountClientId),
                    new SqlParameter("@ServiceType", model.ServiceType),
                    new SqlParameter("@ClientId", model.ClientId),
                    new SqlParameter("@PaymentStatus", model.PaymentStatus),

                };

                return Convert.ToInt32(_DbWorker.ExecuteScalar("SP_InsertAllotmentUse", objParam));
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - AllotmentUseDAL: " + ex);
                return -1;
            }
        }

        public int Update(AllotmentUse model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@DataId", model.DataId),
                    new SqlParameter("@AmountUse", model.AmountUse),
                    new SqlParameter("@AllomentFundId", model.AllotmentFundId),
                    new SqlParameter("@AccountClientId", model.AccountClientId),
                    new SqlParameter("@ServiceType", model.ServiceType),
                    new SqlParameter("@ClientId", model.ClientId),
                    new SqlParameter("@CreateDate", model.CreateDate==null||model.CreateDate<=DateTime.MinValue ? DBNull.Value:model.CreateDate),
                    new SqlParameter("@PaymentStatus", model.PaymentStatus),

                };

                return _DbWorker.ExecuteNonQuery("SP_UpdateAllotmentUse", objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - AllotmentUseDAL: " + ex);
                return -1;
            }
        }

        public List<AllotmentUse> GetByAccountClientId(long accountClientId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@AccountClientId", accountClientId)
                };

                var dt= _DbWorker.GetDataTable("SP_GetAllotmentUseByAccountClientId", objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<AllotmentUse>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByAccountClientId - AllotmentUseDAL: " + ex);
            }
            return null;

        }
    }
}

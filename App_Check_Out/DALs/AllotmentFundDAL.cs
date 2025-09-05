using APP_CHECKOUT.Common;
using APP_CHECKOUT.Utilities.Lib;
using DAL.Generic;
using DAL.StoreProcedure;
using HuloToys_Service.Models.Models;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class AllotmentFundDAL : GenericService<AllotmentFund>
    {
        private static DbWorker _DbWorker;

        public AllotmentFundDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public int Insert(AllotmentFund model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@FundType", model.FundType),
                    new SqlParameter("@AccountBalance", model.AccountBalance),
                    new SqlParameter("@AccountClientId", model.AccountClientId),
                    new SqlParameter("@CreateDate", model.CreateDate ?? (object)DBNull.Value),
                    new SqlParameter("@UpdateTime", model.UpdateTime ?? (object)DBNull.Value),
                };

                return Convert.ToInt32(_DbWorker.ExecuteScalar("SP_InsertAllotmentFund", objParam));
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - AllotmentFundDAL: " + ex);
                return -1;
            }
        }

        public int Update(AllotmentFund model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@FundType", model.FundType),
                    new SqlParameter("@AccountBalance", model.AccountBalance),
                    new SqlParameter("@AccountClientId", model.AccountClientId),
                    new SqlParameter("@UpdateTime", model.UpdateTime ?? (object)DBNull.Value),
                };

                return _DbWorker.ExecuteNonQuery("SP_UpdateAllotmentFund", objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - AllotmentFundDAL: " + ex);
                return -1;
            }
        }

        public AllotmentFund GetByAccountClientId(long accountClientId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@AccountClientId", accountClientId)
                };

                var dt = _DbWorker.GetDataTable("SP_GetAllotmentFundByAccountClientId", objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<AllotmentFund>();
                    return data[0];
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByAccountClientId - AllotmentFundDAL: " + ex);
            }
            return null;

        }
    }
}

using APP_CHECKOUT.Common;
using DAL.Generic;
using DAL.StoreProcedure;
using APP_CHECKOUT.Models.Location;
using Microsoft.Data.SqlClient;
using System.Data;
namespace DAL
{
    public class LocationDAL : GenericService<Province>
    {
        private static DbWorker _DbWorker;
        public LocationDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public List<Province> GetListProvinces()
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {

                };

                DataTable tb = _DbWorker.GetDataTable("SP_GetListProvinces", objParam);
                if (tb != null && tb.Rows.Count > 0)
                {
                    var data = tb.ToList<Province>();
                    return data;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public List<District> GetListDistrict()
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {

                };

                DataTable tb = _DbWorker.GetDataTable("SP_GetListDistrict", objParam);
                if (tb != null && tb.Rows.Count > 0)
                {
                    var data = tb.ToList<District>();
                    return data;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public List<Ward> GetListWard()
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {

                };

                DataTable tb = _DbWorker.GetDataTable("SP_GetListWard", objParam);
                if (tb != null && tb.Rows.Count > 0)
                {
                    var data = tb.ToList<Ward>();
                    return data;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
  
}

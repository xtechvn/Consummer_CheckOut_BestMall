using System.Data;
using System.Reflection;
namespace APP_CHECKOUT.Common
{
    public static class Common
    {
        public static List<T> ToList<T>(this DataTable data) where T : new()
        {
            List<T> dtReturn = new List<T>();
            if (data == null)
                return dtReturn;

            Type typeParameterType = typeof(T);

            var props = typeParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetGetMethod() != null);

            foreach (DataRow item in data.AsEnumerable())
            {
                dtReturn.Add(GetValueT<T>(item, props));
            }
            return dtReturn;
        }

        public static List<T> ToListBasic<T>(this DataTable data)
        {
            List<T> dtReturn = new List<T>();
            if (data == null)
                return dtReturn;
            Type typeParameterType = typeof(T);
            foreach (DataRow item in data.AsEnumerable())
            {
                dtReturn.Add((T)Convert.ChangeType(item[0], typeParameterType));
            }
            return dtReturn;
        }

        private static T GetValueT<T>(DataRow row, IEnumerable<PropertyInfo> props) where T : new()
        {
            T objRow = new T();
            foreach (var field in props)
            {
                string fieldName = field.Name;
                var columnName = field.CustomAttributes.FirstOrDefault(x => x.AttributeType.UnderlyingSystemType.Name == "ColumnAttribute");
                if (columnName != null)
                    fieldName = columnName.ConstructorArguments[0].Value.ToString();
                if (row.Table.Columns.Contains(fieldName) && row[fieldName] != DBNull.Value)
                {
                    Type t = Nullable.GetUnderlyingType(field.PropertyType) ?? field.PropertyType;
                    if (field.PropertyType.IsValueType)
                        field.SetValue(objRow, Convert.ChangeType(row[fieldName] == DBNull.Value ? Activator.CreateInstance(t) : row[fieldName], t));
                    else
                    {
                        field.SetValue(objRow, Convert.ChangeType(row[fieldName], t));
                    }
                }
            }
            return objRow;
        }


    }
}

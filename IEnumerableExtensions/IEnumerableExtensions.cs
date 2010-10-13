using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace IEnumerableExtensions {
    public static class IEnumerableExtensions {

        public static DataTable ToDataTable<T>(this IEnumerable<T> enumeration)
        {
            PropertyInfo[] properties = typeof (T).GetProperties();
            DataTable dt = BuildDataTable(properties);
            foreach (T element in enumeration)
            {
                var values = properties.Select(p => p.GetValue(element, null)).ToArray();
                dt.Rows.Add(values);
            }
            return dt;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> enumeration, IEnumerable<PropertyInfo> flattenProperties)
        {
            List<PropertyInfo> properties = typeof (T).GetProperties().ToList();
            foreach (PropertyInfo property in flattenProperties)
            {
                PropertyInfo replaceProperty = properties.SingleOrDefault(p => p.Equals(property));
                if (replaceProperty != null)
                {
                    int replaceIndex = properties.IndexOf(replaceProperty) + 1;
                    foreach (PropertyInfo flatProperty in property.PropertyType.GetProperties())
                    {
                        properties.Insert(replaceIndex, flatProperty);
                        replaceIndex++;
                    }
                    properties.Remove(replaceProperty);
                }
            }

            DataTable dt = BuildDataTable(properties);
            PopulateDataTable(dt, properties, enumeration);

            return dt;
        }
        
        public static DataTable ToDataTableExplicit<T>(this IEnumerable<T> enumeration, IEnumerable<PropertyInfo> onlyTheseProperties)
        {
            DataTable dt = BuildDataTable(onlyTheseProperties);
            PopulateDataTable(dt, onlyTheseProperties, enumeration);
            return dt;
        }

        private static DataTable BuildDataTable(IEnumerable<PropertyInfo> properties)
        {
            DataColumn[] columns = properties.Select(pi => new DataColumn(pi.Name, pi.PropertyType)).ToArray();
            DataTable dt = new DataTable();
            dt.Columns.AddRange(columns);
            return dt;
        }

        private static void PopulateDataTable<T>(DataTable dt, IEnumerable<PropertyInfo> properties, IEnumerable<T> enumeration) {
            foreach (T element in enumeration) 
            {
                List<object> values = new List<object>();
                foreach (PropertyInfo property in properties) 
                {
                    if (typeof(T).GetProperties().Contains(property)) 
                    {
                        values.Add(property.GetValue(element, null));
                    } else 
                    {
                        PropertyInfo parentProperty = typeof(T).GetProperties()
                            .SingleOrDefault(p => p.PropertyType.Equals(property.DeclaringType));
                        if (parentProperty != null) 
                        {
                            object parentValue = parentProperty.GetValue(element, null);
                            object value = null;
                            if (parentValue != null) 
                            {
                                value = property.GetValue(parentValue, null);
                            }
                            values.Add(value);
                        }
                    }
                }
                dt.Rows.Add(values.ToArray());
            }
        }
    }
}

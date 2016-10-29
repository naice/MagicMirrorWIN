using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace API.YouTube.Provider
{
    internal class Properties
    {
        private static IEnumerable<PropertyInfo> Get(object instance)
        {
            return instance.GetType().GetProperties();
        }

        public static string GetQuery(object instance)
        {
            return string.Join("&",
                Get(instance)
                    .Select(property => new { name = GetName(property), value = GetValue(property.GetValue(instance)) })
                    .Where(a => a.value != null)
                    .Select(a => $"{a.name}={Uri.EscapeDataString(a.value)}")
            );
        }

        private static string GetName(PropertyInfo property)
        {
            var name = property.Name;
            if (string.IsNullOrEmpty(name) || char.IsLower(name, 0))
                return name;

            return char.ToLower(name[0]).ToString() + name.Substring(1);
        }

        private static string GetValue(object instance, int tiefe = 0)
        {
            if (instance == null)
            {
                return null;
            }
            if (instance is string)
            {
                return (string)instance;
            }
            if (instance is IEnumerable)
            {
                if (tiefe == 1)
                {
                    throw new InvalidOperationException("Verschachtelte Listen derzeit nicht möglich.");
                }

                List<object> list = new List<object>();
                foreach (var item in (IEnumerable)instance)
                {
                    list.Add(GetValue(item, tiefe++));
                }

                return string.Join(",", list);
            }

            return instance.ToString();
        }
    }
}

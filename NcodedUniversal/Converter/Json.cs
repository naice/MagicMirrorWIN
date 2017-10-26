using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NcodedUniversal.Converter
{
    /// <summary>
    /// <see cref="IConverter"/> for converting from and to Json.
    /// </summary>
    public class Json : IConverter
    {
        private static Json _instance;
        /// <summary>
        /// Instance based on <see cref="Configuration.DefaultJsonConverter"/> warning: will create new instance if configuration changes.
        /// </summary>
        public static Json Instance
        {
            get
            {
                if (_instance == null || _instance.JsonConvert != Configuration.DefaultJsonConverter)
                {
                    _instance = new Json(Configuration.DefaultJsonConverter);
                }

                return _instance;
            }
        }

        private readonly IJsonConvert JsonConvert;
        /// <summary>
        /// Initzializes a new instance of <see cref="Json"/>-class.  
        /// </summary>
        /// <param name="jsonConvert">[Dependency] Underlying converter.</param>
        public Json(IJsonConvert jsonConvert)
        {
            JsonConvert = jsonConvert ?? throw new ArgumentNullException(nameof(jsonConvert));
        }

        /// <summary>
        /// Deserialize object T from json string.
        /// </summary>
        /// <returns>default(T) if failed.</returns>
        public T DeserializeObject<T>(string jsonString) where T : class
        {
            T value = default(T);

            if (!string.IsNullOrEmpty(jsonString))
            {
                try { value = JsonConvert.DeserializeObject<T>(jsonString); }
                catch { }
            }

            return value;
        }
        /// <summary>
        /// Serialize object to json string.
        /// </summary>
        /// <returns>Serialized json string.</returns>
        public string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

    }
}

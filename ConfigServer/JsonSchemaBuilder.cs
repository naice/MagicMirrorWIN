using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using System.Reflection;

namespace ConfigServer
{
    public class JsonSchemaBuilderException : Exception
    {
        public JsonSchemaBuilderException(string message)
            : base(message)
        {

        }
    }

    public class JsonSchemaBuilder
    {
        #region SINGLETON
        private static JsonSchemaBuilder _Instance;
        public static JsonSchemaBuilder Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new JsonSchemaBuilder();
                return _Instance;
            }
        }
        JsonSchemaBuilder()
        {

        }
        #endregion

        private static JsonSchemaBuilderException EX(string msg, params object[] args) => new JsonSchemaBuilderException(string.Format(msg, args));

        public string Build(object from)
        {
            string result = null;

            var baseType = from.GetType();
            var baseTypeInfo = baseType.GetTypeInfo();

            var schemaForm = baseTypeInfo.GetCustomAttribute<SchemaFormAttribute>();
            if (schemaForm == null)
                throw EX($"The Type of the given object got no {nameof(SchemaFormAttribute)} defined.");
            


            return result;
        }
    }
}

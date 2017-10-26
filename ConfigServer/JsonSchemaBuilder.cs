using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigServer
{
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

        public string Build(object from)
        {
            string result = null;



            return result;
        }
    }
}

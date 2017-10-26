using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigServer.Converter
{
    public interface IJsonConvert
    {
        object DeserializeObject(string jsonString, Type type);
        string SerializeObject(object obj);
    }
}

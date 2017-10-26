using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NcodedUniversal.Converter
{
    public interface IJsonConvert
    {
        T DeserializeObject<T>(string jsonString) where T : class;
        string SerializeObject(object obj);
    }
}

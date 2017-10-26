using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigServer.INetPub
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class SchemaFormAttribute : Attribute
    {
        // This is a positional argument
        public SchemaFormAttribute()
        {
        }

        public string title { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        /// <summary>
        /// comma separated list.
        /// </summary>
        public string required { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class SchemaPropertyAttribute : Attribute
    {
        // This is a positional argument
        public SchemaPropertyAttribute()
        {
        }

        public string title { get; set; }
        public string type { get; set; }
        public int? minLength { get; set; } = null;
        public int? maxLength { get; set; } = null;
    }
}

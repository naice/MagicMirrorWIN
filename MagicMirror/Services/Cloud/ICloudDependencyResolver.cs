using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services.Cloud
{
    public interface ICloudDependencyResolver
    {
        object GetDependency(Type dependencyType);
        object[] GetDependecys(Type[] dependencyTypes);
    }
}

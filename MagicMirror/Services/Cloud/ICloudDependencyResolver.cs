using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services.Cloud
{
    /// <summary>
    /// Simple dependency resolver for your <see cref="CloudService"/> implementations.
    /// </summary>
    public interface ICloudServiceDependencyResolver
    {
        /// <summary>
        /// Return a single dependecy matching the given type.
        /// </summary>
        object GetDependency(Type dependencyType);
        /// <summary>
        /// Return dependecys in equal order as requested. Should use <see cref="ICloudServiceDependencyResolver.GetDependency(Type)"/>.
        /// </summary>
        object[] GetDependecys(Type[] dependencyTypes);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETStandard.RestServer
{
    /// <summary>
    /// Simple dependency resolver for your <see cref="RestServerService"/> implementations.
    /// </summary>
    public interface IRestServerServiceDependencyResolver
    {
        /// <summary>
        /// Return a single dependecy matching the given type.
        /// </summary>
        object GetDependency(Type dependencyType);
        /// <summary>
        /// Return dependecys in equal order as requested. Should use <see cref="IRestServerServiceDependencyResolver.GetDependency(Type)"/>.
        /// </summary>
        object[] GetDependecys(Type[] dependencyTypes);
    }
}

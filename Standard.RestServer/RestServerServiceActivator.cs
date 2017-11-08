using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NETStandard.RestServer
{
    internal class RestServerServiceActivator
    {
        public RestServerService Activate(Type serviceType, IRestServerServiceDependencyResolver dependecyResolver)
        {
            var constuctors = serviceType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            foreach (var constructor in constuctors.OrderByDescending(constructor => constructor.GetParameters().Length))
            {
                var parameters = constructor.GetParameters();
                if (parameters == null || parameters.Length == 0)
                {
                    // default constructor
                    return constructor.Invoke(new object[0]) as RestServerService;
                }

                if (dependecyResolver != null)
                {
                    // resolve dependencys
                    var dependencys = dependecyResolver.GetDependecys(parameters.Select(param => param.ParameterType).ToArray());
                    if (dependencys == null || parameters.Length != dependencys.Length)
                    {
                        // no dependecys found.
                        continue;
                    }
                    if (constructor.Invoke(dependencys) is RestServerService RestServerService)
                    {
                        return RestServerService;
                    }

                    throw new InvalidOperationException("Should never happen.");
                }
            }

            throw new InvalidOperationException(
                $"{nameof(RestServerServiceActivator)}: Could not find a matching constructor for "+
                $"{serviceType.FullName}. Either provide a parameterless constructor or provide "+
                $"the correct dependencys via {nameof(IRestServerServiceDependencyResolver)}.");
        }
    }
}

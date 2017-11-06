using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MagicMirror.Services.Cloud
{
    public partial class CloudServer
    {
        private class ExposedCloudService
        {
            public Type ServiceType { get; set; }
            public CloudServiceInstanceType InstanceType { get; set; } = CloudServiceInstanceType.Instance;
            public CloudService SingletonInstance { get; set; }
            public List<ExposedCloudAction> Routes { get; set; } = new List<ExposedCloudAction>();

            private readonly ICloudServiceDependencyResolver _cloudDependencyResolver;

            public ExposedCloudService(ICloudServiceDependencyResolver cloudDependencyResolver)
            {
                _cloudDependencyResolver = cloudDependencyResolver;
            }

            public object GetInstance(CloudHttpContext context)
            {
                if (InstanceType == CloudServiceInstanceType.Instance)
                {
                    return ApplyContext(InternalCreateInstance(), context);
                }

                if (InstanceType == CloudServiceInstanceType.SingletonStrict ||
                    InstanceType == CloudServiceInstanceType.SingletonLazy)
                {
                    if (SingletonInstance == null)
                    {
                        SingletonInstance = InternalCreateInstance();
                    }

                    return ApplyContext(SingletonInstance, context);
                }

                throw new NotImplementedException($"{nameof(ExposedCloudService)}: InstanceType not implemented. {InstanceType}");
            }

            private CloudService InternalCreateInstance()
            {
                var activator = new CloudServiceActivator();

                return activator.Activate(ServiceType, _cloudDependencyResolver);
            }

            private static CloudService ApplyContext(CloudService cloudService, CloudHttpContext context)
            {
                if (cloudService == null)
                {
                    throw new ArgumentNullException(nameof(cloudService));
                }

                cloudService.Request = context?.Request;
                cloudService.Response = context?.Response;

                return cloudService;
            }
        }
    }
}

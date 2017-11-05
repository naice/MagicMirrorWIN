using System;

namespace MagicMirror.Services.Cloud
{
    public enum CloudServiceInstanceType
    {
        /// <summary>
        /// Default Option.
        /// </summary>
        Instance,
        /// <summary>
        /// The Service is marked as singleton and will be instanciated only once. Lazy loading of instance, 
        /// the instance will be created on first request.
        /// </summary>
        SingletonLazy,
        /// <summary>
        /// The Service is marked as singleton and will be instanciated only once. The instance is created on Server startup.
        /// </summary>
        SingletonStrict,
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CloudServiceInstanceAttribute : Attribute
    {
        readonly CloudServiceInstanceType cloudServiceInstanceType;
        
        public CloudServiceInstanceAttribute(CloudServiceInstanceType cloudServiceInstanceType)
        {
            this.cloudServiceInstanceType = cloudServiceInstanceType;
        }

        public CloudServiceInstanceType CloudServiceInstanceType
        {
            get { return cloudServiceInstanceType; }
        }
    }
}

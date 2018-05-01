using System;

namespace NETStandard.RestServer
{
    public enum RestServerServiceInstanceType
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
    public sealed class RestServerServiceInstanceAttribute : Attribute
    {
        readonly RestServerServiceInstanceType _restServerServiceInstanceType;
        
        public RestServerServiceInstanceAttribute(RestServerServiceInstanceType RestServerServiceInstanceType)
        {
            this._restServerServiceInstanceType = RestServerServiceInstanceType;
        }

        public RestServerServiceInstanceType RestServerServiceInstanceType
        {
            get { return _restServerServiceInstanceType; }
        }
    }
}

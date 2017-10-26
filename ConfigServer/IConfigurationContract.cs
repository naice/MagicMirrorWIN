using System.Threading.Tasks;

namespace ConfigServer
{
    public interface IConfigurationContract { }
    public interface IConfigurationContract<T> : IConfigurationContract
    {
        Task ConfigurationUpdated(T newConfigurationData);
        Task<T> ConfigurationRequest();
    }
}
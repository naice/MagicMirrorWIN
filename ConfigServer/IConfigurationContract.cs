using System.Threading.Tasks;

namespace ConfigServer
{
    public interface IConfigurationContract
    {
        Task ConfigurationUpdated(object newConfigurationData);
        Task<object> ConfigurationRequest();
    }
}
using System;

namespace ConfigServer
{
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string defaultConfig)
            : base($"Invalid configuration detected. No {defaultConfig} set. Setup default dependecys via {nameof(DependencyConfiguration)} class before using.")
        {

        }
    }
}
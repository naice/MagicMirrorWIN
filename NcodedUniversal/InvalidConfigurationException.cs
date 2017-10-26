using System;

namespace NcodedUniversal
{
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string defaultConfig)
            : base($"Invalid configuration detected. No {defaultConfig} set. Setup default dependecys via {nameof(Configuration)} class before using.")
        {

        }
    }
}
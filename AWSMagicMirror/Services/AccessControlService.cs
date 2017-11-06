using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AWSMagicMirror.Services
{
    public class AccessControlService
    {
        private readonly Dictionary<Guid, string> _knownClients = new Dictionary<Guid, string>();
        private readonly Dictionary<string, Guid> _amazonUserIdMappings = new Dictionary<string, Guid>();

        public AccessControlService()
        {
            try
            {
                var knownClients = ConfigurationManager.AppSettings["KnownClients"] ?? string.Empty;
                if (!string.IsNullOrEmpty(knownClients))
                {
                    _knownClients = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(knownClients);
                }

                var amazonUserIdMappings = ConfigurationManager.AppSettings["AmazonUserIdMappings"] ?? string.Empty;
                if (!string.IsNullOrEmpty(amazonUserIdMappings))
                {
                    _amazonUserIdMappings = JsonConvert.DeserializeObject<Dictionary<string, Guid>>(amazonUserIdMappings);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public string GetClientURLFromClientId(Guid clientId)
        {
//                 "http://emmusss.ddns.net"
            return "http://emmusss.ddns.net:8886";

            if (!_knownClients.TryGetValue(clientId, out string clientURL))
            {
                return null;
            }

            return clientURL;
        }

        public Guid? GetClientUidFromAmazonUserId(string amazonUserId)
        {
            return Guid.NewGuid();

            if (!_amazonUserIdMappings.TryGetValue(amazonUserId, out Guid clientId))
            {
                return null;
            }

            return clientId;
        }
    }
}
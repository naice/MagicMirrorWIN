using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;

namespace AWSMagicMirror.Models
{
    public class MirrorRegistrationRequest
    {
        /// <summary>
        /// Mirror ID (Guid)
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// Communication Url to remote control the Mirror
        /// </summary>
        public string Url { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Mirror
    {
        /// <summary>
        /// Mirror ID (Guid)
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// Communication Url to remote control the Mirror
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Reference to device. WRONG...
        /// </summary>
        public string AlexaSkillReference { get; set; }
        /// <summary>
        /// Creation of the Magic Mirror registration
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Last active.
        /// </summary>
        public DateTime LastActive { get; set; }

        /// <summary>
        /// Connected echo devices
        /// </summary>
        public ICollection<AlexaEchoDevice> Devices { get; set; }
    }

    public class AlexaEchoDevice
    {
        /// <summary>
        /// Auto increment id.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Reference to alexa device
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// The Mirror.
        /// </summary>
        public Mirror Mirror { get; set; }
    }
}
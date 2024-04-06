using System;
using System.Runtime.Serialization;

namespace OpenStack.Configuration
{
    [Serializable]
    public class OpenStackConfigurationException : Exception
    {
        public OpenStackConfigurationException(string message) : base(message) { }
        public OpenStackConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        protected OpenStackConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

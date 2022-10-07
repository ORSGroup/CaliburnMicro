using System;
using System.Globalization;

namespace CSharpCaliburnMicro1.Helpers
{
    [Serializable]
    public class JSonClientException : Exception
    {
        public JSonClientException() : this(string.Format(CultureInfo.InvariantCulture, "An error occurred in the JSon client.")) { }
        public JSonClientException(string message) : base(message) { }
        public JSonClientException(string message, Exception inner) : base(message, inner) { }
        protected JSonClientException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}

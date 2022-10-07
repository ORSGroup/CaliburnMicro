using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Configuration
{
    public class AppSettings
    {
        public string authHeaderName { get; set; }
        public string authType { get; set; }

        public string dataUrl { get; set; }
        public string mockJson { get; set; }
        public string saveJson { get; set; }
        public string xps2pdf { get; set; }
    }
}

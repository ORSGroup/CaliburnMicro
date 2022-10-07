using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Definition.ReportFake.DataContracts
{
    public class ReportData : IReportData
    {
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }

        public string? Foo { get; set; }
    }
}

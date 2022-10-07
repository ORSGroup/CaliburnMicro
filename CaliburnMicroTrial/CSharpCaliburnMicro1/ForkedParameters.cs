using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1
{
    public sealed class ForkedParameters
    {
        public string PdfFileName { get; set; }
        public string XpsFileName { get; set; }
        public string UserId { get; set; }
        public string CustomerIds { get; set; }
        public string ReferenceDate { get; set; }
        public string ParamsFileName { get; set; }
        public string ReportKind { get; set; }
        public string HolderIds { get; set; }

        public string PortfolioId { get; set; }

        public string ProposalId { get; set; }
    }
}

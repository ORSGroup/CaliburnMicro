using CSharpCaliburnMicro1.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CSharpCaliburnMicro1.Engine
{
    public interface IReportEngine
    {
        Stream MakeReport(string userId, string customerIDS, string referenceDate, string holderIds, string kind, string portfolioId, string proposalId, Configuration.ReportsSection reportsSection);

    }
}

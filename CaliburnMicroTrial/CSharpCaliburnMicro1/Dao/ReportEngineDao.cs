using CSharpCaliburnMicro1.Helpers;
using System.IO;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Dao
{
    public class ReportEngineDao
    {
        private Serilog.ILogger logger;

        public ReportEngineDao(Serilog.ILogger logger)
        {
            this.logger = logger;
        }

        public Task<Stream> MakeReport(string userId, string customerIDS, string referenceDate, string holderIds, string kind, string portfolioId, string proposalId, Configuration.ReportsSection reportsSection)
        {
            var engine = new Engine.ReportEngine(logger);
            var report = engine.MakeReport(userId, customerIDS, referenceDate, holderIds, kind, portfolioId, proposalId, reportsSection);
            return Task.FromResult(report); 
        }
    }
}

using CSharpCaliburnMicro1.Dao;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Controllers
{
    [ApiController]
    [Route("api/reportengine/")]
    public class ReportEngineController : ControllerBase
    {
        private readonly Configuration.ReportsSection _configReports;

        public ReportEngineController(IOptions<Configuration.ReportsSection> configReports)
        {
            _configReports = configReports.Value;
        }

        [HttpGet]
        [Route("customreport")]
        [Produces(MediaTypeNames.Application.Pdf)]
        public async Task<Stream> GetCustomReport(string userId, string customerIDS, string referenceDate, string holderIds, string kind, string portfolioId, string proposalId)
        {
            var dao = new ReportEngineDao(Log.Logger);
            var report = await dao.MakeReport(userId, customerIDS, referenceDate, holderIds, kind, portfolioId, proposalId, _configReports);
            return report;
        }

        [HttpGet]
        [Route("customreportpdf")]
        public async Task<HttpResponseMessage> DownloadCustomReport(string userId, string customerIDS, string referenceDate, string holderIds, string kind, string portfolioId, string proposalId)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var dao = new ReportEngineDao(Log.Logger);
            var report = await dao.MakeReport(userId, customerIDS, referenceDate, holderIds, kind, portfolioId, proposalId, _configReports);
            if (report != null)
            {
                var fileName = System.IO.Path.Combine(@"C:\Temp", kind) + ".pdf";
                FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
                report.CopyTo(fileStream);
                fileStream.Close();                
                var contentLength = report.Length;

                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(report);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentLength = contentLength;
                ContentDispositionHeaderValue contentDisposition = null;
                if (ContentDispositionHeaderValue.TryParse("inline; filename=" + kind + ".pdf", out contentDisposition))
                {
                    response.Content.Headers.ContentDisposition = contentDisposition;
                }
            }

            return response;
        }
    }
}

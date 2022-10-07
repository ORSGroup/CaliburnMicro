using CSharpCaliburnMicro1.Definition.ReportFake.Views;
using CSharpCaliburnMicro1.Views;

namespace CSharpCaliburnMicro1.Definition.ReportFake
{
    public class ReportDefinition : ReportDefinitionBase
    {
        public ReportDefinition() : base()
        {
            var data = DataProvider.Instance.WSData;
            if (data != null && !data.HasError)
            {
                Add(new ReportDefinitionSimplePage<PageCoverView>(u => new { }));
            }
            else
            {
                string message;
                if (DataProvider.Instance.WSData == null)
                {
                    message = "Report data is null. This could be due to an empty Json received from data retrieving.";
                }
                else
                {
                    message = DataProvider.Instance.WSData.ErrorMessage;
                }
                Add(new ReportDefinitionSimplePage<PageErrorView>(u => new { ErrorMessage = message }));
            }
        }
    }
}

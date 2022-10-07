namespace CSharpCaliburnMicro1
{
    public interface IReportData
    {
        bool HasError { get; set; }
        string ErrorMessage { get; set; }
    }
}
namespace CSharpCaliburnMicro1.ViewInterfaces
{
    public interface IAssetClassHistogramItem
    {
        string AssetClass { get; set; }
        decimal Weight { get; set; }
        string Color { get; set; }
    }
}

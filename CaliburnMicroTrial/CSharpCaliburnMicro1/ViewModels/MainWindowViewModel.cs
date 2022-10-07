using Caliburn.Micro;
using CSharpCaliburnMicro1.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase, IViewAware, IHasTargetFileName
    {
        public string CustomerName { get; set; }
        public string Date { get; set; }
        public string CurrentDate { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public ReportDefinitionBase ReportDefinition { get; set; }


        private string target;
        public string Target
        {
            get { return target; }
            set { target = value; NotifyOfPropertyChange(() => Target); }
        }

        public event EventHandler<ViewAttachedEventArgs> ViewAttached = delegate { };

        void IViewAware.AttachView(object view, object context)
        {
            switch (ReportResources.PageOrientation)
            {
                case PageOrientation.Portrait:
                    //create a portrait A4 for the current configured device  DPI
                    Height = GraphUtils.GetA4WideSideDeviceIndependentUnit();
                    Width = GraphUtils.GetA4NarrowSideDeviceIndependentUnit();
                    break;
                case PageOrientation.Landscape:
                    //create a landscape A4 for the current configured device  DPI
                    Width = GraphUtils.GetA4WideSideDeviceIndependentUnit();
                    Height = GraphUtils.GetA4NarrowSideDeviceIndependentUnit();
                    break;
            }
            Target = string.Format(CultureInfo.InvariantCulture, "{0}x{1}", Width, Height);
        }

        object IViewAware.GetView(object context)
        {
            return null;
        }

        string pdfFileName = PathExtension.GetTempFileWithExtension("pdf");
        public string PdfFileName
        {
            get { return pdfFileName; }
            set { pdfFileName = value; }
        }
        string xpsFileName = PathExtension.GetTempFileWithExtension("xps");
        public string XpsFileName
        {
            get { return xpsFileName; }
            set { xpsFileName = value; }
        }
    }
}

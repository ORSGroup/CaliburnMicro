using Caliburn.Micro;
using CSharpCaliburnMicro1.Behaviors;
using CSharpCaliburnMicro1.Helpers;
using CSharpCaliburnMicro1.ViewModels;
using CSharpCaliburnMicro1.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CSharpCaliburnMicro1.Engine
{
    internal class ReportEngine : IReportEngine
    {
        private Serilog.ILogger logger;

        private static string AssemblyDirectory
        {
            get
            {
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                UriBuilder uri = new UriBuilder(assemblyLocation);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public ReportEngine(Serilog.ILogger logger)
        {
            this.logger = logger;
        }

        public Stream MakeReport(string userId, string customerIDS, string referenceDate, string holderIds, string kind, string portfolioId, string proposalId, Configuration.ReportsSection reportsSection)
        {
            if (!AppEnvironment.Instance.Forked)
            {
                if (String.IsNullOrEmpty(userId))
                {
                    throw new ArgumentNullException("userId", "Missing or null parameter \"userId\". Impossible to generate report.");
                }
                if (String.IsNullOrEmpty(kind))
                {
                    throw new ArgumentNullException("kind", "Missing or null parameter \"kind\". Impossible to generate report.");
                }

                ForkedParameters fp = PrepareForExec(userId, customerIDS, referenceDate, holderIds, kind, portfolioId, proposalId);
                Spawn(fp);
                return EpilogueOfExec(fp);
            }
            else
            {
                ManualResetEvent me = new ManualResetEvent(false);

                // Retrieving parameters from file serialized before the fork (during spawn)
                var prms = File.ReadAllText(AppEnvironment.Instance.ParamFiles);
                ForkedParameters fp = JsonSerializer.Deserialize(prms, typeof(ForkedParameters)) as ForkedParameters;

                // Reading config section to get appropriate remote method call 
                var reportSettings = reportsSection.Reports.AsEnumerable().Where(x => x.Name == fp.ReportKind).SingleOrDefault();
                if (reportSettings == null)
                {
                    throw new Exception(String.Format("Report with name = \"{0}\" not found in configuration section \"ReportsSection\" .", fp.ReportKind));
                }
                if (string.IsNullOrEmpty(reportSettings.RemoteCall) && string.IsNullOrEmpty(reportSettings.Path))
                {
                    throw new Exception(String.Format("Report with name = \"{0}\" is missing argument \"remoteCall\" or \"path\" value.", fp.ReportKind));
                }
                if (string.IsNullOrEmpty(reportSettings.Definition))
                {
                    throw new Exception(String.Format("Report with name = \"{0}\" is missing argument \"definition\" value.", fp.ReportKind));
                }

                // Load Report Definition Assembly and register its types
                string assemblyName = String.Format("CSharpCaliburnMicro1.Definition.{0}.dll", reportSettings.Definition);
                Assembly reportAssembly = Assembly.LoadFrom(Path.Combine(AssemblyDirectory, assemblyName));
                AssemblySource.Instance.Add(reportAssembly);
                Bootstrapper.SetBindings(reportAssembly);

                #region Load referenced assemblies and register its types

                foreach (AssemblyName referencedAssembly in reportAssembly.GetReferencedAssemblies())
                {
                    if (referencedAssembly.Name.StartsWith("CSharpCaliburnMicro1.Definition"))
                    {
                        var reportRefAssembly = Assembly.Load(referencedAssembly);
                        AssemblySource.Instance.Add(reportRefAssembly);
                        Bootstrapper.SetBindings(reportRefAssembly);
                    }
                }

                #endregion

                // Inspecting dataProvider, reportDefinition and reportData according to ReportKind parameter
                Type reportDefinitionType = reportAssembly.GetUniqueAssignableTo(typeof(ReportDefinitionBase));
                Type reportDataType = reportAssembly.GetUniqueAssignableTo(typeof(IReportData));
                Type reportDataProviderType = reportAssembly.GetUniqueAssignableTo(typeof(DataProviderBase));

                // Get reportDataProvider Instance and set its parameters
                PropertyInfo property = reportDataProviderType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (property == null)
                {
                    throw new NotImplementedException(String.Format("DataProvider type \"{0}\" do not implement public static property \"Instance\" to get singleton instance.", reportDataProviderType));
                }
                DataProviderBase reportDataProvider = property.GetValue(null, null) as DataProviderBase;
                reportDataProvider.UserId = fp.UserId;
                reportDataProvider.CustomerIds = fp.CustomerIds;
                reportDataProvider.ReferenceDate = fp.ReferenceDate;
                reportDataProvider.HolderIds = fp.HolderIds;
                reportDataProvider.PortfolioId = fp.PortfolioId;
                reportDataProvider.ProposalId = fp.ProposalId;
                // Fetch Data
                var methodInfo = typeof(DataProviderBase).GetMethod("FetchData");
                var fetchDataMI = methodInfo.MakeGenericMethod(reportDataType);
                fetchDataMI.Invoke(reportDataProvider, new[] { reportSettings.RemoteCall, reportSettings.Path });

                // Creation of appropriate reportDefinition
                ReportDefinitionBase reportDefinition = Activator.CreateInstance(reportDefinitionType) as ReportDefinitionBase;

                // Setting resources
                ReportResources.Instance.SetFooterImage(reportDefinition.FooterImage);
                ReportResources.Instance.SetFeaturingColor(reportDefinition.FeaturingColor);
                ReportResources.Instance.SetSectionLabelWidth(reportDefinition.SectionLabelWidth);
                ReportResources.Instance.SetPageOrientation(reportDefinition.PageOrientation);
                ReportResources.Instance.SetWatermarkImage(reportDefinition.WatermarkImage);

                // Istanzio la main Window
                var vm = IoC.Get<MainWindowViewModel>();

                // Setting ViewModel properties
                vm.ReportDefinition = reportDefinition;
                vm.PdfFileName = fp.PdfFileName;
                vm.XpsFileName = fp.XpsFileName;

                // Bind View to Model and Exec
                List<string> tempFiles = new List<string>();
                Execute.OnUIThread(() =>
                {
                    try
                    {
                        logger.Information("Rendering report request:");
                        var view = new MainWindowView();
                        ViewModelBinder.Bind(vm, view, null);
                        view.Show();
                        Coroutine.BeginExecute(Save(view, vm, me, tempFiles).GetEnumerator());

                    }
                    catch (Exception e)
                    {
                        logger.Error(e.Message);
                        me.Set();
                    }
                });
                me.WaitOne();
                if (File.Exists(vm.PdfFileName))
                {
                    logger.Warning("File generated in fork mode");
                    if (File.Exists(vm.XpsFileName))
                        File.Delete(vm.XpsFileName);
                    logger.Warning(String.Format("Generated file:{0}", vm.PdfFileName));

                }
                else
                {
                    logger.Error(string.Format("Expected file {0} not produced", vm.PdfFileName));
                }
                Process.GetCurrentProcess().Kill();
                return null;
            }
        }

        private IEnumerable<IResult> Save(MainWindowView view, MainWindowViewModel vm, ManualResetEvent me, List<string> tempFiles)
        {
            //God bless me, this is really a bad hack
            //To understand when layout is really done...
            int cnt = 0;
            view.LayoutUpdated += (s, e) =>
            {
                cnt++;
            };
            view.UpdateLayout();

            int prevCnt;
            do
            {
                prevCnt = cnt;
                yield return new CoMethod(() => { Thread.Sleep(100); });
                //this basically mean, when layout does not change anymore during a sleep period
                //layout is done
            } while (prevCnt != cnt);
            var viewer = InternalTreeHelper.FindChild<DocumentViewer>(view, null);
            var doc = viewer.Document as FixedDocument;


            logger.Information("Saving XPS");
            doc.Save(vm.XpsFileName);

            //remove next two line to disable enhanchements
            Enhanche3dViewports(doc, tempFiles);
            view.UpdateLayout();

            doc.Save(vm.XpsFileName);
            logger.Information("Saving PDF");

            yield return new CoMethod(() =>
            {
                XpsToPdf converter = new XpsToPdf(vm.XpsFileName, vm.PdfFileName);
                converter.Convert();
            }
            );
            view.Close();
            me.Set();
        }

        private void Enhanche3dViewports(FixedDocument doc, List<string> tempFiles)
        {
            foreach (PageContent pc in doc.Pages)
            {
                var page = pc.Child as FrameworkElement;
                var piechart = InternalTreeHelper.FindChild<Viewport3D>(page, "piechart");
                if (null != piechart)
                {
                    //ImageSource brush = VisualSaver.HiResSave(piechart);
                    ImageSource brush = VisualSaver.HiResGLSave(piechart, PieChart3d.Palette, tempFiles);


                    var twodLayer = InternalTreeHelper.FindChild<Canvas>(page, "TwoDOverlay");
                    var img = new Image() { Source = brush, Stretch = Stretch.Uniform };
                    Canvas.SetTop(img, PieChart3d.GetVMargin(piechart));
                    Canvas.SetLeft(img, PieChart3d.GetHMargin(piechart));

                    twodLayer.Children.Insert(0, img);
                    Panel.SetZIndex(twodLayer, 100);

                    piechart.Visibility = Visibility.Hidden;
                }
            }
        }

        private ForkedParameters PrepareForExec(string userId, string customerIDS, string referenceDate, string holderIds, string reportKind, string portfolioId, string proposalId)
        {
            ForkedParameters fp = new ForkedParameters();
            var paramFile = PathExtension.GetTempFileWithExtension("txt");
            var pdfFileName = PathExtension.GetTempFileWithExtension("pdf");
            var xpsFileName = PathExtension.GetTempFileWithExtension("xps");
            fp = new ForkedParameters();
            fp.CustomerIds = customerIDS;
            fp.PdfFileName = pdfFileName;
            fp.UserId = userId;
            fp.XpsFileName = xpsFileName;
            fp.ReferenceDate = referenceDate;
            fp.ParamsFileName = paramFile;
            fp.ReportKind = reportKind;
            fp.HolderIds = holderIds;
            fp.PortfolioId = portfolioId;
            fp.ProposalId = proposalId;
            using (var sw = new StreamWriter(paramFile))
            {
                var s = JsonSerializer.Serialize(fp);
                sw.Write(s);
            }
            logger.Information("PrepareForExec parameter file is: {0}", fp.ParamsFileName);
            return fp;
        }

        private void Spawn(ForkedParameters fp)
        {
            logger.Warning("Forking process");
            var exeName = Assembly.GetExecutingAssembly().Location.Replace(".dll",".exe");
            string parameters = string.Concat("-f ", @"-p:""", fp.ParamsFileName, @"""");
            var process = Process.Start(exeName, parameters);
            process.WaitForExit();
        }

        private void SpawnAsynch(ForkedParameters fp, IAsyncResult res, ManualResetEvent mre)
        {

            logger.Warning("Forking process asynchronously");
            var exeName = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
            ProcessStartInfo pi = new ProcessStartInfo(exeName, string.Concat("-f ", @"-p:""", fp.ParamsFileName, @""""));
            var process = new Process();
            process.StartInfo = pi;
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                if (File.Exists(fp.PdfFileName))
                {
                    mre.Set();

                }
                else
                {
                    Thread.Sleep(2000);
                    mre.Set();
                }
            };
            process.Start();
        }

        private Stream EpilogueOfExec(ForkedParameters fp)
        {
            File.Delete(fp.ParamsFileName);
            if (File.Exists(fp.PdfFileName))
            {
                var b = File.ReadAllBytes(fp.PdfFileName);
                logger.Warning("Sending back pdf:{0} {1} bytes long", fp.PdfFileName, b.Length);
                MemoryStream stream = new MemoryStream(b);
                //OperationContext.Current.OutgoingResponse.ContentType = "application/pdf";
                stream.Position = 0;
#if CLEANUP
				if (File.Exists(fp.PdfFileName))
					File.Delete(fp.PdfFileName);
#endif
                return stream;
            }
            else
            {
                logger.Error("Error:Pdf not generated");
                //WebOperationContext.Current.OutgoingResponse.ContentType = "application/pdf";
                return new MemoryStream(new byte[] { });
            }
        }
    }
}

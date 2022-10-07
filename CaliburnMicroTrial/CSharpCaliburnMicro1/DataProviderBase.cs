using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpCaliburnMicro1.Helpers;
using Newtonsoft.Json;

namespace CSharpCaliburnMicro1
{
    public abstract class DataProviderBase
    {
        private Serilog.ILogger logger;

        public DataProviderBase()
        {
            this.logger = Serilog.Log.Logger;
        }

        public IReportData ReportData { get; set; }

        public string UserId { get; set; }
        public string CustomerIds { get; set; }
        public string ReferenceDate { get; set; }
        public string HolderIds { get; set; }

        public string PortfolioId { get; set; }

        /// <summary>
        /// Aggiunto nuovo parametro per i report Retail.
        /// In attesa della gestione dinamica dei parametri (https://redmine.ors.it/issues/20830)
        /// </summary>
        public string ProposalId { get; set; }

        public virtual void FetchData<T>(string call, string path)
            where T : IReportData
        {
            ManualResetEvent me = new ManualResetEvent(false);


#if !MOCK

            string mockDataJson = AppEnvironment.Instance.MockDataJson;
            if (!string.IsNullOrEmpty(mockDataJson) && File.Exists(mockDataJson))
            {
                this.logger.Information(string.Format("DataProviderBase.FetchData found mock json: {0}. (see appsetting DataProviderBase.MockData.Json)", mockDataJson));
                GetValueFromMock<T>(me, mockDataJson);
            }
            else
            {
                this.LogInfo(String.Format("Retrieving data for CustomerId={0} @{1}", CustomerIds, ReferenceDate));


                var client = new JSonHelperClient<T>(AppEnvironment.Instance.SaveJson);
                client.Server = AppEnvironment.Instance.DataUrl;
                client.Parameters.Add(new KeyValuePair<string, object>("userId", UserId));
                client.Parameters.Add(new KeyValuePair<string, object>("customerIds", CustomerIds));
                client.Parameters.Add(new KeyValuePair<string, object>("referenceDate", ReferenceDate));
                client.Parameters.Add(new KeyValuePair<string, object>("holderIds", HolderIds));
                client.Parameters.Add(new KeyValuePair<string, object>("portfolioId", PortfolioId));
                client.Parameters.Add(new KeyValuePair<string, object>("proposalId", ProposalId));
                client.Call(call, path);
                client.Completed = (k, s) =>
                {
                    this.LogInfo(String.Format("from url:{2}\nData for CustomerId={0} @{1} succesfully got", CustomerIds, ReferenceDate, s));
                    ReportData = k;
                    if (ReportData == null)
                    {
                        this.LogDebug("ReportData is null.");
                    }
                    me.Set();
                };
                client.Failed = (e) =>
                {
                    this.LogError(string.Format("Error retriving data for CustomerId={0} @{1}", CustomerIds, ReferenceDate), e);
                    ReportData = Activator.CreateInstance<T>();
                    ReportData.HasError = true;
                    ReportData.ErrorMessage = e.Message;

                    me.Set();
                };
                me.WaitOne();
            }

#else
            logger.InfoFormat("DataProviderBase: MOCK mode");
            var path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                              "MockData\\" + CustomerIDS + ".json");
            GetValueFromMock<T>(me,path);
#endif
        }


        private void GetValueFromMock<T>(ManualResetEvent me, string mockFilePath) where T : IReportData
        {
            logger.Information(String.Format("DataProviderBase: Mock Data file is: {0}", mockFilePath));
            var ser = JsonSerializer.Create(new JsonSerializerSettings());
            ReportData = ser.Deserialize<T>(new JsonTextReader(new System.IO.StreamReader(mockFilePath)));
            me.Set();
            logger.Information(String.Format("DataProviderBase: Mock Data read."));
        }


        public void LogInfo(string message)
        {
            if (this.logger != null)
            {
                this.logger.Information(message);
            }
        }

        public void LogWarn(string message)
        {
            if (this.logger != null)
            {
                this.logger.Warning(message);
            }
        }

        public void LogDebug(string message)
        {
            if (this.logger != null)
            {
                this.logger.Debug(message);
            }
        }

        public void LogError(string message, Exception exception)
        {
            if (this.logger != null)
            {
                this.logger.Error(message, exception);
            }
        }

    }
}

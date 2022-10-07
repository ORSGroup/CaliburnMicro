using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace CSharpCaliburnMicro1.Helpers
{
    public class JSonHelperClient<T>
    {
        const SecurityProtocolType SecurityProtocolType_Tls12 = (SecurityProtocolType)3072;

        String saveJson;
        private Serilog.ILogger logger;

        public JSonHelperClient(string saveJson)
        {
            this.logger = Serilog.Log.Logger;
            Parameters = new List<KeyValuePair<string, object>>();
            this.saveJson = saveJson;
        }
        public Action<T, string> Completed;
        public Action<Exception> Failed;
        public Action Cancelled;
        public string Server { get; set; }
        public List<KeyValuePair<string, object>> Parameters { get; private set; }
        public void Call(string methodName, string path)
        {
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.UTF8Encoding.UTF8;
            string uriString = GetUriString(methodName, path);
            this.logger.Information("Uri:" + uriString);
            // Autenticazione ripresa dati #36143
            AddHeaderByAuthType(wc);

            wc.DownloadStringCompleted += (s, e) =>
            {
                var ser = JsonSerializer.Create(new JsonSerializerSettings());
                if (e.Error != null)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "An error occurred while executing method {0}: {1}", methodName, e.Error.Message);
                    this.OnFailed(e.Error);
                }
                else if (e.Cancelled)
                {
                    this.OnCancelled();
                }
                else
                {
                    this.logger.Debug("Json received.");
                    if (!String.IsNullOrEmpty(saveJson))
                    {
                        this.logger.Debug(string.Format("Start writing json to file {0}", saveJson));
                        using (StreamWriter sW = new StreamWriter(saveJson))
                        {
                            sW.Write(e.Result);
                            this.logger.Debug(string.Format("Json written to file {0}", saveJson));
                        }
                    }
                    T result;
                    if (string.IsNullOrEmpty(e.Result))
                    {
                        this.logger.Debug(string.Format("Json is empty. ReportData will be initialized to default of type \"{0}\" .", typeof(T).FullName));
                        result = default(T);
                    }
                    else
                    {
                        result = ser.Deserialize<T>(new JsonTextReader(new StringReader(e.Result)));
                    }
                    this.OnCompleted(result, uriString);
                }
            };

            // 36211: aggiunto per gestire schema https 
            var uri = new Uri(uriString);
            if (uri.Scheme == "https")
            {
                ServicePointManager.Expect100Continue = true;
                // Use SecurityProtocolType.Tls12 if needed for compatibility reasons
                ServicePointManager.SecurityProtocol = SecurityProtocolType_Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }

            wc.DownloadStringAsync(uri);
        }

        protected virtual void OnCancelled()
        {
            if (this.Cancelled != null)
            {
                this.Cancelled();
            }
        }

        protected virtual void OnCompleted(T result, string uri)
        {
            if (this.Completed != null)
            {
                this.Completed(result, uri);
            }
        }

        protected virtual void OnFailed(Exception exception)
        {
            JSonClientException jSonClientException;
            if (exception == null)
            {
                jSonClientException = new JSonClientException(string.Format(CultureInfo.InvariantCulture, "An unknown error occurred in the JSon client."));
            }
            else
            {
                jSonClientException = new JSonClientException(exception.Message, exception);
            }
            if (this.Failed != null)
            {
                this.Failed(jSonClientException);
            }
        }

        private string GetUriString(string methodName, string path)
        {
            // NB solo uno dei 2 parametri può essere nullo
            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Purge(Server), methodName ?? path);
            if (this.Parameters.Count > 0)
            {
                // codice originale per chiamate ai servizi WCF
                // ad esempio: http://localhost/Ors.Rams.ReportServices.Aletti/ReportDataProvider.svc/GetWMAdvisoryReportData?userId=864&customerIds=55725&referenceDate=20130611
                var res = string.Concat(uri, "?", MakeQS());
                return res;
            }
            return uri;
        }

        private static string Purge(string Server)
        {
            return Server.TrimEnd('/');
        }

        private string MakeQS()
        {
            return string.Join("&", this.Parameters.Where(v => v.Value != null)
                                                   .Select(p => string.Concat(p.Key, "=", Uri.EscapeDataString(p.Value.ToString())))
                                                   .ToArray());
        }

        /// <summary>
        /// Creazione header X-FRAMS4-UserName a partire da query string 
        /// </summary>
        /// <param name="wc"></param>
        private void AddHeaderByAuthType(WebClient wc)
        {
            try
            {
                var authType = AppEnvironment.Instance.AuthType ?? "disabled";
                if (authType.ToUpper() == "USERIDHEADER")
                {
                    string headerName = AppEnvironment.Instance.AuthHeaderName;
                    if (string.IsNullOrEmpty(headerName))
                        this.logger.Warning(string.Format("Authentication.HeaderName config setting is missing!"));
                    var headerValue = GetHeaderValue(headerName);
                    this.logger.Information(string.Format("Adding authentication header, name={0} value={1}", headerName, headerValue));
                    wc.Headers.Add(headerName, headerValue);
                }
                else if (authType.ToUpper() == "DISABLED")
                {
                    this.logger.Information(string.Format("Skipping authentication, type={0}", authType));
                }
                else
                {
                    this.logger.Warning(string.Format("Unmanaged authentication, type={0}", authType));
                }
            }
            catch (Exception e)
            {
                this.logger.Error(string.Format("Failed adding header, error={0}", e.Message));
            }
        }

        private string GetHeaderValue(string headerName)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentNullException("headerName");
            }

            this.logger.Debug(string.Format("Trying to get parameter value by header {0}", headerName));
            if (headerName.ToUpper() == "X-FRAMS4-USERNAME")
            {
                var header = this.Parameters
                    .Where(v => v.Value != null)
                    .FirstOrDefault(p => p.Key == "userId");
                var headerValue = Uri.EscapeDataString(header.Value.ToString());
                this.logger.Debug(string.Format("Got parameter value {0} by header {1}", headerValue, headerName));
                return headerValue;
            }
            return null;
        }
    }
}

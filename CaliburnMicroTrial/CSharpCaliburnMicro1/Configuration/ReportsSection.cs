using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Configuration
{
    public class ReportsSection
    {

        private Report[] reports;

        public Report[] Reports
        {
            get
            {
                if (this.reports == null)
                {
                    this.reports = new Report[0];
                }
                return this.reports;
            }
            set { this.reports = value; }
        }

    }

    public class Report
    {

        private string name;
        [JsonProperty("name")]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        private string remoteCall;
        [JsonProperty("remoteCall")]
        public string RemoteCall
        {
            get { return this.remoteCall; }
            set { this.remoteCall = value; }
        }

        private string definition;
        [JsonProperty("definition")]
        public string Definition
        {
            get { return this.definition; }
            set { this.definition = value; }
        }

        private string path;
        [JsonProperty("path")]
        public string Path
        {
            get { return this.path; }
            set { this.path = value; }
        }

    }
}

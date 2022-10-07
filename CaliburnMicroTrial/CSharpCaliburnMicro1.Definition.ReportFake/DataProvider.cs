using CSharpCaliburnMicro1.Definition.ReportFake.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Definition.ReportFake
{
    public class DataProvider : DataProviderBase
    {
        static DataProvider instance;
        public static DataProvider Instance
        {
            get
            {
                lock (typeof(DataProvider))
                {
                    if (null == instance)
                        instance = new DataProvider();
                }
                return instance;
            }
        }
        protected DataProvider()
            : base()
        {
        }

        public ReportData WSData
        {
            get
            {
                return (ReportData) this.ReportData;
            }
        }
    }
}

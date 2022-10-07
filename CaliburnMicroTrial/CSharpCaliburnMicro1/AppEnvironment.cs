using CSharpCaliburnMicro1.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1
{
    internal class AppEnvironment
    {
		public string AuthHeaderName { get; set; }
		public string AuthType { get; set; }
		public string DataUrl { get; set; }
		public string SaveJson { get; set; }
		public string MockDataJson { get; set; }
		public string Xps2Pdf { get; set; }


		public string Xps { get; set; }
		public bool Forked { get; set; }
		public string ParamFiles { get; set; }

		static AppEnvironment instance;
		private AppEnvironment()
		{
			var os = new OptionSet() {
				{ "xps:",  k=>Xps=Environment.ExpandEnvironmentVariables(k)    },
				{ "f",  k=>Forked=true    },
				{ "p:",  k=>ParamFiles=k    }
			};
			os.Parse(Environment.GetCommandLineArgs());
		}

		static public AppEnvironment Instance
		{
			get
			{
				lock (typeof(AppEnvironment))
				{
					if (null == instance)
						instance = new AppEnvironment();
				}
				return instance;
			}
		}
	}
}

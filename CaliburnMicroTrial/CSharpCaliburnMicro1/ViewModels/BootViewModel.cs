using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSharpCaliburnMicro1.ViewModels
{
    public class BootViewModel
    {
		public BootViewModel()
		{
			Log = Bootstrapper.Log;
		}
		public void Closed()
		{
		}
		public void Shutdown()
		{
			Application.Current.Shutdown();
		}
		public LogViewModel Log { get; set; }
	}
}

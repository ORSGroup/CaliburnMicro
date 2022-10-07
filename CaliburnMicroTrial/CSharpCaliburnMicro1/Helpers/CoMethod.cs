using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Helpers
{
	public class CoMethod : AbstractBackgroundAction
	{
		Action toRun;
		public CoMethod(Action toRun)
		{
			this.toRun = toRun;
		}

		protected override void OnExecute(Caliburn.Micro.CoroutineExecutionContext context)
		{
			toRun();
		}
	}
}

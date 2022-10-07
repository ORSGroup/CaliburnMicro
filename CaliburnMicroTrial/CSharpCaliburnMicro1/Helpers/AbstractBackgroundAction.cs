using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace CSharpCaliburnMicro1.Helpers
{
	public abstract class AbstractBackgroundAction : IResult
	{
		abstract protected void OnExecute(CoroutineExecutionContext context);

		#region IResult Members

		public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

		public void Execute(CoroutineExecutionContext context)
        {
			using (BackgroundWorker bw = new BackgroundWorker())
			{
				Exception exception = null;
				bw.DoWork += (s, e) =>
				{
					try
					{
						OnExecute(context);
					}
					catch (Exception workException)
					{
						exception = workException;
					}
				};
				bw.RunWorkerCompleted += (s, e) =>
				{
					Completed(this, new ResultCompletionEventArgs { Error = exception });
				};
				bw.RunWorkerAsync();
			}
		}

        #endregion
    }
}

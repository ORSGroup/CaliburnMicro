using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;

namespace CSharpCaliburnMicro1.Helpers
{
	internal class XpsToPdf
	{
		private Serilog.ILogger logger;

		string xps;
		string pdf;
		public XpsToPdf(string xps, string pdf)
		{
			this.xps = xps;
			this.pdf = pdf;
			logger = Log.Logger;
		}

		public void Convert()
		{
			var fmt = AppEnvironment.Instance.Xps2Pdf;
			if (string.IsNullOrEmpty(fmt))
			{
				logger.Error("Conversion can't be performed since xps2pdf setting is not present in configuration.");
			}
			else
			{
				var tokens = Regex.Split(fmt, "->");


				ProcessStartInfo processStartInfo = new ProcessStartInfo(tokens[0]);
				if (tokens.Length > 1)
				{
					processStartInfo.Arguments = string.Format(tokens[1], @"""" + xps + @"""", @"""" + pdf + @"""");
				}
				processStartInfo.CreateNoWindow = true;
				logger.Information(string.Format("Spawning converter {0} {1}", processStartInfo.FileName, processStartInfo.Arguments));

				processStartInfo.RedirectStandardError = true;
				processStartInfo.RedirectStandardOutput = true;
				processStartInfo.ErrorDialog = false;

				processStartInfo.RedirectStandardInput = true;
				processStartInfo.UseShellExecute = false;
				try
				{
					using (Process process = new Process())
					{
						process.StartInfo = processStartInfo;
						process.ErrorDataReceived += process_ErrorDataReceived;
						process.OutputDataReceived += process_OutputDataReceived;

						Stopwatch sw = new Stopwatch();
						sw.Start();
						process.Start();

						process.BeginErrorReadLine();
						process.BeginOutputReadLine();
						process.StandardInput.Close();
						if (false == process.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds))
							throw new ArgumentException("The program '{0}' did not finish in time, aborting.");
						sw.Stop();
					}
				}
				catch (Exception e)
				{
					throw new Exception("Fatal error spawning Converter. See inner exception for details.", e);
				}
			}
		}

		void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			logger.Information(e.Data);
		}

		void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			logger.Warning(e.Data);
		}

	}
}

using Caliburn.Micro;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1
{
    internal class TraceLogger : ILog
    {
        private Type t;
        private Serilog.ILogger logger;

        public TraceLogger(Type t)
        {
            this.t = t;
            logger = Log.Logger;
        }

        void ILog.Error(Exception exception)
        {
            Trace.WriteLine(t.Name + "-ERROR:" + exception.Message);
            logger.Error("Exception", exception);
        }

        void ILog.Info(string format, params object[] args)
        {
            Trace.WriteLine(t.Name + "-INFO:" + string.Format(format, args));
            logger.Information(string.Format(format, args));
        }

        void ILog.Warn(string format, params object[] args)
        {
            Trace.WriteLine(t.Name + "-WARN:" + string.Format(format, args));
            logger.Warning(string.Format(format, args));
        }
    }
}

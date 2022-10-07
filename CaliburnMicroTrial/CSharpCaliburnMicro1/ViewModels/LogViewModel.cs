using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CSharpCaliburnMicro1.ViewModels
{
    public class LogViewModel : Caliburn.Micro.PropertyChangedBase, Serilog.ILogger
    {
        public LogViewModel()
        {
            LogMessages = new ObservableCollection<LogEvent>();
            FilteredMessages = CollectionViewSource.GetDefaultView(LogMessages);
            Errors = Warnings = Messages = Debugs = true;
        }

        void Serilog.ILogger.Write(LogEvent logEvent)
        {
            Caliburn.Micro.Execute.OnUIThread(() =>
            {
                if (LogMessages.Count > 100)
                    LogMessages.RemoveAt(LogMessages.Count - 1);
                LogMessages.Insert(0, logEvent);
                NotifyOfPropertyChange(() => ErrorsCount);
                NotifyOfPropertyChange(() => DebugsCount);
                NotifyOfPropertyChange(() => WarningsCount);
                NotifyOfPropertyChange(() => MessagesCount);

            });
        }

        public ICollectionView FilteredMessages { get; private set; }
        public ObservableCollection<LogEvent> LogMessages { get; set; }

        private bool errors;
        public bool Errors
        {
            get { return errors; }
            set { errors = value; }
        }
        private void SetFilter()
        {
            FilteredMessages.Filter = (k) =>
            {
                var item = k as LogEvent;
                return item.Level == LogEventLevel.Information && Messages
                    ||
                    item.Level == LogEventLevel.Error && Errors
                    ||
                    item.Level == LogEventLevel.Debug && Debugs
                    ||
                    item.Level == LogEventLevel.Warning && Warnings
                    ;
            };
        }

        public int ErrorsCount
        {
            get { return LogMessages.Where(z => z.Level == LogEventLevel.Error).Count(); }
        }

        public int WarningsCount
        {
            get { return LogMessages.Where(z => z.Level == LogEventLevel.Warning).Count(); }
        }
        private bool warnings;

        public bool Warnings
        {
            get { return warnings; }
            set { warnings = value; SetFilter(); }
        }

        public int MessagesCount
        {
            get { return LogMessages.Where(z => z.Level == LogEventLevel.Information).Count(); }
        }

        private bool messages;

        public bool Messages
        {
            get { return messages; }
            set { messages = value; SetFilter(); }
        }

        public int DebugsCount
        {
            get { return LogMessages.Where(z => z.Level == LogEventLevel.Debug).Count(); }
        }

        private bool debugs;

        public bool Debugs
        {
            get { return debugs; }
            set { debugs = value; SetFilter(); }
        }
    }
}

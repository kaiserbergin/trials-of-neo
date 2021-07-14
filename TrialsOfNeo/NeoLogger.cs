using System;
using Neo4j.Driver;

namespace TrialsOfNeo
{
    public class NeoLogger : ILogger
    {
        private readonly Serilog.ILogger _serilogger;
        private readonly bool _isDebugEnabled;
        private readonly bool _isTraceEnabled;

        public NeoLogger(Serilog.ILogger serilogger, NeoDriverConfigurationSettings settings = null)
        {
            _serilogger = serilogger ?? throw new ArgumentNullException(nameof(serilogger));
            _isDebugEnabled = settings?.IsDebugLoggingEnabled ?? false;
            _isTraceEnabled = settings?.IsTraceLoggingEnabled ?? false;
        }

        public void Error(Exception cause, string message, params object[] args) => 
            _serilogger.Error(cause, message, args);

        public void Warn(Exception cause, string message, params object[] args) =>
            _serilogger.Warning(cause, message, args);

        public void Info(string message, params object[] args) =>
            _serilogger.Information(message, args);

        public void Debug(string message, params object[] args) =>
            _serilogger.Debug(message, args);

        public void Trace(string message, params object[] args) =>
            _serilogger.Verbose(message, args);

        public bool IsTraceEnabled() => _isTraceEnabled;

        public bool IsDebugEnabled() => _isDebugEnabled;
    }
}
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Logger
{
    public class Logger
    {
        private static readonly TelemetryClient clientTelemetry;
        private static readonly TelemetryConfiguration conf=TelemetryConfiguration.CreateDefault();


        static Logger()
        {
            clientTelemetry = new TelemetryClient(conf);
        }

        public static void Log(string serviceId, int logType, string logMessage, Dictionary<string, string> fields = null)
        {
            clientTelemetry.TrackTrace($"{serviceId} : {logMessage}", (SeverityLevel)logType, fields);
        }

        public static void Log(string serviceId, int logType, Exception error, string logMessage, Dictionary<string, string> fields = null)
        {
            var exceptionTelemetry = new ExceptionTelemetry()
            {
                Exception = error,
                SeverityLevel = (SeverityLevel)logType,
                Message = logMessage,
                ProblemId = serviceId                
            };
            
            clientTelemetry.TrackException(exceptionTelemetry);
        }
        public static void LogException(Exception e, IDictionary<string, string> properties)
        {
            clientTelemetry.TrackException(e, properties);
        }

        public static void Log(Exception ex)
        {
            clientTelemetry.TrackException(ex);
        }

        public static void TrackEvent(string eventName, string accountCode, IDictionary<string, string> properties = null)
        {
            clientTelemetry.TrackEvent(eventName, properties);
        }

        public enum InsightsLogType
        {
            Verbose = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Critical = 4
        }
    }
}

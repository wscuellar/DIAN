using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.logger
{
    public class LoggerService
    {
        private static readonly TelemetryClient clientTelemetry;

        static LoggerService()
        {
            clientTelemetry = new TelemetryClient();
        }
        
        public static void Log(string serviceId, int logType, string logMessage, Dictionary<string, string> fields)
        {
            var message = serviceId + " : " + logMessage;
            clientTelemetry.TrackTrace(message, (SeverityLevel)logType, fields);
        }

        public static void Log(string serviceId, int logType, Exception error, string logMessage, Dictionary<string, string> fields)
        {
            var exceptionTelemetry = new ExceptionTelemetry()
            {
                Exception = error,
                SeverityLevel = (SeverityLevel)logType,
                Message = logMessage,
                ProblemId = serviceId,
            };

            clientTelemetry.TrackException(exceptionTelemetry);
        }

        public static void Log(Exception ex)
        {
            clientTelemetry.TrackException(ex);
        }

        public static void TrackEvent(string eventName, string accountCode, IDictionary<string, string> properties = null)
        {
            clientTelemetry.TrackEvent(eventName, properties);
        }

    }
}

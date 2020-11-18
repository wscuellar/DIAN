using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class RadianLogger : TableEntity
    {
        public RadianLogger()
        {
        }

        public RadianLogger(string pk, string rk) : base(pk, rk)
        {
        }

        public string Action { get; set; }
        public string Controller { get; set; }
        public string Message { get; set; }
        public string RouteData { get; set; }
        public string StackTrace { get; set; }
    }
}

using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class DianEmailTracker : TableEntity
    {
        public DianEmailTracker() { }

        public DianEmailTracker(string pk, string rk) : base(pk, rk)
        {
        }

        public string Description { get; set; }
        public string Receiver { get; set; }
        public string Sender { get; set; }
        public int Status { get; set; }
        public string TrackId { get; set; }
    }
}

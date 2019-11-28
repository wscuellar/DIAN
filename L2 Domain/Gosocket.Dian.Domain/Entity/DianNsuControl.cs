using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class DianNsuControl : TableEntity
    {
        public DianNsuControl() { }
        public DianNsuControl(string pk, string rk) : base(pk, rk)
        { }
        public string SenderCode { get; set; }
        public string ReceiverCode { get; set; }
        public long Actual { get; set; }
        public long Last { get; set; }
        public string DocumentKey { get; set; }
        public string DianNsu { get; set; }
    }
}

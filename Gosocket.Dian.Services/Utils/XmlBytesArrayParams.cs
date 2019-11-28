
namespace Gosocket.Dian.Services.Utils
{
    public class XmlBytesArrayParams
    {
        public string XmlFileName { get; set; }
        public byte[] XmlBytes { get; set; }
        public string XmlErrorCode { get; set; }
        public string XmlErrorMessage { get; set; }
        public bool HasError { get; set; }
        public bool MaxQuantityAllowedFailed { get; set; }
        public bool UnzipError { get; set; }
    }
}

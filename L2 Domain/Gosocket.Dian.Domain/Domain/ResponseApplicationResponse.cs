namespace Gosocket.Dian.Domain.Domain
{
    public class ResponseApplicationResponse
    {
        public byte[] Content { get; set; }
        public string DocumentKey { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}

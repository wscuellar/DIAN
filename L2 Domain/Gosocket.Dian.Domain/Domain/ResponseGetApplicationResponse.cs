namespace Gosocket.Dian.Domain.Domain
{
    public class ResponseGetApplicationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ContentBase64 { get; set; }
        public byte[] Content { get; set; }
    }
}

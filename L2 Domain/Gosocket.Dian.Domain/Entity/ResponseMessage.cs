namespace Gosocket.Dian.Domain.Entity
{
    public class ResponseMessage
    {
        public ResponseMessage()
        {
            Code = 200;
        }

        public ResponseMessage(string message, string messageType)
        {
            Message = message;
            MessageType = messageType;
            Code = 200;
        }

        public ResponseMessage(string message, string messageType, int code)
        {
            Message = message;
            MessageType = messageType;
            Code = code;
        }

        public string data { get; set; }
        public string RedirectTo { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
        public int Code { get; set; }
        public bool ExistOperationModeAsociated { get; set; }
    }
}

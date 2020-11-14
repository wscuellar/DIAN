namespace Gosocket.Dian.Domain.Entity
{
    public class RadianRegistrationValidation
    {
        public RadianRegistrationValidation()
        {

        }

        public RadianRegistrationValidation(string message, string messageType)
        {
            Message = message;
            MessageType = messageType;
        }


        public string Message { get; set; }
        public string MessageType { get; set; }

    }
}

using System.Collections.Generic;

namespace Gosocket.Dian.Domain.Utils
{
    public enum ErrorTypes
    {
        NoError,
        InternalError,
        InputError,
        Warning
    }

    public class EmailSenderResponse
    {
        public string MessageId { get; set; }
        public ErrorTypes ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public List<EmailSenderResponseDetails> ResponseDetails { get; set; }

        public EmailSenderResponse(string messageId, ErrorTypes errorType, string errorMessage, List<EmailSenderResponseDetails> responseDetails)
        {
            MessageId = messageId;
            ErrorType = errorType;
            ErrorMessage = errorMessage;
            ResponseDetails = responseDetails;
        }
    }


    public class EmailSenderResponseDetails
    {
        public string Email { get; set; }
        public bool Sucess { get; set; }
        public int Errorcode { get; set; }
        public string ErrorMessage { get; set; }

        public EmailSenderResponseDetails(string email, bool sucess, int errorcode, string errorMessage)
        {
            Email = email;
            Sucess = sucess;
            Errorcode = errorcode;
            ErrorMessage = errorMessage;
        }
    }
}

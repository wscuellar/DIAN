using System.Web.Services.Protocols;

namespace Gosocket.Dian.Web.Services.Code
{
    public class AuthSignedMessage : SoapHeader
    {
        public string MessageEncoded;
        public string TokenGenerated;
    }
}
using System.Web.Services.Protocols;

namespace Gosocket.Dian.Web.Services.Code
{
    public class AuthUser : SoapHeader
    {
        public string UserName;
        public string Password;
    }
}
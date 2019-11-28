using System;
using System.Web.Services.Protocols;

namespace Gosocket.Dian.Web.Services.Code
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthExtensionAttribute : SoapExtensionAttribute
    {
        int _priority = 1;

        public override int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public override Type ExtensionType
        {
            get { return typeof(AuthExtension); }
        }
    }
}
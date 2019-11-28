using System.IdentityModel.Tokens;

namespace Gosocket.Dian.Web.Services.Validator
{
    public class CustomIssuerNameRegistry : IssuerNameRegistry
    {
        public override string GetIssuerName(SecurityToken securityToken)
        {
            var x509SecurityToken = securityToken as X509SecurityToken;
            if (x509SecurityToken == null)
                return null;

            return x509SecurityToken.Certificate.Issuer;
        }
    }
}
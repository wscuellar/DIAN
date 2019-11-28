using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Gosocket.Dian.Web.Services.Validator
{
    public class CertificateClaimsAuthenticationManager : ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            var subject = incomingPrincipal.FindFirst(ClaimTypes.X500DistinguishedName);
            var claims = incomingPrincipal.FindAll(c => !c.Type.Equals(ClaimTypes.Name)).ToList();

            var parts = GetSubjectInfo(subject.Value);
            string email = "";
            string nit = "";
            if (parts.Keys.Contains("E"))
                email = $"({parts["E"].ToLower()})";

            if (parts.Keys.Contains("1.3.6.1.4.1.23267.2.3"))
                nit = ExtractNumbers(parts["1.3.6.1.4.1.23267.2.3"]);
            else if (parts.Keys.Contains("OID.1.3.6.1.4.1.23267.2.3"))
                nit = ExtractNumbers(parts["OID.1.3.6.1.4.1.23267.2.3"]);
            else if (parts.Keys.Contains("SERIALNUMBER"))
                nit = ExtractNumbers(parts["SERIALNUMBER"]);
            else if (parts.Keys.Contains("SN"))
                nit = ExtractNumbers(parts["SN"]);
            else if (parts.Keys.Contains("1.3.6.1.4.1.31136.1.1.20.2"))
                nit = ExtractNumbers(parts["1.3.6.1.4.1.31136.1.1.20.2"]);
            else if (parts.Keys.Contains("2.5.4.97"))
                nit = ExtractNumbers(parts["2.5.4.97"]);
            else if (parts.Keys.Contains("OID.2.5.4.97"))
                nit = ExtractNumbers(parts["OID.2.5.4.97"]);


            claims.Add(new Claim(CustomClaimTypes.AuthCode, nit));
            claims.Add(new Claim(CustomClaimTypes.AuthEmail, email));
            var identity = new ClaimsIdentity(claims);
            return new ClaimsPrincipal(identity);
        }

        private string ExtractNumbers(string input)
        {
            return Regex.Replace(input, @"[^\d]", string.Empty);
        }
        private Dictionary<string, string> GetSubjectInfo(string subject)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                string[] subjectSplited = subject.Split(',');
                foreach (var item in subjectSplited)
                {
                    string[] itemSplit = item.Split('=');
                    result.Add(itemSplit[0].Trim(), itemSplit[1].Trim());
                }
            }
            catch { return result; }
            return result;
        }
    }
}
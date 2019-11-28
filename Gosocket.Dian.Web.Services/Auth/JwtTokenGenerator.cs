using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gosocket.Dian.Web.Services.Auth
{
    public class JwtTokenGenerator
    {
        public JwtTokenGenerator() { }

        public string GenerateToken(X509Certificate2 paCertificate)
        {
            //Crear claims con datos obligatorios para el JwtToken y otros datos del Certificado
            //que pudiera hacer falta pensando en validaciones
            var ident = new ClaimsIdentity(
                new[]
                {
                        new Claim("jti", Guid.NewGuid().ToString()),
                        new Claim("aud", "https://www2.gosocket.net/"),
                        new Claim("iat", DateTime.UtcNow.ToString()),
                        new Claim("nbf", DateTime.UtcNow.ToString()),
                        new Claim("exp", DateTime.Now.AddDays(90).ToString()),
                        new Claim("iss", paCertificate.Issuer),
                        new Claim("sub", paCertificate.Subject),
                        new Claim(ClaimTypes.Thumbprint, paCertificate.Thumbprint),
                        new Claim(ClaimTypes.SerialNumber, paCertificate.SerialNumber),
                        new Claim(ClaimTypes.Name, "test"),
                        new Claim(ClaimTypes.Role, "PA"),
                }, "TestAuthType"
            );

            var claimsPrincipal = new ClaimsPrincipal(ident);

            //Security Key para la firma de Credenciales del JwtToken
            var securityKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(paCertificate.Thumbprint));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            //Creacion del JwtToken con datos de la cabecera y los datos del PA o quien sea
            var secToken = new JwtSecurityToken(null, null, claimsPrincipal.Claims, DateTime.UtcNow,
                DateTime.UtcNow.AddDays(90), signingCredentials);

            secToken.SigningKey = securityKey;

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            //JwtToken Codificado con todos los datos del PA y del Certificado
            var signedAndEncodedToken = tokenHandler.WriteToken(secToken);

            return signedAndEncodedToken;
        }
    }
}
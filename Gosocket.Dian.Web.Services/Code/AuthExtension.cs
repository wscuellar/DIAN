using System;
using System.Security.Cryptography.X509Certificates;
using System.Web.Services.Protocols;
using Gosocket.Dian.Web.Services.Auth;
using System.Web;

namespace Gosocket.Dian.Web.Services.Code
{
    public class AuthExtension : SoapExtension
    {
        public override void ProcessMessage(SoapMessage message)
        {
            if (message.Stage == SoapMessageStage.AfterDeserialize)
            {
                //Check for an AuthHeader containing valid
                //credentials
                if (message.Headers.Count == 0)
                    throw new SoapException("El encabezado de seguridad es incorrecto.", SoapException.ClientFaultCode);

                foreach (SoapHeader header in message.Headers)
                {
                    if (header is AuthUser)
                    {
                        AuthUser authUser = (AuthUser)header;
                        if (string.IsNullOrEmpty(authUser.UserName) || string.IsNullOrEmpty(authUser.Password))
                            throw new SoapException("El encabezado de seguridad es incorrecto.", SoapException.ClientFaultCode);

                        if (CheckUser(authUser))
                            return; // Allow call to execute

                        throw new SoapException("Usuario no autenticado, credenciales incorrectas.", SoapException.ClientFaultCode);
                    }
                    else if(header is AuthSignedMessage)
                    {
                        AuthSignedMessage authSignedMessage = (AuthSignedMessage)header;
                        if (string.IsNullOrEmpty(authSignedMessage.MessageEncoded))
                            throw new SoapException("El encabezado de seguridad es incorrecto.", SoapException.ClientFaultCode);

                        if (CheckSignedMessage(authSignedMessage))
                            return;
                    }
                    else if (header is AuthToken)
                    {
                        AuthToken authToken = (AuthToken)header;
                        if (string.IsNullOrEmpty(authToken.SecurityToken))
                            throw new SoapException("El encabezado de seguridad es incorrecto.", SoapException.ClientFaultCode);

                        if (CheckToken(authToken))
                            return;

                        throw new SoapException("Usuario no autenticado, credenciales incorrectas.", SoapException.ClientFaultCode);
                    }
                    else if(header is AuthHeaderSoapMessage)
                    {

                    }
                }

                // Fail the call if we get to here. Either the header
                // isn't there or it contains invalid credentials.
                throw new SoapException("Unauthorized", SoapException.ClientFaultCode);
            }
        }

        private static bool CheckUser(AuthUser authUser)
        {
            //var software = System.Configuration.ConfigurationManager.AppSettings["Sofware"].ToString();
            //var url = $"LoginUser?code=UMI9LYwBFbZNZgz0c3eQ4ieb1hgfEeZ7mZrUTmovvGQMonGoyaYYqA==&user={authUser.UserName}&password={authUser.Password}&software={software}";
            //var authUrl = System.Configuration.ConfigurationManager.AppSettings["AuthFunctionsUrl"].ToString();
            //var client = new RestClient(authUrl);
            //var request = new RestRequest($"api/{url}", Method.GET);

            //var restResponse = (RestResponse)client.Execute(request);
            //var result = restResponse.Content.Replace("\"", string.Empty);
            //if (!string.IsNullOrEmpty(result))
            //    return bool.Parse(result);
            return false;
        }

        private static bool CheckSignedMessage(AuthSignedMessage authSignedMessage)
        {
            if (!string.IsNullOrEmpty(authSignedMessage.MessageEncoded))
            {
                byte[] bytes = Convert.FromBase64String(authSignedMessage.MessageEncoded);
                X509Certificate2 certXmlSign = new X509Certificate2(bytes);

                var certValidations = new CertificateValidations(certXmlSign);

                if (!certValidations.isChainTrusted || !certValidations.isExpired || !certValidations.isRevocated)
                    return false;

                var jwtTokenGenerator = new JwtTokenGenerator();
                var jwtToken = jwtTokenGenerator.GenerateToken(certXmlSign);
                authSignedMessage.TokenGenerated = jwtToken;

                return true;
            }

            return false;
        }

        private static bool CheckToken(AuthToken authToken)
        {
            if (authToken.SecurityToken == "123")
                return true;

            return false;
        }

        public override Object GetInitializer(Type type)
        {
            return GetType();
        }

        public override Object GetInitializer(LogicalMethodInfo info,
            SoapExtensionAttribute attribute)
        {
            return null;
        }

        public override void Initialize(Object initializer)
        {
        }
    }
}
using Gosocket.Dian.Application.Common;
using Gosocket.Dian.Infrastructure;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Manager = Gosocket.Dian.Application.Managers;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using static Gosocket.Dian.Logger.Logger;
using Microsoft.ApplicationInsights.DataContracts;

namespace Gosocket.Dian.Web.Services.Validator
{
    public class CertificateValidator : X509CertificateValidator
    {
        private static readonly FileManager fileManager = new FileManager();
        private static List<string> revoked = new List<string>();
        private static List<string> untrusted = new List<string>();

        public override void Validate(X509Certificate2 certificate)
        {
            ValidateCertificate(certificate);
        }

        public void ValidateCertificate(X509Certificate2 certificate)
        {
            
            ////Valida vigencia
            if (DateTime.Now < certificate.NotBefore)
            {
                LogValidacion("ValidateCertificate", "Certificado aún no se encuentra vigente", certificate);
                throw new FaultException("Certificado aún no se encuentra vigente.", new FaultCode("Client"));
            }
            if (DateTime.Now > certificate.NotAfter)
            {
                LogValidacion("ValidateCertificate", "Certificado se encuentra expirado", certificate);
                throw new FaultException("Certificado se encuentra expirado.", new FaultCode("Client"));
            }

            // Get all crt certificates
            var crts = Manager.CertificateManager.Instance.GetRootCertificates();

            // Get all crls
            var crls = Manager.CertificateManager.Instance.GetCrls();

            var primary = GetPrimaryCertificate(certificate);

            if (!primary.IsTrusted(crts))
            {
                try
                {
                    if (!untrusted.Contains(certificate.SerialNumber))
                    {
                        var authCode = GetAuthCode(certificate);
                        untrusted.Add(certificate.SerialNumber);
                        fileManager.Upload("certificates", $"untrusted/{authCode}/{certificate.SerialNumber}.cer", certificate.RawData);
                    }
                }
                catch (Exception e) {
                    Log(e);                   
                }
                
                LogValidacion("ValidateCertificate", "Certificado no pasó validaciones contra crts ", certificate);
                throw new FaultException(ConfigurationManager.GetValue("UnTrustedCertificateMessage"), new FaultCode("Client"));
            }

            if (primary.IsRevoked(crls))
            {
                try
                {
                    if (!revoked.Contains(certificate.SerialNumber))
                    {
                        var authCode = GetAuthCode(certificate);
                        revoked.Add(certificate.SerialNumber);
                        fileManager.Upload("certificates", $"revoked/{authCode}/{certificate.SerialNumber}.cer", certificate.RawData);
                    }
                }
                catch (Exception e) {
                    Log(e);
                }
                LogValidacion("ValidateCertificate", "Certificado se encuentra revocado en crls", certificate);                
                throw new FaultException("Certificado se encuentra revocado.", new FaultCode("Client"));
            }
        }

        private X509Certificate GetPrimaryCertificate(X509Certificate2 certificate)
        {
            X509Certificate x509Certificate = new X509CertificateParser().ReadCertificate(certificate.RawData);
            return x509Certificate;
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


        private static void LogValidacion(string id, string mensaje, X509Certificate2 certificate)
        {
            var datos = new Dictionary<string, string>()
                {
                    {"certificate.Subject",certificate.Subject},
                    {"certificate.SerialNumber",certificate.SerialNumber }
                };
            Log(id, (int)SeverityLevel.Error, mensaje, datos);
        }



        private string GetAuthCode(X509Certificate2 certificate)
        {
            var parts = GetSubjectInfo(certificate.Subject);

            string nit = "";
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

            return nit;
        }
    }
}
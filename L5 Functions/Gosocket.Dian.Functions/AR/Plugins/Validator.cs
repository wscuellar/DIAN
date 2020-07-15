using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Functions.Global.Plugins.Models;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace Gosocket.Dian.Functions.Global.Plugins
{
    public class Validator
    {
        private static ContributorService contributorService = null;
        private static SoftwareService softwareService = null;
        static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");

        private static Validator _instance = null;
        public static Validator Instance => _instance ?? (_instance = new Validator());

        public Validator()
        {
            contributorService = new ContributorService();
            softwareService = new SoftwareService();
        }

        public List<ValidateListResponse> ValidateNit(ResponseXpathDataValue xpathValues)
        {
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "", ErrorMessage = "" };

            var senderCode = xpathValues.XpathsValues["SenderCodeXpath"];
            var SenderCodeProvider = xpathValues.XpathsValues["SenderCodeProviderXpath"];
            var sheldHolderCode = xpathValues.XpathsValues["SheldHolderCodeXpath"].Split('|');
            var supplierPartyCode = xpathValues.XpathsValues["SupplierPartyCodeXpath"];
            var sender = contributorService.GetByCode(senderCode);
            var provider = contributorService.GetByCode(SenderCodeProvider);
            var supplierParty = contributorService.GetByCode(supplierPartyCode);

            if (ConfigurationManager.GetValue("Environment") == "Hab" || ConfigurationManager.GetValue("Environment") == "Test")
            {
                if (sender != null)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB19b", ErrorMessage = $"{sender.Code} del emisor de servicios autorizado." });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB19b", ErrorMessage = $"{sender.Code} del emisor de servicios no está autorizado." });

                if (provider != null)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ21", ErrorMessage = $"{provider.Code} del prestador de servicios autorizado a facturar electrónicamente." });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ21", ErrorMessage = $"{provider.Code} del prestador de servicios no autorizado a facturar electrónicamente." });
                if (supplierParty != null)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ44", ErrorMessage = $"{supplierParty.Code} del emisor de servicios autorizado.." });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ44", ErrorMessage = $"{supplierParty.Code} del emisor de servicios no está autorizado." });

            }
            else if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                if (sender.AcceptanceStatusId == (int)ContributorStatus.Enabled)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB19b", ErrorMessage = $"{sender.Code} del emisor de servicios autorizado." });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB19b", ErrorMessage = $"{sender.Code} del emisor de servicios no está autorizado." });

                if (provider.AcceptanceStatusId == (int)ContributorStatus.Enabled)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ21", ErrorMessage = $"{provider.Code} del prestador de servicios autorizado a facturar electrónicamente." });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ21", ErrorMessage = $"{provider.Code} del prestador de servicios no autorizado a facturar electrónicamente." });

                if (supplierParty.AcceptanceStatusId == (int)ContributorStatus.Enabled)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ44", ErrorMessage = $"{supplierParty.Code} del emisor de servicios autorizado.." });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ44", ErrorMessage = $"{supplierParty.Code} del emisor de servicios no está autorizado." });
            }

            foreach (var shell in sheldHolderCode)
            {
                if (!string.IsNullOrEmpty(shell))
                {
                    var shellHolder = contributorService.GetByCode(shell);
                    if (ConfigurationManager.GetValue("Environment") == "Hab" || ConfigurationManager.GetValue("Environment") == "Test")
                    {
                        if (shellHolder != null)
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ57", ErrorMessage = $"{shell} del participante de consorcio válido" });
                        else
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ57", ErrorMessage = $"{shell} del participante de consorcio no válido" });
                    }
                    else if (ConfigurationManager.GetValue("Environment") == "Prod")
                    {
                        if (shellHolder.AcceptanceStatusId == (int)ContributorStatus.Enabled)
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ57", ErrorMessage = $"{shell} del participante de consorcio válido" });
                        else
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ57", ErrorMessage = $"{shell} del participante de consorcio no válido" });
                    }
                }
            }
            return responses;
        }

        public ValidateListResponse ValidateSoftware(ResponseXpathDataValue xpathValues, string trackId)
        {
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorMessage = $"Huella no corresponde a un software autorizado para este OFE." };
            response.ErrorCode = "FAB27b";
            if (documentMeta.DocumentTypeId == "91")
                response.ErrorCode = "CAB27b";
            if (documentMeta.DocumentTypeId == "92")
                response.ErrorCode = "DAB27b";

            var number = xpathValues.XpathsValues["NumberXpath"];
            var softwareId = xpathValues.XpathsValues["SoftwareIdXpath"];
            var SoftwareSecurityCode = xpathValues.XpathsValues["SoftwareSecurityCodeXpath"];

            var billerSoftwareId = ConfigurationManager.GetValue("BillerSoftwareId");
            var billerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin");

            string hash = "";
            if (softwareId == billerSoftwareId)
                hash = EncryptSHA384($"{billerSoftwareId}{billerSoftwarePin}{number}");
            else
            {
                if (softwareService == null)
                    softwareService = new SoftwareService();
                var software = softwareService.Get(Guid.Parse(softwareId));

                if (software == null)
                {
                    response.ErrorCode = "FAB27d";
                    if (documentMeta.DocumentTypeId == "91")
                        response.ErrorCode = "CAB27d";
                    if (documentMeta.DocumentTypeId == "92")
                        response.ErrorCode = "DAB27d";
                    response.ErrorMessage = "SoftwareId informado no existe.";
                    return response;
                }
                else if (software.AcceptanceStatusSoftwareId == (int)SoftwareStatus.Inactive)
                {
                    response.ErrorCode = "FAB27c";
                    if (documentMeta.DocumentTypeId == "91")
                        response.ErrorCode = "CAB27c";
                    if (documentMeta.DocumentTypeId == "92")
                        response.ErrorCode = "DAB27c";
                    response.ErrorMessage = "SoftwareId informado se encuentra inactivo.";
                    return response;
                }
                hash = EncryptSHA384($"{software.Id}{software.Pin}{number}");
            }

            if (SoftwareSecurityCode == hash)
            {
                response.IsValid = true;
                response.ErrorMessage = "Huella del software correcta.";
            }

            return response;
        }

        private static string EncryptSHA384(string input)
        {
            using (SHA384 shaM = new SHA384Managed())
            {
                StringBuilder Sb = new StringBuilder();

                using (var hash = SHA384.Create())
                {
                    Encoding enc = Encoding.UTF8;
                    byte[] result = hash.ComputeHash(enc.GetBytes(input));

                    foreach (byte b in result)
                        Sb.Append(b.ToString("x2"));
                }

                return Sb.ToString();
            }
        }
    }
}
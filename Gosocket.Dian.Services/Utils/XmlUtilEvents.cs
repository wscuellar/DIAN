using eFacturacionColombia_V2.Firma;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace Gosocket.Dian.Services.Utils
{
    public class XmlUtilEvents
    {
        private const string CategoryContainerName = "dian";
        private static readonly XNamespace ns = "urn:oasis:names:specification:ubl:schema:xsd:ApplicationResponse-2";
        private static readonly XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        private static readonly XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        private static readonly XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        private static readonly XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";
        private static readonly XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";

        private static object obj = new object();

        private static readonly FileManager fileManager = new FileManager();
        private static readonly FirmaElectronica signer = new FirmaElectronica();

        public static byte[] GenerateApplicationResponseBytes(string trackId,
            GlobalDocValidatorDocumentMeta documentMeta,
            List<GlobalDocValidatorTracking> validations,
            List<GlobalDocValidatorDocumentMeta> events,
            List<GlobalDocValidatorDocumentMeta> originalEvents,
            List<GlobalDocValidatorTracking> originalValidations)
        {
            var responseBytes = new byte[] { };

            var docTypeCode = documentMeta.DocumentTypeId;

            var messageIdNode = SerieNumberMessageFromDocType(documentMeta); 
            var number = messageIdNode.Item2; 

            var errors = new List<GlobalDocValidatorTracking>();
            var notifications = new List<GlobalDocValidatorTracking>();

            errors = validations.Where(r => r.Mandatory && !r.IsValid).ToList();
            notifications = validations.Where(r => r.IsNotification).ToList();

            bool mandatoryInvalid = false;

            if (errors.Count > 0)
                mandatoryInvalid = true;

            XElement root = BuildRootNode(documentMeta);
            root.Add(BuildSenderNode(documentMeta));
            root.Add(BuildReceiverNode(documentMeta));

            // events...
            if (events != null && events.Count > 0)
            {
                root.Add(BuildDocumentResponseNodeList(events, originalEvents, originalValidations));
            }

            var xml = FormatterXml(root);
            responseBytes = Encoding.UTF8.GetBytes(xml);

            var responseToSign = Encoding.UTF8.GetString(responseBytes);

            signer.Certificate2 = GetCertificate();
            var date = DateTime.UtcNow.AddHours(-5);
            responseBytes = signer.FirmarEvento(responseToSign, date);

            if (responseBytes == null) return null;

            if (!mandatoryInvalid)
            {
                var folder = "Success";
                var container = $"{CategoryContainerName}";
                var serieFolder = string.IsNullOrEmpty(documentMeta.Serie) ? "NOTSERIE" : documentMeta.Serie;
                var fileName = $"responses/{documentMeta.EmissionDate.Year}/{documentMeta.EmissionDate.Month.ToString().PadLeft(2, '0')}/{documentMeta.EmissionDate.Day.ToString().PadLeft(2, '0')}/{folder}/{documentMeta.SenderCode}/{docTypeCode}/{serieFolder}/{number}/{trackId}.xml";
                fileManager.Upload(container, fileName, responseBytes);
            }

            return responseBytes;
        }
        public static Tuple<string, string, string> SerieNumberMessageFromDocType(GlobalDocValidatorDocumentMeta processResultEntity)
        {
            if (processResultEntity == null) return null;

            var series = !string.IsNullOrEmpty(processResultEntity.Serie) ? processResultEntity.Serie : string.Empty;
            var number = !string.IsNullOrEmpty(processResultEntity.Number) ? processResultEntity.Number : string.Empty;
            var message = (string.IsNullOrEmpty(series)) ? $"La {processResultEntity.DocumentTypeName} {number}, ha sido autorizada." : $"La {processResultEntity.DocumentTypeName} {series}-{number}, ha sido autorizada.";

            return new Tuple<string, string, string>(series, number, message);
        }

        public static XElement BuildDianExtensionsNode()
        {
            return new XElement(sts + "DianExtensions",
                    new XElement(sts + "InvoiceSource",
                        new XElement(cbc + "IdentificationCode", "CO",
                            new XAttribute("listAgencyID", "6"),
                            new XAttribute("listAgencyName", "United Nations Economic Commission for Europe"),
                            new XAttribute("listSchemeURI", "urn:oasis:names:specification:ubl:codelist:gc:CountryIdentificationCode-2.1"))),
                        new XElement(sts + "SoftwareProvider",
                            new XElement(sts + "ProviderID", "800197268",
                                new XAttribute("schemeID", "4"),
                                new XAttribute("schemeName", "31"),
                                new XAttribute("schemeAgencyID", "195"),
                                new XAttribute("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)")),
                            new XElement(sts + "SoftwareID", "...",
                                new XAttribute("schemeAgencyID", "195"),
                                new XAttribute("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)"))),
                            new XElement(sts + "SoftwareSecurityCode", "...",
                                    new XAttribute("schemeAgencyID", "195"),
                                    new XAttribute("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)")),
                            new XElement(sts + "AuthorizationProvider",
                                new XElement(sts + "AuthorizationProviderID", "800197268",
                                    new XAttribute("schemeID", "4"),
                                    new XAttribute("schemeName", "31"),
                                    new XAttribute("schemeAgencyID", "195"),
                                    new XAttribute("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)"))));
        }

        public static XElement BuildRootNode(GlobalDocValidatorDocumentMeta processResultEntity)
        {
            var uuId = $"{processResultEntity.UblVersion}{processResultEntity.DocumentTypeId}{processResultEntity.SenderCode}{processResultEntity.ReceiverCode}{processResultEntity.Serie}{processResultEntity.Number}";
            var profileExecutionId = "1";
            var schemeID = "1";
            if (ConfigurationManager.GetValue("Environment") != "Prod")
            {
                profileExecutionId = "2";
                schemeID = "2";
            }

            var cufe = CreateCufeId(uuId);
            var issueDate = DateTime.UtcNow;
            return new XElement(ns + "ApplicationResponse",
                new XAttribute(XNamespace.Xmlns + "cac",
                    "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
                new XAttribute(XNamespace.Xmlns + "cbc",
                    "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"),
                new XAttribute(XNamespace.Xmlns + "ext",
                    "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"),
                new XAttribute(XNamespace.Xmlns + "sts", "dian:gov:co:facturaelectronica:Structures-2-1"),
                new XAttribute(XNamespace.Xmlns + "ds", "http://www.w3.org/2000/09/xmldsig#"),
                new XElement(ext + "UBLExtensions",
                    new XElement(ext + "UBLExtension", new XElement(ext + "ExtensionContent", BuildDianExtensionsNode())),
                    new XElement(ext + "UBLExtension", new XElement(ext + "ExtensionContent", string.Empty))),
                new XElement(cbc + "UBLVersionID", "UBL 2.1"), new XElement(cbc + "CustomizationID", "1"),
                new XElement(cbc + "ProfileID", "DIAN 2.1"),
                new XElement(cbc + "ProfileExecutionID", profileExecutionId),
                new XElement(cbc + "ID", $"{GetRandomInt()}"),
                new XElement(cbc + "UUID", cufe,
                    new XAttribute("schemeID", schemeID),
                    new XAttribute("schemeName", "CUDE-SHA384")),
                new XElement(cbc + "IssueDate", issueDate.AddHours(-5).ToString("yyyy-MM-dd")),
                new XElement(cbc + "IssueTime", $"{issueDate.AddHours(-5).ToString("HH:mm:ss")}-05:00"));
        }

        public static XElement BuildSenderNode(GlobalDocValidatorDocumentMeta processResultEntity)
        {
            return new XElement(cac + "SenderParty",
                    new XElement(cac + "PartyTaxScheme",
                        new XElement(cbc + "RegistrationName", "Unidad Especial Dirección de Impuestos y Aduanas Nacionales"),
                        new XElement(cbc + "CompanyID", $"800197268",
                            new XAttribute("schemeID", "4"),
                            new XAttribute("schemeName", $"{processResultEntity.SenderSchemeCode}")),
                        new XElement(cac + "TaxScheme",
                            new XElement(cbc + "ID", "01"),
                            new XElement(cbc + "Name", "IVA"))));
        }

        public static XElement BuildReceiverNode(GlobalDocValidatorDocumentMeta docMetadataEntity)
        {
            return new XElement(cac + "ReceiverParty",
                new XElement(cac + "PartyTaxScheme",
                 new XElement(cbc + "RegistrationName", $"{docMetadataEntity.SenderName}"),
                    new XElement(cbc + "CompanyID", $"{docMetadataEntity.SenderCode}",
                        new XAttribute("schemeID", $"{GetDigitCode(docMetadataEntity.SenderCode)}"),
                        new XAttribute("schemeName", $"{docMetadataEntity.SenderSchemeCode}")),
                    new XElement(cac + "TaxScheme",
                        new XElement(cbc + "ID", "01"),
                        new XElement(cbc + "Name", "IVA"))));
        }

        private static List<XElement> BuildDocumentResponseNodeList(List<GlobalDocValidatorDocumentMeta> events,
            List<GlobalDocValidatorDocumentMeta> originalEvents,
            List<GlobalDocValidatorTracking> originalValidations)
        {
            var list = new List<XElement>();

            int lineId = 1;
            events.ForEach(e =>
            {
                // se busca la información del evento original que se esta procesando.
                var originalEvent = originalEvents.FirstOrDefault(x => x.PartitionKey == e.DocumentKey);
                if (originalEvent != null)
                {
                    // se busca las notificaciones del evento original que se esta procesando.
                    var originalEventNotifications = originalValidations.Where(x => x.PartitionKey == e.DocumentKey && x.IsNotification).ToList();
                    list.Add(BuildDocumentResponseEventNode(lineId, e, originalEvent, originalEventNotifications));
                    lineId++; 
                }
            });

            return list;
        }

        public static XElement BuildDocumentResponseEventNode(int line,
            GlobalDocValidatorDocumentMeta _event,
            GlobalDocValidatorDocumentMeta originalEvent,
            List<GlobalDocValidatorTracking> notifications)
        {
            var documentResponse = new XElement(cac + "DocumentResponse",
                BuildResponseDianEventNode(line, _event),
                BuildDocumentReferenceNode(_event),
                BuildIssuerPartyEventNode(originalEvent),
                BuildRecipientPartyEventNode(originalEvent));

            if (notifications != null && notifications.Count > 0)
            {
                documentResponse.Add(BuildLineResponseEventNodeList(_event, notifications));
            }

            return documentResponse;
        }

        public static XElement BuildIssuerPartyEventNode(GlobalDocValidatorDocumentMeta originalEvent)
        {
            return new XElement(cac + "IssuerParty",
                        new XElement(cac + "PartyTaxScheme",
                            new XElement(cbc + "RegistrationName", $"{originalEvent.SenderName}"),
                            new XElement(cbc + "CompanyID", $"{originalEvent.SenderCode}",
                                new XAttribute("schemeID", $"{GetDigitCode(originalEvent.SenderCode)}"),
                                new XAttribute("schemeName", $"{originalEvent.SenderSchemeCode}")),
                            new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "01"),
                                new XElement(cbc + "Name", "IVA"))));
        }

        public static XElement BuildRecipientPartyEventNode(GlobalDocValidatorDocumentMeta originalEvent)
        {
            return new XElement(cac + "RecipientParty",
                        new XElement(cac + "PartyTaxScheme",
                            new XElement(cbc + "RegistrationName", $"{originalEvent.ReceiverName}"),
                            new XElement(cbc + "CompanyID", $"{originalEvent.ReceiverCode}",
                                new XAttribute("schemeID", $"{GetDigitCode(originalEvent.ReceiverCode)}"),
                                new XAttribute("schemeName", $"{originalEvent.ReceiverSchemeCode}")),
                            new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "01"),
                                new XElement(cbc + "Name", "IVA"))));
        }

        public static XElement BuildResponseDianEventNode(int line, GlobalDocValidatorDocumentMeta _event)
        {
            return new XElement(cac + "Response",
                        new XElement(cbc + "ReferenceID", $"{line}"),
                        new XElement(cbc + "ResponseCode", $"{_event.EventCode}"),
                        new XElement(cbc + "Description", $"{_event.EventCodeDescription}"),
                        new XElement(cbc + "EffectiveDate", $"{_event.SigningTimeStamp.ToString("yyyy-MM-dd")}"),
                        new XElement(cbc + "EffectiveTime", $"{_event.SigningTimeStamp.ToString("HH-mm-ss")}-05:00"));
        }

        public static XElement BuildDocumentReferenceNode(GlobalDocValidatorDocumentMeta processResultEntity)
        {
            var ticketId = processResultEntity.DocumentKey;

            switch (processResultEntity.DocumentTypeId)
            {
                case "11":
                case "12":
                    {
                        return new XElement(cac + "DocumentReference",
                               new XElement(cbc + "ID", $"{processResultEntity.SerieAndNumber}"),
                               new XElement(cbc + "UUID", ticketId,
                                new XAttribute("schemeName", "CUNE-SHA384")));
                    }
                case "96":
                    {
                        return new XElement(cac + "DocumentReference",
                              new XElement(cbc + "ID", $"{processResultEntity.SerieAndNumber}"),
                              new XElement(cbc + "UUID", ticketId,
                                  new XAttribute("schemeName", "CUDE-SHA384")));
                    }
                default:
                    {
                        return new XElement(cac + "DocumentReference",
                               new XElement(cbc + "ID", $"{processResultEntity.SerieAndNumber}"),
                               new XElement(cbc + "UUID", ticketId,
                                   new XAttribute("schemeName", "CUFE-SHA384")));
                    }
            }

        }

        public static XElement BuildResponseReferenceLineId(int line)
        {
            return new XElement(cac + "LineReference",
                    new XElement(cbc + "LineID", line));
        }

        public static XElement BuildResponseNode(int line, string code, string message, bool withObservations, bool withErrors, GlobalDocValidatorDocumentMeta processResultEntity)
        {
            var messageIdNode = SerieNumberMessageFromDocType(processResultEntity); 
            var approvedMessage = messageIdNode.Item3;

            return new XElement(cac + "LineResponse",
                   BuildResponseReferenceLineId(line),
                        new XElement(cac + "Response",
                            new XElement(cbc + "ResponseCode", withErrors || withObservations ? code : "0"),
                            new XElement(cbc + "Description", withErrors || withObservations ? message : approvedMessage)));
        }

        public static List<XElement> BuildLineResponseEventNodeList(GlobalDocValidatorDocumentMeta _event, List<GlobalDocValidatorTracking> notifications)
        {
            var lineId = 1;
            var list = new List<XElement>();

            notifications.ForEach(n =>
            {
                list.Add(BuildResponseNode(lineId, n.ErrorCode, n.ErrorMessage, true, false, _event));
                lineId++;
            });

            return list;
        }

        public static string FormatterXml(XElement root)
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>" + root.ToString(SaveOptions.None);
        }

        public static string CreateCufeId(string joinedCombination)
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA384.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(joinedCombination));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public static int GetRandomInt()
        {
            Random rnd = new Random();
            return rnd.Next(1, 100000000);
        }

        public static bool ApplicationResponseExist(GlobalDocValidatorDocumentMeta documentMeta)
        {
            var fileManager2 = new FileManager(); 
            var serieFolder = string.IsNullOrEmpty(documentMeta.Serie) ? "NOTSERIE" : documentMeta.Serie;
            var isValidFolder = "Success";

            var container = "dian";
            var fileName = $"responses/{documentMeta.Timestamp.Year}/{documentMeta.Timestamp.Month.ToString().PadLeft(2, '0')}/{documentMeta.Timestamp.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";
            var exist = fileManager2.Exists(container, fileName);

            if (!exist)
            {
                fileName = $"responses/{documentMeta.EmissionDate.Year}/{documentMeta.EmissionDate.Month.ToString().PadLeft(2, '0')}/{documentMeta.EmissionDate.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";
                exist = fileManager2.Exists(container, fileName);
            }
            return exist;
        }

        public static byte[] GetApplicationResponseIfExist(GlobalDocValidatorDocumentMeta documentMeta)
        {
            byte[] responseBytes = null;
            var fileManager2 = new FileManager();

            byte[] xmlBytes = null;
             

            var serieFolder = string.IsNullOrEmpty(documentMeta.Serie) ? "NOTSERIE" : documentMeta.Serie;

            var isValidFolder = "Success";

            var container = CategoryContainerName;
            var fileName = $"responses/{documentMeta.Timestamp.Year}/{documentMeta.Timestamp.Month.ToString().PadLeft(2, '0')}/{documentMeta.Timestamp.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";

            xmlBytes = fileManager2.GetBytes(container, fileName);
            if (xmlBytes == null)
            {
                fileName = $"responses/{documentMeta.EmissionDate.Year}/{documentMeta.EmissionDate.Month.ToString().PadLeft(2, '0')}/{documentMeta.EmissionDate.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";
                xmlBytes = fileManager2.GetBytes("dian", fileName);
            }
            if (xmlBytes != null) responseBytes = xmlBytes;

            return responseBytes;
        }

        public static X509Certificate2 GetCertificate()
        {
            var certificate = GetCertificateByThumbprint();
            if (certificate == null) return null;

            return certificate;
        }

        private static X509Certificate2 GetCertificateByThumbprint()
        {
            var thumbprint = ConfigurationManager.GetValue("CertificateThumbprint"); //"BF6B7AE700D03E317C8792D93C4C3DD488C1A002";
            lock (obj)
            {
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                store.Close();

                foreach (var certificate in certificateCollection)
                {
                    if (certificate.Thumbprint == thumbprint)
                    {
                        using (certificate.GetRSAPrivateKey()) { }
                        return certificate;
                    }
                }
                throw new CryptographicException($"No certificate found with thumbprint: {thumbprint}");
            }
        }

        private static int GetDigitCode(string code)
        {
            try
            {
                int[] cousins = new int[] { 0, 3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71 };
                int dv, actualCousin, _totalOperacion = 0, residue, totalDigits = code.Length;

                for (int i = 0; i < totalDigits; i++)
                {
                    actualCousin = int.Parse(code.Substring(i, 1));
                    _totalOperacion += actualCousin * cousins[totalDigits - i];
                }
                residue = _totalOperacion % 11;
                if (residue > 1)
                    dv = 11 - residue;
                else
                    dv = residue;

                return dv;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
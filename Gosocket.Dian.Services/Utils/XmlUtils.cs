using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Gosocket.Dian.Services.Utils
{
    public class XmlUtil
    {
        private static readonly XNamespace ns = "urn:oasis:names:specification:ubl:schema:xsd:ApplicationResponse-2";
        private static readonly XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        private static readonly XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        private static readonly XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        private static readonly XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";
        private static readonly XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";

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
                            new XAttribute("listSchemeUri", "urn:oasis:names:specification:ubl:codelist:gc:CountryIdentificationCode-2.1"))),
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
            var messageIdNode = SerieNumberMessageFromDocType(processResultEntity);
            var series = messageIdNode.Item1;
            var number = messageIdNode.Item2;

            var uuId = $"{processResultEntity.UblVersion}{processResultEntity.DocumentTypeId}{processResultEntity.SenderCode}{processResultEntity.ReceiverCode}{processResultEntity.Serie}{processResultEntity.Number}";

            var cufe = DianServicesUtils.CreateCufeId(uuId);

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
                    new XElement(ext + "UBLExtension", new XElement(ext + "ExtensionContent", BuildDianExtensionsNode()))),
                new XElement(cbc + "UBLVersionID", "UBL 2.1"), new XElement(cbc + "CustomizationID", "1"),
                new XElement(cbc + "ProfileID", "DIAN 2.1"),
                new XElement(cbc + "ProfileExecutionID", "2"),
                new XElement(cbc + "ID", $"{GetRandomInt()}"),
                new XElement(cbc + "UUID", cufe,
                    new XAttribute("schemeName", "CUFE-SHA384")),
                new XElement(cbc + "IssueDate", DateTime.UtcNow.ToString("yyyy-MM-dd")),
                new XElement(cbc + "IssueTime", $"{DateTime.UtcNow.ToString("hh:mm:ss")}-05:00")
                //new XElement(cac + "Signature",
                //    new XElement(cbc + "ID", "214124"),
                //    new XElement(cac + "SignatoryParty",
                //        new XElement(cac + "PartyName",
                //        new XElement(cbc + "Name", "SOUTH CONSULTING SIGNATURE COLOMBIA S.A."))),
                //    new XElement(cac + "DigitalSignatureAttachment",
                //        new XElement(cac + "ExternalReference",
                //        new XElement(cbc + "URI", "#signatureKG"))))
                        );
        }

        public static XElement BuildSenderNode(GlobalDocValidatorDocumentMeta processResultEntity)
        {
            return new XElement(cac + "SenderParty",
                    new XElement(cac + "PartyTaxScheme",
                        new XElement(cbc + "RegistrationName", "Unidad Especial Dirección de Impuestos y Aduanas Nacionales"),
                        new XElement(cbc + "CompanyID", $"800197268",
                            new XAttribute("schemeID", "4"),
                            new XAttribute("schemeName", $"{processResultEntity.SenderTypeCode}")),
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
                        new XAttribute("schemeID", $"{docMetadataEntity.SenderTypeCode}"),
                        new XAttribute("schemeName", $"{docMetadataEntity.SenderSchemeCode}")),
                    new XElement(cac + "TaxScheme",
                        new XElement(cbc + "ID", "01"),
                        new XElement(cbc + "Name", "IVA"))));
        }

        public static XElement BuildDocumentResponseNode(int line, GlobalDocValidatorDocumentMeta processResultEntity, bool withObservations, bool withErrors)
        {
            return new XElement(cac + "DocumentResponse",
                BuildResponseDianEventDescriptionNode(withErrors),
                BuildDocumentReferenceNode(processResultEntity));
            //BuildIssuerParty());
        }

        public static XElement BuildResponseDianEventDescriptionNode(bool withErrors)
        {
            var responseCode = withErrors ? "04" : "02";
            var responseDescription = withErrors ? "Uso no autorizado por la DIAN" : "Uso autorizado por la DIAN";

            return new XElement(cac + "Response",
                        new XElement(cbc + "ResponseCode", $"{responseCode}"),
                        new XElement(cbc + "Description", $"{responseDescription}"));
        }

        public static XElement BuildDocumentReferenceNode(GlobalDocValidatorDocumentMeta processResultEntity)
        {
            var ticketId = processResultEntity.DocumentKey;

            return new XElement(cac + "DocumentReference",
                        new XElement(cbc + "ID", $"{processResultEntity.SerieAndNumber}"),
                        new XElement(cbc + "UUID", ticketId,
                            new XAttribute("schemeName", "CUFE-SHA384")));
        }

        public static XElement BuildResponseLineResponse(int line, long nsu)
        {
            return new XElement(cac + "LineResponse",
                    BuildResponseReferenceLineId(line),
                    BuildResponseDianEventNsuNode(nsu));
        }

        public static XElement BuildResponseDianEventNsuNode(long nsu)
        {
            return new XElement(cac + "Response",
                new XElement(cbc + "ResponseCode", "0000"),
                new XElement(cbc + "Description", $"{nsu}"));
        }

        public static XElement BuildResponseReferenceLineId(int line)
        {
            return new XElement(cac + "LineReference",
                    new XElement(cbc + "LineID", line));
        }

        public static XElement BuildResponseNode(int line, string code, string message, bool withObservations, bool withErrors, GlobalDocValidatorDocumentMeta processResultEntity)
        {
            var messageIdNode = SerieNumberMessageFromDocType(processResultEntity);
            var series = messageIdNode.Item1;
            var number = messageIdNode.Item2;
            var approvedMessage = messageIdNode.Item3;

            return new XElement(cac + "LineResponse",
                   BuildResponseReferenceLineId(line),
                        new XElement(cac + "Response",
                            new XElement(cbc + "ResponseCode", withErrors ? code : "0"),
                            new XElement(cbc + "Description", withErrors ? message : approvedMessage)));
        }

        public static XElement BuildStatusNode(string code, string message)
        {
            return new XElement(cac + "Status",
                                new XElement(cbc + "StatusReasonCode", $"{code}"),
                                new XElement(cbc + "StatusReason", $"{code}-{message}"));
        }

        public static XElement BuildIssuerParty()
        {
            return new XElement(cac + "IssuerParty",
              new XElement(cbc + "AdditionalAccountID",
                      new XAttribute("schemeName", "1")),
              new XElement(cac + "Party",
              new XElement(cac + "PartyName",
              new XAttribute(cbc + "Name", "DIAN (Dirección de Impuestos y Aduanas Nacionales)")),
              new XElement(cac + "PhysicalLocation",
              new XElement(cac + "Address",
                  new XAttribute(cbc + "CityName", "Bogotá DC"),
                  new XAttribute(cbc + "CountrySubentityCode", "DC"),
                  new XAttribute(cbc + "Country", "Colombia"),
              new XElement(cac + "AddressLine",
              new XElement(cbc + "Line", "Av. Jiménez #7 - 13, Piso 3, Local 3012")))),
              new XElement(cac + "PartyTaxScheme",
                  new XElement(cbc + "RegistrationName", "Sociedad de Tiendas de Colombia SAS"),
                  new XElement(cbc + "CompanyID", "700085464",
                  new XAttribute("schemeAgencyID", "195"),
                  new XAttribute("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)"),
                  new XAttribute("schemeID", "32")),
                  new XElement(cbc + "TaxLevelCode", "O-06"),
             new XElement(cac + "TaxScheme",
                new XElement(cbc + "ID", "01"),
                new XElement(cbc + "Name", "IVA"))),
                new XElement(cbc + "PartyLegalEntity",
                new XElement(cbc + "RegistrationName", "Nombre"))));
        }

        public static string FormatterXml(XElement root)
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>" + root.ToString(SaveOptions.None);
        }

        public static Tuple<byte[], byte[]> GetEmbeddedXElementFromAttachment(byte[] attachment)
        {
            var ubl = new byte[] { }; 
            var response = new byte[] { };

            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Encoding.UTF8.GetString(attachment));

                var xmlReader = new XmlTextReader(new MemoryStream(attachment));

                var document = new XPathDocument(xmlReader);
                var navigator = document.CreateNavigator();

                var binaryStringUbl = navigator.SelectSingleNode("//*[local-name()='AttachedDocument']/*[local-name()='DocumentoFiscalElectronico']").InnerXml;
                binaryStringUbl = ReplaceTagsFromCdata(binaryStringUbl);
                ubl = Encoding.UTF8.GetBytes(binaryStringUbl);

                var binaryStringResponse = navigator.SelectSingleNode("//*[local-name()='AttachedDocument']/*[local-name()='Attachment']").InnerXml;
                binaryStringResponse = ReplaceTagsFromCdata(binaryStringResponse);
                response = Encoding.UTF8.GetBytes(binaryStringResponse);
            }
            catch (Exception)
            {

            }
                return new Tuple<byte[], byte[]>(ubl, response);            
            }

        public static bool ValidateIfAttachmentSameDocumentKey(Tuple<byte[], byte[]> attachmentElements, string fileName, ref List<XmlParamsResponseTrackId> trackIdList)
        {
            bool isSame = false;

            if (attachmentElements.Item1.Count() > 0 && attachmentElements.Item2.Count() > 0)
            {
                try
                {
                    var ublReplaced = ReplaceTagsFromCdata(Encoding.UTF8.GetString(attachmentElements.Item1));

                    var navigatorUbl = XDocument.Parse(ublReplaced).CreateNavigator();
                    
                    var documentKeyUbl = navigatorUbl.SelectSingleNode("//*[local-name()='Invoice']/*[local-name()='UUID']|//*[local-name()='CreditNote']/*[local-name()='UUID']|//*[local-name()='DebitNote']/*[local-name()='UUID']").InnerXml;

                    var responseReplaced = ReplaceTagsFromCdata(Encoding.UTF8.GetString(attachmentElements.Item2));

                    var navigatorResponse = XDocument.Parse(responseReplaced).CreateNavigator();

                    var documentKeyResponse = navigatorResponse.SelectSingleNode("//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='UUID']").InnerXml;

                    if (documentKeyUbl == documentKeyResponse)
                        isSame = true;
                    else
                    {
                        trackIdList.Add(new XmlParamsResponseTrackId
                        {
                            XmlFileName = fileName,
                            ProcessedMessage = $"El UUID del DE {documentKeyUbl} no coincide con el UUID del APR {documentKeyResponse}"
                        });
                    }
                }
                catch (Exception ex)
                {
                    trackIdList.Add(new XmlParamsResponseTrackId
                    {
                        XmlFileName = fileName,
                        ProcessedMessage = $"Error validando el nodo CUFE del DE y/o APR"
                    });
                }
            }

            return isSame;
        }

        public static string ReplaceTagsFromCdata(string xml)
        {
            var xmlSplittedStart = Regex.Replace(xml, "&lt;", "<");
            return Regex.Replace(xmlSplittedStart, "&gt;", ">");
        }

        public static int GetRandomInt()
        {
            Random rnd = new Random();
            return rnd.Next(1, 100000000);
        }

        public static async Task<byte[]> GetApplicationResponseIfExist(GlobalDocValidatorDocumentMeta documentMeta)
        {
            byte[] responseBytes = null;
            var fileManager = new FileManager();

            byte[] xmlBytes = null;

            var processDate = documentMeta.Timestamp;

            var serieFolder = string.IsNullOrEmpty(documentMeta.Serie) ? "NOTSERIE" : documentMeta.Serie;

            var isValidFolder = "Success";

            var container = "dian";
            var fileName = $"responses/{documentMeta.Timestamp.Year}/{documentMeta.Timestamp.Month.ToString().PadLeft(2, '0')}/{documentMeta.Timestamp.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";

            xmlBytes = await fileManager.GetBytesAsync(container, fileName);
            if (xmlBytes != null) responseBytes = xmlBytes;

            return responseBytes;
        }

        public static bool ApplicationResponseExist(GlobalDocValidatorDocumentMeta documentMeta)
        {
            var fileManager = new FileManager();
            var processDate = documentMeta.Timestamp;
            var serieFolder = string.IsNullOrEmpty(documentMeta.Serie) ? "NOTSERIE" : documentMeta.Serie;
            var isValidFolder = "Success";

            var container = "dian";
            var fileName = $"responses/{documentMeta.Timestamp.Year}/{documentMeta.Timestamp.Month.ToString().PadLeft(2, '0')}/{documentMeta.Timestamp.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";
            var exist = fileManager.Exists(container, fileName);
            //var xmlBytes = fileManager.GetBytes(container, fileName);
            if (!exist)
            {
                fileName = $"responses/{documentMeta.EmissionDate.Year}/{documentMeta.EmissionDate.Month.ToString().PadLeft(2, '0')}/{documentMeta.EmissionDate.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";
                exist = fileManager.Exists(container, fileName);
            }
            return exist;
        }

        //public static XElement BuildNodeWithoutObservations(GlobalOseProcessResult processResultEntity)
        //{
        //    var series = processResultEntity.SerieNumber.Split('-')[0];
        //    var number = processResultEntity.SerieNumber.Split('-')[1];
        //    return new XElement(cac + "DocumentResponse",
        //            new XElement(cac + "Response",
        //                new XElement(cbc + "ResponseCode", "0",
        //                    new XAttribute("listAgencyName", "PE:SUNAT"),
        //                    new XElement(cbc + "Description", $"La fatura número {series}-{number}, ha sido aceptada."))),
        //            new XElement(cac + "DocumentReference",
        //                new XElement(cbc + "ID", $"{series}-{number}"),
        //                new XElement(cbc + "IssueDate", processResultEntity.EmisionDate.ToString("yy-MM-dd")),
        //                new XElement(cbc + "IssueTime", processResultEntity.EmisionDate.ToString("hh:mm:ss")),
        //                new XElement(cbc + "DocumentTypeCode", processResultEntity.DocumentType),
        //                new XElement(cac + "Attachment",
        //                    new XElement(cac + "ExternalReference",
        //                        new XElement(cbc + "DocumentHash", "")))),
        //            new XElement(cac + "IssuerParty",
        //                new XElement(cac + "PartyLegalEntity",
        //                    new XElement(cbc + "CompanyID", processResultEntity.SenderCode,
        //                        new XAttribute("schemeID", "")))),
        //            new XElement(cac + "RecipientParty",
        //                new XElement(cac + "PartyLegalEntity",
        //                    new XElement(cbc + "CompanyID", processResultEntity.ReceiverCode,
        //                        new XAttribute("schemeID", "")))));
        //}

        //public static XElement BuildNodeWithObservations(GlobalOseProcessResult processResultEntity, string code, string message)
        //{
        //    var series = processResultEntity.SerieNumber.Split('-')[0];
        //    var number = processResultEntity.SerieNumber.Split('-')[1];
        //    return new XElement(cac + "DocumentResponse",
        //                new XElement(cac + "Response",
        //                    new XElement(cbc + "ResponseCode", "0",
        //                        new XAttribute("listAgencyName", "PE:SUNAT"),
        //                        new XElement(cbc + "Description", $"La fatura número {series}-{number}, ha sido aceptada.")),
        //                    new XElement(cac + "Status",
        //                        new XElement(cbc + "StatusReasonCode", $"{code}",
        //                            new XAttribute("listURI", "urn:pe:gob:sunat:cpe:see:gem:codigos:codigoretorno")),
        //                        new XElement(cbc + "StatusReason", $"{code}-{message}"))),
        //                new XElement(cac + "DocumentReference",
        //                    new XElement(cbc + "ID", $"{series}-{number}"),
        //                    new XElement(cbc + "IssueDate", processResultEntity.EmisionDate.ToString("yy-MM-dd")),
        //                    new XElement(cbc + "IssueTime", processResultEntity.EmisionDate.ToString("hh:mm:ss")),
        //                    new XElement(cbc + "DocumentTypeCode", processResultEntity.DocumentType),
        //                    new XElement(cac + "Attachment",
        //                        new XElement(cac + "ExternalReference",
        //                            new XElement(cbc + "DocumentHash", "")))),
        //                new XElement(cac + "IssuerParty",
        //                    new XElement(cac + "PartyLegalEntity",
        //                        new XElement(cbc + "CompanyID", processResultEntity.SenderCode,
        //                            new XAttribute("schemeID", "")))),
        //                new XElement(cac + "RecipientParty",
        //                    new XElement(cac + "PartyLegalEntity",
        //                        new XElement(cbc + "CompanyID", processResultEntity.ReceiverCode,
        //                            new XAttribute("schemeID", "")))));
        //}

        //public static XElement BuildNodeWithErrors(GlobalOseProcessResult processResultEntity, string code, string message)
        //{
        //    var series = processResultEntity.SerieNumber.Split('-')[0];
        //    var number = processResultEntity.SerieNumber.Split('-')[1];
        //    return new XElement(cac + "DocumentResponse",
        //                new XElement(cac + "Response",
        //                    new XElement(cbc + "ResponseCode", code,
        //                        new XElement(cbc + "Description", $"{message}."))),
        //                new XElement(cac + "DocumentReference",
        //                    new XElement(cbc + "ID", $"{series}-{number}"),
        //                    new XElement(cbc + "IssueDate", processResultEntity.EmisionDate.ToString("yy-MM-dd")),
        //                    new XElement(cbc + "IssueTime", processResultEntity.EmisionDate.ToString("hh:mm:ss")),
        //                    new XElement(cbc + "DocumentTypeCode", processResultEntity.DocumentType),
        //                    new XElement(cac + "Attachment",
        //                        new XElement(cac + "ExternalReference",
        //                            new XElement(cbc + "DocumentHash", "")))),
        //                new XElement(cac + "IssuerParty",
        //                    new XElement(cac + "PartyLegalEntity",
        //                        new XElement(cbc + "CompanyID", processResultEntity.SenderCode,
        //                            new XAttribute("schemeID", "")))),
        //                new XElement(cac + "RecipientParty",
        //                    new XElement(cac + "PartyLegalEntity",
        //                        new XElement(cbc + "CompanyID", processResultEntity.ReceiverCode,
        //                            new XAttribute("schemeID", "")))));
        //}
    }
}

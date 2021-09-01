using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Models;
using Gosocket.Dian.Functions.Utils;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Gosocket.Dian.Functions.Common
{
    public class DocumentMetaData
    {
        /// <summary>
        /// Global properties
        /// </summary>
        readonly XmlDocument _xmlDocument;
        readonly XPathDocument _document;
        readonly XPathNavigator _navigator;
        readonly XPathNavigator _navNs;
        readonly XmlNamespaceManager _ns;
        readonly byte[] _xmlBytes;

        /// <summary>
        /// Instances of table managers
        /// </summary>
        private readonly TableManager documentTableManager = new TableManager("GlobalDocValidatorDocument");
        private readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        private readonly TableManager documentTypeTableManager = new TableManager("GlobalDocumentType");
        private readonly CategoryManager categoryManager = new CategoryManager();


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xmlBytes"></param>
        public DocumentMetaData(byte[] xmlBytes)
        {
            _xmlBytes = xmlBytes;
            _xmlDocument = new XmlDocument();
            _xmlDocument.LoadXml(Encoding.UTF8.GetString(xmlBytes));

            var xmlReader = new XmlTextReader(new MemoryStream(xmlBytes)) { Namespaces = true };

            _document = new XPathDocument(xmlReader);
            _navigator = _document.CreateNavigator();

            _navNs = _document.CreateNavigator();
            _navNs.MoveToFollowing(XPathNodeType.Element);
            IDictionary<string, string> nameSpaceList = _navNs.GetNamespacesInScope(XmlNamespaceScope.All);

            _ns = new XmlNamespaceManager(_xmlDocument.NameTable);

            foreach (var nsItem in nameSpaceList)
            {
                if (string.IsNullOrEmpty(nsItem.Key))
                    _ns.AddNamespace("sig", nsItem.Value);
                else
                    _ns.AddNamespace(nsItem.Key, nsItem.Value);
            }
            _ns.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        }

        /// <summary>
        /// Insert global document meta (document head)
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="documentTypeId"></param>
        /// <param name="category"></param>
        /// <param name="fileName"></param>
        public async Task InsertDocumentDataAsync(RequestRegisterDocumentData requestObject)
        {
            requestObject.Category = GetCategory()?.RowKey;
            if (requestObject.Category == null)
                throw new Exception($"No se encontró una categoría configurada para el objecto seleccionado.");

            var globalDocumentType = Application.Managers.DocumentTypeManager.Instance.GetAll().FirstOrDefault(d => d.PartitionKey == requestObject.Category && d.RowKey == requestObject.DocumentTypeId);
            if (globalDocumentType == null)
                throw new Exception($"Tipo de documento {requestObject.DocumentTypeId} no implementado.");
            var rules = Application.Managers.RuleManager.Instance.GetAll().Where(r => r.Category == requestObject.Category && r.DocumentTypeCode == requestObject.DocumentTypeId && r.Active);
            if (rules.Count() == 0)
                throw new Exception($"No hay reglas creadas para tipo de documento {requestObject.DocumentTypeId} y categoría {requestObject.Category}.");
            GlobalDocValidatorDocumentMeta documentMeta = SetDocumentDataInstance(requestObject, globalDocumentType);
            var document = documentTableManager.Find<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
            if (document != null)
                throw new Exception("El comprobante ya fue registrado previamente con otros datos.");
            await documentMetaTableManager.InsertOrUpdateAsync(documentMeta);
        }

        /// <summary>
        /// Set global document meta
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="documentTypeId"></param>
        /// <param name="fileName"></param>
        /// <param name="globalDocumentType"></param>
        /// <returns></returns>
        private GlobalDocValidatorDocumentMeta SetDocumentDataInstance(RequestRegisterDocumentData requestObject, GlobalDocumentType globalDocumentType)
        {
            try
            {
                var documentMeta = new GlobalDocValidatorDocumentMeta(requestObject.TrackId, requestObject.TrackId)
                {
                    // file name
                    FileName = requestObject.FileName,

                    // document type information
                    DocumentTypeId = requestObject.DocumentTypeId,
                    SoftwareId = requestObject.SoftwareId
                };

                documentMeta.DocumentTypeName = Domain.Common.EnumHelper.GetEnumDescription((Domain.Common.DocumentType)int.Parse(requestObject.DocumentTypeId));

                // emission date
                if (!string.IsNullOrEmpty(globalDocumentType.EmissionDateXPath))
                {
                    var emissionDatenode = _xmlDocument.SelectSingleNode(globalDocumentType.EmissionDateXPath, _ns);
                    documentMeta.EmissionDate = DateTime.Parse(emissionDatenode?.InnerText);
                }

                // document referenced key
                if (!string.IsNullOrEmpty(globalDocumentType.DocumentReferencedKeyXpath))
                    documentMeta.DocumentReferencedKey = _xmlDocument.SelectSingleNode(globalDocumentType.DocumentReferencedKeyXpath, _ns) != null ? _xmlDocument.SelectSingleNode(globalDocumentType.DocumentReferencedKeyXpath, _ns)?.InnerText : "";

                // invoice authorization
                if (!string.IsNullOrWhiteSpace(globalDocumentType.InvoiceAuthorizationXpath))
                    documentMeta.InvoiceAuthorization = _xmlDocument.SelectSingleNode(globalDocumentType.InvoiceAuthorizationXpath, _ns) != null ? _xmlDocument.SelectSingleNode(globalDocumentType.InvoiceAuthorizationXpath, _ns)?.InnerText : "";

                // serie and number information
                if (!string.IsNullOrEmpty(globalDocumentType.SerieXpath))
                    documentMeta.Serie = _xmlDocument.SelectSingleNode(globalDocumentType.SerieXpath, _ns)?.InnerText;
                if (!string.IsNullOrEmpty(globalDocumentType.SerieAndNumberXpath))
                    documentMeta.SerieAndNumber = _xmlDocument.SelectSingleNode(globalDocumentType.SerieAndNumberXpath, _ns)?.InnerText;

                if (!string.IsNullOrEmpty(documentMeta.Serie))
                    documentMeta.Number = documentMeta.SerieAndNumber.Replace(documentMeta.Serie, string.Empty);
                else
                    documentMeta.Number = documentMeta.SerieAndNumber;

                // sender information
                if (!string.IsNullOrEmpty(globalDocumentType.SenderCodeXpath))
                    documentMeta.SenderCode = _xmlDocument.SelectSingleNode(globalDocumentType.SenderCodeXpath, _ns)?.InnerText;
                if (!string.IsNullOrEmpty(globalDocumentType.SenderNameXpath))
                    documentMeta.SenderName = StringUtils.NormalizeInput(_xmlDocument.SelectSingleNode(globalDocumentType.SenderNameXpath, _ns)?.InnerText);
                if (!string.IsNullOrEmpty(globalDocumentType.SenderTypeCodeXpath))
                    documentMeta.SenderTypeCode = _xmlDocument.SelectSingleNode(globalDocumentType.SenderTypeCodeXpath, _ns) != null ? _xmlDocument.SelectSingleNode(globalDocumentType.SenderTypeCodeXpath, _ns)?.InnerText : "";
                if (!string.IsNullOrEmpty(globalDocumentType.SenderSchemeCodeXpath))
                    documentMeta.SenderSchemeCode = _xmlDocument.SelectSingleNode(globalDocumentType.SenderSchemeCodeXpath, _ns) != null ? _xmlDocument.SelectSingleNode(globalDocumentType.SenderSchemeCodeXpath, _ns)?.InnerText : "";

                // receiver information
                if (!string.IsNullOrEmpty(globalDocumentType.ReceiverCodeXpath))
                    documentMeta.ReceiverCode = _xmlDocument.SelectSingleNode(globalDocumentType.ReceiverCodeXpath, _ns)?.InnerText;
                if (!string.IsNullOrEmpty(globalDocumentType.ReceiverNameXpath))
                    documentMeta.ReceiverName = StringUtils.NormalizeInput(_xmlDocument.SelectSingleNode(globalDocumentType.ReceiverNameXpath, _ns)?.InnerText);
                if (!string.IsNullOrEmpty(globalDocumentType.ReceiverTypeCodeXpath))
                    documentMeta.ReceiverTypeCode = _xmlDocument.SelectSingleNode(globalDocumentType.ReceiverTypeCodeXpath, _ns) != null ? _xmlDocument.SelectSingleNode(globalDocumentType.ReceiverTypeCodeXpath, _ns)?.InnerText : "";

                // totals information
                double ica = 0;
                double iva = 0;
                double ipc = 0;
                double total = 0;
                if (!string.IsNullOrEmpty(globalDocumentType.TotalICAXpath))
                    double.TryParse(_xmlDocument.SelectSingleNode(globalDocumentType.TotalICAXpath, _ns)?.InnerText, out ica);
                if (!string.IsNullOrEmpty(globalDocumentType.TotalICAXpath))
                    double.TryParse(_xmlDocument.SelectSingleNode(globalDocumentType.TotalIVAXpath, _ns)?.InnerText, out iva);
                if (!string.IsNullOrEmpty(globalDocumentType.TotalICAXpath))
                    double.TryParse(_xmlDocument.SelectSingleNode(globalDocumentType.TotalIPCXpath, _ns)?.InnerText, out ipc);
                if (!string.IsNullOrEmpty(globalDocumentType.TotalICAXpath))
                    double.TryParse(_xmlDocument.SelectSingleNode(globalDocumentType.TotalAmountXpath, _ns)?.InnerText, out total);

                documentMeta.TotalAmount = total;
                documentMeta.TaxAmountIva = iva;
                documentMeta.TaxAmountIpc = ipc;
                documentMeta.TaxAmountIca = ica;
                documentMeta.FreeAmount = 0;

                // document key
                documentMeta.DocumentKey = $"{requestObject.TrackId}";
                // identifier
                documentMeta.Identifier = $"{documentMeta.SenderCode}{documentMeta.DocumentTypeId}{documentMeta.SerieAndNumber}".EncryptSHA256();
                // test set id
                documentMeta.TestSetId = requestObject.TestSetId;
                // key zip
                documentMeta.ZipKey = requestObject.ZipKey;

                return documentMeta;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al parsear xml. {ex.Message}");
            }
        }

        /// <summary>
        /// Get category
        /// </summary>
        /// <returns></returns>
        private GlobalDocValidatorCategory GetCategory()
        {
            var categories = categoryManager.GetAll();
            var category = categories.FirstOrDefault(c => !string.IsNullOrEmpty(c.XpathCondition) && (bool)_navigator.Evaluate(c.XpathCondition, _ns));
            return category;
        }
    }
}

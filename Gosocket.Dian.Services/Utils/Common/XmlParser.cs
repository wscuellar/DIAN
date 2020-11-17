﻿using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Xml;

namespace Gosocket.Dian.Services.Utils.Common
{
    public class XmlParser
    {
        private static MemoryCache xmlParserDefinitionsInstanceCache = MemoryCache.Default;

        public Dictionary<string, object> Fields { get; set; }
        public XmlDocument AllXmlDefinitions { get; set; }
        public XmlNode CurrentXmlDefinition { get; set; }
        public XmlDocument XmlDocument { get; set; }
        public XmlNode Extentions { get; set; }
        public XPathQuery XPathQuery { get; set; }
        public byte[] XmlContent { get; set; }


        public string Type { get; set; }
        public string Prefix { get; set; }
        public string Namespace { get; set; }
        public string Encoding { get; set; }
        public string ParserError { get; set; }
        public string SigningTime { get; set; }
        public string CustomizationID { get; set; }
        public string DocumentReferenceId { get; set; }
        public string PaymentMeansID { get; set; }
        public string PaymentDueDate { get; set; }
        public string DiscountRateEndoso { get; set; }
        public string PriceToPay { get; set; }
        public string TotalEndoso { get; set; }
        public string TotalInvoice { get; set; }
        public string ListID { get; set; }
        public string DocumentID { get; set; }
        public string NoteMandato { get; set; }

        public XmlParser()
        {
            Fields = new Dictionary<string, object>();
            AllXmlDefinitions = new XmlDocument();
            var xmlParserDefinitions = GetXmlParserDefinitions();
            AllXmlDefinitions.LoadXml(xmlParserDefinitions);
            
        }

        public XmlParser(byte[] xmlContentBytes, XmlNode extensions = null)
            : this()
        {
            var utf8Preamble = System.Text.Encoding.UTF8.GetPreamble();
            if (xmlContentBytes.StartsWith(utf8Preamble))
                xmlContentBytes = xmlContentBytes.SubArray(utf8Preamble.Length);

            XmlContent = xmlContentBytes;
            Extentions = extensions;

            CurrentXmlDefinition = GetMessageType();

            if (CurrentXmlDefinition == null)
                return;

            var nodeType = CurrentXmlDefinition.SelectSingleNode("@Type");
            var nodeEncoding = CurrentXmlDefinition.SelectSingleNode("Encoding");

            if (nodeType == null || nodeEncoding == null)
                return;

            Type = nodeType.InnerText;
            Encoding = nodeEncoding.InnerText;

            XmlDocument = new XmlDocument { PreserveWhitespace = true };

            using (var ms = new MemoryStream(XmlContent))
            {
                using (var sr = new StreamReader(ms, System.Text.Encoding.GetEncoding(Encoding)))
                {
                    XmlDocument.XmlResolver = null;
                    XmlDocument.Load(sr);
                    var node = XmlDocument.GetElementsByTagName("xades:SigningTime")[0];
                    var nodePaymentMeansValuesXpath = "//*[local-name()='PaymentMeans']/*[local-name()='ID']";                    
                    var nodePaymentDueDateValuesXpath = "//*[local-name()='PaymentMeans']/*[local-name()='PaymentDueDate']";
                    var listIDValueXpath = XmlDocument.GetElementsByTagName("cbc:ResponseCode")[0];
                    var documentReferenceIdValueXpath = "//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ID']";
                    var valueDiscountRateEndoso = XmlDocument.GetElementsByTagName("InformacionNegociacion")[0];
                    var valueTotalInvoice = "//*[local-name()='LegalMonetaryTotal']/*[local-name()='PayableAmount']";
                    var valueNote = "//*[local-name()='Note']";

                    var documentReferenceId = XmlDocument.SelectSingleNode(documentReferenceIdValueXpath)?.InnerText;
                    var nodePaymentMeans = XmlDocument.SelectSingleNode(nodePaymentMeansValuesXpath)?.InnerText;
                    var nodePaymentDueDate = XmlDocument.SelectSingleNode(nodePaymentDueDateValuesXpath)?.InnerText;
                    var nodeTotalInvoice = XmlDocument.SelectSingleNode(valueTotalInvoice)?.InnerText;
                    var valueNoteMandato = XmlDocument.SelectSingleNode(valueNote)?.InnerText;


                    SigningTime = node?.InnerText;
                    PaymentMeansID = nodePaymentMeans;
                    DocumentReferenceId = documentReferenceId;
                    PaymentDueDate = nodePaymentDueDate;
                    DiscountRateEndoso = valueDiscountRateEndoso?.InnerText;

                    if(DiscountRateEndoso != null)
                    {
                        //string[] datos = DiscountRateEndoso.Split(new string[] { "\r\n\t\t\t\t\t", "\r\n\t\t\t\t", " " }, StringSplitOptions.None);
                        string auxD = DiscountRateEndoso.Replace("\r\n\t\t\t\t\t", " ");
                        string[] datos = auxD.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        DiscountRateEndoso = datos[5];
                        PriceToPay = datos[3];
                        TotalEndoso = datos[1];
                    }
                    if(nodeTotalInvoice != null)
                    {
                        TotalInvoice = nodeTotalInvoice;
                    }
                    if (valueNoteMandato != null)
                    {
                        NoteMandato = valueNoteMandato;
                    }
                }
            }
        }

        public virtual bool Parser(bool validate = true)
        {
            try
            {
                var fields = CurrentXmlDefinition.SelectNodes("Field");
                if (fields != null)
                    foreach (XmlNode field in fields)
                    {
                        if (field.Attributes == null)
                            continue;

                        var key = field.Attributes["Name"].InnerText;
                        var val = FieldValue(key, validate);
                        Fields.Add(key, val);
                    }

                //var referencesNodesXpath = CurrentXmlDefinition
                //    .SelectSingleNode("List[@Name=\'Referencia\']/XPathValue");

                //if (string.IsNullOrEmpty(referencesNodesXpath?.InnerText))
                //    return true;

                return true;
            }
            catch (Exception error)
            {
                ParserError = error.ToStringMessage();
                return false;
            }
        }

        protected XmlNode GetMessageType()
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };
            using (var ms = new MemoryStream(XmlContent))
            {
                //var defaultEncoding = ConfigurationManager.Settings["Encoding"];
                using (var sr = new StreamReader(ms, System.Text.Encoding.UTF8))
                {
                    xmlDocument.XmlResolver = null;
                    xmlDocument.Load(sr);
                }
            }

            if (xmlDocument.DocumentElement == null || AllXmlDefinitions.DocumentElement == null)
                throw new Exception("MessagesType not found.");

            Namespace = xmlDocument.DocumentElement.NamespaceURI;
            Prefix = xmlDocument.DocumentElement.Prefix;
            if (string.IsNullOrEmpty(Prefix))
                Prefix = "sig";

            XPathQuery = new XPathQuery();

            if (!string.IsNullOrEmpty(Namespace))
            {
                XPathQuery.Prefix = Prefix;
                XPathQuery.NameSpace = Namespace;
            }

            foreach (XmlNode node in AllXmlDefinitions.DocumentElement.ChildNodes)
            {
                var xmlElement = node["XPathAssociation"];
                if (xmlElement == null)
                    continue;

                var query = xmlElement.InnerText;
                if (Prefix != "sig")
                    query = query.Replace("sig:", string.Format("{0}:", Prefix));

                XPathQuery.Query = query;

                var result = XPathQuery.Evaluate(xmlDocument);
                if ((XPathQuery.HasError) || (result == null) || !(bool)result)
                    continue;

                return node;
            }

            throw new Exception("MessagesType not found.");
        }

        public object FieldValue(string fieldName, bool validate = true)
        {
            if (CurrentXmlDefinition == null)
                return null;

            object result;
            var nd = CurrentXmlDefinition.SelectSingleNode(string.Format("Field[@Name='{0}']/XPathValue", fieldName));
            if (nd != null && nd.InnerText != string.Empty)
            {
                var query = nd.InnerText;
                if (Prefix != "sig")
                    query = query.Replace("sig:", string.Format("{0}:", Prefix));

                XPathQuery.Query = query;
                result = XPathQuery.Evaluate(XmlDocument);
                if (result != null)
                    return result;
            }

            var xpath = new XPathQuery { Query = string.Format("Field[@Name='{0}']/DefaultValue", fieldName) };
            result = xpath.Evaluate(CurrentXmlDefinition);
            if (result != null)
                return result;

            if (!validate)
                return null;

            throw new Exception(string.Format("No se pudo mapear el campo: '{0}'.", fieldName));
        }

        public XmlNode SelectSingleNode(string xPath)
        {
            if (Prefix != "sig")
                xPath = xPath.Replace("sig:", string.Format("{0}:", Prefix));

            XPathQuery.Query = xPath;
            var nodeList = XPathQuery.Select(XmlDocument);
            return nodeList.Count > 0 ? nodeList[0] : null;
        }

        public XmlNodeList SelectNodes(string xPath, XmlNode relative = null)
        {
            if (Prefix != "sig")
                xPath = xPath.Replace("sig:", string.Format("{0}:", Prefix));

            XPathQuery.Query = xPath;
            return XPathQuery.Select(XmlDocument, relative);
        }

        private string GetXmlParserDefinitions()
        {
            var xmlParserDefinitions = "";
            var cacheItem = xmlParserDefinitionsInstanceCache.GetCacheItem("XmlParserDefinitions");
            if (cacheItem == null)
            {
                var fileManager = new FileManager();
                xmlParserDefinitions = fileManager.GetText("configurations", "XmlParserDefinitions.config");
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1)
                };
                xmlParserDefinitionsInstanceCache.Set(new CacheItem("XmlParserDefinitions", xmlParserDefinitions), policy);
            }
            else
                xmlParserDefinitions = (string)cacheItem.Value;
            return xmlParserDefinitions;
        }
    }
}

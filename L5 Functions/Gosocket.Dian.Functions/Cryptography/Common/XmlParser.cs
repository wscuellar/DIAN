using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace Gosocket.Dian.Functions.Cryptography.Common
{
    public class XmlParser
    {
        public Dictionary<string, object> Fields { get; set; }

        public List<Dictionary<string, object>> RefFields { get; set; }

        public XmlDocument AllXmlDefinitions { get; set; }

        public XmlNode CurrentXmlDefinition { get; set; }

        public byte[] XmlContent { get; set; }

        public XmlDocument XmlDocument { get; set; }

        public XmlNode Extentions { get; set; }

        public XPathQuery XPathQuery { get; set; }

        public string Type { get; set; }

        public string Prefix { get; set; }

        public string Namespace { get; set; }

        public string Encoding { get; set; }

        public string ParserError { get; set; }

        public string SchemaErrors { get; set; }

        public XmlParser()
        {
            this.Fields = new Dictionary<string, object>();
            this.RefFields = new List<Dictionary<string, object>>();
            this.AllXmlDefinitions = new XmlDocument();
            //this.AllXmlDefinitions.LoadXml(new FileManager().GetText("configurations", "XmlParserDefinitions.config"));
        }

        public XmlParser(byte[] xmlContentBytes, XmlNode extensions = null)
            : this()
        {
            byte[] preamble = System.Text.Encoding.UTF8.GetPreamble();
            if (xmlContentBytes.StartsWith(preamble))
                xmlContentBytes = xmlContentBytes.SubArray(preamble.Length);
            this.XmlContent = xmlContentBytes;
            this.Extentions = extensions;
            //this.CurrentXmlDefinition = this.GetMessageType();
            //if (this.CurrentXmlDefinition == null)
            //    return;
            //XmlNode xmlNode1 = this.CurrentXmlDefinition.SelectSingleNode("@Type");
            //XmlNode xmlNode2 = this.CurrentXmlDefinition.SelectSingleNode("Encoding");
            //if (xmlNode1 == null || xmlNode2 == null)
            //    return;
            //this.Type = xmlNode1.InnerText;
            this.Encoding = "ISO-8859-1";
            this.XmlDocument = new XmlDocument()
            {
                PreserveWhitespace = true
            };
            using (MemoryStream memoryStream = new MemoryStream(this.XmlContent))
            {
                using (StreamReader streamReader = new StreamReader((Stream)memoryStream, System.Text.Encoding.GetEncoding(this.Encoding)))
                {
                    this.XmlDocument.XmlResolver = (XmlResolver)null;
                    this.XmlDocument.Load((TextReader)streamReader);
                }
            }
        }

        protected XmlNode GetMessageType()
        {
            XmlDocument xmlDoc = new XmlDocument()
            {
                PreserveWhitespace = true
            };
            using (MemoryStream memoryStream = new MemoryStream(this.XmlContent))
            {
                string setting = ConfigurationManager.Settings["Encoding"];
                using (StreamReader streamReader = new StreamReader((Stream)memoryStream, System.Text.Encoding.GetEncoding(setting)))
                {
                    xmlDoc.XmlResolver = (XmlResolver)null;
                    xmlDoc.Load((TextReader)streamReader);
                }
            }
            if (xmlDoc.DocumentElement == null || this.AllXmlDefinitions.DocumentElement == null)
                throw new Exception("MessagesType not found.");
            this.Namespace = xmlDoc.DocumentElement.NamespaceURI;
            this.Prefix = xmlDoc.DocumentElement.Prefix;
            if (string.IsNullOrEmpty(this.Prefix))
                this.Prefix = "sig";
            this.XPathQuery = new XPathQuery();
            if (!string.IsNullOrEmpty(this.Namespace))
            {
                this.XPathQuery.Prefix = this.Prefix;
                this.XPathQuery.NameSpace = this.Namespace;
            }
            foreach (XmlNode childNode in this.AllXmlDefinitions.DocumentElement.ChildNodes)
            {
                XmlElement xmlElement = childNode["XPathAssociation"];
                if (xmlElement != null)
                {
                    string str = xmlElement.InnerText;
                    if (this.Prefix != "sig")
                        str = str.Replace("sig:", string.Format("{0}:", (object)this.Prefix));
                    this.XPathQuery.Query = str;
                    object obj = this.XPathQuery.Evaluate(xmlDoc);
                    if (!this.XPathQuery.HasError && obj != null && (bool)obj)
                        return childNode;
                }
            }
            throw new Exception("MessagesType not found.");
        }

        public virtual bool ValidateSchema()
        {
            this.SchemaErrors = "";
            bool isValid = true;
            ValidationEventHandler validationEventHandler = (ValidationEventHandler)((sender, eventArgs) =>
            {
                if (eventArgs.Exception == null)
                    return;
                isValid = false;
                this.SchemaErrors = this.SchemaErrors + string.Format("Error Message: {0}{1}", (object)eventArgs.Exception.Message, (object)Formatter.LineBreak);
            });
            XmlNode xmlNode = this.CurrentXmlDefinition.SelectSingleNode("XmlSchema/XsdFiles");
            if (xmlNode == null)
                return isValid;
            foreach (string schemaName in ((IEnumerable<string>)xmlNode.InnerText.Split(';')).Select<string, string>((Func<string, string>)(t => t.Trim())).Where<string>((Func<string, bool>)(s => !string.IsNullOrEmpty(s))))
                this.XmlDocument.Schemas.Add(XmlSchema.Read((TextReader)new StreamReader((Stream)new MemoryStream(Schemas.GetSchema(schemaName))), validationEventHandler));
            if (this.XmlDocument.Schemas.Count > 0)
                this.XmlDocument.Validate(validationEventHandler);
            return isValid;
        }

        public virtual bool Parser(bool validate = true)
        {
            try
            {
                XmlNodeList xmlNodeList1 = this.CurrentXmlDefinition.SelectNodes("Field");
                if (xmlNodeList1 != null)
                {
                    foreach (XmlNode xmlNode in xmlNodeList1)
                    {
                        if (xmlNode.Attributes != null)
                        {
                            string innerText = xmlNode.Attributes["Name"].InnerText;
                            object obj = this.FieldValue(innerText, validate);
                            this.Fields.Add(innerText, obj);
                        }
                    }
                }
                XmlNode xmlNode1 = this.CurrentXmlDefinition.SelectSingleNode("List[@Name='Referencia']/XPathValue");
                if (string.IsNullOrEmpty(xmlNode1 != null ? xmlNode1.InnerText : (string)null))
                    return true;
                foreach (object selectNode in this.SelectNodes(xmlNode1.InnerText, (XmlNode)null))
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    XmlNodeList xmlNodeList2 = this.CurrentXmlDefinition.SelectNodes("List[@Name='Referencia']/Field");
                    if (xmlNodeList2 != null)
                    {
                        foreach (XmlNode xmlNode2 in xmlNodeList2)
                        {
                            if (xmlNode2.Attributes != null)
                            {
                                string innerText = xmlNode2.Attributes["Name"].InnerText;
                                object referenceFieldValue = this.GetReferenceFieldValue(innerText, selectNode as XmlNode, validate);
                                dictionary.Add(innerText, referenceFieldValue);
                            }
                        }
                        this.RefFields.Add(dictionary);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                this.ParserError = ex.ToStringMessage();
                //SignatureLogger.Log("XmlParser", SignatureLogType.Error, ex, "");
                return false;
            }
        }

        public IEnumerable<string> GetXsdPdfFiles(string messageTypeId, string version)
        {
            XmlNode xmlNode = this.AllXmlDefinitions.SelectSingleNode("//XmlParserDefinition[@Type='" + messageTypeId + "' and @Version='" + version + "']/XmlSchema/XsdPdfFiles") ?? this.AllXmlDefinitions.SelectSingleNode("//XmlParserDefinition[@Type='" + messageTypeId + "' and @Version='1.0']/XmlSchema/XsdPdfFiles");
            if (xmlNode == null)
                return (IEnumerable<string>)new List<string>();
            return (IEnumerable<string>)xmlNode.InnerText.Split(';');
        }

        public System.Text.Encoding GetEncoding(string messageTypeId, string version = "1.0")
        {
            XmlNode xmlNode = this.AllXmlDefinitions.SelectSingleNode("//XmlParserDefinition[@Type='" + messageTypeId + "' and @Version='" + version + "']/Encoding") ?? this.AllXmlDefinitions.SelectSingleNode("//XmlParserDefinition[@Type='" + messageTypeId + "' and @Version='1.0']/Encoding");
            if (xmlNode != null)
                return System.Text.Encoding.GetEncoding(xmlNode.InnerText);
            return Formatter.ISO_8859_1;
        }

        protected object GetListFieldValue(string listName, string fieldName, XmlNode referenceNode)
        {
            XmlNode xmlNode = this.CurrentXmlDefinition.SelectSingleNode(string.Format("List[@Name='{0}']/Field[@Name='{1}']/XPathValue", (object)listName, (object)fieldName));
            if (string.IsNullOrEmpty(xmlNode != null ? xmlNode.InnerText : (string)null))
                return (object)null;
            string str = xmlNode.InnerText;
            if (this.Prefix != "sig")
                str = str.Replace("sig:", string.Format("{0}:", (object)this.Prefix));
            this.XPathQuery.Query = str;
            return this.XPathQuery.Evaluate(referenceNode);
        }

        protected object GetReferenceFieldValue(string fieldName, XmlNode referenceNode, bool validate = true)
        {
            XmlNode xmlNode = this.CurrentXmlDefinition.SelectSingleNode(string.Format("List[@Name='Referencia']/Field[@Name='{0}']/XPathValue", (object)fieldName));
            if (xmlNode != null && xmlNode.InnerText != string.Empty)
            {
                string str = xmlNode.InnerText;
                if (this.Prefix != "sig")
                    str = str.Replace("sig:", string.Format("{0}:", (object)this.Prefix));
                this.XPathQuery.Query = str;
                object obj = this.XPathQuery.Evaluate(referenceNode);
                if (obj != null)
                    return obj;
            }
            XPathQuery xpathQuery = new XPathQuery();
            xpathQuery.Query = string.Format("List[@Name='Referencia']/Field[@Name='{0}']/DefaultValue", (object)fieldName);
            XmlNode currentXmlDefinition = this.CurrentXmlDefinition;
            object obj1 = xpathQuery.Evaluate(currentXmlDefinition);
            if (obj1 != null)
                return obj1;
            if (!validate)
                return (object)null;
            throw new Exception(string.Format("No se pudo mapear el campo de referencia: '{0}'.", (object)fieldName));
        }

        public object FieldValue(string fieldName, bool validate = true)
        {
            if (this.CurrentXmlDefinition == null)
                return (object)null;
            XmlNode xmlNode = this.CurrentXmlDefinition.SelectSingleNode(string.Format("Field[@Name='{0}']/XPathValue", (object)fieldName));
            if (xmlNode != null && xmlNode.InnerText != string.Empty)
            {
                string str = xmlNode.InnerText;
                if (this.Prefix != "sig")
                    str = str.Replace("sig:", string.Format("{0}:", (object)this.Prefix));
                this.XPathQuery.Query = str;
                object obj = this.XPathQuery.Evaluate(this.XmlDocument);
                if (obj != null)
                    return obj;
            }
            XPathQuery xpathQuery = new XPathQuery();
            xpathQuery.Query = string.Format("Field[@Name='{0}']/DefaultValue", (object)fieldName);
            XmlNode currentXmlDefinition = this.CurrentXmlDefinition;
            object obj1 = xpathQuery.Evaluate(currentXmlDefinition);
            if (obj1 != null)
                return obj1;
            if (!validate)
                return (object)null;
            throw new Exception(string.Format("No se pudo mapear el campo: '{0}'.", (object)fieldName));
        }

        public string GetExtensionValue(string fieldName)
        {
            XmlNode extentions = this.Extentions;
            XmlNode xmlNode1;
            if (extentions == null)
            {
                xmlNode1 = (XmlNode)null;
            }
            else
            {
                string xpath = string.Format("Extension[@Name='{0}']", (object)fieldName);
                xmlNode1 = extentions.SelectSingleNode(xpath);
            }
            XmlNode xmlNode2 = xmlNode1;
            if (string.IsNullOrEmpty(xmlNode2 != null ? xmlNode2.InnerText : (string)null))
                return (string)null;
            return xmlNode2.InnerText;
        }

        public XmlNode SelectSingleNode(string xPath)
        {
            if (this.Prefix != "sig")
                xPath = xPath.Replace("sig:", string.Format("{0}:", (object)this.Prefix));
            this.XPathQuery.Query = xPath;
            XmlNodeList xmlNodeList = this.XPathQuery.Select(this.XmlDocument);
            if (xmlNodeList.Count <= 0)
                return (XmlNode)null;
            return xmlNodeList[0];
        }

        public XmlNodeList SelectNodes(string xPath, XmlNode relative = null)
        {
            if (this.Prefix != "sig")
                xPath = xPath.Replace("sig:", string.Format("{0}:", (object)this.Prefix));
            this.XPathQuery.Query = xPath;
            return this.XPathQuery.Select(this.XmlDocument, relative);
        }

        public IEnumerable<string> GetVersions(string messageTypeId)
        {
            XmlNodeList source = this.AllXmlDefinitions.SelectNodes("//XmlParserDefinition[Type='" + messageTypeId + "']");
            if (source == null)
                return (IEnumerable<string>)new List<string>();
            return (IEnumerable<string>)source.Cast<XmlNode>().Select(node => new
            {
                node = node,
                attributes = node.Attributes
            }).Where(t => t.attributes != null).Select(t => t.attributes["Version"].Value).ToList<string>();
        }

        public IEnumerable<string> GetDisplayFormats(string messageTypeId, string version)
        {
            XmlNodeList source = this.AllXmlDefinitions.SelectNodes("//XmlParserDefinition[Type='" + messageTypeId + "' and Version='" + version + "']/DisplayFormats/DisplayFormat");
            return (source != null ? (IEnumerable<string>)source.Cast<XmlNode>().Select<XmlNode, string>((Func<XmlNode, string>)(node => node.InnerText)).ToList<string>() : (IEnumerable<string>)null) ?? (IEnumerable<string>)new List<string>();
        }
    }
}

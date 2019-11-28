using System;
using System.Xml;
using System.Xml.XPath;

namespace Gosocket.Dian.Services.Utils.Common
{
    public class XPathQuery
    {
        public string NameSpace { get; set; }

        public string Prefix { get; set; }

        public string Query { get; set; }

        public bool HasError { get; set; }

        public string Error { get; set; }

        public bool EmptyResult { get; set; }

        public XPathQuery()
        {
            this.EmptyResult = true;
        }

        public XPathQuery(string query)
            : this(query, string.Empty, string.Empty)
        {
        }

        public XPathQuery(string query, string prefix)
            : this(query, string.Empty, prefix)
        {
        }

        public XPathQuery(string query, string nameSpace, string prefix)
        {
            this.Query = query;
            this.NameSpace = nameSpace;
            this.Prefix = prefix;
            this.EmptyResult = true;
        }

        public XmlNodeList Select(XmlDocument xmlDoc)
        {
            return this.Select(xmlDoc, (XmlNode)null);
        }

        public XmlNodeList Select(XmlDocument xmlDoc, XmlNode relative)
        {
            this.HasError = false;
            this.Error = string.Empty;
            this.EmptyResult = true;
            try
            {
                if (xmlDoc.DocumentElement == null)
                    return (XmlNodeList)null;
                if (this.NameSpace == string.Empty && xmlDoc.DocumentElement.NamespaceURI != string.Empty)
                    this.NameSpace = xmlDoc.DocumentElement.NamespaceURI;
                string xpath = this.Query;
                if (string.IsNullOrEmpty(this.Prefix))
                    xpath = xpath.Replace("sig:", "");
                XmlNodeList xmlNodeList;
                if (!string.IsNullOrEmpty(this.NameSpace) && !string.IsNullOrEmpty(this.Prefix))
                {
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                    nsmgr.AddNamespace(this.Prefix, this.NameSpace);
                    foreach (XmlAttribute attribute in (XmlNamedNodeMap)xmlDoc.DocumentElement.Attributes)
                    {
                        if (attribute.Name.StartsWith("xmlns:"))
                        {
                            string prefix = attribute.Name.Replace("xmlns:", string.Empty);
                            string innerText = attribute.InnerText;
                            if (!nsmgr.HasNamespace(prefix))
                                nsmgr.AddNamespace(prefix, innerText);
                        }
                    }
                    xmlNodeList = relative == null ? xmlDoc.SelectNodes(xpath, nsmgr) : relative.SelectNodes(xpath, nsmgr);
                }
                else
                    xmlNodeList = relative == null ? xmlDoc.SelectNodes(xpath) : relative.SelectNodes(xpath);
                this.HasError = false;
                if (xmlNodeList != null)
                    this.EmptyResult = false;
                return xmlNodeList;
            }
            catch (Exception ex)
            {
                this.HasError = true;
                this.Error = ex.ToStringMessage();
                return (XmlNodeList)null;
            }
        }

        public object Evaluate(XmlDocument xmlDoc)
        {
            return this.Evaluate(xmlDoc, (XmlNode)null);
        }

        public object Evaluate(XmlDocument xmlDoc, XmlNode relative)
        {
            this.HasError = false;
            this.Error = string.Empty;
            this.EmptyResult = true;
            try
            {
                if (xmlDoc.DocumentElement == null)
                    return (object)null;
                if (this.NameSpace == string.Empty && xmlDoc.DocumentElement.NamespaceURI != string.Empty)
                    this.NameSpace = xmlDoc.DocumentElement.NamespaceURI;
                string xpath = this.Query;
                if (string.IsNullOrEmpty(this.Prefix))
                    xpath = xpath.Replace("sig:", "");
                XPathNavigator xpathNavigator = (relative != null ? relative.CreateNavigator() : (XPathNavigator)null) ?? xmlDoc.CreateNavigator();
                XPathExpression expr = xpathNavigator.Compile(xpath);
                if (!string.IsNullOrEmpty(this.NameSpace) && !string.IsNullOrEmpty(this.Prefix))
                {
                    XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                    nsManager.AddNamespace(this.Prefix, this.NameSpace);
                    XmlElement documentElement = xmlDoc.DocumentElement;
                    XmlAttributeCollection attributeCollection = documentElement != null ? documentElement.Attributes : (XmlAttributeCollection)null;
                    if (attributeCollection != null)
                    {
                        foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap)attributeCollection)
                        {
                            if (xmlAttribute.Name.StartsWith("xmlns:"))
                            {
                                string prefix = xmlAttribute.Name.Replace("xmlns:", string.Empty);
                                string innerText = xmlAttribute.InnerText;
                                if (!nsManager.HasNamespace(prefix))
                                    nsManager.AddNamespace(prefix, innerText);
                            }
                        }
                    }
                    expr.SetContext(nsManager);
                }
                object obj = (object)null;
                switch (expr.ReturnType)
                {
                    case XPathResultType.Number:
                        obj = xpathNavigator.Evaluate(expr);
                        if (obj != null)
                        {
                            this.EmptyResult = Math.Abs(Convert.ToDouble(obj)) <= 0.0;
                            break;
                        }
                        break;
                    case XPathResultType.String:
                        obj = xpathNavigator.Evaluate(expr);
                        if (obj != null)
                        {
                            this.EmptyResult = string.IsNullOrEmpty(Convert.ToString(obj));
                            break;
                        }
                        break;
                    case XPathResultType.Boolean:
                        obj = xpathNavigator.Evaluate(expr);
                        if (obj != null)
                        {
                            this.EmptyResult = !Convert.ToBoolean(obj);
                            break;
                        }
                        break;
                    case XPathResultType.NodeSet:
                        XPathNodeIterator xpathNodeIterator = xpathNavigator.Select(expr);
                        if (xpathNodeIterator.MoveNext())
                            obj = (object)xpathNodeIterator.Current.Value;
                        this.EmptyResult = obj == null;
                        break;
                    default:
                        this.HasError = true;
                        this.Error = "Invalid XPathResultType";
                        break;
                }
                return obj;
            }
            catch (Exception ex)
            {
                this.HasError = true;
                this.Error = ex.ToStringMessage();
                return (object)null;
            }
        }

        public object Evaluate(XmlNode xmlNode)
        {
            return this.Evaluate(xmlNode, (XmlNode)null);
        }

        public object Evaluate(XmlNode xmlNode, XmlNode relative)
        {
            this.HasError = false;
            this.Error = string.Empty;
            this.EmptyResult = true;
            try
            {
                XmlDocument ownerDocument = xmlNode.OwnerDocument;
                if ((ownerDocument != null ? ownerDocument.DocumentElement : (XmlElement)null) == null)
                    return (object)null;
                if (this.NameSpace == string.Empty && xmlNode.OwnerDocument.DocumentElement.NamespaceURI != string.Empty)
                    this.NameSpace = xmlNode.OwnerDocument.DocumentElement.NamespaceURI;
                string xpath = this.Query;
                if (string.IsNullOrEmpty(this.Prefix))
                    xpath = xpath.Replace("sig:", "");
                XPathNavigator xpathNavigator = (relative != null ? relative.CreateNavigator() : (XPathNavigator)null) ?? xmlNode.CreateNavigator();
                XPathExpression expr = xpathNavigator.Compile(xpath);
                if (!string.IsNullOrEmpty(this.NameSpace) && !string.IsNullOrEmpty(this.Prefix) && xmlNode.OwnerDocument != null)
                {
                    XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlNode.OwnerDocument.NameTable);
                    nsManager.AddNamespace(this.Prefix, this.NameSpace);
                    XmlElement documentElement = xmlNode.OwnerDocument.DocumentElement;
                    XmlAttributeCollection attributeCollection = documentElement != null ? documentElement.Attributes : (XmlAttributeCollection)null;
                    if (attributeCollection != null)
                    {
                        foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap)attributeCollection)
                        {
                            if (xmlAttribute.Name.StartsWith("xmlns:"))
                            {
                                string prefix = xmlAttribute.Name.Replace("xmlns:", string.Empty);
                                string innerText = xmlAttribute.InnerText;
                                if (!nsManager.HasNamespace(prefix))
                                    nsManager.AddNamespace(prefix, innerText);
                            }
                        }
                    }
                    expr.SetContext(nsManager);
                }
                object obj = (object)null;
                switch (expr.ReturnType)
                {
                    case XPathResultType.Number:
                        obj = xpathNavigator.Evaluate(expr);
                        if (obj != null)
                        {
                            this.EmptyResult = Math.Abs(Convert.ToDouble(obj)) <= 0.0;
                            break;
                        }
                        break;
                    case XPathResultType.String:
                        obj = xpathNavigator.Evaluate(expr);
                        if (obj != null)
                        {
                            this.EmptyResult = string.IsNullOrEmpty(Convert.ToString(obj));
                            break;
                        }
                        break;
                    case XPathResultType.Boolean:
                        obj = xpathNavigator.Evaluate(expr);
                        if (obj != null)
                        {
                            this.EmptyResult = !Convert.ToBoolean(obj);
                            break;
                        }
                        break;
                    case XPathResultType.NodeSet:
                        XPathNodeIterator xpathNodeIterator = xpathNavigator.Select(expr);
                        if (xpathNodeIterator.MoveNext())
                            obj = (object)xpathNodeIterator.Current.Value;
                        this.EmptyResult = obj == null;
                        break;
                    default:
                        this.HasError = true;
                        this.Error = "Invalid XPathResultType";
                        break;
                }
                return obj;
            }
            catch (Exception ex)
            {
                this.HasError = true;
                this.Error = ex.ToStringMessage();
                return (object)null;
            }
        }
    }
}

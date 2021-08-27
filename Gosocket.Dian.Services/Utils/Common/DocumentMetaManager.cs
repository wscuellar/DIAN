using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Saxon.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Gosocket.Dian.Services.Utils.Common
{
    public class DocumentMetaManager
    {
        /// <summary>
        /// Global properties
        /// </summary>
        readonly XmlDocument xmlDocument;
        readonly XPathDocument xpathDocument;
        readonly XPathNavigator navigator;
        readonly XPathNavigator navNs;
        readonly XmlNamespaceManager ns;

        readonly Processor processor;
        readonly XPathCompiler xPathCompiler;
        readonly XdmNode xdmNode;

        readonly byte[] _xmlBytes;

        /// <summary>
        /// Instances of table managers
        /// </summary>
        private readonly CategoryManager categoryManager = new CategoryManager();

        private static MemoryCache categoriesInstanceCache = MemoryCache.Default;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xmlBytes"></param>
        public DocumentMetaManager(byte[] xmlBytes)
        {
            _xmlBytes = xmlBytes;
            xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(Encoding.UTF8.GetString(xmlBytes));

            var xmlReader = new XmlTextReader(new MemoryStream(xmlBytes)) { Namespaces = true };

            xpathDocument = new XPathDocument(xmlReader);
            navigator = xpathDocument.CreateNavigator();

            navNs = xpathDocument.CreateNavigator();
            navNs.MoveToFollowing(XPathNodeType.Element);
            IDictionary<string, string> nameSpaceList = navNs.GetNamespacesInScope(XmlNamespaceScope.All);
            ns = new XmlNamespaceManager(xmlDocument.NameTable);

            //SAXONHE LIB EVALUATE XPATH
            processor = new Saxon.Api.Processor();
            processor.SetProperty(FeatureKeys.DEFAULT_REGEX_ENGINE, "N");
            var documentBuilder = processor.NewDocumentBuilder();
            documentBuilder.BaseUri = new Uri("file:///C:/");
            MemoryStream stream = new MemoryStream(xmlBytes);
            // Load the source document from MemoryStream
            xdmNode = documentBuilder.Build(stream);
            // Create an XPath compiler
            xPathCompiler = processor.NewXPathCompiler();

            foreach (var nsItem in nameSpaceList)
            {
                if (string.IsNullOrEmpty(nsItem.Key))
                {
                    xPathCompiler.DeclareNamespace("sig", nsItem.Value);
                    ns.AddNamespace("sig", nsItem.Value);
                }
                else
                {
                    xPathCompiler.DeclareNamespace(nsItem.Key, nsItem.Value);
                    ns.AddNamespace(nsItem.Key, nsItem.Value);
                }
            }
            xPathCompiler.DeclareNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            xPathCompiler.DeclareNamespace("xs", "http://www.w3.org/2001/XMLSchema");

            // Enable caching, so each expression is only compiled once
            xPathCompiler.Caching = true;
        }

        public GlobalDocValidatorCategory GetCategory()
        {
            GlobalDocValidatorCategory category = null;
            List<GlobalDocValidatorCategory> categories;
            var cacheItem = categoriesInstanceCache.GetCacheItem("Categories");
            if (cacheItem == null)
            {
                categories = categoryManager.GetAll().ToList();
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24)
                };
                categoriesInstanceCache.Set(new CacheItem("Categories", categories), policy);
            }
            else
                categories = (List<GlobalDocValidatorCategory>)cacheItem.Value;

            category = categories.FirstOrDefault(c => c.Active && !string.IsNullOrEmpty(c.XpathCondition) && bool.Parse(xPathCompiler.Evaluate(c.XpathCondition, xdmNode)?.Simplify?.ToString()));
            if (category?.RowKey == "otros") category = categories?.FirstOrDefault(c => c.RowKey == "ose-pe-ubl21");
            return category;
        }
    }
}

using Gosocket.Dian.Infrastructure;
using Saxon.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Gosocket.Dian.Functions.Utils
{
    public class HtmlGDoc
    {
        readonly XmlDocument _xml;
        readonly XmlNamespaceManager _nsmgr;
        readonly byte[] _document;

        public HtmlGDoc(byte[] document)
        {
            _document = document;
            _xml = new XmlDocument();
            using (var ms = new MemoryStream(document))
            {
                using (var sr = new StreamReader(ms, Encoding.UTF8))
                {
                    _xml.XmlResolver = null;
                    _xml.Load(sr);
                }
            }
            _nsmgr = new XmlNamespaceManager(_xml.NameTable);
            _nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            _nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            _nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            _nsmgr.AddNamespace("sts", "http://www.dian.gov.co/contratos/facturaelectronica/v1/Structures");
            _nsmgr.AddNamespace("fe", _xml.DocumentElement.NamespaceURI);
        }

        public string GetHtmlGDoc(Dictionary<string, string> parameters = null)
        {
            // Create a processor instance.
            Processor processor = new Processor();

            // Load the source document.
            var xmlReader = new StringReader(GetXmlGDoc(parameters).OuterXml);
            DocumentBuilder newDocumentBuilder = processor.NewDocumentBuilder();
            newDocumentBuilder.BaseUri = new Uri("file:///C:/");
            XdmNode input = newDocumentBuilder.Build(xmlReader);

            // Load XSLT Transform GDoc To HTML
            var fileManager = new FileManager();
            var htmlXsltBytes = fileManager.GetBytes("dian", "configurations/transform_gdoc_to_html.xslt");



            TextReader streamReaderHtmlXslt = new StreamReader(new MemoryStream(htmlXsltBytes));

            // Create a transformer for the stylesheet.
            XsltTransformer transformer = processor.NewXsltCompiler().Compile(streamReaderHtmlXslt).Load();

            // Set the root node of the source document to be the initial context node.
            transformer.InitialContextNode = input;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    transformer.SetParameter(new QName("", "", parameter.Key), new XdmAtomicValue(parameter.Value));
                }
            }

            var result = new StringWriter();

            // Create a serializer.
            Serializer serializer = new Serializer();
            serializer.SetOutputWriter(result);

            // Transform the source XML to System.out.
            transformer.Run(serializer);

            return result.ToString();
        }

        public XmlDocument GetXmlGDoc(Dictionary<string, string> parameters)
        {
            var stream = TransformToGDoc(parameters);

            stream.Seek(0, SeekOrigin.Begin);

            var xmlGDoc = new XmlDocument();
            xmlGDoc.Load(stream);

            xmlGDoc.DocumentElement.RemoveAllAttributes();

            return xmlGDoc;
        }


        public Stream TransformToGDoc(Dictionary<string, string> parameters = null)
        {
            var fileManager = new FileManager();
            var xsltBytes = fileManager.GetBytes("dian", "configurations/transform_dte_to_gdoc.xslt");

            var processor = new Processor();
            var input = processor.NewDocumentBuilder().Build(_xml);

            var transformer = processor.NewXsltCompiler().Compile(new MemoryStream(xsltBytes)).Load();
            transformer.InitialContextNode = input;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    transformer.SetParameter(new QName("", "", parameter.Key), new XdmAtomicValue(parameter.Value));
                }
            }

            var serializer = new Serializer();
            var ms = new MemoryStream();
            serializer.SetOutputStream(ms);
            transformer.Run(serializer);
            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        public string GetQRNote()
        {
            string stringToQrCode;

            try
            {
                stringToQrCode = _xml.SelectSingleNode(@"/descendant::*[local-name()='QRCode'][1]", _nsmgr)?.InnerText;
            }
            catch (Exception)
            {
                try
                {
                    stringToQrCode = _xml.SelectSingleNode(@"/descendant::*[local-name()='QRCode'][1]", _nsmgr)?.InnerText;
                }
                catch (Exception)
                {
                    try
                    {
                        stringToQrCode = _xml.SelectSingleNode(@"/descendant::*[local-name()='QRCode'][1]", _nsmgr)?.InnerText;
                    }
                    catch (Exception)
                    {
                        stringToQrCode = "";
                    }
                }
            }

            return stringToQrCode;
        }
    }
}

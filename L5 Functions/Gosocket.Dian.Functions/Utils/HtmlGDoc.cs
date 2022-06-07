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

        public HtmlGDoc(byte[] document, byte[] application)
        {
            _document = document;
            _xml = new XmlDocument();
            XmlDocument _xmlApplication = new XmlDocument();

            using (var ms = new MemoryStream(document))
            {
                using (var sr = new StreamReader(ms, Encoding.UTF8))
                {
                    _xml.XmlResolver = null;
                    _xml.Load(sr);
                }
            }

            if (application != null)
            {
                using (var ms = new MemoryStream(application))
                {
                    using (var sr = new StreamReader(ms, Encoding.UTF8))
                    {
                        _xmlApplication.XmlResolver = null;
                        _xmlApplication.Load(sr);
                    }
                }
            }

            _nsmgr = new XmlNamespaceManager(_xml.NameTable);
            _nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            _nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            _nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            _nsmgr.AddNamespace("sts", "http://www.dian.gov.co/contratos/facturaelectronica/v1/Structures");
            _nsmgr.AddNamespace("fe", _xml.DocumentElement.NamespaceURI);

            //Add new element to XML after validation
            if (_xmlApplication.OuterXml != null && _xmlApplication.OuterXml != "")
            {
                var stringDescription = _xmlApplication.SelectSingleNode(@"/descendant::*[local-name()='ApplicationResponse']/descendant::*[local-name()='DocumentResponse']/descendant::*[local-name()='Response']/descendant::*[local-name()='Description']", _nsmgr)?.InnerText;
                var stringIssueDate = _xmlApplication.SelectSingleNode(@"/descendant::*[local-name()='ApplicationResponse']/descendant::*[local-name()='IssueDate']", _nsmgr)?.InnerText;
                var stringIssueTime = _xmlApplication.SelectSingleNode(@"/descendant::*[local-name()='ApplicationResponse']/descendant::*[local-name()='IssueTime']", _nsmgr)?.InnerText;
                var stringSigningTime = _xml.SelectSingleNode(@"/descendant::*[local-name()='UBLExtension']/descendant::*[local-name()='ExtensionContent']/descendant::*[local-name()='Signature']/descendant::*[local-name()='Object']/descendant::*[local-name()='QualifyingProperties']/descendant::*[local-name()='SignedProperties']/descendant::*[local-name()='SignedSignatureProperties']/descendant::*[local-name()='SigningTime']", _nsmgr)?.InnerText;
                var stringDocument = "Documento generado el: ";
                DateTime fecha = Convert.ToDateTime($"{stringIssueDate} {stringIssueTime}");
                DateTime signingTime = Convert.ToDateTime(stringSigningTime);
                stringSigningTime = signingTime.ToString("dd/MM/yyyy HH:mm:ss");
                var stringIssueDateTime = fecha.ToString("dd/MM/yyyy HH:mm:ss");

                XmlNode createElementDocumentResponse = _xml.CreateElement("DocumentResponse", _xml.DocumentElement.NamespaceURI);
                XmlNode createElementSigningTime = _xml.CreateElement("ConvertSigningTime", _xml.DocumentElement.NamespaceURI);
                createElementDocumentResponse.InnerText = string.Concat(stringDescription, " ", stringIssueDateTime);
                createElementSigningTime.InnerText = string.Concat(stringDocument, stringSigningTime);
                _xml.DocumentElement.AppendChild(createElementDocumentResponse);
                _xml.DocumentElement.AppendChild(createElementSigningTime);
            }
        }

        public string GetHtmlGDoc(string typeFormat, Dictionary<string, string> parameters = null)
        {
            // Create a processor instance.
            Processor processor = new Processor();

            // Load the source document.
            var xmlReader = new StringReader(GetXmlGDoc(parameters).OuterXml);
            DocumentBuilder newDocumentBuilder = processor.NewDocumentBuilder();
            newDocumentBuilder.BaseUri = new Uri("file:///C:/");
            XdmNode input = newDocumentBuilder.Build(xmlReader);

            // Load XSLT Transform GDoc To HTML
            byte[] htmlXsltBytes;
            htmlXsltBytes = GetTemplate(typeFormat);

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
            Serializer serializer = processor.NewSerializer();
            serializer.SetOutputWriter(result);

            // Transform the source XML to System.out.
            transformer.Run(serializer);

            return result.ToString();
        }

        private static byte[] GetTemplate(string typeFormat)
        {
            var fileManager = new FileManager();
            byte[] htmlXsltBytes;
            if (string.IsNullOrWhiteSpace(typeFormat) || typeFormat?.ToLower() == "formato tipo carta")
            {
                htmlXsltBytes = fileManager.GetBytes("dian", "configurations/transform_gdoc_to_htmlSE.xslt");
            }
            else if (string.IsNullOrWhiteSpace(typeFormat) || typeFormat?.ToLower() == "formato tipo tirilla")
            {
                htmlXsltBytes = fileManager.GetBytes("dian", "configurations/transform_gdoc_to_htmlSE_Tirilla.xslt");
            }
            else
            {
                htmlXsltBytes = fileManager.GetBytes("dian", "configurations/transform_gdoc_to_htmlSE_customizable.xslt");
            }

            return htmlXsltBytes;
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
            var xsltBytes = fileManager.GetBytes("dian", "configurations/transform_dte_to_gdocSE.xslt");

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

            var serializer = processor.NewSerializer();
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

        //Get value of DocumentResponse
        public string GetDocumentResponse()
        {
            string stringDocumentResponse;
            try
            {
                stringDocumentResponse = _xml.SelectSingleNode(@"/descendant::*[local-name()='DocumentResponse'][1]", _nsmgr)?.InnerText;
            }
            catch (Exception)
            {
                stringDocumentResponse = "";
            }
            return stringDocumentResponse;
        }

        //Get value of SigningTime
        public string GetSigningTime()
        {
            string stringSigningTime;
            try
            {
                stringSigningTime = _xml.SelectSingleNode(@"/descendant::*[local-name()='ConvertSigningTime'][1]", _nsmgr)?.InnerText;
            }
            catch (Exception)
            {
                stringSigningTime = "";
            }
            return stringSigningTime;
        }
    }
}

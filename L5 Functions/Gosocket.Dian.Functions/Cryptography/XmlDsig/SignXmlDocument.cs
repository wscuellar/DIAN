using SBCustomCertStorage;
using SBX509;
using SBXMLAdESIntf;
using SBXMLCore;
using SBXMLSec;
using SBXMLSig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Gosocket.Dian.Functions.Cryptography.XmlDsig
{
    public class SignXmlDocument : XmlAdvancedElectronicSignature2
    {

        private string _certX509File;
        private string _certX509Password;
        private string _prefix = "";

        public override void Save(string filename)
        {
            using (TextWriter sw = new StreamWriter(filename, false, new UTF8Encoding(false)))
            {
                Save(sw);
            }
        }
        public SignXmlDocument()
        {

        }

        public SignXmlDocument(string cert1Issuername, string cert2Issuername, string cert3Issuername)
        {
            Cert1Issuername = cert1Issuername;
            Cert2Issuername = cert2Issuername;
            Cert3Issuername = cert3Issuername;
            //
            // TODO: Add constructor logic here
            //
        }

        private System.Xml.XmlNamespaceManager _nsmgr = null;

        public XmlNamespaceManager NamespaceManager
        {
            get
            {
                return this._nsmgr;
            }
            set
            {
                this._nsmgr = value;
            }
        }

        /// <summary>
        /// Devuelve o asigna el path del archivo de certificado.
        /// </summary>
        public string CertX509File
        {
            get { return _certX509File; }
            set { _certX509File = value; }
        }

        /// <summary>
        /// Devuelve o asigna el password de certificado.
        /// </summary>
        public string CertX509Password
        {
            get { return _certX509Password; }
            set { _certX509Password = value; }
        }

        /// <summary>
        /// Carga el archivo XML especificado.
        /// </summary>
        /// <param name="filename">PATH o URL del archivo XML a cargar.</param>
        public override void Load(string filename)
        {
            this.PreserveWhitespace = true;
            base.Load(filename);
            this._nsmgr = new System.Xml.XmlNamespaceManager(this.NameTable);
            this._nsmgr.AddNamespace(_prefix, this.DocumentElement.NamespaceURI);
            this._nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            this.LoadNamespaces();
        }

        /// <summary>
        /// Carga el documento XML especificado.
        /// </summary>
        /// <param name="document">Documento XML</param>
        public void Load(System.Xml.XmlDocument document)
        {
            this.PreserveWhitespace = true;
            this.LoadXml(document.OuterXml);
        }

        /// <summary>
        /// Carga la cadena XML especificada
        /// </summary>
        /// <param name="xml">Cadena XML</param>
        public override void LoadXml(string xml)
        {
            this.PreserveWhitespace = true;
            base.LoadXml(xml);
            this._nsmgr = new System.Xml.XmlNamespaceManager(this.NameTable);
            this._nsmgr.AddNamespace(_prefix, this.DocumentElement.NamespaceURI);
            this._nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            this.LoadNamespaces();
        }

        /// <summary>
        /// Selecciona un nodo en el XML cargado.
        /// </summary>
        /// <param name="xpath">Cadena de XPath para seleccionar el nodo.</param>
        /// <returns></returns>
        public new System.Xml.XmlNode SelectSingleNode(string xpath)
        {
            return (base.SelectSingleNode(xpath, this._nsmgr));
        }

        /// <summary>
        /// Selecciona nodos en el XML cargado.
        /// </summary>
        /// <param name="xpath">Cadena de XPath para seleccionar nodos.</param>
        /// <returns></returns>
        public new System.Xml.XmlNodeList SelectNodes(string xpath)
        {
            return (base.SelectNodes(xpath, this._nsmgr));
        }

        /// <summary>
        /// Elimina el nodo Extensions del XML cargado.
        /// </summary>
        public void RemoveExtensions()
        {
            System.Xml.XmlNode ext = this.SelectSingleNode("//" + _prefix + ":Extensions");
            if (ext != null)
                ext.ParentNode.RemoveChild(ext);
        }


        /// <summary>
        /// Construye la firma Enveloped del documento.
        /// </summary>
        //public void BuildSignature()
        //{

        //    //X509Certificate2 certificatefile = new X509Certificate2(this.CertX509File, this.CertX509Password);
        //    Signature.Cryptography.KeyManager key = new Signature.Cryptography.KeyManager(this.CertX509File, this.CertX509Password);
        //    this.PreserveWhitespace = true;

        //}

        /// <summary>
        /// Verifica la firma Enveloped del documento XML
        /// </summary>
        /// <returns>Devuelve true si la firma es válida o false si ésta no lo es.</returns>
        /*
        public override bool CheckSignature()
        {
            return true;
        }
        */

        public void LoadNamespaces()
        {
            XmlAttributeCollection atrcol = this.DocumentElement.Attributes;
            foreach (XmlAttribute atr in atrcol)
            {
                if (atr.Name.StartsWith("xmlns:") && !this.NamespaceManager.HasNamespace(atr.Name.Replace("xmlns:", string.Empty)))
                    this.NamespaceManager.AddNamespace(atr.Name.Replace("xmlns:", string.Empty), atr.InnerText);
            }
        }

    }

    public class XmlAdvancedElectronicSignature2 : ValidXmlDocument
    {
        private RSACryptoServiceProvider sp;
        private int _certCount = 0;
        private string _cert1Issuername;
        private string _cert2Issuername;
        private string _cert3Issuername;

        public string Cert1Issuername
        {
            get
            {
                return _cert1Issuername;
            }

            set
            {
                _cert1Issuername = value;
            }
        }

        public string Cert2Issuername
        {
            get
            {
                return _cert2Issuername;
            }

            set
            {
                _cert2Issuername = value;
            }
        }

        public string Cert3Issuername
        {
            get
            {
                return _cert3Issuername;
            }

            set
            {
                _cert3Issuername = value;
            }
        }

        private string SaveFileWhitoutCustoms(String file, out XmlNode NodeCustoms)
        {
            NodeCustoms = null;

            String newName = Path.GetDirectoryName(file) + @"\" + Path.GetFileNameWithoutExtension(file) + ".temp";
            XmlDocument doc = new XmlDocument();//document without extensions
            doc.Load(file);

            XmlNodeList nodes = doc.GetElementsByTagName("Personalizados");
            if (nodes != null && nodes[0] != null)
            {
                NodeCustoms = nodes[0];
                NodeCustoms.ParentNode.RemoveChild(NodeCustoms);

            }

            Regex regex = new Regex(@"\s*/>");
            var trimXml = regex.Replace(doc.OuterXml, "/>");
            doc.PreserveWhitespace = true;
            doc.LoadXml(trimXml);
            doc.Save(newName);
            doc = null;
            return newName;
        }

        private string SaveFileWhitoutExtensions(String file, out XmlNode NodeExtensions)
        {
            NodeExtensions = null;

            String newName = Path.GetDirectoryName(file) + @"\" + Path.GetFileNameWithoutExtension(file) + ".temp";
            XmlDocument doc = new XmlDocument();//document without extensions
            doc.Load(file);
            XmlNodeList nodes = doc.GetElementsByTagName("ext:UBLExtensions");
            if (nodes != null && nodes[0] != null)
            {
                NodeExtensions = nodes[0];
                NodeExtensions.ParentNode.RemoveChild(NodeExtensions);
            }
            doc.Save(newName);
            doc = null;
            return newName;
        }

        private void AddNodeToDocument(String filepath, String newFilePath, String newFileToPrintPath, XmlNode NodeExtensions)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            nsmgr.AddNamespace("fe", doc.DocumentElement.NamespaceURI);

            XmlNode root = doc.DocumentElement;
            root.Attributes.RemoveNamedItem("id");

            if (NodeExtensions != null)
                root.InsertAfter(root.OwnerDocument.ImportNode(NodeExtensions, true), root.LastChild);



            // Putting signature element in the right place
            var SignatureNode = doc.SelectSingleNode(@"/" + root.Name + "/ds:Signature", nsmgr);
            if (SignatureNode != null)
                SignatureNode.ParentNode.RemoveChild(SignatureNode);

            // Adding element for insert signature
            var UBLExtension = doc.SelectSingleNode(@"/" + root.Name + "/ext:UBLExtensions/ext:UBLExtension[2]", nsmgr);
            UBLExtension.ChildNodes[0].AppendChild(SignatureNode);

            doc.Save(newFilePath);
            doc.Save(newFileToPrintPath);
            doc = null;
        }

        public void SignWithXades2(string certFile, string certPassword)
        {
            SignWithXades2SHA256(File.ReadAllBytes(certFile), certPassword);
        }

        public void SignWithXades2SHA256(byte[] certFile, string certPassword)
        {
            XmlNode NodeCustom = null, NodeParent = null;
            //var nodes = this.GetElementsByTagName("Personalizados");
            var nodes = this.GetElementsByTagName("//ext:ExtensionContent");
            if (nodes != null && nodes[0] != null)
            {
                NodeCustom = nodes[0];
                NodeParent = NodeCustom.ParentNode;
                NodeParent.RemoveChild(NodeCustom);
            }

            var xmlDocument = new TElXMLDOMDocument();
            SBUtils.Unit.SetLicenseKey("7BB1ABEA50B8BECBCBA6E7C845DAA78737DD0D2B218837667BB38BF8EFF379E16EEC62285086F7E4670C67A90E965BBAAD128F880A03ADEB2FEA075259A9565A399D707726B78B578D53856BD7EFD0F5792B135E09FC9A048C7DB2F707904BC8955F2235929FB6419EC4B900650067FBE0CAD1CC4DFB4A0D0A5D360C99E3A3E48BF146305BD02320CD95BCE7DFBAA5BA6D4672EA38E23C755F538BBC8190729E72F55DDC943F9D466797B4EDADC4ECC00C04C888F6EEB9C72F0504DF8F2DDB3100DBBFB1434472A01073FD6F3EED963E1095CB4EF8B782C6C695CAE0959CBA1545B7D5ED9EFE08DCF66A6DF23A209357007CDDF8498B9D7433724B7CEB448B39");

            var nsmgr = new XmlNamespaceManager(this.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

            var root = this.DocumentElement;
            //root.Attributes.RemoveNamedItem("id");

            // Adding element for insert signature
            var UBLExtensionsNode = this.SelectSingleNode(@"/" + root.Name + "/ext:UBLExtensions", nsmgr);
            //var ExtensionContent = this.CreateElement("ext", "ExtensionContent");
            var ExtensionContent = this.SelectSingleNode("//*[local-name()='ExtensionContent'][2]");

            var UBLExtension = this.CreateElement("ext", "UBLExtension");
            //UBLExtension.AppendChild(ExtensionContent);

            this.InnerXml = this.InnerXml.Replace(
            "ext:UBLExtension xmlns:ext=\"http://www.dian.gov.co/contratos/facturaelectronica/v1\"", "ext:UBLExtension");

            //var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(this.OuterXml));
            using (var stream = GenerateStreamFromString(this.InnerXml))
            {
                xmlDocument.LoadFromStream(stream, "UTF-8", true);
            }

            TElXMLKeyInfoX509Data X509KeyData = new TElXMLKeyInfoX509Data(true);
            System.Collections.ArrayList CertificatesCollection;
            var cert = LoadCertificate(certFile, certPassword, X509KeyData, out CertificatesCollection);

            TElXMLSigner Signer = new TElXMLSigner();
            TElXAdESSigner XAdESSigner = new TElXAdESSigner();
            TElXMLKeyInfoX509Data X509KeyInfoData = new TElXMLKeyInfoX509Data(false);
            try
            {
                Signer.XAdESProcessor = XAdESSigner;
                Signer.SignatureMethodType = SBXMLSec.Unit.xmtSig;
                Signer.SignatureMethod = SBXMLSec.Unit.xsmRSA_SHA256;
                Signer.CanonicalizationMethod = SBXMLDefs.Unit.xcmCanon;
                Signer.IncludeKey = true;

                var EnvelopingObjectID = "xmldsig-" + Guid.NewGuid().ToString();
                Signer.EnvelopingObjectID = EnvelopingObjectID;

                int k = Signer.References.Add();
                TElXMLReference Ref = Signer.References[k];
                Ref.DigestMethod = SBXMLSec.Unit.xdmSHA256;
                Ref.ID = Signer.EnvelopingObjectID + "-ref0";
                Ref.URI = "";
                Ref.URINode = xmlDocument.DocumentElement;
                Ref.TransformChain.AddEnvelopedSignatureTransform();

                Signer.UpdateReferencesDigest();

                k = Signer.References.Add();
                Ref = Signer.References[k];
                Ref.DigestMethod = SBXMLSec.Unit.xdmSHA256;
                Ref.URI = "#KeyInfo";
                Ref.ID = Signer.EnvelopingObjectID + "-ref1";

                XAdESSigner.XAdESVersion = SBXMLAdES.Unit.XAdES_v1_4_1;
                XAdESSigner.Included = SBXMLAdESIntf.Unit.xipSignerRole;
                XAdESSigner.SigningTime = DateTime.Now;
                XAdESSigner.SignerRole.ClaimedRoles.AddText(XAdESSigner.XAdESVersion, xmlDocument, "third party");

                string URL = "https://facturaelectronica.dian.gov.co/politicadefirma/v2/politicadefirmav2.pdf";
                XAdESSigner.PolicyId.SigPolicyId.Identifier = URL;
                XAdESSigner.PolicyId.SigPolicyId.IdentifierQualifier = SBXMLAdES.Unit.xqtNone;
                //XAdESSigner.PolicyId.SigPolicyId.Description = "Política de Firma FacturaE v3.1";
                XAdESSigner.PolicyId.SigPolicyHash.DigestMethod = SBXMLSec.Unit.DigestMethodToURI(SBXMLSec.Unit.xdmSHA256);
                // uncomment to calculate a digest value or use precalculated value
                //Buf := DownloadData(URL);
                //XAdESSigner.PolicyId.SigPolicyHash.DigestValue := CalculateDigest(@Buf[0], Length(Buf), xdmSHA1);
                var pdfSHA256 = "dMoMvtcG5aIzgYo0tIsSQeVJBDnUnfSOfBpxXrmor0Y=";
                XAdESSigner.PolicyId.SigPolicyHash.DigestValue = SBXMLUtils.Unit.ConvertFromBase64String(pdfSHA256);

                XAdESSigner.SigningCertificatesDigestMethod = SBXMLSec.Unit.xdmSHA256;
                XAdESSigner.SignedPropertiesReferenceDigestMethod = SBXMLSec.Unit.xdmSHA256;

                XAdESSigner.SigningCertificates = new TElMemoryCertStorage();
                XAdESSigner.OwnSigningCertificates = false;
                //XAdESSigner.SigningCertificates.Add(cert);

                foreach (X509Certificate2 cert1 in CertificatesCollection)
                {
                    var sbbcert = new TElX509Certificate();
                    sbbcert.LoadFromBuffer(cert1.RawData);
                    XAdESSigner.SigningCertificates.Add(sbbcert);
                }

                XAdESSigner.Generate(SBXMLAdES.Unit.XAdES_EPES);

                XAdESSigner.QualifyingProperties.XAdESPrefix = "xades";
                XAdESSigner.QualifyingProperties.SignedProperties.ID = Signer.EnvelopingObjectID + "-signedprops";
                XAdESSigner.QualifyingProperties.Target = "#" + Signer.EnvelopingObjectID;
                XAdESSigner.QualifyingProperties.XAdESv141Prefix = "xades141";

                //TElXMLDataObjectFormat DataFormat = new TElXMLDataObjectFormat(XAdESSigner.XAdESVersion);
                //DataFormat.Description = "Factura electrónica";
                //DataFormat.MimeType = "text/xml";
                //DataFormat.ObjectReference = "#Ref1";
                //XAdESSigner.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormats.Add(DataFormat);

                X509KeyInfoData.IncludeKeyValue = false;
                X509KeyInfoData.IncludeDataParams = SBXMLSec.Unit.xkidX509Certificate;
                X509KeyInfoData.Certificate = cert;
                Signer.KeyData = X509KeyInfoData;

                Signer.OnFormatElement += new TSBXMLFormatElementEvent(FormatElement);
                Signer.OnFormatText += new TSBXMLFormatTextEvent(FormatText);

                Signer.GenerateSignature();
                Signer.Signature.ID = EnvelopingObjectID;
                Signer.Signature.SignatureValue.ID = EnvelopingObjectID + "-sigvalue";
                Signer.Signature.KeyInfo.ID = "KeyInfo";
                TElXMLDOMNode node = xmlDocument.DocumentElement;
                Signer.Save(ref node);

                using (var ms = new MemoryStream())
                {
                    xmlDocument.SaveToStream(ms);
                    ms.Flush();
                    ms.Position = 0;

                    this.PreserveWhitespace = true;
                    this.Load(ms);
                }

                //this.DocumentElement.AppendChild(NodeCustom);

                //root = this.DocumentElement;
                //root.Attributes.RemoveNamedItem("id");

                // Putting signature element in the right place
                var SignatureNode = this.SelectSingleNode("//*[local-name()='Signature']");
                if (SignatureNode != null)
                    SignatureNode.ParentNode.RemoveChild(SignatureNode);

                ExtensionContent.AppendChild(SignatureNode);
                
                // Adding element for insert signature
                var UBLExtensionn = this.SelectSingleNode("//*[local-name()='ExtensionContent']");
                UBLExtensionn.AppendChild(SignatureNode);
            }
            finally
            {
                X509KeyInfoData.Dispose();
                Signer.Dispose();
                XAdESSigner.Dispose();
            }
        }

        public void SignWithXades2SHA256(X509Certificate2 certificate2)
        {
            XmlNode NodeCustom = null, NodeParent = null;
            //var nodes = this.GetElementsByTagName("Personalizados");
            var nodes = this.GetElementsByTagName("//ext:ExtensionContent");
            if (nodes != null && nodes[0] != null)
            {
                NodeCustom = nodes[0];
                NodeParent = NodeCustom.ParentNode;
                NodeParent.RemoveChild(NodeCustom);
            }

            var xmlDocument = new TElXMLDOMDocument();
            SBUtils.Unit.SetLicenseKey("7BB1ABEA50B8BECBCBA6E7C845DAA78737DD0D2B218837667BB38BF8EFF379E16EEC62285086F7E4670C67A90E965BBAAD128F880A03ADEB2FEA075259A9565A399D707726B78B578D53856BD7EFD0F5792B135E09FC9A048C7DB2F707904BC8955F2235929FB6419EC4B900650067FBE0CAD1CC4DFB4A0D0A5D360C99E3A3E48BF146305BD02320CD95BCE7DFBAA5BA6D4672EA38E23C755F538BBC8190729E72F55DDC943F9D466797B4EDADC4ECC00C04C888F6EEB9C72F0504DF8F2DDB3100DBBFB1434472A01073FD6F3EED963E1095CB4EF8B782C6C695CAE0959CBA1545B7D5ED9EFE08DCF66A6DF23A209357007CDDF8498B9D7433724B7CEB448B39");

            var nsmgr = new XmlNamespaceManager(this.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

            var root = this.DocumentElement;
            //root.Attributes.RemoveNamedItem("id");

            // Adding element for insert signature
            var UBLExtensionsNode = this.SelectSingleNode(@"/" + root.Name + "/ext:UBLExtensions", nsmgr);
            var ExtensionContent = this.CreateElement("ext", "ExtensionContent");

            var UBLExtension = this.CreateElement("ext", "UBLExtension");
            UBLExtension.AppendChild(ExtensionContent);

            this.InnerXml = this.InnerXml.Replace(
            "ext:UBLExtension xmlns:ext=\"http://www.dian.gov.co/contratos/facturaelectronica/v1\"", "ext:UBLExtension");

            //var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(this.OuterXml));
            using (var stream = GenerateStreamFromString(this.InnerXml))
            {
                xmlDocument.LoadFromStream(stream, "UTF-8", true);
            }

            TElXMLKeyInfoX509Data X509KeyData = new TElXMLKeyInfoX509Data(true);
            System.Collections.ArrayList CertificatesCollection;
            var cert = LoadCertificate(certificate2, X509KeyData, out CertificatesCollection);

            TElXMLSigner Signer = new TElXMLSigner();
            TElXAdESSigner XAdESSigner = new TElXAdESSigner();
            TElXMLKeyInfoX509Data X509KeyInfoData = new TElXMLKeyInfoX509Data(false);
            try
            {
                Signer.XAdESProcessor = XAdESSigner;
                Signer.SignatureMethodType = SBXMLSec.Unit.xmtSig;
                Signer.SignatureMethod = SBXMLSec.Unit.xsmRSA_SHA256;
                Signer.CanonicalizationMethod = SBXMLDefs.Unit.xcmCanon;
                Signer.IncludeKey = true;

                var EnvelopingObjectID = "xmldsig-" + Guid.NewGuid().ToString();
                Signer.EnvelopingObjectID = EnvelopingObjectID;

                int k = Signer.References.Add();
                TElXMLReference Ref = Signer.References[k];
                Ref.DigestMethod = SBXMLSec.Unit.xdmSHA256;
                Ref.ID = Signer.EnvelopingObjectID + "-ref0";
                Ref.URI = "";
                Ref.URINode = xmlDocument.DocumentElement;
                Ref.TransformChain.AddEnvelopedSignatureTransform();

                Signer.UpdateReferencesDigest();

                k = Signer.References.Add();
                Ref = Signer.References[k];
                Ref.DigestMethod = SBXMLSec.Unit.xdmSHA256;
                Ref.URI = "#KeyInfo";
                Ref.ID = Signer.EnvelopingObjectID + "-ref1";

                XAdESSigner.XAdESVersion = SBXMLAdES.Unit.XAdES_v1_4_1;
                XAdESSigner.Included = SBXMLAdESIntf.Unit.xipSignerRole;
                XAdESSigner.SigningTime = DateTime.Now;
                XAdESSigner.SignerRole.ClaimedRoles.AddText(XAdESSigner.XAdESVersion, xmlDocument, "third party");

                string URL = "https://facturaelectronica.dian.gov.co/politicadefirma/v2/politicadefirmav2.pdf";
                XAdESSigner.PolicyId.SigPolicyId.Identifier = URL;
                XAdESSigner.PolicyId.SigPolicyId.IdentifierQualifier = SBXMLAdES.Unit.xqtNone;
                //XAdESSigner.PolicyId.SigPolicyId.Description = "Política de Firma FacturaE v3.1";
                XAdESSigner.PolicyId.SigPolicyHash.DigestMethod = SBXMLSec.Unit.DigestMethodToURI(SBXMLSec.Unit.xdmSHA256);
                // uncomment to calculate a digest value or use precalculated value
                //Buf := DownloadData(URL);
                //XAdESSigner.PolicyId.SigPolicyHash.DigestValue := CalculateDigest(@Buf[0], Length(Buf), xdmSHA1);
                var pdfSHA256 = "dMoMvtcG5aIzgYo0tIsSQeVJBDnUnfSOfBpxXrmor0Y=";
                XAdESSigner.PolicyId.SigPolicyHash.DigestValue = SBXMLUtils.Unit.ConvertFromBase64String(pdfSHA256);

                XAdESSigner.SigningCertificatesDigestMethod = SBXMLSec.Unit.xdmSHA256;
                XAdESSigner.SignedPropertiesReferenceDigestMethod = SBXMLSec.Unit.xdmSHA256;

                XAdESSigner.SigningCertificates = new TElMemoryCertStorage();
                XAdESSigner.OwnSigningCertificates = false;
                //XAdESSigner.SigningCertificates.Add(cert);

                foreach (X509Certificate2 cert1 in CertificatesCollection)
                {
                    var sbbcert = new TElX509Certificate();
                    sbbcert.LoadFromBuffer(cert1.RawData);
                    XAdESSigner.SigningCertificates.Add(sbbcert);
                }

                XAdESSigner.Generate(SBXMLAdES.Unit.XAdES_EPES);

                XAdESSigner.QualifyingProperties.XAdESPrefix = "xades";
                XAdESSigner.QualifyingProperties.SignedProperties.ID = Signer.EnvelopingObjectID + "-signedprops";
                XAdESSigner.QualifyingProperties.Target = "#" + Signer.EnvelopingObjectID;
                XAdESSigner.QualifyingProperties.XAdESv141Prefix = "xades141";

                //TElXMLDataObjectFormat DataFormat = new TElXMLDataObjectFormat(XAdESSigner.XAdESVersion);
                //DataFormat.Description = "Factura electrónica";
                //DataFormat.MimeType = "text/xml";
                //DataFormat.ObjectReference = "#Ref1";
                //XAdESSigner.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormats.Add(DataFormat);

                X509KeyInfoData.IncludeKeyValue = false;
                X509KeyInfoData.IncludeDataParams = SBXMLSec.Unit.xkidX509Certificate;
                X509KeyInfoData.Certificate = cert;
                Signer.KeyData = X509KeyInfoData;

                Signer.OnFormatElement += new TSBXMLFormatElementEvent(FormatElement);
                Signer.OnFormatText += new TSBXMLFormatTextEvent(FormatText);

                Signer.GenerateSignature();
                Signer.Signature.ID = EnvelopingObjectID;
                Signer.Signature.SignatureValue.ID = EnvelopingObjectID + "-sigvalue";
                Signer.Signature.KeyInfo.ID = "KeyInfo";
                TElXMLDOMNode node = xmlDocument.DocumentElement;
                Signer.Save(ref node);

                using (var ms = new MemoryStream())
                {
                    xmlDocument.SaveToStream(ms);
                    ms.Flush();
                    ms.Position = 0;

                    this.PreserveWhitespace = true;
                    this.Load(ms);
                }

                //this.DocumentElement.AppendChild(NodeCustom);

                //root = this.DocumentElement;
                //root.Attributes.RemoveNamedItem("id");

                // Putting signature element in the right place
                var SignatureNode = this.SelectSingleNode("//*[local-name()='Signature']");
                if (SignatureNode != null)
                    SignatureNode.ParentNode.RemoveChild(SignatureNode);

                // Adding element for insert signature
                var UBLExtensionn = this.SelectSingleNode("//*[local-name()='ExtensionContent']");
                UBLExtensionn.AppendChild(SignatureNode);

                try
                {
                    sp.Clear();
                }
                catch (Exception)
                {
                }
            }
            finally
            {
                X509KeyInfoData.Dispose();
                Signer.Dispose();
                XAdESSigner.Dispose();
            }
        }

        public void SignWithXades2(byte[] certFile, string certPassword)
        {
            SignWithXades2SHA256(certFile, certPassword);
        }

        private void TransformNewLineEncoding(string filePath, string fileToPrintPath)
        {
            string nfile = System.IO.File.ReadAllText(filePath);
            Regex regex_newline = new Regex("(\r\n|\r|\n)");
            nfile = regex_newline.Replace(nfile, Environment.NewLine);
            System.IO.File.WriteAllText(filePath, nfile);

            nfile = System.IO.File.ReadAllText(fileToPrintPath);
            nfile = regex_newline.Replace(nfile, Environment.NewLine);
            System.IO.File.WriteAllText(fileToPrintPath, nfile);
        }

        private string TransformNewLineEncoding(string value)
        {
            Regex regex_newline = new Regex("(\r\n|\r|\n)");
            value = regex_newline.Replace(value, Environment.NewLine);
            return value;
        }

        private void FormatElement(object Sender, TElXMLDOMElement Element, int Level, string Path, ref string StartTagWhitespace, ref string EndTagWhitespace)
        {

            string newLine = "\n";
            if (Path.Contains("Object"))
            {
                StartTagWhitespace = "";
            }
            else
            {
                StartTagWhitespace = newLine;
            }

            string s = "";
            StartTagWhitespace = StartTagWhitespace + s;
            if (Element.FirstChild != null)
            {
                bool HasElements = false;
                TElXMLDOMNode Node = Element.FirstChild;
                while (Node != null)
                {
                    if (Node.NodeType == SBXMLCore.Unit.ntElement)
                    {
                        HasElements = true;
                        break;
                    }

                    Node = Node.NextSibling;
                }

                if (HasElements)
                    if (Path.Contains("Object") || Element.LocalName.Contains("Object"))
                    {
                        EndTagWhitespace = "";
                    }
                    else
                    {
                        EndTagWhitespace = newLine + s;
                    }

            }
        }

        private void FormatText(object Sender, ref string Text, short TextType, int Level, string Path)
        {


            if ((TextType == SBXMLDefs.Unit.ttBase64) && (Text.Length > 76))
            {
                string newLine = "\n";
                string s = newLine;
                while (Text.Length > 0)
                {
                    if (Text.Length > 76)
                    {
                        s = s + Text.Substring(0, 76) + newLine;
                        Text = Text.Remove(0, 76);
                    }
                    else
                    {
                        s = s + Text + newLine;
                        Text = "";
                    }
                }

                Text = s;
            }
            else
            {
                try
                {
                    DateTime dt = DateTime.Parse(Text.Replace("Z", ""));
                    string s = dt.ToString("yyyy-MM-dd\"T\"HH:mm:ss.fffzzz");
                    Text = s;
                }
                catch (Exception)
                {
                }
            }

            if (Path.Contains("X509IssuerName"))
            {
                if (_certCount == 0)
                    Text = transformRDN(Text);//Cert1Issuername;//
                if (_certCount == 1)
                {
                    Text = transformRDN(Text); //Cert2Issuername;
                    this._cert3Issuername = Text;
                }
                if (_certCount == 2)
                    Text = transformRDN(Text); //this._cert3Issuername; //Cert3Issuername;//this._cert3Issuername;

                _certCount = _certCount + 1;
            }

            //if(Path.Contains("SigPolicyHash") && Path.Contains("DigestValue"))
            //{
            //    Text = "Ohixl6upD6av8N7pEvDABhEL6hM=";
            //}
        }

        private string transformRDN(string RDN)
        {
            Dictionary<string, string> oidsDict = new Dictionary<string, string>();

            oidsDict.Add("T", "2.5.4.12;0c");
            oidsDict.Add("2.5.4.12", "2.5.4.12;0c");
            oidsDict.Add("E", "1.2.840.113549.1.9.1;16");
            oidsDict.Add("1.2.840.113549.1.9.1", "1.2.840.113549.1.9.1;16");
            oidsDict.Add("SERIALNUMBER", "2.5.4.5;13");
            oidsDict.Add("2.5.4.5", "2.5.4.5;13");
            oidsDict.Add("Description", "2.5.4.13;0c");
            oidsDict.Add("2.5.4.13", "2.5.4.13;0c");
            oidsDict.Add("1.3.6.1.4.1.31136.1.1.10.2", "1.3.6.1.4.1.31136.1.1.10.2;0c");



            System.Collections.ArrayList rdnNodes = new System.Collections.ArrayList();

            //DN myDN = new DN(RDN);
            //foreach (var ardn in myDN.RDNs)
            //{
            //    rdnNodes.Add(ardn.ToString());
            //}

            string newSubjectTransformed = "";
            foreach (var rdnNode in rdnNodes)
            {
                var keyValue = rdnNode.ToString().Split('=');
                keyValue[1] = transformValue(keyValue[1]);
                if (oidsDict.ContainsKey(keyValue[0].ToString()))
                {
                    var hexString = "";
                    string hexLength = "";

                    byte[] ba = System.Text.Encoding.UTF8.GetBytes(keyValue[1]);
                    hexString = BitConverter.ToString(ba);
                    hexString = hexString.Replace("-", "").ToLower();

                    if (ba.Length < 128)
                    {
                        hexLength = ba.Length.ToString("X").ToLower();
                        if (hexLength.Length < 2) hexLength = "0" + hexLength;
                    }
                    else
                        hexLength = getLengthPart(ba.Length) + ba.Length.ToString("X").ToLower();


                    hexString = "#" + oidsDict[keyValue[0]].Split(';')[1] + hexLength + hexString;

                    keyValue[0] = oidsDict[keyValue[0]].Split(';')[0];
                    keyValue[1] = hexString;
                }

                newSubjectTransformed += String.Join("=", keyValue) + ",";

            }
            newSubjectTransformed = newSubjectTransformed.TrimEnd(',');
            return newSubjectTransformed;
        }

        private string getLengthPart(int valueLength)
        {
            string sbinary = Convert.ToString(valueLength, 2).PadLeft(8, '0');
            int byteLength = sbinary.Length / 8;
            string sbLength = "1" + Convert.ToString(byteLength, 2).PadLeft(7, '0');
            return Convert.ToInt32(sbLength, 2).ToString("X");
        }

        private string transformValue(string value)
        {
            if ((value[0] == '"') && (value[value.Length - 1] == '"'))
            {
                value = value.Replace(",", @"\,");
                value = value.Replace("\"", "");
            }
            return value;
        }


        private TElX509Certificate LoadCertificate(string path, string Password, TElXMLKeyInfoX509Data X509KeyData, out System.Collections.ArrayList CertificatesCollection)
        {
            System.Security.Cryptography.X509Certificates.X509Certificate2 cert2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(path, Password, X509KeyStorageFlags.Exportable);
            RSACryptoServiceProvider sp = (RSACryptoServiceProvider)cert2.PrivateKey;
            RSAParameters pars = sp.ExportParameters(true);
            byte[] buf = null;
            int size = 0;
            SBRSA.Unit.EncodePrivateKey(pars.Modulus, pars.Exponent, pars.D, pars.P,
                            pars.Q, pars.DP, pars.DQ, pars.InverseQ, ref buf, ref size);
            buf = new byte[size];
            SBRSA.Unit.EncodePrivateKey(pars.Modulus, pars.Exponent, pars.D, pars.P,
                            pars.Q, pars.DP, pars.DQ, pars.InverseQ, ref buf, ref size);
            TElX509Certificate sbbcert = new TElX509Certificate();
            sbbcert.LoadFromBuffer(cert2.RawData);
            sbbcert.LoadKeyFromBuffer(buf, 0, size);


            X509KeyData.Certificate = sbbcert;
            X509KeyData.IncludeKeyValue = false;
            X509KeyData.IncludeDataParams = 8;

            X509Certificate2Collection collection = new X509Certificate2Collection();
            collection.Import(path, Password, X509KeyStorageFlags.PersistKeySet);
            CertificatesCollection = new System.Collections.ArrayList();
            foreach (X509Certificate2 cert in collection)
            {
                CertificatesCollection.Add(cert);
            }
            CertificatesCollection.Sort(new CertificatesComparer());
            CertificatesCollection.Reverse();
            return X509KeyData.Certificate;
        }

        private TElX509Certificate LoadCertificate(byte[] path, string Password, TElXMLKeyInfoX509Data X509KeyData, out System.Collections.ArrayList CertificatesCollection)
        {
            System.Security.Cryptography.X509Certificates.X509Certificate2 cert2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(path, Password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            RSACryptoServiceProvider sp = (RSACryptoServiceProvider)cert2.PrivateKey;
            RSAParameters pars = sp.ExportParameters(true);
            byte[] buf = null;
            int size = 0;
            SBRSA.Unit.EncodePrivateKey(pars.Modulus, pars.Exponent, pars.D, pars.P,
                            pars.Q, pars.DP, pars.DQ, pars.InverseQ, ref buf, ref size);
            buf = new byte[size];
            SBRSA.Unit.EncodePrivateKey(pars.Modulus, pars.Exponent, pars.D, pars.P,
                            pars.Q, pars.DP, pars.DQ, pars.InverseQ, ref buf, ref size);
            TElX509Certificate sbbcert = new TElX509Certificate();
            sbbcert.LoadFromBuffer(cert2.RawData);
            sbbcert.LoadKeyFromBuffer(buf, 0, size);


            X509KeyData.Certificate = sbbcert;
            X509KeyData.IncludeKeyValue = false;
            X509KeyData.IncludeDataParams = 8;

            X509Certificate2Collection collection = new X509Certificate2Collection();
            collection.Import(path, Password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
            CertificatesCollection = new System.Collections.ArrayList();
            foreach (X509Certificate2 cert in collection)
            {
                CertificatesCollection.Add(cert);
            }
            CertificatesCollection.Sort(new CertificatesComparer());
            CertificatesCollection.Reverse();
            cert2.Reset();
            return X509KeyData.Certificate;
        }

        private TElX509Certificate LoadCertificate(X509Certificate2 certificate2, TElXMLKeyInfoX509Data X509KeyData, out System.Collections.ArrayList CertificatesCollection)
        {
            X509Certificate2 cert2 = new X509Certificate2(certificate2.RawData, "", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            
            sp = (RSACryptoServiceProvider)certificate2.PrivateKey;
            RSAParameters pars = sp.ExportParameters(true);
            byte[] buf = null;
            int size = 0;
            SBRSA.Unit.EncodePrivateKey(pars.Modulus, pars.Exponent, pars.D, pars.P, pars.Q, pars.DP, pars.DQ, pars.InverseQ, ref buf, ref size);
            buf = new byte[size];
            SBRSA.Unit.EncodePrivateKey(pars.Modulus, pars.Exponent, pars.D, pars.P, pars.Q, pars.DP, pars.DQ, pars.InverseQ, ref buf, ref size);

            TElX509Certificate sbbcert = new TElX509Certificate();
            sbbcert.LoadFromBufferPFX(cert2.RawData, "");
            sbbcert.LoadKeyFromBufferAuto(buf, 0, size, "");
            


            X509KeyData.Certificate = sbbcert;
            X509KeyData.IncludeKeyValue = true;
            X509KeyData.IncludeDataParams = 8;

            //X509Certificate2Collection collection = new X509Certificate2Collection();
            //collection.Import(cert2.RawData, "", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
            //collection.Import(cert2.RawData);
            CertificatesCollection = new System.Collections.ArrayList();
            //foreach (X509Certificate2 cert in collection)
            //{
            //    CertificatesCollection.Add(cert2);
            //}
            //CertificatesCollection.Sort(new CertificatesComparer());
            //CertificatesCollection.Reverse();
            //cert2.Reset();
            return X509KeyData.Certificate;
        }

        private string StandardBase64(string base64, int charsPerLine)
        {
            int lines = base64.Length / charsPerLine;
            int count = 0;
            string result = "\r\n";
            while (count < lines)
            {
                result = result + base64.Substring((count * charsPerLine), charsPerLine) + "\r\n";
                count++;
            }
            if (base64.Length % charsPerLine != 0)
            {
                int restchar = (base64.Length - ((base64.Length / charsPerLine) * charsPerLine));
                result = result + base64.Substring(base64.Length - restchar, restchar) + "\r\n";
            }
            return (result);
        }


        private Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public class CertificatesComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((X509Certificate2)x).SerialNumber.CompareTo(((X509Certificate2)y).SerialNumber);
            }
        }
    }
}

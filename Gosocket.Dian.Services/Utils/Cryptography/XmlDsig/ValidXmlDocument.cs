using System;
using System.Xml;
using System.IO;
using System.Xml.Schema;
using System.Text;
using Gosocket.Dian.Services.Utils.Common;

namespace Gosocket.Dian.Services.Utils.Cryptography.XmlDsig
{
    public class ValidXmlDocument : XmlDocument
    {
        private bool _schemaok;
        private Encoding _encoding;
        private readonly StringBuilder _schemaerrdetails = new StringBuilder();

        public string SchemaErrors { get; private set; }

        public string SchemaErrorsDetails
        {
            get
            {
                return _schemaerrdetails.ToString();
            }
        }

        public Encoding Encoding
        {
            get
            {
                if (_encoding == null)
                    _encoding = Formatter.ISO_8859_1;

                if (!(ChildNodes[0] is XmlDeclaration))
                    return _encoding;

                var senc = ((XmlDeclaration)ChildNodes[0]).Encoding;
                if (senc != string.Empty)
                    _encoding = Encoding.GetEncoding(senc);

                return _encoding;
            }
            set { _encoding = value; }
        }

        public bool VerifySchema(byte[] xmlBytes, string[] schemaFiles)
        {
            try
            {
                var xmlsett = new XmlReaderSettings();
                var schset = new XmlSchemaSet();
                foreach (var schemaFile in schemaFiles)
                {
                    schset.Add(XmlSchema.Read(new XmlTextReader(schemaFile), ValidationEventHandle));
                    xmlsett.Schemas.Add(schset);
                    xmlsett.ValidationType = ValidationType.Schema;
                    xmlsett.ValidationEventHandler += ValidationEventHandle;
                }

                var ms = new MemoryStream(xmlBytes);
                var xmlval = XmlReader.Create(ms, xmlsett);

                _schemaok = true;
                while (xmlval.Read()) { }
                xmlval.Close();

                return _schemaok;
            }
            catch (Exception ex)
            {
                SchemaErrors = ex.ToStringMessage();

                return false;
            }
        }

        public bool VerifySchema(byte[] xmlBytes, string schemaFile)
        {
            try
            {
                var xmlsett = new XmlReaderSettings();
                var schset = new XmlSchemaSet();
                schset.Add(XmlSchema.Read(new XmlTextReader(schemaFile), ValidationEventHandle));
                xmlsett.Schemas.Add(schset);
                xmlsett.ValidationType = ValidationType.Schema;
                xmlsett.ValidationEventHandler += ValidationEventHandle;

                var ms = new MemoryStream(xmlBytes);
                var xmlval = XmlReader.Create(ms, xmlsett);

                _schemaok = true;
                while (xmlval.Read()) { }
                xmlval.Close();

                return _schemaok;
            }
            catch (Exception ex)
            {
                SchemaErrors = ex.ToStringMessage();

                return false;
            }
        }

        protected virtual void ValidationEventHandle(object sender, ValidationEventArgs args)
        {
            if (args.Exception == null)
                return;

            _schemaok = false;
            SchemaErrors = SchemaErrors + args.Exception.Message + "," + args.Exception.SourceUri + "," + args.Exception.LinePosition + "," + args.Exception.LineNumber + "\n";
            _schemaerrdetails.AppendLine(args.Exception.Message);
        }
    }
}

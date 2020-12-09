using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Xml;
using System.Xml.Serialization;

namespace Gosocket.Dian.Services.Utils.Common
{
    public class XmlParseNomina
    {
        private static MemoryCache xmlParserDefinitionsInstanceCache = MemoryCache.Default;

        public Dictionary<string, object> Fields { get; set; }
        public XmlDocument AllXmlDefinitions { get; set; }
        public XmlNode CurrentXmlDefinition { get; set; }
        public XmlDocument XmlDocument { get; set; }
        public XmlNode Extentions { get; set; }
        public XPathQuery XPathQuery { get; set; }
        public byte[] XmlContent { get; set; }
        public GlobalDocPayroll globalDocPayrolls { get; set; } = new GlobalDocPayroll();


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

        public XmlParseNomina()
        {
            Fields = new Dictionary<string, object>();
            AllXmlDefinitions = new XmlDocument();
            var xmlParserDefinitions = GetXmlParserDefinitions();
            AllXmlDefinitions.LoadXml(xmlParserDefinitions);
        }
        public XmlParseNomina(byte[] xmlContentBytes, XmlNode extensions = null)
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
                    var nodeDevengadoTotal = "//*[local-name()='DevengadosTotal']";
                    var nodeDeduccionTotal = "//*[local-name()='DeduccionesTotal']";
                    var nodeComprobanteTotal = "//*[local-name()='ComprobanteTotal']";
                    var nodeNotas = "//*[local-name()='Notas']";

                    globalDocPayrolls.devengadosTotal = Convert.ToDecimal(XmlDocument.SelectSingleNode(nodeDevengadoTotal)?.InnerText);
                    globalDocPayrolls.deduccionesTotal = Convert.ToDecimal(XmlDocument.SelectSingleNode(nodeDeduccionTotal)?.InnerText);
                    globalDocPayrolls.comprobanteTotal = Convert.ToDecimal(XmlDocument.SelectSingleNode(nodeComprobanteTotal)?.InnerText);
                    globalDocPayrolls.Notas = XmlDocument.SelectSingleNode(nodeNotas)?.InnerText;

                    // Load xml document.
                    XmlNodeList xPersonas = XmlDocument.GetElementsByTagName("Periodo");
                    for (int i = 0; i < xPersonas.Count; i++) {
                        globalDocPayrolls.FechaIngreso = Convert.ToDateTime(xPersonas[i].Attributes["FechaIngreso"].Value);
                        globalDocPayrolls.FechaPagoInicio = Convert.ToDateTime(xPersonas[i].Attributes["FechaPagoInicio"].Value);
                        globalDocPayrolls.FechaPagoFin = Convert.ToDateTime(xPersonas[i].Attributes["FechaPagoFin"].Value);
                        globalDocPayrolls.TiempoLaborado = xPersonas[i].Attributes["TiempoLaborado"].Value;
                        globalDocPayrolls.FechaLiquidacion = Convert.ToDateTime(xPersonas[i].Attributes["FechaLiquidacion"].Value);
                        globalDocPayrolls.FechaGen = Convert.ToDateTime(xPersonas[i].Attributes["FechaGen"].Value);
                    }
                    XmlNodeList xNumeroSecuenciaXML = XmlDocument.GetElementsByTagName("NumeroSecuenciaXML");
                    for (int j = 0; j < xNumeroSecuenciaXML.Count; j++)
                    {
                        globalDocPayrolls.CodigoTrabajador = Convert.ToUInt64(xNumeroSecuenciaXML[j].Attributes["CodigoTrabajador"].Value);
                        globalDocPayrolls.Prefijo = xNumeroSecuenciaXML[j].Attributes["Prefijo"].Value;
                        globalDocPayrolls.Consecutivo = Convert.ToByte(xNumeroSecuenciaXML[j].Attributes["Consecutivo"].Value);
                        globalDocPayrolls.Numero = xNumeroSecuenciaXML[j].Attributes["Numero"].Value;
                    }
                    XmlNodeList xLugarGeneracionXML = XmlDocument.GetElementsByTagName("LugarGeneracionXML");
                    for (int j = 0; j < xLugarGeneracionXML.Count; j++)
                    {
                        globalDocPayrolls.Pais = xLugarGeneracionXML[j].Attributes["Pais"].Value;
                        globalDocPayrolls.DepartamentoEstado = Convert.ToByte(xLugarGeneracionXML[j].Attributes["DepartamentoEstado"].Value);
                        globalDocPayrolls.MunicipioCiudad = Convert.ToUInt16(xLugarGeneracionXML[j].Attributes["MunicipioCiudad"].Value);
                        globalDocPayrolls.Idioma = xLugarGeneracionXML[j].Attributes["Idioma"].Value;
                    }
                    XmlNodeList xProveedorXML = XmlDocument.GetElementsByTagName("ProveedorXML");
                    for (int j = 0; j < xProveedorXML.Count; j++)
                    {
                        globalDocPayrolls.NIT = Convert.ToUInt32(xProveedorXML[j].Attributes["NIT"].Value);
                        globalDocPayrolls.DV = Convert.ToByte(xProveedorXML[j].Attributes["DV"].Value);
                        globalDocPayrolls.SoftwareID = xProveedorXML[j].Attributes["SoftwareID"].Value;
                        globalDocPayrolls.SoftwareSC = xProveedorXML[j].Attributes["SoftwareSC"].Value;
                    }
                    XmlNodeList xInformacionGeneral = XmlDocument.GetElementsByTagName("InformacionGeneral");
                    for (int j = 0; j < xInformacionGeneral.Count; j++)
                    {
                        globalDocPayrolls.Version = xInformacionGeneral[j].Attributes["Version"].Value;
                        globalDocPayrolls.Ambiente = Convert.ToByte(xInformacionGeneral[j].Attributes["Ambiente"].Value);
                        globalDocPayrolls.CUNE = xInformacionGeneral[j].Attributes["CUNE"].Value;
                        globalDocPayrolls.EncripCUNE = xInformacionGeneral[j].Attributes["EncripCUNE"].Value;
                        globalDocPayrolls.Info_FechaGen = Convert.ToDateTime(xInformacionGeneral[j].Attributes["FechaGen"].Value);
                        globalDocPayrolls.HoraGen = Convert.ToDateTime(xInformacionGeneral[j].Attributes["HoraGen"].Value);
                        globalDocPayrolls.TipoNomina = Convert.ToByte(xInformacionGeneral[j].Attributes["TipoNomina"].Value);
                        globalDocPayrolls.PeriodoNomina = Convert.ToByte(xInformacionGeneral[j].Attributes["PeriodoNomina"].Value);
                        globalDocPayrolls.TipoMoneda = xInformacionGeneral[j].Attributes["TipoMoneda"].Value;
                    }
                    XmlNodeList xReemplazandoPredecesor = XmlDocument.GetElementsByTagName("ReemplazandoPredecesor");
                    for (int j = 0; j < xReemplazandoPredecesor.Count; j++)
                    {
                        globalDocPayrolls.NumeroPred = xReemplazandoPredecesor[j].Attributes["NumeroPred"].Value;
                        globalDocPayrolls.CUNEPred = xReemplazandoPredecesor[j].Attributes["CUNEPred"].Value;
                        globalDocPayrolls.FechaGenPred = Convert.ToDateTime(xReemplazandoPredecesor[j].Attributes["FechaGenPred"].Value);
                    }
                    XmlNodeList xEmpleador = XmlDocument.GetElementsByTagName("Empleador");
                    for (int j = 0; j < xEmpleador.Count; j++)
                    {
                        globalDocPayrolls.Emp_RazonSocial = xEmpleador[j].Attributes["RazonSocial"].Value;
                        globalDocPayrolls.Emp_NIT = Convert.ToUInt32(xEmpleador[j].Attributes["NIT"].Value);
                        globalDocPayrolls.Emp_DV = Convert.ToByte(xEmpleador[j].Attributes["DV"].Value);
                        globalDocPayrolls.Emp_Pais = xEmpleador[j].Attributes["Pais"].Value;
                        globalDocPayrolls.Emp_DepartamentoEstado = Convert.ToByte(xEmpleador[j].Attributes["DepartamentoEstado"].Value);
                        globalDocPayrolls.Emp_MunicipioCiudad = Convert.ToUInt16(xEmpleador[j].Attributes["MunicipioCiudad"].Value);
                        globalDocPayrolls.Emp_Direccion = xEmpleador[j].Attributes["Direccion"].Value;
                        globalDocPayrolls.Emp_Celular = Convert.ToUInt32(xEmpleador[j].Attributes["Celular"].Value);
                        globalDocPayrolls.Emp_Correo = xEmpleador[j].Attributes["Correo"].Value;
                    }
                    XmlNodeList xTrabajador = XmlDocument.GetElementsByTagName("Trabajador");
                    for (int j = 0; j < xTrabajador.Count; j++)
                    {
                        globalDocPayrolls.TipoTrabajador = Convert.ToByte(xTrabajador[j].Attributes["TipoTrabajador"].Value);
                        globalDocPayrolls.SubTipoTrabajador = Convert.ToByte(xTrabajador[j].Attributes["SubTipoTrabajador"].Value);
                        globalDocPayrolls.AltoRiesgoPension = Convert.ToBoolean(xTrabajador[j].Attributes["AltoRiesgoPension"].Value);
                        globalDocPayrolls.TipoDocumento = Convert.ToByte(xTrabajador[j].Attributes["TipoDocumento"].Value);
                        globalDocPayrolls.NumeroDocumento = Convert.ToUInt32(xTrabajador[j].Attributes["NumeroDocumento"].Value);
                        globalDocPayrolls.PrimerApellido = xTrabajador[j].Attributes["PrimerApellido"].Value;
                        globalDocPayrolls.SegundoApellido = xTrabajador[j].Attributes["SegundoApellido"].Value;
                        globalDocPayrolls.PrimerNombre = xTrabajador[j].Attributes["PrimerNombre"].Value;
                        globalDocPayrolls.OtrosNombres = xTrabajador[j].Attributes["OtrosNombres"].Value;
                        globalDocPayrolls.LugarTrabajoPais = xTrabajador[j].Attributes["LugarTrabajoPais"].Value;
                        globalDocPayrolls.LugarTrabajoDepartamentoEstado = Convert.ToByte(xTrabajador[j].Attributes["LugarTrabajoDepartamentoEstado"].Value);
                        globalDocPayrolls.LugarTrabajoMunicipioCiudad = Convert.ToUInt16(xTrabajador[j].Attributes["LugarTrabajoMunicipioCiudad"].Value);
                        globalDocPayrolls.LugarTrabajoDireccion = xTrabajador[j].Attributes["LugarTrabajoDireccion"].Value;
                        globalDocPayrolls.Celular = Convert.ToUInt32(xTrabajador[j].Attributes["Celular"].Value);
                        globalDocPayrolls.Correo = xTrabajador[j].Attributes["Correo"].Value;
                        globalDocPayrolls.SalarioIntegral = Convert.ToBoolean(xTrabajador[j].Attributes["SalarioIntegral"].Value);
                        globalDocPayrolls.CodigoArea = Convert.ToByte(xTrabajador[j].Attributes["CodigoArea"].Value);
                        globalDocPayrolls.NombreArea = xTrabajador[j].Attributes["NombreArea"].Value;
                        globalDocPayrolls.CodigoCargo = Convert.ToByte(xTrabajador[j].Attributes["CodigoCargo"].Value);
                        globalDocPayrolls.NombreCargo = xTrabajador[j].Attributes["NombreCargo"].Value;
                        globalDocPayrolls.TipoContrato = Convert.ToByte(xTrabajador[j].Attributes["TipoContrato"].Value);
                        globalDocPayrolls.Salario = Convert.ToDecimal(xTrabajador[j].Attributes["Salario"].Value);
                        globalDocPayrolls.CodigoTrabajador = Convert.ToUInt64(xTrabajador[j].Attributes["CodigoTrabajador"].Value);
                    }
                    XmlNodeList xPago = XmlDocument.GetElementsByTagName("Pago");
                    for (int j = 0; j < xPago.Count; j++)
                    {
                        globalDocPayrolls.Forma = Convert.ToByte(xPago[j].Attributes["Forma"].Value);
                        globalDocPayrolls.Metodo = Convert.ToByte(xPago[j].Attributes["Metodo"].Value);
                        globalDocPayrolls.Banco = xPago[j].Attributes["Banco"].Value;
                        globalDocPayrolls.TipoCuenta = xPago[j].Attributes["TipoCuenta"].Value;
                        globalDocPayrolls.NumeroCuenta = xPago[j].Attributes["NumeroCuenta"].Value;
                    }
                    XmlNodeList xBasico = XmlDocument.GetElementsByTagName("Basico");
                    for (int j = 0; j < xBasico.Count; j++)
                    {
                        globalDocPayrolls.DiasTrabajados = Convert.ToByte(xBasico[j].Attributes["DiasTrabajados"].Value);
                        globalDocPayrolls.SalarioTrabajado = Convert.ToDecimal(xBasico[j].Attributes["SalarioTrabajado"].Value);
                    }
                    XmlNodeList xVacacionesComunes = XmlDocument.GetElementsByTagName("VacacionesComunes");
                    for (int j = 0; j < xVacacionesComunes.Count; j++)
                    {
                        globalDocPayrolls.FechaInicio = Convert.ToDateTime(xVacacionesComunes[j].Attributes["FechaInicio"].Value);
                        globalDocPayrolls.FechaFin = Convert.ToDateTime(xVacacionesComunes[j].Attributes["FechaFin"].Value);
                        globalDocPayrolls.Cantidad = Convert.ToByte(xVacacionesComunes[j].Attributes["Cantidad"].Value);
                        globalDocPayrolls.Pago = Convert.ToDecimal(xVacacionesComunes[j].Attributes["Pago"].Value);
                    }
                    XmlNodeList xBonificacion = XmlDocument.GetElementsByTagName("Bonificacion");
                    for (int j = 0; j < xBonificacion.Count; j++)
                    {
                        globalDocPayrolls.BonificacionNS = Convert.ToDecimal(xBonificacion[j].Attributes["BonificacionNS"].Value);
                    }
                    XmlNodeList xSalud = XmlDocument.GetElementsByTagName("Salud");
                    for (int j = 0; j < xSalud.Count; j++)
                    {
                        globalDocPayrolls.s_Porcentaje = Convert.ToDecimal(xSalud[j].Attributes["Porcentaje"].Value);
                        globalDocPayrolls.s_ValorBase = Convert.ToDecimal(xSalud[j].Attributes["ValorBase"].Value);
                        globalDocPayrolls.s_Deduccion = Convert.ToDecimal(xSalud[j].Attributes["Deduccion"].Value);
                    }
                    XmlNodeList xFondoPension = XmlDocument.GetElementsByTagName("FondoPension");
                    for (int j = 0; j < xFondoPension.Count; j++)
                    {
                        globalDocPayrolls.FP_Porcentaje = Convert.ToDecimal(xFondoPension[j].Attributes["Porcentaje"].Value);
                        globalDocPayrolls.FP_ValorBase = Convert.ToDecimal(xFondoPension[j].Attributes["ValorBase"].Value);
                        globalDocPayrolls.FP_Deduccion = Convert.ToDecimal(xFondoPension[j].Attributes["Deduccion"].Value);
                    }
                    XmlNodeList xFondoSP = XmlDocument.GetElementsByTagName("FondoSP");
                    for (int j = 0; j < xFondoSP.Count; j++)
                    {
                        globalDocPayrolls.FSP_Porcentaje = Convert.ToDecimal(xFondoSP[j].Attributes["Porcentaje"].Value);
                        globalDocPayrolls.FSP_Porcentaje = Convert.ToDecimal(xFondoSP[j].Attributes["Deduccion"].Value);
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
            var cacheItem = xmlParserDefinitionsInstanceCache.GetCacheItem("XmlParserDefinitionsNomina");
            if (cacheItem == null)
            {
                var fileManager = new FileManager();
                xmlParserDefinitions = fileManager.GetText("configurations", "XmlParserDefinitionsNomina.config");
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1)
                };
                xmlParserDefinitionsInstanceCache.Set(new CacheItem("XmlParserDefinitionsNomina", xmlParserDefinitions), policy);
            }
            else
                xmlParserDefinitions = (string)cacheItem.Value;
            return xmlParserDefinitions;
        }
    }
}

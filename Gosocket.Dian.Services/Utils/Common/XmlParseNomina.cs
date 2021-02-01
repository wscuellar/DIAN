using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Xml;

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
        public bool Novelty { get; set; }


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

                    var novedadXmlNode = XmlDocument.SelectSingleNode("//*[local-name()='Novedad']");
                    this.Novelty = (novedadXmlNode != null) ? bool.Parse(novedadXmlNode.InnerText) : false;
                    globalDocPayrolls.DevengadosTotal = XmlDocument.SelectSingleNode(nodeDevengadoTotal)?.InnerText;
                    globalDocPayrolls.DeduccionesTotal = XmlDocument.SelectSingleNode(nodeDeduccionTotal)?.InnerText;
                    globalDocPayrolls.ComprobanteTotal = XmlDocument.SelectSingleNode(nodeComprobanteTotal)?.InnerText;
                    globalDocPayrolls.Notas = XmlDocument.SelectSingleNode(nodeNotas)?.InnerText;

                    // Load xml document.
                    XmlNodeList xPersonas = XmlDocument.GetElementsByTagName("Periodo");
                    for (int i = 0; i < xPersonas.Count; i++) {
                        globalDocPayrolls.FechaIngreso = xPersonas[i].Attributes["FechaIngreso"]?.InnerText;
                        globalDocPayrolls.FechaPagoInicio = xPersonas[i].Attributes["FechaPagoInicio"]?.InnerText;
                        globalDocPayrolls.FechaPagoFin = xPersonas[i].Attributes["FechaPagoFin"]?.InnerText;
                        globalDocPayrolls.TiempoLaborado = xPersonas[i].Attributes["TiempoLaborado"]?.InnerText;
                        globalDocPayrolls.FechaLiquidacion = xPersonas[i].Attributes["FechaLiquidacion"]?.InnerText;
                        globalDocPayrolls.FechaGen = xPersonas[i].Attributes["FechaGen"]?.InnerText;
                    }
                    XmlNodeList xNumeroSecuenciaXML = XmlDocument.GetElementsByTagName("NumeroSecuenciaXML");
                    for (int j = 0; j < xNumeroSecuenciaXML.Count; j++)
                    {
                        globalDocPayrolls.CodigoTrabajador = xNumeroSecuenciaXML[j].Attributes["CodigoTrabajador"]?.InnerText;
                        globalDocPayrolls.Prefijo = xNumeroSecuenciaXML[j].Attributes["Prefijo"]?.InnerText;
                        globalDocPayrolls.Consecutivo = xNumeroSecuenciaXML[j].Attributes["Consecutivo"]?.InnerText;
                        globalDocPayrolls.Numero = xNumeroSecuenciaXML[j].Attributes["Numero"]?.InnerText;
                    }
                    XmlNodeList xLugarGeneracionXML = XmlDocument.GetElementsByTagName("LugarGeneracionXML");
                    for (int j = 0; j < xLugarGeneracionXML.Count; j++)
                    {
                        globalDocPayrolls.Pais = xLugarGeneracionXML[j].Attributes["Pais"]?.InnerText;
                        globalDocPayrolls.DepartamentoEstado = xLugarGeneracionXML[j].Attributes["DepartamentoEstado"]?.InnerText;
                        globalDocPayrolls.MunicipioCiudad = xLugarGeneracionXML[j].Attributes["MunicipioCiudad"]?.InnerText;
                        globalDocPayrolls.Idioma = xLugarGeneracionXML[j].Attributes["Idioma"]?.InnerText;
                    }
                    XmlNodeList xProveedorXML = XmlDocument.GetElementsByTagName("ProveedorXML");
                    for (int j = 0; j < xProveedorXML.Count; j++)
                    {
                        globalDocPayrolls.NIT = xProveedorXML[j].Attributes["NIT"]?.InnerText;
                        globalDocPayrolls.DV = xProveedorXML[j].Attributes["DV"]?.InnerText;
                        globalDocPayrolls.SoftwareID = xProveedorXML[j].Attributes["SoftwareID"]?.InnerText;
                        globalDocPayrolls.SoftwareSC = xProveedorXML[j].Attributes["SoftwareSC"]?.InnerText;
                    }
                    XmlNodeList xInformacionGeneral = XmlDocument.GetElementsByTagName("InformacionGeneral");
                    for (int j = 0; j < xInformacionGeneral.Count; j++)
                    {
                        globalDocPayrolls.Version = xInformacionGeneral[j].Attributes["Version"]?.InnerText;
                        globalDocPayrolls.Ambiente = xInformacionGeneral[j].Attributes["Ambiente"]?.InnerText;
                        globalDocPayrolls.CUNE = xInformacionGeneral[j].Attributes["CUNE"]?.InnerText;
                        globalDocPayrolls.EncripCUNE = xInformacionGeneral[j].Attributes["EncripCUNE"]?.InnerText;
                        globalDocPayrolls.Info_FechaGen = xInformacionGeneral[j].Attributes["FechaGen"]?.InnerText;
                        globalDocPayrolls.HoraGen = xInformacionGeneral[j].Attributes["HoraGen"]?.InnerText;
                        globalDocPayrolls.TipoNomina = xInformacionGeneral[j].Attributes["TipoNomina"]?.InnerText;
                        globalDocPayrolls.PeriodoNomina = xInformacionGeneral[j].Attributes["PeriodoNomina"]?.InnerText;
                        globalDocPayrolls.TipoMoneda = xInformacionGeneral[j].Attributes["TipoMoneda"]?.InnerText;
                    }
                    XmlNodeList xReemplazandoPredecesor = XmlDocument.GetElementsByTagName("ReemplazandoPredecesor");
                    for (int j = 0; j < xReemplazandoPredecesor.Count; j++)
                    {
                        globalDocPayrolls.NumeroPred = xReemplazandoPredecesor[j].Attributes["NumeroPred"]?.InnerText;
                        globalDocPayrolls.CUNEPred = xReemplazandoPredecesor[j].Attributes["CUNEPred"]?.InnerText;
                        globalDocPayrolls.FechaGenPred = Convert.ToDateTime(xReemplazandoPredecesor[j].Attributes["FechaGenPred"]?.InnerText);
                    }
                    XmlNodeList xEmpleador = XmlDocument.GetElementsByTagName("Empleador");
                    for (int j = 0; j < xEmpleador.Count; j++)
                    {
                        globalDocPayrolls.Emp_RazonSocial = xEmpleador[j].Attributes["RazonSocial"]?.InnerText;
                        globalDocPayrolls.Emp_NIT = xEmpleador[j].Attributes["NIT"]?.InnerText;
                        globalDocPayrolls.Emp_DV = xEmpleador[j].Attributes["DV"]?.InnerText;
                        globalDocPayrolls.Emp_Pais = xEmpleador[j].Attributes["Pais"]?.InnerText;
                        globalDocPayrolls.Emp_DepartamentoEstado = xEmpleador[j].Attributes["DepartamentoEstado"]?.InnerText;
                        globalDocPayrolls.Emp_MunicipioCiudad = xEmpleador[j].Attributes["MunicipioCiudad"]?.InnerText;
                        globalDocPayrolls.Emp_Direccion = xEmpleador[j].Attributes["Direccion"]?.InnerText;                     
                    }
                    XmlNodeList xTrabajador = XmlDocument.GetElementsByTagName("Trabajador");
                    for (int j = 0; j < xTrabajador.Count; j++)
                    {
                        globalDocPayrolls.TipoTrabajador = xTrabajador[j].Attributes["TipoTrabajador"]?.InnerText;
                        globalDocPayrolls.SubTipoTrabajador = xTrabajador[j].Attributes["SubTipoTrabajador"]?.InnerText;
                        globalDocPayrolls.AltoRiesgoPension = Convert.ToBoolean(xTrabajador[j].Attributes["AltoRiesgoPension"]?.InnerText);
                        globalDocPayrolls.TipoDocumento = xTrabajador[j].Attributes["TipoDocumento"]?.InnerText;
                        globalDocPayrolls.NumeroDocumento = xTrabajador[j].Attributes["NumeroDocumento"]?.InnerText;
                        globalDocPayrolls.PrimerApellido = xTrabajador[j].Attributes["PrimerApellido"]?.InnerText;
                        globalDocPayrolls.SegundoApellido = xTrabajador[j].Attributes["SegundoApellido"]?.InnerText;
                        globalDocPayrolls.PrimerNombre = xTrabajador[j].Attributes["PrimerNombre"]?.InnerText;
                        globalDocPayrolls.OtrosNombres = xTrabajador[j].Attributes["OtrosNombres"]?.InnerText;
                        globalDocPayrolls.LugarTrabajoPais = xTrabajador[j].Attributes["LugarTrabajoPais"]?.InnerText;
                        globalDocPayrolls.LugarTrabajoDepartamentoEstado = xTrabajador[j].Attributes["LugarTrabajoDepartamentoEstado"]?.InnerText;
                        globalDocPayrolls.LugarTrabajoMunicipioCiudad = xTrabajador[j].Attributes["LugarTrabajoMunicipioCiudad"]?.InnerText;
                        globalDocPayrolls.LugarTrabajoDireccion = xTrabajador[j].Attributes["LugarTrabajoDireccion"]?.InnerText;                      
                        globalDocPayrolls.SalarioIntegral = Convert.ToBoolean(xTrabajador[j].Attributes["SalarioIntegral"]?.InnerText);                   
                        globalDocPayrolls.TipoContrato = xTrabajador[j].Attributes["TipoContrato"]?.InnerText;
                        globalDocPayrolls.Sueldo = xTrabajador[j].Attributes["Salario"]?.InnerText;
                        globalDocPayrolls.CodigoTrabajador = xTrabajador[j].Attributes["CodigoTrabajador"]?.InnerText;
                    }
                    XmlNodeList xPago = XmlDocument.GetElementsByTagName("Pago");
                    for (int j = 0; j < xPago.Count; j++)
                    {
                        globalDocPayrolls.Forma = xPago[j].Attributes["Forma"]?.InnerText;
                        globalDocPayrolls.Metodo = xPago[j].Attributes["Metodo"]?.InnerText;
                        globalDocPayrolls.Banco = xPago[j].Attributes["Banco"]?.InnerText;
                        globalDocPayrolls.TipoCuenta = xPago[j].Attributes["TipoCuenta"]?.InnerText;
                        globalDocPayrolls.NumeroCuenta = xPago[j].Attributes["NumeroCuenta"]?.InnerText;
                    }
                    XmlNodeList xBasico = XmlDocument.GetElementsByTagName("Basico");
                    for (int j = 0; j < xBasico.Count; j++)
                    {
                        globalDocPayrolls.DiasTrabajados = xBasico[j].Attributes["DiasTrabajados"]?.InnerText;
                        globalDocPayrolls.SalarioTrabajado = xBasico[j].Attributes["SalarioTrabajado"]?.InnerText;
                    }
                    XmlNodeList xVacacionesComunes = XmlDocument.GetElementsByTagName("VacacionesComunes");
                    for (int j = 0; j < xVacacionesComunes.Count; j++)
                    {
                        globalDocPayrolls.FechaInicio = xVacacionesComunes[j].Attributes["FechaInicio"]?.InnerText;
                        globalDocPayrolls.FechaFin = xVacacionesComunes[j].Attributes["FechaFin"]?.InnerText;
                        globalDocPayrolls.Cantidad = xVacacionesComunes[j].Attributes["Cantidad"]?.InnerText;
                        globalDocPayrolls.Pago = xVacacionesComunes[j].Attributes["Pago"]?.InnerText;
                    }
                    XmlNodeList xBonificacion = XmlDocument.GetElementsByTagName("Bonificacion");
                    for (int j = 0; j < xBonificacion.Count; j++)
                    {
                        globalDocPayrolls.BonificacionNS = xBonificacion[j].Attributes["BonificacionNS"]?.InnerText;
                    }
                    XmlNodeList xSalud = XmlDocument.GetElementsByTagName("Salud");
                    for (int j = 0; j < xSalud.Count; j++)
                    {
                        globalDocPayrolls.s_Porcentaje = xSalud[j].Attributes["Porcentaje"]?.InnerText;
                        globalDocPayrolls.s_ValorBase = xSalud[j].Attributes["ValorBase"]?.InnerText;
                        globalDocPayrolls.s_Deduccion = xSalud[j].Attributes["Deduccion"]?.InnerText;
                    }
                    XmlNodeList xFondoPension = XmlDocument.GetElementsByTagName("FondoPension");
                    for (int j = 0; j < xFondoPension.Count; j++)
                    {
                        globalDocPayrolls.FP_Porcentaje = xFondoPension[j].Attributes["Porcentaje"]?.InnerText;
                        globalDocPayrolls.FP_ValorBase = xFondoPension[j].Attributes["ValorBase"]?.InnerText;
                        globalDocPayrolls.FP_Deduccion = xFondoPension[j].Attributes["Deduccion"]?.InnerText;
                    }
                    XmlNodeList xFondoSP = XmlDocument.GetElementsByTagName("FondoSP");
                    for (int j = 0; j < xFondoSP.Count; j++)
                    {
                        globalDocPayrolls.FSP_Porcentaje = xFondoSP[j].Attributes["Porcentaje"]?.InnerText;
                        globalDocPayrolls.FSP_Porcentaje = xFondoSP[j].Attributes["Deduccion"]?.InnerText;
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

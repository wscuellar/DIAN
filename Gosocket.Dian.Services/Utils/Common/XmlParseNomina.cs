using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Xml;
using System.Text.RegularExpressions;

namespace Gosocket.Dian.Services.Utils.Common
{
    public class XmlParseNomina
    {
        private static MemoryCache xmlParserDefinitionsInstanceCache = MemoryCache.Default;

        public Dictionary<string, object> Fields { get; set; }
        public XmlDocument AllXmlDefinitions { get; set; }
        public XmlNode CurrentXmlDefinition { get; set; }
        public XmlDocument xmlDocument { get; set; }
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
        public string SequenceConsecutive { get; set; }
        public bool HasReplaceNode { get; set; }
        public bool HasRemoveNode { get; set; }


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

            xmlDocument = new XmlDocument { PreserveWhitespace = true };

            using (var ms = new MemoryStream(XmlContent))
            {
                using (var sr = new StreamReader(ms, System.Text.Encoding.GetEncoding(Encoding)))
                {
                    xmlDocument.XmlResolver = null;
                    xmlDocument.Load(sr);

                    var noteTypePath = "//*[local-name()='TipoNota']";
                    var noteTypeNodeValue = xmlDocument.SelectSingleNode(noteTypePath)?.InnerText;

                    if (string.IsNullOrWhiteSpace(noteTypeNodeValue)) // Invidual Payroll
                    {
                        this.ProcessXmlDocument(xmlDocument, noteTypeNodeValue);
                    }
                    else // Payroll Adjustment
                    {
                        this.globalDocPayrolls.TipoNota = int.Parse(noteTypeNodeValue);
                        this.ProcessXmlDocument(xmlDocument, (this.globalDocPayrolls.TipoNota.Value == (int)IndividualPayrollAdjustmentNoteType.Replace) ? "Reemplazar" : "Eliminar");
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
                result = XPathQuery.Evaluate(xmlDocument);
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
            var nodeList = XPathQuery.Select(xmlDocument);
            return nodeList.Count > 0 ? nodeList[0] : null;
        }

        public XmlNodeList SelectNodes(string xPath, XmlNode relative = null)
        {
            if (Prefix != "sig")
                xPath = xPath.Replace("sig:", string.Format("{0}:", Prefix));

            XPathQuery.Query = xPath;
            return XPathQuery.Select(xmlDocument, relative);
        }

        private static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            return Regex.Replace(strIn, @"[^\w\.@-]", "");
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

        private void ProcessXmlDocument(XmlDocument xmlDocument, string rootNodeName)
        {
            var rootNodeXPath = string.Empty;
            if(!string.IsNullOrWhiteSpace(rootNodeName))
            {
                rootNodeXPath = $"/*[local-name()='{rootNodeName}']";
            }

            var nodeDevengadoTotal = $"/{rootNodeXPath}/*[local-name()='DevengadosTotal']";
            var nodeDeduccionTotal = $"/{rootNodeXPath}/*[local-name()='DeduccionesTotal']";
            var nodeComprobanteTotal = $"/{rootNodeXPath}/*[local-name()='ComprobanteTotal']";
            var nodeNotas = $"/{rootNodeXPath}/*[local-name()='Notas']";
            var novedadPath = $"/{rootNodeXPath}/*[local-name()='Novedad']";
            var paymentsDatesPath = $"/{rootNodeXPath}/*[local-name()='FechasPagos']";

            var tempNovedadXmlNode = xmlDocument.SelectSingleNode(novedadPath);
            var tempDevengadosTotal = xmlDocument.SelectSingleNode(nodeDevengadoTotal)?.InnerText;
            var tempDeduccionesTotal = xmlDocument.SelectSingleNode(nodeDeduccionTotal)?.InnerText;
            var tempComprobanteTotal = xmlDocument.SelectSingleNode(nodeComprobanteTotal)?.InnerText;

            if(tempNovedadXmlNode != null && !string.IsNullOrWhiteSpace(tempNovedadXmlNode.InnerText))
            {
                this.Novelty = bool.Parse(tempNovedadXmlNode.InnerText);
                globalDocPayrolls.CUNENov = tempNovedadXmlNode.Attributes["CUNENov"]?.InnerText.ToLower();
            }

            globalDocPayrolls.Novedad = this.Novelty;
            globalDocPayrolls.DevengadosTotal = (!string.IsNullOrWhiteSpace(tempDevengadosTotal) ? double.Parse(tempDevengadosTotal) : 0);
            globalDocPayrolls.DeduccionesTotal = (!string.IsNullOrWhiteSpace(tempDeduccionesTotal) ? double.Parse(tempDeduccionesTotal) : 0);
            globalDocPayrolls.ComprobanteTotal = (!string.IsNullOrWhiteSpace(tempComprobanteTotal) ? double.Parse(tempComprobanteTotal) : 0);
            globalDocPayrolls.Notas = xmlDocument.SelectSingleNode(nodeNotas)?.InnerText;

            // Load xml document.
            XmlNodeList xPeriod = xmlDocument.GetElementsByTagName("Periodo");
            for (int i = 0; i < xPeriod.Count; i++)
            {
                globalDocPayrolls.FechaIngreso = Convert.ToDateTime(xPeriod[i].Attributes["FechaIngreso"]?.InnerText);
                globalDocPayrolls.FechaRetiro = string.IsNullOrWhiteSpace(xPeriod[i].Attributes["FechaRetiro"]?.InnerText) ? (DateTime?)null : Convert.ToDateTime(xPeriod[i].Attributes["FechaRetiro"]?.InnerText);
                globalDocPayrolls.FechaPagoInicio = Convert.ToDateTime(xPeriod[i].Attributes["FechaLiquidacionInicio"]?.InnerText);
                globalDocPayrolls.FechaPagoFin = Convert.ToDateTime(xPeriod[i].Attributes["FechaLiquidacionFin"]?.InnerText);
                globalDocPayrolls.TiempoLaborado = xPeriod[i].Attributes["TiempoLaborado"]?.InnerText;
                //globalDocPayrolls.FechaLiquidacion = Convert.ToDateTime(xPeriod[i].Attributes["FechaLiquidacion"]?.InnerText);
                globalDocPayrolls.FechaGen = Convert.ToDateTime(xPeriod[i].Attributes["FechaGen"]?.InnerText);
            }

            var paymentsDateNode = xmlDocument.SelectSingleNode(paymentsDatesPath);
            if (paymentsDateNode != null)
            {
                var datesList = new List<string>();
                XmlNodeList paymentsDatesList = paymentsDateNode.SelectNodes($"/{rootNodeXPath}/*[local-name()='FechasPagos']");
                for (int i = 0; i < paymentsDatesList.Count; i++)
                {
                    int j = 0;
                    while (j < CleanInput(paymentsDatesList[i]?.InnerText).Length)
                    {
                        var initIndex = j;
                        datesList.Add(CleanInput(paymentsDatesList[i]?.InnerText).Substring(initIndex, 10));
                        j = initIndex + 10;
                    }
                }
                globalDocPayrolls.FechasPagos = string.Join(";", datesList);
            }

            XmlNodeList xNumeroSecuenciaXML = xmlDocument.GetElementsByTagName("NumeroSecuenciaXML");
            for (int j = 0; j < xNumeroSecuenciaXML.Count; j++)
            {
                globalDocPayrolls.CodigoTrabajador = xNumeroSecuenciaXML[j].Attributes["CodigoTrabajador"]?.InnerText;
                globalDocPayrolls.Prefijo = xNumeroSecuenciaXML[j].Attributes["Prefijo"]?.InnerText;
                this.SequenceConsecutive = xNumeroSecuenciaXML[j].Attributes["Consecutivo"]?.InnerText;
                globalDocPayrolls.Consecutivo = (!string.IsNullOrWhiteSpace(this.SequenceConsecutive) ? this.SequenceConsecutive : "0");
                globalDocPayrolls.Numero = xNumeroSecuenciaXML[j].Attributes["Numero"]?.InnerText;
            }
            XmlNodeList xLugarGeneracionXML = xmlDocument.GetElementsByTagName("LugarGeneracionXML");
            for (int j = 0; j < xLugarGeneracionXML.Count; j++)
            {
                globalDocPayrolls.Pais = xLugarGeneracionXML[j].Attributes["Pais"]?.InnerText;
                globalDocPayrolls.DepartamentoEstado = xLugarGeneracionXML[j].Attributes["DepartamentoEstado"]?.InnerText;
                globalDocPayrolls.MunicipioCiudad = xLugarGeneracionXML[j].Attributes["MunicipioCiudad"]?.InnerText;
                globalDocPayrolls.Idioma = xLugarGeneracionXML[j].Attributes["Idioma"]?.InnerText;
            }
            XmlNodeList xProveedorXML = xmlDocument.GetElementsByTagName("ProveedorXML");
            for (int j = 0; j < xProveedorXML.Count; j++)
            {
                globalDocPayrolls.Prov_RazonSocial = xProveedorXML[j].Attributes["RazonSocial"]?.InnerText;
                globalDocPayrolls.Prov_PrimerApellido = xProveedorXML[j].Attributes["PrimerApellido"]?.InnerText;
                globalDocPayrolls.Prov_SegundoApellido = xProveedorXML[j].Attributes["SegundoApellido"]?.InnerText;
                globalDocPayrolls.Prov_PrimerNombre = xProveedorXML[j].Attributes["PrimerNombre"]?.InnerText;
                globalDocPayrolls.Prov_OtrosNombres = xProveedorXML[j].Attributes["OtrosNombres"]?.InnerText;
                globalDocPayrolls.NIT = xProveedorXML[j].Attributes["NIT"]?.InnerText;
                globalDocPayrolls.DV = xProveedorXML[j].Attributes["DV"]?.InnerText;
                globalDocPayrolls.SoftwareID = xProveedorXML[j].Attributes["SoftwareID"]?.InnerText;
                globalDocPayrolls.SoftwareSC = xProveedorXML[j].Attributes["SoftwareSC"]?.InnerText;
            }

            XmlNodeList xQrXML = xmlDocument.GetElementsByTagName("CodigoQR");
            for (int j = 0; j < xQrXML.Count; j++)
            {
                globalDocPayrolls.CodigoQR = xQrXML[j].InnerText;


            }



            XmlNodeList xInformacionGeneral = xmlDocument.GetElementsByTagName("InformacionGeneral");
            for (int j = 0; j < xInformacionGeneral.Count; j++)
            {
                globalDocPayrolls.Version = xInformacionGeneral[j].Attributes["Version"]?.InnerText;
                globalDocPayrolls.Ambiente = xInformacionGeneral[j].Attributes["Ambiente"]?.InnerText;
                globalDocPayrolls.CUNE = xInformacionGeneral[j].Attributes["CUNE"]?.InnerText.ToLower();
                globalDocPayrolls.EncripCUNE = xInformacionGeneral[j].Attributes["EncripCUNE"]?.InnerText;
                globalDocPayrolls.Info_FechaGen = Convert.ToDateTime(xInformacionGeneral[j].Attributes["FechaGen"]?.InnerText);
                globalDocPayrolls.HoraGen = xInformacionGeneral[j].Attributes["HoraGen"]?.InnerText;
                globalDocPayrolls.PeriodoNomina = xInformacionGeneral[j].Attributes["PeriodoNomina"]?.InnerText;
                globalDocPayrolls.TipoMoneda = xInformacionGeneral[j].Attributes["TipoMoneda"]?.InnerText;
                globalDocPayrolls.TRM = xInformacionGeneral[j].Attributes["TRM"]?.InnerText;
                globalDocPayrolls.TipoXML = xInformacionGeneral[j].Attributes["TipoXML"]?.InnerText;
            }
            XmlNodeList xReplaceNode = xmlDocument.GetElementsByTagName("Reemplazar");
            if (xReplaceNode != null && xReplaceNode.Count > 0) HasReplaceNode = true;
            XmlNodeList xReemplazandoPredecesor = xmlDocument.GetElementsByTagName("ReemplazandoPredecesor");
            for (int j = 0; j < xReemplazandoPredecesor.Count; j++)
            {
                globalDocPayrolls.NumeroPred = xReemplazandoPredecesor[j].Attributes["NumeroPred"]?.InnerText;
                globalDocPayrolls.CUNEPred = xReemplazandoPredecesor[j].Attributes["CUNEPred"]?.InnerText.ToLower();
                globalDocPayrolls.FechaGenPred = Convert.ToDateTime(xReemplazandoPredecesor[j].Attributes["FechaGenPred"]?.InnerText);

                var paymentsDateNodeR = xmlDocument.SelectSingleNode(paymentsDatesPath);
                if (paymentsDateNodeR != null)
                {
                    var datesList = new List<string>();
                    XmlNodeList paymentsDatesList = paymentsDateNodeR.ChildNodes;
                    for (int i = 0; i < paymentsDatesList.Count; i++)
                    {
                        datesList.Add(paymentsDatesList[i]?.InnerText);
                    }
                    globalDocPayrolls.FechasPagos = string.Join(";", datesList);
                }

            }
            XmlNodeList xRemoveNode = xmlDocument.GetElementsByTagName("Eliminar");
            if (xRemoveNode != null && xRemoveNode.Count > 0) HasRemoveNode = true;
            XmlNodeList xEliminandoPredecesor = xmlDocument.GetElementsByTagName("EliminandoPredecesor");
            for (int j = 0; j < xEliminandoPredecesor.Count; j++)
            {
                globalDocPayrolls.NumeroPred = xEliminandoPredecesor[j].Attributes["NumeroPred"]?.InnerText;
                globalDocPayrolls.CUNEPred = xEliminandoPredecesor[j].Attributes["CUNEPred"]?.InnerText.ToLower();
                globalDocPayrolls.FechaGenPred = Convert.ToDateTime(xEliminandoPredecesor[j].Attributes["FechaGenPred"]?.InnerText);
            }
            XmlNodeList xEmpleador = xmlDocument.GetElementsByTagName("Empleador");
            for (int j = 0; j < xEmpleador.Count; j++)
            {
                globalDocPayrolls.Emp_RazonSocial = xEmpleador[j].Attributes["RazonSocial"]?.InnerText;
                globalDocPayrolls.Emp_PrimerApellido = xEmpleador[j].Attributes["PrimerApellido"]?.InnerText;
                globalDocPayrolls.Emp_SegundoApellido = xEmpleador[j].Attributes["SegundoApellido"]?.InnerText;
                globalDocPayrolls.Emp_PrimerNombre = xEmpleador[j].Attributes["PrimerNombre"]?.InnerText;
                globalDocPayrolls.Emp_OtrosNombres = xEmpleador[j].Attributes["OtrosNombres"]?.InnerText;
                globalDocPayrolls.Emp_NIT = xEmpleador[j].Attributes["NIT"]?.InnerText;
                globalDocPayrolls.Emp_DV = xEmpleador[j].Attributes["DV"]?.InnerText;
                globalDocPayrolls.Emp_Pais = xEmpleador[j].Attributes["Pais"]?.InnerText;
                globalDocPayrolls.Emp_DepartamentoEstado = xEmpleador[j].Attributes["DepartamentoEstado"]?.InnerText;
                globalDocPayrolls.Emp_MunicipioCiudad = xEmpleador[j].Attributes["MunicipioCiudad"]?.InnerText;
                globalDocPayrolls.Emp_Direccion = xEmpleador[j].Attributes["Direccion"]?.InnerText;
            }
            XmlNodeList xTrabajador = xmlDocument.GetElementsByTagName("Trabajador");
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
                var tempSueldo = xTrabajador[j].Attributes["Sueldo"]?.InnerText;
                globalDocPayrolls.Sueldo = (!string.IsNullOrWhiteSpace(tempSueldo) ? double.Parse(tempSueldo) : 0);
                globalDocPayrolls.Trab_CodigoTrabajador = xTrabajador[j].Attributes["CodigoTrabajador"]?.InnerText;
            }
            XmlNodeList xPago = xmlDocument.GetElementsByTagName("Pago");
            for (int j = 0; j < xPago.Count; j++)
            {
                globalDocPayrolls.Forma = xPago[j].Attributes["Forma"]?.InnerText;
                globalDocPayrolls.Metodo = xPago[j].Attributes["Metodo"]?.InnerText;
                globalDocPayrolls.Banco = xPago[j].Attributes["Banco"]?.InnerText;
                globalDocPayrolls.TipoCuenta = xPago[j].Attributes["TipoCuenta"]?.InnerText;
                globalDocPayrolls.NumeroCuenta = xPago[j].Attributes["NumeroCuenta"]?.InnerText;
            }
            XmlNodeList xBasico = xmlDocument.GetElementsByTagName("Basico");
            for (int j = 0; j < xBasico.Count; j++)
            {
                globalDocPayrolls.DiasTrabajados = xBasico[j].Attributes["DiasTrabajados"]?.InnerText;
                globalDocPayrolls.SalarioTrabajado = xBasico[j].Attributes["SueldoTrabajado"]?.InnerText;
            }
            XmlNodeList xAuxTransporte = xmlDocument.GetElementsByTagName("Transporte");
            for (int j = 0; j < xAuxTransporte.Count; j++)
            {
                globalDocPayrolls.AuxTransporte = xAuxTransporte[j].Attributes["AuxilioTransporte"]?.InnerText;
                globalDocPayrolls.ViaticoManuAlojS = xAuxTransporte[j].Attributes["ViaticoManuAlojS"]?.InnerText;
                globalDocPayrolls.ViaticoManuAlojNS = xAuxTransporte[j].Attributes["ViaticoManuAlojNS"]?.InnerText;
            }
            XmlNodeList xHEDs = xmlDocument.GetElementsByTagName("HED");
            double HEDsTotal = 0;
            for (int j = 0; j < xHEDs.Count; j++)
            {
                var temp = xHEDs[j].Attributes["Pago"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(temp)) HEDsTotal += double.Parse(temp);
            }
            globalDocPayrolls.HED = HEDsTotal.ToString();
            //
            XmlNodeList xHENs = xmlDocument.GetElementsByTagName("HEN");
            double HENsTotal = 0;
            for (int j = 0; j < xHENs.Count; j++)
            {
                var temp = xHENs[j].Attributes["Pago"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(temp)) HENsTotal += double.Parse(temp);
            }
            globalDocPayrolls.HEN = HENsTotal.ToString();
            //
            XmlNodeList xHRNs = xmlDocument.GetElementsByTagName("HRN");
            double HRNsTotal = 0;
            for (int j = 0; j < xHRNs.Count; j++)
            {
                var temp = xHRNs[j].Attributes["Pago"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(temp)) HRNsTotal += double.Parse(temp);
            }
            globalDocPayrolls.HRN = HRNsTotal.ToString();
            //
            XmlNodeList xHEDDFs = xmlDocument.GetElementsByTagName("HEDDF");
            double HEDDFsTotal = 0;
            for (int j = 0; j < xHEDDFs.Count; j++)
            {
                var temp = xHEDDFs[j].Attributes["Pago"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(temp)) HEDDFsTotal += double.Parse(temp);
            }
            globalDocPayrolls.HEDDF = HEDDFsTotal.ToString();
            //
            XmlNodeList xHRDDFs = xmlDocument.GetElementsByTagName("HRDDF");
            double HRDDFsTotal = 0;
            for (int j = 0; j < xHRDDFs.Count; j++)
            {
                var temp = xHRDDFs[j].Attributes["Pago"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(temp)) HRDDFsTotal += double.Parse(temp);
            }
            globalDocPayrolls.HRDDF = HRDDFsTotal.ToString();
            //
            XmlNodeList xHENDFs = xmlDocument.GetElementsByTagName("HENDF");
            double HENDFsTotal = 0;
            for (int j = 0; j < xHENDFs.Count; j++)
            {
                var temp = xHENDFs[j].Attributes["Pago"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(temp)) HENDFsTotal += double.Parse(temp);
            }
            globalDocPayrolls.HENDF = HENDFsTotal.ToString();
            //
            XmlNodeList xHRNDFs = xmlDocument.GetElementsByTagName("HRNDF");
            double HRNDFsTotal = 0;
            for (int j = 0; j < xHRNDFs.Count; j++)
            {
                var temp = xHRNDFs[j].Attributes["Pago"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(temp)) HRNDFsTotal += double.Parse(temp);
            }
            globalDocPayrolls.HRNDF = HRNDFsTotal.ToString();

            XmlNodeList xVacacionesComunes = xmlDocument.GetElementsByTagName("VacacionesComunes");
            for (int j = 0; j < xVacacionesComunes.Count; j++)
            {
                globalDocPayrolls.FechaInicio = Convert.ToDateTime(xVacacionesComunes[j].Attributes["FechaInicio"]?.InnerText);
                globalDocPayrolls.FechaFin = Convert.ToDateTime(xVacacionesComunes[j].Attributes["FechaFin"]?.InnerText);
                globalDocPayrolls.Cantidad = xVacacionesComunes[j].Attributes["Cantidad"]?.InnerText;
                globalDocPayrolls.Pago = xVacacionesComunes[j].Attributes["Pago"]?.InnerText;
            }
            XmlNodeList xPrimas = xmlDocument.GetElementsByTagName("Primas");
            for (int j = 0; j < xPrimas.Count; j++)
            {
                globalDocPayrolls.Pri_Cantidad = xPrimas[j].Attributes["Cantidad"]?.InnerText;
                globalDocPayrolls.Pri_Pago = xPrimas[j].Attributes["Pago"]?.InnerText;
                globalDocPayrolls.Pri_PagoNS = xPrimas[j].Attributes["PagoNS"]?.InnerText;
            }
            XmlNodeList xCesantias = xmlDocument.GetElementsByTagName("Cesantias");
            for (int j = 0; j < xCesantias.Count; j++)
            {
                globalDocPayrolls.Ces_Pago = xCesantias[j].Attributes["Pago"]?.InnerText;
                globalDocPayrolls.Ces_PagoIntereses = xCesantias[j].Attributes["PagoIntereses"]?.InnerText;
                globalDocPayrolls.Ces_Porcentaje = xCesantias[j].Attributes["Porcentaje"]?.InnerText;
            }

            XmlNodeList xIncapacidades = xmlDocument.GetElementsByTagName("Incapacidad");
            double incapacidadesCantidadTotal = 0, incapacidadesPagoTotal = 0;
            for (int j = 0; j < xIncapacidades.Count; j++)
            {
                var tempCantidad = xIncapacidades[j].Attributes["Cantidad"]?.InnerText;
                var tempPago = xIncapacidades[j].Attributes["Pago"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(tempCantidad)) incapacidadesCantidadTotal += double.Parse(tempCantidad);
                if (!string.IsNullOrWhiteSpace(tempPago)) incapacidadesPagoTotal += double.Parse(tempPago);
            }
            globalDocPayrolls.Inc_Cantidad = incapacidadesCantidadTotal.ToString();
            globalDocPayrolls.Inc_Pago = incapacidadesPagoTotal.ToString();

            // PENDIENTE Licencias...



            XmlNodeList xBonificacion = xmlDocument.GetElementsByTagName("Bonificacion");
            for (int j = 0; j < xBonificacion.Count; j++)
            {
                globalDocPayrolls.BonificacionNS = xBonificacion[j].Attributes["BonificacionNS"]?.InnerText;
                globalDocPayrolls.BonificacionS = xBonificacion[j].Attributes["BonificacionS"]?.InnerText;
            }
            XmlNodeList xComisiones = xmlDocument.GetElementsByTagName("Comision");
            for (int j = 0; j < xComisiones.Count; j++)
            {
                globalDocPayrolls.Comisiones = xComisiones[j].InnerText;
            }
            XmlNodeList xCompensacion = xmlDocument.GetElementsByTagName("Compensacion");
            double compensacionETotal = 0, CompensacionOTotal = 0;
            for (int j = 0; j < xCompensacion.Count; j++)
            {
                var tempE = xCompensacion[j].Attributes["CompensacionE"]?.InnerText;
                var tempO = xCompensacion[j].Attributes["CompensacionO"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(tempE)) compensacionETotal += double.Parse(tempE);
                if (!string.IsNullOrWhiteSpace(tempO)) CompensacionOTotal += double.Parse(tempO);
            }
            globalDocPayrolls.CompensacionE = compensacionETotal.ToString();
            globalDocPayrolls.CompensacionO = CompensacionOTotal.ToString();


            XmlNodeList xSalud = xmlDocument.GetElementsByTagName("Salud");
            for (int j = 0; j < xSalud.Count; j++)
            {
                globalDocPayrolls.s_Porcentaje = xSalud[j].Attributes["Porcentaje"]?.InnerText;
                globalDocPayrolls.s_Deduccion = xSalud[j].Attributes["Deduccion"]?.InnerText;
            }
            XmlNodeList xFondoPension = xmlDocument.GetElementsByTagName("FondoPension");
            for (int j = 0; j < xFondoPension.Count; j++)
            {
                globalDocPayrolls.FP_Porcentaje = xFondoPension[j].Attributes["Porcentaje"]?.InnerText;
                globalDocPayrolls.FP_Deduccion = xFondoPension[j].Attributes["Deduccion"]?.InnerText;
            }
            XmlNodeList xRetencionFuente = xmlDocument.GetElementsByTagName("RetencionFuente");
            for (int j = 0; j < xRetencionFuente.Count; j++)
            {
                globalDocPayrolls.RetencionFuente = xRetencionFuente[j].InnerText;
            }
            XmlNodeList xFondoSP = xmlDocument.GetElementsByTagName("FondoSP");
            for (int j = 0; j < xFondoSP.Count; j++)
            {
                globalDocPayrolls.FSP_Porcentaje = xFondoSP[j].Attributes["Porcentaje"]?.InnerText;
                globalDocPayrolls.FSP_PorcentajeSub = xFondoSP[j].Attributes["PorcentajeSub"]?.InnerText;
                globalDocPayrolls.FSP_Deduccion = xFondoSP[j].Attributes["DeduccionSP"]?.InnerText;
                globalDocPayrolls.FSP_DeduccionSub = xFondoSP[j].Attributes["DeduccionSub"]?.InnerText;
            }

            globalDocPayrolls.PartitionKey = globalDocPayrolls.CUNE;
            globalDocPayrolls.RowKey = globalDocPayrolls.Emp_NIT;
        }
    }
}

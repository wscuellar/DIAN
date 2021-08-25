﻿using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Common;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Infrastructure.Utils;
using Gosocket.Dian.Plugin.Functions.Cache;
using Gosocket.Dian.Plugin.Functions.Common.Encryption;
using Gosocket.Dian.Plugin.Functions.Cryptography.Verify;
using Gosocket.Dian.Plugin.Functions.Cufe;
using Gosocket.Dian.Plugin.Functions.Event;
using Gosocket.Dian.Plugin.Functions.EventApproveCufe;
using Gosocket.Dian.Plugin.Functions.Models;
using Gosocket.Dian.Plugin.Functions.SigningTime;
using Gosocket.Dian.Plugin.Functions.ValidateParty;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using ContributorType = Gosocket.Dian.Domain.Common.ContributorType;
using OperationMode = Gosocket.Dian.Domain.Common.OperationMode;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace Gosocket.Dian.Plugin.Functions.Common
{
    public class Validator
    {
        private static readonly TableManager TableManagerGlobalDocHolderExchange = new TableManager("GlobalDocHolderExchange");
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");

        #region Global properties        
        static readonly TableManager documentHolderExchangeTableManager = new TableManager("GlobalDocHolderExchange");
        static readonly TableManager contributorTableManager = new TableManager("GlobalContributor");
        static readonly TableManager contingencyTableManager = new TableManager("GlobalContingency");
        static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        static readonly TableManager documentValidatorTableManager = new TableManager("GlobalDocValidatorDocument");
        static readonly TableManager numberRangeTableManager = new TableManager("GlobalNumberRange");
        static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");
        static readonly TableManager typeListTableManager = new TableManager("GlobalTypeList");
        private readonly TableManager TableManagerGlobalDocReferenceAttorney = new TableManager("GlobalDocReferenceAttorney");
        private readonly TableManager TableManagerGlobalAttorneyFacultity = new TableManager("GlobalAttorneyFacultity");
        private readonly TableManager TableManagerGlobalRadianOperations = new TableManager("GlobalRadianOperations");
        private readonly TableManager TableManagerGlobalDocRegisterProviderAR = new TableManager("GlobalDocRegisterProviderAR");
        private readonly TableManager TableManagerGlobalOtherDocElecOperation = new TableManager("GlobalOtherDocElecOperation");
        private readonly TableManager TableManagerGlobalTaxRate = new TableManager("GlobalTaxRate");
        private readonly TableManager payrollTableManager = new TableManager("GlobalDocPayroll");
        private static readonly TableManager TableManagerRadianTestSetResult = new TableManager("RadianTestSetResult");
        private static readonly string pdfMimeType = "application/pdf";
        private static readonly AssociateDocumentService associateDocumentService = new AssociateDocumentService();
        private static readonly TableManager docEventTableManager = new TableManager("GlobalDocEvent");

        XmlDocument _xmlDocument;
        XPathDocument _document;
        XPathNavigator _navigator;
        XPathNavigator _navNs;
        XmlNamespaceManager _ns;
        byte[] _xmlBytes;

        private const string NoErrorCode = "SinCódigo";
        private const string DefaultStringValue = "true";
        private const bool DefaultBoolValue = false;
        private const string DefaultStatusRutValidationErrorMessage = "El facturador electrónico y/o proveedor tecnólogico tiene el RUT en estado cancelado, suspendido o inactivo.";        
        private const string DefaultStatusRutValidationOkMessage = "El facturador tiene el RUT en estado válido.";        
        private const int CacheTimePolicy24HoursInMinutes = 1440;
        private const int CacheTimePolicy1HourInMinutes = 60;

        private const String BASE64_REGEX_STRING = @"^[a-zA-Z0-9\+/]*={0,3}$";

        #endregion

        #region Constructors
        public Validator()
        {
        }

        public Validator(byte[] xmlBytes)
        {
            validatorDocumentNameSpaces(xmlBytes);
        }
        #endregion


        #region Economic activity validation
        public ValidateListResponse ValidateEconomicActivity(ResponseXpathDataValue responseXpathDataValue)
        {
            //var typesList = typeListTableManager.FindByPartition<GlobalTypeList>("new-dian-ubl21");
            //var typeList = typesList.FirstOrDefault(t => t.Name == "Tipo Responsabilidad");
            //var typeListvalues = typeList.Value.Split(';');
            var typeListInstance = GetTypeListInstanceCache();
            var typeListvalues = typeListInstance.Value.Split(';');

            var response = new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ04", ErrorMessage = "Codigos informados corresponden a los que estan en lista." };

            //Economic ativity
            var isValid = true;
            var senderTaxLevelCodes = responseXpathDataValue.XpathsValues["SenderTaxLevelCodeXpath"].Split(';');
            foreach (var code in senderTaxLevelCodes)
                if (!typeListvalues.Contains(code))
                    return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ04", ErrorMessage = "Codigos no informados o no corresponden a los que estan en lista." };
            if (isValid && senderTaxLevelCodes.Any())
                return new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ04", ErrorMessage = "Codigos informados corresponden a los que estan en lista." };

            return null;
        }
        #endregion

        #region Contingecy region
        public ValidateListResponse ValidateContingency()
        {
            DateTime startDate = DateTime.UtcNow;
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "CTG01", ErrorMessage = "La fecha y hora de firmado del documento no corresponde a un período de contingencia establecido por la DIAN." };

            var contingencies = contingencyTableManager.FindAll<GlobalContingency>();

            var node = _xmlDocument.GetElementsByTagName("xades:SigningTime")[0];

            var signingDateTime = node?.InnerText;
            long signingTimeNumber = 0;
            try
            {
                var signingDate = signingDateTime.Split('T')[0];
                var signingTime = $"{signingDateTime.Split('T')[1]?.Substring(0, 6)}00";
                signingTimeNumber = long.Parse(DateTime.ParseExact($"{signingDate}T{signingTime}", "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyyMMddHHmmss"));
            }
            catch { }

            contingencies = contingencies.Where(c => !c.Deleted && c.Active).ToList();
            if (contingencies.Any(c => signingTimeNumber >= c.StartDateNumber && c.EndDateNumber >= signingTimeNumber))
            {
                response.IsValid = true;
                response.ErrorMessage = "La fecha y hora de firmado del documento corresponde a un período de contingencia establecido por la DIAN.";
            }

            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion

        #region ValidateTaxWithHolding
        public List<ValidateListResponse> ValidateTaxWithHolding(XmlParser xmlParser)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            string xmlID = string.Empty;
            string xmlPercent = string.Empty;
            bool validTax = true;

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento ValidateTaxWithHolding referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            XmlNodeList withholdingListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice'][1]/*[local-name()='InvoiceLine']/*[local-name()='WithholdingTaxTotal']/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']");
            XmlNodeList invoiceWithholdingListResponseId = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice']/*[local-name()='WithholdingTaxTotal']/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']");            

            for (int i = 0; i < invoiceWithholdingListResponseId.Count; i++)
            {
                xmlID = invoiceWithholdingListResponseId.Item(i).SelectNodes("//*[local-name()='Invoice']/*[local-name()='WithholdingTaxTotal']/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']").Item(i)?.InnerText.ToString().Trim();
                xmlPercent = invoiceWithholdingListResponseId.Item(i).SelectNodes("//*[local-name()='Invoice']/*[local-name()='WithholdingTaxTotal']/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='Percent']").Item(i)?.InnerText.ToString().Trim();

                var taxShemeIDparameterized = ConfigurationManager.GetValue("TaxShemeID").Split('|');
                if (taxShemeIDparameterized.Contains(xmlID)) validTax = true;

                if (validTax)
                {
                    GlobalTaxRate existTaxRate = TableManagerGlobalTaxRate.ExistTarifa<GlobalTaxRate>(xmlID, xmlPercent);
                    if (existTaxRate == null)
                    {                       
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = false,
                            ErrorCode = "DSAT10",
                            ErrorMessage = "Sin mensaje en el AT",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            int[] arraywithholding = new int[withholdingListResponse.Count];
            for (int i = 0; i < withholdingListResponse.Count; i++)
            {
                var xmlTaxSchemeID = withholdingListResponse.Item(i).SelectNodes("//*[local-name()='InvoiceLine']/*[local-name()='WithholdingTaxTotal']/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']").Item(i)?.InnerText.ToString().Trim();
                arraywithholding[i] = Convert.ToInt32(xmlTaxSchemeID);            
            }

            bool pares = arraywithholding.Distinct().Count() == arraywithholding.Length;
            if (!pares)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "DSAY01",
                    ErrorMessage = "Existe más de un grupo con información de totales para un mismo tributo en una línea de el documento soporte",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            return responses;
        }
        #endregion


        #region ValidateTaxCategory
        public List<ValidateListResponse> ValidateTaxCategory(XmlParser xmlParser)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();                      
            string xmlID = string.Empty;
            string xmlPercent = string.Empty;            

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento ValidateTaxCategory referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            XmlNodeList invoiceLineListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice']/*[local-name()='InvoiceLine']/*[local-name()='ID']");
          
            var isErrorConsecutiveInvoice = false;
            int[] arrayInvoiceListResponse = new int[invoiceLineListResponse.Count];

            int tempIDInvoice = 0;
            int indexInvoiceLine = 0;
            for (int i = 0; i < invoiceLineListResponse.Count; i++)
            {
                bool validTax = true;
                indexInvoiceLine += 1;
                var value = invoiceLineListResponse.Item(i).SelectNodes("//*[local-name()='Invoice']/*[local-name()='InvoiceLine']/*[local-name()='ID']").Item(i)?.InnerText.ToString().Trim();
                // cuando no llega valor, se asume -1
                var xmlIDInvoice = !string.IsNullOrWhiteSpace(value) ? Convert.ToInt32(value) : -1;

                if (i == 0)
                {
                    tempIDInvoice = xmlIDInvoice;
                    if (xmlIDInvoice != 1) isErrorConsecutiveInvoice = true;
                }
                else
                {
                    if (!int.Equals(xmlIDInvoice, tempIDInvoice + 1))
                        isErrorConsecutiveInvoice = true;
                    else
                        tempIDInvoice = xmlIDInvoice;
                }

                arrayInvoiceListResponse[i] = xmlIDInvoice;

                if (validTax)
                {
                    int indexTaxCategory = 0;
                    XmlNodeList taxCategoryListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes($"//*[local-name()='Invoice']/*[local-name()='InvoiceLine'][{indexInvoiceLine}]/*[local-name()='TaxTotal']/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']");
                    for (int j = 0; j < taxCategoryListResponse.Count; j++)
                    {
                        indexTaxCategory += 1;
                        xmlID = taxCategoryListResponse.Item(j).SelectNodes($"//*[local-name()='Invoice']/*[local-name()='InvoiceLine'][{indexInvoiceLine}]/*[local-name()='TaxTotal'][{indexTaxCategory}]/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']").Item(0)?.InnerText.ToString();
                        xmlPercent = taxCategoryListResponse.Item(j).SelectNodes($"//*[local-name()='Invoice']/*[local-name()='InvoiceLine'][{indexInvoiceLine}]/*[local-name()='TaxTotal'][{indexTaxCategory}]/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='Percent']").Item(0)?.InnerText.ToString();

                        var taxShemeIDparameterized = ConfigurationManager.GetValue("TaxShemeID").Split('|');
                        if (taxShemeIDparameterized.Contains(xmlID)) validTax = true;

                        if (validTax)
                        {
                            GlobalTaxRate existTaxRate = TableManagerGlobalTaxRate.ExistTarifa<GlobalTaxRate>(xmlID, xmlPercent);
                            if (existTaxRate == null)
                            {
                                validTax = false;
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = false,
                                    ErrorCode = "DSAX14",
                                    ErrorMessage = "Reporta una tarifa diferente para uno de los tributos enunciados en la tabla 11.3.9",
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                                break;
                            }
                            validTax = false;
                        }
                    }                    
                }
            }


            return responses;
        }
        #endregion

        #region ValidateInvoiceLine
        public List<ValidateListResponse> ValidateInvoiceLine(XmlParser xmlParser)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento ValidateInvoiceLine referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            //Validacion Documento soporte
            string documentTypeId = xmlParser.Fields["DocumentTypeId"].ToString();
            #region Documento Soporte
            if (Convert.ToInt32(documentTypeId) == (int)DocumentType.DocumentSupportInvoice)
            {
                XmlNodeList invoiceLineListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice']/*[local-name()='InvoiceLine']/*[local-name()='ID']");
                XmlNodeList allowanceChargeListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice']/*[local-name()='AllowanceCharge']/*[local-name()='ID']");
                XmlNodeList deliveryTermsListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice']/*[local-name()='DeliveryTerms']/*[local-name()='ID']");              

                int tempID = 0;
                var isErrorConsecutiveDelivery = false;
                var isErrorConsecutiveAllowance = false;
                var isErrorConsecutiveInvoice = false;
                var isErrorConsecutiveInvoiceLineAllowanceCharge = false;
                int[] arrayInvoiceListResponse = new int[invoiceLineListResponse.Count];


                int tempIDInvoice = 0;
                int indexInvoiceLine = 0;
                for (int i = 0; i < invoiceLineListResponse.Count; i++)
                {
                    indexInvoiceLine += 1;
                    var value = invoiceLineListResponse.Item(i).SelectNodes("//*[local-name()='Invoice']/*[local-name()='InvoiceLine']/*[local-name()='ID']").Item(i)?.InnerText.ToString().Trim();
                    // cuando no llega valor, se asume -1
                    var xmlID = !string.IsNullOrWhiteSpace(value) ? Convert.ToInt32(value) : -1;

                    if (i == 0)
                    {
                        tempIDInvoice = xmlID;
                        if (xmlID != 1) isErrorConsecutiveInvoice = true;
                    }
                    else
                    {
                        if (!int.Equals(xmlID, tempIDInvoice + 1))
                            isErrorConsecutiveInvoice = true;
                        else
                            tempIDInvoice = xmlID;
                    }

                    arrayInvoiceListResponse[i] = xmlID;

                    #region AllowanceCharge
                    if (!isErrorConsecutiveInvoiceLineAllowanceCharge)
                    {
                        XmlNodeList invoiceLineAllowanceChargeListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes($"//*[local-name()='Invoice']/*[local-name()='InvoiceLine'][{indexInvoiceLine}]/*[local-name()='AllowanceCharge']/*[local-name()='ID']");
                        int[] arrayAllowanceChargeListResponse = new int[invoiceLineAllowanceChargeListResponse.Count];

                        int tempIDAllowanceCharge = 0;
                        for (int k = 0; k < invoiceLineAllowanceChargeListResponse.Count; k++)
                        {
                            var valueAllowance = invoiceLineAllowanceChargeListResponse.Item(k).SelectNodes("//*[local-name()='InvoiceLine']/*[local-name()='AllowanceCharge']/*[local-name()='ID']").Item(k)?.InnerText.ToString().Trim();
                            // cuando no llega valor, se asume -1
                            var xmlIDAllowance = !string.IsNullOrWhiteSpace(valueAllowance) ? Convert.ToInt32(valueAllowance) : -1;

                            if (k == 0)
                            {
                                tempIDAllowanceCharge = xmlIDAllowance;
                                if (xmlIDAllowance != 1) isErrorConsecutiveInvoiceLineAllowanceCharge = true;
                            }
                            else
                            {
                                if (!int.Equals(xmlIDAllowance, tempIDAllowanceCharge + 1))
                                    isErrorConsecutiveInvoiceLineAllowanceCharge = true;
                                else
                                    tempIDAllowanceCharge = xmlIDAllowance;
                            }

                            arrayAllowanceChargeListResponse[k] = xmlIDAllowance;
                        }

                        if (isErrorConsecutiveInvoiceLineAllowanceCharge)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = false,
                                ErrorCode = "DSBE02",
                                ErrorMessage = "Sin Mensaje de error en AT",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    #endregion
                }

                if (isErrorConsecutiveInvoice)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = false,
                        ErrorCode = "DSAV02b",
                        ErrorMessage = "Los números de línea de Documento Soporte utilizados en los diferentes grupos no son consecutivos, empezando con “1”",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

                //Consecutivo regla DSBC02
                for (int i = 0; i < deliveryTermsListResponse.Count; i++)
                {                    
                    var xmlID = deliveryTermsListResponse.Item(i).SelectNodes("//*[local-name()='DeliveryTerms']/*[local-name()='ID']").Item(i)?.InnerText.ToString().Trim();

                    int number1 = 0;
                    bool valNumber = int.TryParse(xmlID, out number1);
                    if (valNumber)
                    {
                        if (i == 0)
                        {
                            tempID = number1;
                            if (number1 != 1)
                            {
                                isErrorConsecutiveDelivery = true;
                                break;
                            }
                        }                                                       
                        else
                        {
                            if (!int.Equals(number1, tempID + 1))
                            {
                                isErrorConsecutiveDelivery = true;
                                break;
                            }                         
                            else
                                tempID = Convert.ToInt32(number1);
                        }
                    }                   
                }

                if (isErrorConsecutiveDelivery)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "DSBC02",
                        ErrorMessage = "Valida que los números de línea del documento sean consecutivo",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

                //Consecutivo regla DSAQ02
                for (int i = 0; i < allowanceChargeListResponse.Count; i++)
                {
                    var xmlID = allowanceChargeListResponse.Item(i).SelectNodes("//*[local-name()='AllowanceCharge']/*[local-name()='ID']").Item(i)?.InnerText.ToString().Trim();

                    if(string.IsNullOrWhiteSpace(xmlID))
                    {
                        isErrorConsecutiveAllowance = true;
                        break;
                    }

                    int number1 = 0;
                    bool valNumber = int.TryParse(xmlID, out number1);
                    if (valNumber)
                    {
                        if (i == 0)
                        {
                            tempID = number1;
                            if (number1 != 1)
                            {
                                isErrorConsecutiveAllowance = true;
                                break;
                            }
                        }                           
                        else
                        {
                            if (!int.Equals(number1, tempID + 1))
                            {
                                isErrorConsecutiveAllowance = true;
                                break;
                            }
                            else
                                tempID = Convert.ToInt32(number1);
                        }
                    }                   
                }

                if (isErrorConsecutiveAllowance)
                {                   
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "DSAQ02",
                        ErrorMessage = "Valida que los números de línea del documento sean consecutivo",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });                                           
                }
            }
            #endregion Documento Soporte
            #region Documento Importaciones
            else
            {

                //Validacion documento de impotacion 
                XmlNodeList invoiceListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice'][1]/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='ExtensionContent']/*[local-name()='Lines']/*[local-name()='InvoiceLine']/*[local-name()='ID']");
                XmlNodeList invoiceLineListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice']/*[local-name()='InvoiceLine']/*[local-name()='ID']");
                
                int[] arrayInvoiceLine = new int[invoiceListResponse.Count];
                int[] arrayInvoiceListResponse = new int[invoiceLineListResponse.Count];
                var isErrorConsecutive = false;
                var isErrorConsecutiveInvoice = false;
                var isErrorConsecutiveAllowanceCharge = false;

                //bool paresAllowanceCharge = arrayAllowanceChargeListResponse.Distinct().Count() == arrayAllowanceChargeListResponse.Length;
                //if (!paresAllowanceCharge || arrayAllowanceChargeListResponse.Contains(-1))
                //{
                //    responses.Add(new ValidateListResponse
                //    {
                //        IsValid = false,
                //        Mandatory = false,
                //        ErrorCode = "DIAV02a",
                //        ErrorMessage = "Más de un grupo conteniendo el elemento /de:Invoice/de:InvoiceLine/cbc:ID con la misma información o no existe ningún valor",
                //        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                //    });
                //}

                int tempIDInvoice = 0;
                int indexInvoiceLine = 0;
                for (int i = 0; i < invoiceLineListResponse.Count; i++)
                {
                    indexInvoiceLine += 1;
                    var value = invoiceLineListResponse.Item(i).SelectNodes("//*[local-name()='Invoice']/*[local-name()='InvoiceLine']/*[local-name()='ID']").Item(i)?.InnerText.ToString().Trim();
                    // cuando no llega valor, se asume -1
                    var xmlID = !string.IsNullOrWhiteSpace(value) ? Convert.ToInt32(value) : -1;

                    if (i == 0)
                    {
                        tempIDInvoice = xmlID;
                        if (xmlID != 1) isErrorConsecutiveInvoice = true;
                    }
                    else
                    {
                        if (!int.Equals(xmlID, tempIDInvoice + 1))
                            isErrorConsecutiveInvoice = true;
                        else
                            tempIDInvoice = xmlID;
                    }

                    arrayInvoiceListResponse[i] = xmlID;

                    #region AllowanceCharge
                    if (!isErrorConsecutiveAllowanceCharge)
                    {
                        XmlNodeList allowanceChargeListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes($"//*[local-name()='Invoice']/*[local-name()='InvoiceLine'][{indexInvoiceLine}]/*[local-name()='AllowanceCharge']/*[local-name()='ID']");
                        int[] arrayAllowanceChargeListResponse = new int[allowanceChargeListResponse.Count];

                        int tempIDAllowanceCharge = 0;
                        for (int k = 0; k < allowanceChargeListResponse.Count; k++)
                        {
                            var valueAllowance = allowanceChargeListResponse.Item(k).SelectNodes("//*[local-name()='InvoiceLine']/*[local-name()='AllowanceCharge']/*[local-name()='ID']").Item(k)?.InnerText.ToString().Trim();
                            // cuando no llega valor, se asume -1
                            var xmlIDAllowance = !string.IsNullOrWhiteSpace(valueAllowance) ? Convert.ToInt32(valueAllowance) : -1;

                            if (k == 0)
                            {
                                tempIDAllowanceCharge = xmlIDAllowance;
                                if (xmlIDAllowance != 1) isErrorConsecutiveAllowanceCharge = true;
                            }
                            else
                            {
                                if (!int.Equals(xmlIDAllowance, tempIDAllowanceCharge + 1))
                                    isErrorConsecutiveAllowanceCharge = true;
                                else
                                    tempIDAllowanceCharge = xmlIDAllowance;
                            }

                            arrayAllowanceChargeListResponse[k] = xmlIDAllowance;
                        }

                        if (isErrorConsecutiveAllowanceCharge)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = false,
                                ErrorCode = "DIBE02",
                                ErrorMessage = "Empieza con “1”, los números utilizados en los diferentes grupos deben ser consecutivos.",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        } 
                    }
                    #endregion
                }

                if (isErrorConsecutiveInvoice)
                {                   
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = false,
                        ErrorCode = "DIAV02b",
                        ErrorMessage = "Los números de línea de factura utilizados en los diferentes grupos no son consecutivos, empezando con “1”",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

                bool paresInvoiceLine = arrayInvoiceListResponse.Distinct().Count() == arrayInvoiceListResponse.Length;
                if (!paresInvoiceLine || arrayInvoiceListResponse.Contains(-1))
                {                 
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = false,
                        ErrorCode = "DIAV02a",
                        ErrorMessage = "Más de un grupo conteniendo el elemento /de:Invoice/de:InvoiceLine/cbc:ID con la misma información o no existe ningún valor",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

                int tempID = 0;               
                for (int i = 0; i < invoiceListResponse.Count; i++)
                {
                    var value = invoiceListResponse.Item(i).SelectNodes("//*[local-name()='ID']").Item(i)?.InnerText.ToString().Trim();
                    // cuando no llega valor, se asume -1
                    var xmlID = !string.IsNullOrWhiteSpace(value) ? Convert.ToInt32(value) : -1;

                    if (i == 0)
                    {
                        tempID = xmlID;
                        if (xmlID != 1) isErrorConsecutive = true;
                    }
                    else
                    {
                        if (!int.Equals(xmlID, tempID + 1))
                            isErrorConsecutive = true;
                        else
                            tempID = xmlID;
                    }

                    arrayInvoiceLine[i] = xmlID;
                }

                if (isErrorConsecutive)
                {                   
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "DIBC05b",
                        ErrorMessage = "Los números de línea de factura utilizados en los diferentes grupos no son consecutivos, empezando con “1”",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

                bool pares = arrayInvoiceLine.Distinct().Count() == arrayInvoiceLine.Length;
                if (!pares || arrayInvoiceLine.Contains(-1))
                {                    
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "DIBC05a",
                        ErrorMessage = "Más de un grupo conteniendo el elemento /de:Invoice/de:InvoiceLine/cbc:ID con la misma información o no existe ningún valor",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }
            #endregion Documento Importaciones

            return responses;
        }

        #endregion


        #region Cufe validation
        public ValidateListResponse ValidateCufe(CufeModel cufeModel, string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            string errorMessarge = string.Empty;
            string key = string.Empty;
            var errorCode = "FAD06";            
            var prop = "CUFE";            

            string[] codesWithCUDE = { "03", "05", "91", "92", "96", "101" };
            if (codesWithCUDE.Contains(documentMeta.DocumentTypeId))
                prop = "CUDE";
            if (documentMeta.DocumentTypeId == "05")
                errorCode = "DSAD06";
            else if (documentMeta.DocumentTypeId == "91")
                errorCode = "CAD06";
            else if (documentMeta.DocumentTypeId == "92")
                errorCode = "DAD06";
            else if (documentMeta.DocumentTypeId == "96")
                errorCode = "AAD06";
            else if (documentMeta.DocumentTypeId == "101")
            {
                errorCode = "DIAD06";
                prop = "CUDI";
            }
                

            var billerSoftwareId = ConfigurationManager.GetValue("BillerSoftwareId");
            var billerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin");

            if (!codesWithCUDE.Contains(documentMeta.DocumentTypeId))
                key = ConfigurationManager.GetValue("TestHabTechnicalKey");
            else
            {
                var softwareId = cufeModel.SoftwareId;
                if (softwareId == billerSoftwareId)
                    key = billerSoftwarePin;
                else
                {
                    var software = GetSoftwareInstanceCache(softwareId);
                    key = software?.Pin;
                }
            }

            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                if (!codesWithCUDE.Contains(documentMeta.DocumentTypeId))
                {
                    var documentType = documentMeta.DocumentTypeId;
                    if (new string[] { "01", "02", "04" }.Contains(documentType)) documentType = "01";
                    var rk = $"{documentMeta?.Serie}|{documentType}|{documentMeta?.InvoiceAuthorization}";
                    var range = numberRangeTableManager.Find<GlobalNumberRange>(documentMeta.SenderCode, rk);
                    key = range?.TechnicalKey;
                }
            }
            errorMessarge = documentMeta.DocumentTypeId == "96" ? "El valor UUID no está correctamente calculado" : $"Valor del { prop} no está calculado correctamente.";
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCode, ErrorMessage = errorMessarge };

            var number = cufeModel.SerieAndNumber;
            var emissionDate = cufeModel.EmissionDate;
            var emissionHour = cufeModel.HourEmission;
            var amount = cufeModel.Amount?.Trim();
            if (string.IsNullOrEmpty(amount)) amount = "0.00"; else amount = TruncateDecimal(decimal.Parse(amount), 2).ToString("F2");
            var taxCode1 = "01";
            var taxCode2 = "04";
            var taxCode3 = "03";
            var taxAmount1 = cufeModel.TaxAmount1?.Trim();
            taxAmount1 = taxAmount1?.Split('|')[0];
            var taxAmount2 = cufeModel.TaxAmount2?.Trim();
            var taxAmount3 = cufeModel.TaxAmount3?.Trim();

            if (string.IsNullOrEmpty(taxAmount1)) taxAmount1 = "0.00"; else taxAmount1 = TruncateDecimal(decimal.Parse(taxAmount1), 2).ToString("F2");
            if (string.IsNullOrEmpty(taxAmount2)) taxAmount2 = "0.00"; else taxAmount2 = TruncateDecimal(decimal.Parse(taxAmount2), 2).ToString("F2");
            if (string.IsNullOrEmpty(taxAmount3)) taxAmount3 = "0.00"; else taxAmount3 = TruncateDecimal(decimal.Parse(taxAmount3), 2).ToString("F2");
            var amountToPay = cufeModel.TotalAmount?.Trim();
            if (string.IsNullOrEmpty(amountToPay)) amountToPay = "0.00"; else amountToPay = TruncateDecimal(decimal.Parse(amountToPay), 2).ToString("F2");
            var senderCode = cufeModel.SenderCode;
            var receiverCode = cufeModel.ReceiverCode;
            var environmentType = cufeModel.EnvironmentType;

            var fakeData = $"{number}---{emissionDate}---{emissionHour}---{amount}---{taxCode1}---{taxAmount1}---{taxCode2}---{taxAmount2}---{taxCode3}---{taxAmount3}---{amountToPay}---{senderCode}---{receiverCode}---{key}---{environmentType}";

            var data = $"{number}{emissionDate}{emissionHour}{amount}{taxCode1}{taxAmount1}{taxCode2}{taxAmount2}{taxCode3}{taxAmount3}{amountToPay}{senderCode}{receiverCode}{key}{environmentType}";
            var documentKey = cufeModel.DocumentKey;

            if(cufeModel.DocumentTypeId == "05")
            {
               data = $"{number}{emissionDate}{emissionHour}{amount}{taxCode1}{taxAmount1}{taxCode2}{taxAmount2}{taxCode3}{taxAmount3}{amountToPay}{receiverCode}{senderCode}{key}{environmentType}";
            }

            if(cufeModel.DocumentTypeId == "101")
            {
                data = $"{number}{emissionDate}{emissionHour}{amount}{amountToPay}{senderCode}{receiverCode}{key}{environmentType}";
                documentKey = cufeModel.DocumentKey;
            }

            // Only for AR
            if (cufeModel.DocumentTypeId == "96")
            {
                fakeData = $"{cufeModel.SerieAndNumber}---{cufeModel.EmissionDate}---{cufeModel.HourEmission}---{cufeModel.SenderCode}---{cufeModel.ReceiverCode}---{cufeModel.ResponseCode}---{cufeModel.ReferenceId}---{cufeModel.ReferenceTypeCode}---{key}";

                if ((cufeModel.ResponseCode == "037" || cufeModel.ResponseCode == "038" || cufeModel.ResponseCode == "039") && cufeModel.ResponseCodeListID == "2")
                {
                    //Endoso en garantia en blanco
                    data = $"{cufeModel.SerieAndNumber}{cufeModel.EmissionDate}{cufeModel.HourEmission}{cufeModel.ReceiverCode}{cufeModel.ResponseCode}{cufeModel.ResponseCodeListID}{cufeModel.ReferenceId}{cufeModel.ReferenceTypeCode}{key}";
                }
                else
                {
                    data = $"{cufeModel.SerieAndNumber}{cufeModel.EmissionDate}{cufeModel.HourEmission}{cufeModel.SenderCode}{cufeModel.ReceiverCode}{cufeModel.ResponseCode}{cufeModel.ReferenceId}{cufeModel.ReferenceTypeCode}{key}";
                }
                documentKey = cufeModel.Cude;
            }

            var hash = data.EncryptSHA384();

            if (documentKey.ToLower() == hash)
            {
                response.IsValid = true;
                response.ErrorMessage = $"Valor del {prop} calculado correctamente.";
            }

            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion

        #region Evento Cune

        public ValidateListResponse ValidateCune(CuneModel objCune, RequestObjectCune data)
        {
            DateTime startDate = DateTime.UtcNow;
            data.trackId = data.trackId.ToLower();

            var ValDev = objCune?.ValDev.ToString("F2");
            var ValDesc = objCune?.ValDesc.ToString("F2");
            var ValTol = objCune?.ValTol.ToString("F2");

            string errorMessarge = string.Empty;
            var errorCode = Convert.ToInt32(objCune?.DocumentType) == (int)DocumentType.IndividualPayrollAdjustments && objCune.TipNota == 2
                ? "NIAE238" 
                : Convert.ToInt32(objCune?.DocumentType) == (int)DocumentType.IndividualPayroll
                    ? "NIE024" 
                    : "NIAE024";

            string key = string.Empty;

            var billerSoftwareId = ConfigurationManager.GetValue("BillerSoftwareId");
            var billerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin");

            var softwareId = objCune?.SoftwareId;

            if (softwareId == billerSoftwareId || string.IsNullOrEmpty(softwareId))
            {
                key = billerSoftwarePin;
            }
            else
            {
                var software = GetSoftwareInstanceCache(softwareId);
                key = software?.Pin;
            }

            errorMessarge = ConfigurationManager.GetValue("ErrorMessage_NIE024");
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCode, ErrorMessage = errorMessarge };

            ValDev = (ValDev == null) ? "0.00" : ValDev = TruncateDecimal(decimal.Parse(ValDev), 2).ToString("F2");
            ValDesc = (ValDesc == null) ? "0.00" : ValDesc = TruncateDecimal(decimal.Parse(ValDesc), 2).ToString("F2");
            ValTol = (ValTol == null) ? "0.00" : ValTol = TruncateDecimal(decimal.Parse(ValTol), 2).ToString("F2");

            var NumNIE = objCune.NumNIE;
            var FechNIE = objCune.FecNIE;
            var HorNIE = objCune.HorNIE;
            var NitNIE = objCune.NitNIE;
            var DocEmp = objCune.DocEmp;
            var SoftwarePin = key;
            var TipAmb = objCune.TipAmb;
            var tipoXml = objCune.TipoXML;

            if (string.IsNullOrWhiteSpace(DocEmp)) DocEmp = "0";

            var numberSha384 = $"{NumNIE}{FechNIE}{HorNIE}{ValDev}{ValDesc}{ValTol}{NitNIE}{DocEmp}{tipoXml}{SoftwarePin}{TipAmb}";

            var hash = numberSha384.EncryptSHA384();

            if (objCune.Cune.ToLower() == hash)
            {
                response.IsValid = true;
                response.ErrorMessage = $"Valor calculado correctamente.";
            }

            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion

        #region Document
        public ValidateListResponse ValidateDocumentDuplicity(string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();

            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            var documentEntity = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "90" };
            if (documentEntity == null)
            {
                response.IsValid = true;
                response.ErrorMessage = "Documento no procesado anteriormente.";
            }
            else response.ErrorMessage = $"Documento procesado anteriormente.";


            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion

        #region Payroll

        public List<ValidateListResponse> ValidateIndividualPayroll(XmlParseNomina xmlParser, DocumentParsedNomina model)
        {
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            responses.AddRange(this.CheckIndividualPayrollDuplicity(model.EmpleadorNIT, model.SerieAndNumber));

            if (Convert.ToInt32(model.DocumentTypeId) == Convert.ToInt32(DocumentType.IndividualPayroll))
            {
                responses.AddRange(this.CheckIndividualPayrollInSameMonth(xmlParser));
            }

            return responses;
        }

        #endregion

        #region NIT validations
        public List<ValidateListResponse> ValidateNit(NitModel nitModel, string trackId, NominaModel nominaModel = null)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            GlobalContributor softwareProvider = null;
            GlobalRadianOperations softwareProviderRadian = null;
            bool habilitadoRadian = false;
            string testSetId = string.Empty;
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            // Validación de DV para documentos de Nómina
            if (nominaModel != null)
            {
                int ProveedorDV, EmpleadorDV;

                int.TryParse(nominaModel.ProveedorDV, out ProveedorDV);
                int.TryParse(nominaModel.EmpleadorDV, out EmpleadorDV);

                ProveedorDV = nominaModel.ProveedorDV.Equals(ProveedorDV.ToString()) ? ProveedorDV : -1;
                EmpleadorDV = nominaModel.EmpleadorDV.Equals(EmpleadorDV.ToString()) ? EmpleadorDV : -1;

                if (Convert.ToInt32(nominaModel.DocumentTypeId) == (int)DocumentType.IndividualPayroll)
                {
                    // Proveedor
                    if (ValidateDigitCode(nominaModel.ProveedorNIT, ProveedorDV))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "NIE018", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "NIE018", ErrorMessage = "Se debe colocar el DV de la empresa dueña del Software que genera el Documento, debe estar registrado en la DIAN", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    // Empleador                   
                    if (ValidateDigitCode(nominaModel.EmpleadorNIT, EmpleadorDV))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "NIE034", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "NIE034", ErrorMessage = "Debe ir el DV del Empleador", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
                else
                {
                    string errorCodeProveedor = nominaModel.TipoNota == "2" ? "NIAE231" : "NIAE018";
                    string errorCodeEmpleador = nominaModel.TipoNota == "2" ? "NIAE249" : "NIAE034";

                    // Proveedor
                    if (ValidateDigitCode(nominaModel.ProveedorNIT, ProveedorDV))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = errorCodeProveedor, ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCodeProveedor, ErrorMessage = "Se debe colocar el DV de la empresa dueña del Software que genera el Documento, debe estar registrado en la DIAN", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    // Empleador
                    if (ValidateDigitCode(nominaModel.EmpleadorNIT, EmpleadorDV))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = errorCodeEmpleador, ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCodeEmpleador, ErrorMessage = "Debe ir el DV del Empleador", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }

                //Valida emisor habilitado en Nomina GlobalOtherDocElecOperation
                if (!documentMeta.SendTestSet)
                {
                    var errorCodeProveedor = Convert.ToInt32(nominaModel.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments && nominaModel.TipoNota == "2"
                         ? "NIAE230"
                         : Convert.ToInt32(nominaModel.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                             ? "NIE017"
                             : "NIAE017";

                    var errorCodeEmpleador = Convert.ToInt32(nominaModel.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments && nominaModel.TipoNota == "2"
                         ? "NIAE248"
                         : Convert.ToInt32(nominaModel.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                             ? "NIE033"
                             : "NIAE033";

                    //Valida Proveedor se encuentre habilitado
                    var otherElectricDocuments = TableManagerGlobalOtherDocElecOperation
                    .FindGlobalOtherDocElecOperationByPartition_RowKey_Deleted_State<GlobalOtherDocElecOperation>(nominaModel.ProveedorNIT,
                    nominaModel.ProveedorSoftwareID, false, "Habilitado");

                    if (otherElectricDocuments != null && otherElectricDocuments.Count > 0)
                    {
                        // ElectronicDocumentId = 1. Es para Documentos 102 y 103 (Nómina Individual y Nómina Individual de Ajuste).
                        var electricDocumentFound = otherElectricDocuments.FirstOrDefault(x => x.ElectronicDocumentId == 1);
                        if (electricDocumentFound != null)
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = errorCodeProveedor, ErrorMessage = "El Emisor del Documento se encuentra Habilitado en la Plataforma.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCodeProveedor, ErrorMessage = "Se debe colocar el NIT sin guiones ni DV de la empresa dueña del Software que genera el Documento, debe estar registrado en la DIAN.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                    }
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCodeProveedor, ErrorMessage = "Se debe colocar el NIT sin guiones ni DV de la empresa dueña del Software que genera el Documento, debe estar registrado en la DIAN.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                    //Valida Empleador se encuentre habilitado
                        var otherElectricDocumentsEmpleador = TableManagerGlobalOtherDocElecOperation
                    .FindGlobalOtherDocElecOperationByPartition_RowKey_Deleted_State<GlobalOtherDocElecOperation>(nominaModel.EmpleadorNIT,
                    nominaModel.ProveedorSoftwareID, false, "Habilitado");

                    if (otherElectricDocumentsEmpleador != null && otherElectricDocumentsEmpleador.Count > 0)
                    {
                        // ElectronicDocumentId = 1. Es para Documentos 11 y 12 (Nómina Individual y Nómina Individual de Ajuste).
                        var electricDocumentFoundEmpleador = otherElectricDocumentsEmpleador.FirstOrDefault(x => x.ElectronicDocumentId == 1);
                        if (electricDocumentFoundEmpleador != null)                        
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "92", ErrorMessage = "El Emisor del Documento se encuentra Habilitado en la Plataforma.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });                        
                        else
                        {
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "92", ErrorMessage = "El Emisor del Documento no se encuentra Habilitado en la Plataforma.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCodeEmpleador, ErrorMessage = "Debe ir el NIT del Empleador sin guiones ni DV.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        }
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "92", ErrorMessage = "El Emisor del Documento no se encuentra Habilitado en la Plataforma.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCodeEmpleador, ErrorMessage = "Debe ir el NIT del Empleador sin guiones ni DV.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    }                    
                }

                return responses;
            }

            var senderCode = nitModel.SenderCode;
            var senderCodeDigit = nitModel.SenderCodeDigit;
            var senderSchemeCode = nitModel.SenderSchemeCode;

            var senderCodeProvider = nitModel.ProviderCode;
            var senderCodeProviderDigit = nitModel.ProviderCodeDigit;
            var softwareId = nitModel.SoftwareId;

            var issuerPartyCode = nitModel.IssuerPartyID;
            var IssuerPartyCodeDigit = nitModel.IssuerPartySchemeID;

            var receiverCode = nitModel.ReceiverCode;
            var receiverCodeSchemeNameValue = nitModel.ReceiverCodeSchemaValue;
            if (receiverCodeSchemeNameValue == "31")
            {
                string receiverDvErrorCode = "FAK24";
                string receiverDvrErrorDescription = "DV no corresponde al NIT informado";
                if (documentMeta.DocumentTypeId == "05")
                {
                    receiverDvErrorCode = "DSAJ24b";
                    receiverDvrErrorDescription = "El DV del NIT no es correcto";
                }
                if (documentMeta.DocumentTypeId == "91") receiverDvErrorCode = "CAK24";
                else if (documentMeta.DocumentTypeId == "92") receiverDvErrorCode = "DAK24";
                else if (documentMeta.DocumentTypeId == "96") receiverDvErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAK24;               

                var receiverCodeDigit = nitModel.ReceiverCodeDigit;

                long numberReceiver = 0;
                bool valNumberReceiver = long.TryParse(receiverCodeDigit, out numberReceiver);

                if (valNumberReceiver)
                {
                    if (string.IsNullOrEmpty(receiverCodeDigit) || receiverCodeDigit == "undefined") receiverCodeDigit = "11";
                    if (ValidateDigitCode(receiverCode, int.Parse(receiverCodeDigit)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = receiverDvErrorCode, ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiverDvErrorCode, ErrorMessage = receiverDvrErrorDescription, ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiverDvErrorCode, ErrorMessage = receiverDvrErrorDescription, ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            }


            var receiver2Code = nitModel.ReceiverCode2;
            if (receiverCode != receiver2Code)
            {
                var receiver2CodeSchemeNameValue = nitModel.ReceiverCode2SchemaValue;
                if (receiver2CodeSchemeNameValue == "31")
                {
                    string receiver2DvErrorCode = "FAK47";
                    string receiver2DvrErrorDescription = "DV no corresponde al NIT informado";
                    if (documentMeta.DocumentTypeId == "05")
                    {
                        receiver2DvErrorCode = "DSAJ47";
                        receiver2DvrErrorDescription = "DV del NIT del vendedor no informado";
                    }
                    if (documentMeta.DocumentTypeId == "91") receiver2DvErrorCode = "CAK47";
                    else if (documentMeta.DocumentTypeId == "92") receiver2DvErrorCode = "DAK47";
                    else if (documentMeta.DocumentTypeId == "96") receiver2DvErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAK47;

                    var receiver2CodeDigit = nitModel.ReceiverCode2Digit;

                    long numberReceiver2CodeDigit = 0;
                    bool valNumberReceiver2CodeDigit = long.TryParse(receiver2CodeDigit, out numberReceiver2CodeDigit);

                    if (valNumberReceiver2CodeDigit)
                    {
                        if (string.IsNullOrEmpty(receiver2CodeDigit) || receiver2CodeDigit == "undefined") receiver2CodeDigit = "11";
                        if (ValidateDigitCode(receiver2Code, int.Parse(receiver2CodeDigit)))
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = receiver2DvErrorCode, ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiver2DvErrorCode, ErrorMessage = receiver2DvrErrorDescription, ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    }
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiver2DvErrorCode, ErrorMessage = receiver2DvrErrorDescription, ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                }
            }

            //IssuerParty Adquiriente/deudor de la Factura Electrónica evento Mandato
            if (nitModel.ResponseCode == "043")
            {
                //Valida digito de verificacion SenderParty / PowerOfAttorney
                string senderPartyPowerOfAttorneySchemeID = nitModel.SenderPartyPowerOfAttorneySchemeID;
                string senderPartyPowerOfAttorneySchemeName = nitModel.SenderPartyPowerOfAttorneySchemeName;
                string senderPartyPowerOfAttorneyID = nitModel.SenderPartyPowerOfAttorneyID;
                long numberPowerOfAttorney = 0;
                bool valNumberPowerOfAttorney = long.TryParse(senderPartyPowerOfAttorneySchemeID, out numberPowerOfAttorney);
                if (senderPartyPowerOfAttorneySchemeName == "31")
                {
                    if (valNumberPowerOfAttorney)
                    {
                        if (string.IsNullOrEmpty(senderPartyPowerOfAttorneySchemeID) || senderPartyPowerOfAttorneySchemeID == "undefined") senderPartyPowerOfAttorneySchemeID = "11";
                        if (ValidateDigitCode(senderPartyPowerOfAttorneyID, int.Parse(senderPartyPowerOfAttorneySchemeID)))
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "AAF33", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "AAF33", ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF33"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    }
                    else { responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "AAF33", ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF33"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds }); }
                }

                //Valida digito de verificacion SenderParty / Person
                string senderPartyPersonSchemeID = nitModel.SenderPartyPersonSchemeID;
                string senderPartyPersonSchemeName = nitModel.SenderPartyPersonSchemeName;
                string senderPartyPersonID = nitModel.SenderPartyPersonID;
                long numberPerson = 0;
                bool valNumberPerson = long.TryParse(senderPartyPersonSchemeID, out numberPerson);
                if (senderPartyPersonSchemeName == "31")
                {
                    if (valNumberPerson)
                    {
                        if (string.IsNullOrEmpty(senderPartyPersonSchemeID) || senderPartyPersonSchemeID == "undefined") senderPartyPersonSchemeID = "11";
                        if (ValidateDigitCode(senderPartyPersonID, int.Parse(senderPartyPersonSchemeID)))
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "AAF24", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "AAF24", ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF24"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    }
                    else { responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "AAF24", ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF24"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds }); }
                }

                //Valida digito de verificacion DocumentResponse / DocumentReference / IssuerParty
                if (string.IsNullOrEmpty(IssuerPartyCodeDigit) || IssuerPartyCodeDigit == "undefined") IssuerPartyCodeDigit = "11";
                if (ValidateDigitCode(issuerPartyCode, int.Parse(IssuerPartyCodeDigit)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "AAH63", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "AAH63", ErrorMessage = "El DV no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                //Valida DV del PowerOfAttorney - schemeName
                string agentPartyPersonSchemeName = nitModel.AgentPartyPersonSchemeName;
                string agentPartyPersonSchemeID = nitModel.AgentPartyPersonSchemeID;
                long number1 = 0;
                bool valNumber = long.TryParse(agentPartyPersonSchemeID, out number1);
                if (agentPartyPersonSchemeName == "31")
                {
                    if (valNumber)
                    {
                        if (string.IsNullOrEmpty(agentPartyPersonSchemeID) || agentPartyPersonSchemeID == "undefined") agentPartyPersonSchemeID = "11";
                        if (ValidateDigitCode(issuerPartyCode, int.Parse(agentPartyPersonSchemeID)))
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "AAH71", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "AAH71", ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH71"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    }
                    else { responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "AAH71", ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH71"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds }); }
                }
            }

            var softwareProviderCode = nitModel.SoftwareProviderCode;
            var softwareProviderCodeDigit = nitModel.SoftwareProviderCodeDigit;
            var providerCode = nitModel.ProviderCode;
            var providerCodeDigit = nitModel.ProviderCodeDigit;

            GlobalContributor sender2 = null;
            var sender = GetContributorInstanceCache(senderCode);
            if (documentMeta.DocumentTypeId != "96")
            {
                // Sender
                if (senderSchemeCode == "31")
                {
                    string senderDvErrorCode = "FAJ24";
                    string senderDvrErrorDescription = "DV del NIT del emsior del documento no está correctamente calculado";
                    if (documentMeta.DocumentTypeId == "05")
                    {
                        senderDvErrorCode = "DSAK24";
                        senderDvrErrorDescription = "No está informado el DV del NIT";
                    }
                    else if (documentMeta.DocumentTypeId == "91") senderDvErrorCode = "CAJ24";
                    else if (documentMeta.DocumentTypeId == "92") senderDvErrorCode = "DAJ24";

                    if (string.IsNullOrEmpty(senderCodeDigit) || senderCodeDigit == "undefined") senderCodeDigit = "11";
                    if (ValidateDigitCode(senderCode, int.Parse(senderCodeDigit)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = senderDvErrorCode, ErrorMessage = "DV del NIT del emsior del documento está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = senderDvErrorCode, ErrorMessage = senderDvrErrorDescription, ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                }


                // Sender2               
                if (senderCode != senderCodeProvider)
                {
                    string sender2DvErrorCode = "FAJ47";
                    if (documentMeta.DocumentTypeId == "91") sender2DvErrorCode = "CAJ47";
                    else if (documentMeta.DocumentTypeId == "92") sender2DvErrorCode = "DAJ47";

                    sender2 = GetContributorInstanceCache(senderCodeProvider);
                    if (string.IsNullOrEmpty(senderCodeProviderDigit) || senderCodeProviderDigit == "undefined") senderCodeProviderDigit = "11";
                    if (ValidateDigitCode(senderCodeProvider, int.Parse(senderCodeProviderDigit)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sender2DvErrorCode, ErrorMessage = "DV del NIT del emsior del documento está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sender2DvErrorCode, ErrorMessage = "DV del NIT del emsior del documento no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
            }

            // Software provider
            string softwareproviderDvErrorCode = string.Empty;
            if (documentMeta.DocumentTypeId == "05") softwareproviderDvErrorCode = "DSAB22b";
            //else if (documentMeta.DocumentTypeId == "91") softwareproviderDvErrorCode = "CAB22";
            //else if (documentMeta.DocumentTypeId == "92") softwareproviderDvErrorCode = "DAB22";
            else if (documentMeta.DocumentTypeId == "96") softwareproviderDvErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAB22;

            // Is Radian
            var isRadian = false;
            if (documentMeta.DocumentTypeId == "96")
            {
                var docEvent = docEventTableManager.FindpartitionKey<GlobalDocEvent>(nitModel.ResponseCode).FirstOrDefault();
                if(docEvent != null)
                    isRadian = docEvent.IsRadian;
            }

            //Software provider RADIAN
            if (documentMeta.DocumentTypeId == "96" && !documentMeta.SendTestSet && senderCodeProvider != "800197268" && isRadian)
            {
                senderCodeProvider = senderCode != senderCodeProvider ? senderCodeProvider : senderCode;
                softwareProviderRadian = TableManagerGlobalRadianOperations.FindhByPartitionKeyRadianStatus<GlobalRadianOperations>(
                              senderCodeProvider, false, softwareId);
                if (softwareProviderRadian != null)
                {
                    switch (softwareProviderRadian.SoftwareType)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            if (softwareProviderRadian.TecnologicalSupplier || softwareProviderRadian.Factor || softwareProviderRadian.NegotiationSystem
                                || softwareProviderRadian.ElectronicInvoicer || softwareProviderRadian.IndirectElectronicInvoicer)
                                habilitadoRadian = true;
                            break;
                    }
                }

                if (documentMeta.SendTestSet)
                {
                    testSetId = documentMeta.TestSetId;
                    //Se busca el set de pruebas procesado para el testsetid en curso
                    RadianTestSetResult radianTesSetResult = TableManagerRadianTestSetResult.FindByTestSetId<RadianTestSetResult>(testSetId);
                    
                    if(radianTesSetResult != null && Convert.ToInt32(radianTesSetResult.ContributorTypeId) != (int)RadianContributorType.ElectronicInvoice)
                    {
                        //Valida evento mandato - sender mismo provider mismo mandatario
                        if (String.Equals(senderCode, senderCodeProvider) && String.Equals(senderCode, issuerPartyCode))
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC64",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC64"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }                  
                }
                else
                {
                    //Valida evento mandato - sender mismo provider mismo mandatario
                    if (String.Equals(senderCode, senderCodeProvider) && String.Equals(senderCode, issuerPartyCode))
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC64",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC64"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }
            else
            {
                softwareProvider = GetContributorInstanceCache(documentMeta.DocumentTypeId == "96" ? providerCode : softwareProviderCode);
            }

            if (string.IsNullOrEmpty(providerCodeDigit) || providerCodeDigit == "undefined") providerCodeDigit = "11";
            if (string.IsNullOrEmpty(softwareProviderCodeDigit) || softwareProviderCodeDigit == "undefined") softwareProviderCodeDigit = "11";
            if (ValidateDigitCode(documentMeta.DocumentTypeId == "96" ? providerCode : softwareProviderCode, documentMeta.DocumentTypeId == "96" ? int.Parse(providerCodeDigit) : int.Parse(softwareProviderCodeDigit)))
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareproviderDvErrorCode, ErrorMessage = "DV del NIT del Prestador de Servicios está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            //else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareproviderDvErrorCode, ErrorMessage = "DV del NIT del Prestador de Servicios no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            string senderErrorCode = "FAJ21";
            if (documentMeta.DocumentTypeId == "05") senderErrorCode = "DSAJ21";
            else if (documentMeta.DocumentTypeId == "91") senderErrorCode = "CAJ21";
            else if (documentMeta.DocumentTypeId == "92") senderErrorCode = "DAJ21";


            string sender2ErrorCode = "FAJ44";
            if (documentMeta.DocumentTypeId == "91") sender2ErrorCode = "CAJ44";
            else if (documentMeta.DocumentTypeId == "92") sender2ErrorCode = "DAJ44";


            string softwareProviderErrorCode = "FAB19b";
            if (documentMeta.DocumentTypeId == "05") softwareProviderErrorCode = "DSAB19b";
            else if (documentMeta.DocumentTypeId == "91") softwareProviderErrorCode = "CAB19b";
            else if (documentMeta.DocumentTypeId == "92") softwareProviderErrorCode = "DAB19b";

            //Validar habilitacion RADIAN
            else if (documentMeta.DocumentTypeId == "96") softwareProviderErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAB19b;

            string softwareProviderCodeHab = habilitadoRadian ? softwareProviderRadian?.PartitionKey : softwareProvider?.Code;

            if (ConfigurationManager.GetValue("Environment") == "Hab" || ConfigurationManager.GetValue("Environment") == "Test")
            {
                if (!string.IsNullOrEmpty(senderCodeProvider) && senderCode != senderCodeProvider && documentMeta.DocumentTypeId != "96")
                {
                    if (sender2 != null)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2.Code} del emisor de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2?.Code} Emisor de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }

                if (softwareProvider != null || habilitadoRadian)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{ softwareProviderCodeHab } Prestrador de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{ softwareProviderCodeHab } NIT del Prestador de Servicios No está autorizado para prestar servicios.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }
            else if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                //Valida software proveedor RADIAN Habilitado
                if (documentMeta.DocumentTypeId == "96" )
                {
                    if (softwareProviderRadian != null && habilitadoRadian && isRadian)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{ softwareProviderCodeHab } Prestrador de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else if(!isRadian)
                    {
                        //Valida eventos Fase I Factura Electronica 
                        if (softwareProvider?.StatusId == (int)ContributorStatus.Enabled)
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProviderCodeHab} Prestrador de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        else
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProviderCodeHab} NIT del Prestador de Servicios No está autorizado para prestar servicios.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    }
                    else
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{ softwareProviderCodeHab } NIT del Prestador de Servicios No está autorizado para prestar servicios.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
                else
                {
                    if (sender?.StatusId == (int)ContributorStatus.Enabled)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = senderErrorCode, ErrorMessage = $"{sender.Code} del emisor de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = senderErrorCode, ErrorMessage = $"{sender?.Code} Emisor de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                    if (!string.IsNullOrEmpty(senderCodeProvider) && senderCode != senderCodeProvider)
                    {
                        if (sender2?.StatusId == (int)ContributorStatus.Enabled)
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2.Code} del emisor de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        else
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2?.Code} Emisor de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    }

                    if (softwareProvider?.StatusId == (int)ContributorStatus.Enabled)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProviderCodeHab} Prestrador de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProviderCodeHab} NIT del Prestador de Servicios No está autorizado para prestar servicios.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                }
            }
            /*
            responses = GetStatusRutValidation(responses, sender, sender2);
            24 julio 2021
            desactivar la regla RUT01
            */

            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion

        #region GetStatusRutValidation
        public List<ValidateListResponse> GetStatusRutValidation(List<ValidateListResponse> responses, GlobalContributor sender, GlobalContributor sender2)
        {
            var statusRutValidationErrorCode = !String.IsNullOrEmpty(ConfigurationManager.GetValue("StatusRutValidationErrorCode")) ? ConfigurationManager.GetValue("StatusRutValidationErrorCode") : NoErrorCode;
            var isStatusRutValidationMandatory = !String.IsNullOrEmpty(ConfigurationManager.GetValue("IsStatusRutValidationMandatory")) ? ConfigurationManager.GetValue("IsStatusRutValidationMandatory") == DefaultStringValue : DefaultBoolValue;
            var statusRutValidationErrorMessage = !String.IsNullOrEmpty(ConfigurationManager.GetValue("StatusRutValidationErrorMessage")) ? ConfigurationManager.GetValue("StatusRutValidationErrorMessage") : DefaultStatusRutValidationErrorMessage;
            var statusRutValidationListStatusCode = !String.IsNullOrEmpty(ConfigurationManager.GetValue("StatusRutValidationListStatusCode")) ? ConfigurationManager.GetValue("StatusRutValidationListStatusCode") : new System.Text.StringBuilder().Append(((int)StatusRut.Cancelled).ToString()).Append('|').Append(((int)StatusRut.Inactive).ToString()).Append('|').Append(((int)StatusRut.Suspension).ToString()).ToString();

            if (sender != null)
            {
                if (statusRutValidationListStatusCode.Contains(sender.StatusRut.ToString()))
                {
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = isStatusRutValidationMandatory, ErrorCode = statusRutValidationErrorCode, ErrorMessage = statusRutValidationErrorMessage, ExecutionTime = DateTime.UtcNow.Subtract(DateTime.UtcNow).TotalSeconds });
                }
                else
                {
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = isStatusRutValidationMandatory, ErrorCode = statusRutValidationErrorCode, ErrorMessage = DefaultStatusRutValidationOkMessage, ExecutionTime = DateTime.UtcNow.Subtract(DateTime.UtcNow).TotalSeconds });
                }
            }
            if (sender2 != null)
            {
                if (statusRutValidationListStatusCode.Contains(sender2.StatusRut.ToString()))
                {
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = isStatusRutValidationMandatory, ErrorCode = statusRutValidationErrorCode, ErrorMessage = statusRutValidationErrorMessage, ExecutionTime = DateTime.UtcNow.Subtract(DateTime.UtcNow).TotalSeconds });
                }
                else
                {
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = isStatusRutValidationMandatory, ErrorCode = statusRutValidationErrorCode, ErrorMessage = DefaultStatusRutValidationOkMessage, ExecutionTime = DateTime.UtcNow.Subtract(DateTime.UtcNow).TotalSeconds });
                }
            }

            return responses;
        }
        #endregion


        #region Note validations
        public ValidateListResponse ValidateNoteReference(string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            var digit = documentMeta.DocumentTypeId == "91" ? "C" : "D";

            if (string.IsNullOrEmpty(documentMeta.DocumentReferencedKey))
                return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = $"{digit}BG02", ErrorMessage = "Se requiere obligatoriamente referencia a documento.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };

            var referencedDocumentData = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(documentMeta.DocumentReferencedKey, documentMeta.DocumentReferencedKey);

            var referencedDocument = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(referencedDocumentData?.Identifier, referencedDocumentData?.Identifier);
            if (referencedDocument == null)
            {
                referencedDocument = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(referencedDocumentData?.DocumentKey, referencedDocumentData?.DocumentKey);
                if (referencedDocument == null)
                    return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = $"{digit}BG04a", ErrorMessage = "Documento referenciado no existe en los registros de la DIAN.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };
            }

            if (referencedDocumentData.SenderCode != documentMeta.SenderCode)
                return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = $"{digit}BG04b", ErrorMessage = "Documento referenciado no pertenece al emisor.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };

            int.TryParse(referencedDocument.EmissionDateNumber, out int emissionDate);
            if (emissionDate > int.Parse(documentMeta.EmissionDate.ToString("yyyyMMdd")))
                return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = $"{digit}BG06", ErrorMessage = "Fecha de emisión de la nota es anterior a la fecha de emisión de FE referenciada.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };

            return new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = $"{digit}BG02", ErrorMessage = "Factura electrónica referenciada correctamente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };
        }
        #endregion

        #region Validate SenderCode and ReceiverCode        
        public List<ValidateListResponse> ValidateParty(NitModel nitModel, RequestObjectParty party, XmlParser xmlParserCude,
            List<string> issuerAttorneyList = null, string issuerAttorney = null, string senderAttorney = null,
            string partyLegalEntityName = null, string partyLegalEntityCompanyID = null, string availabilityCustomizationId = null)
        {
            DateTime startDate = DateTime.UtcNow;
            party.TrackId = party.TrackId.ToLower();
            ErrorCodeMessage errorCodeMessage = getErrorCodeMessage(party.ResponseCode);
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            string eventCode = party.ResponseCode;
            //Valida cambio legitimo tenedor
            string senderCode = nitModel.SenderCode;
            string receiverCode = nitModel.ReceiverCode;
            string receiverNameCude = xmlParserCude.Fields["ReceiverName"].ToString();
            string receiverNameCufe = nitModel.ReceiverName;
            string errorMessageParty = "Evento ValidateParty referenciado correctamente";

            //Endoso en Blanco
            if ((Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad || Convert.ToInt32(eventCode) == (int)EventStatus.EndosoGarantia ||
               Convert.ToInt32(eventCode) == (int)EventStatus.EndosoProcuracion) && party.ListId == "2")
            {
                party.SenderParty = nitModel.SenderCode;
            }

            // Is Radian
            var isRadian = false;
         
            var docEvent = docEventTableManager.FindpartitionKey<GlobalDocEvent>(eventCode).FirstOrDefault();
            if(docEvent != null)
                isRadian = docEvent.IsRadian;
            
            //valida si existe los permisos del mandatario
            if (party.SenderParty != xmlParserCude.ProviderCode
                && xmlParserCude.ProviderCode != "800197268"
                && Convert.ToInt32(eventCode) != (int)EventStatus.Mandato      
                && !party.SendTestSet
                && isRadian)
            {
                var responseVal = ValidateFacultityAttorney(nitModel, party, xmlParserCude);
                if (responseVal != null)
                {
                    foreach (var item in responseVal)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = item.IsValid,
                            Mandatory = item.Mandatory,
                            ErrorCode = item.ErrorCode,
                            ErrorMessage = item.ErrorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }

            switch (Convert.ToInt16(party.ResponseCode))
            {
                case (int)EventStatus.Received:
                case (int)EventStatus.Rejected:
                case (int)EventStatus.Receipt:
                case (int)EventStatus.Accepted:
                    //Valida emeisor documento 
                    if (party.SenderParty != receiverCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeMessage.errorCodeFETV,
                            ErrorMessage = errorCodeMessage.errorMessageFETV,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    //Valida receptor documento AR coincida con el Emisor/Facturador
                    if (party.ReceiverParty != senderCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeMessage.errorCodeReceiverFETV,
                            ErrorMessage = errorCodeMessage.errorMessageReceiverFETV,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;

                case (int)EventStatus.AceptacionTacita:

                    if (party.SenderParty != senderCode && (Convert.ToInt32(eventCode) != (int)EventStatus.Mandato))
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = (Convert.ToInt32(eventCode) == 34) ? errorCodeMessage.errorCodeFETV : errorCodeMessage.errorCode,
                            ErrorMessage = (Convert.ToInt32(eventCode) == 34) ? errorCodeMessage.errorMessageFETV : errorCodeMessage.errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    //Valida receptor documento AR coincida con DIAN
                    if (!string.IsNullOrWhiteSpace(party.ReceiverParty) && party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = Convert.ToInt16(party.ResponseCode) == 34 ? "AAG01e" : "AAG04",
                            ErrorMessage = Convert.ToInt16(party.ResponseCode) == 34 ? ConfigurationManager.GetValue("ErrorMessage_AAG01e") : ConfigurationManager.GetValue("ErrorMessage_AAG04"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;

                case (int)EventStatus.Mandato:

                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = errorMessageParty,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });

                    return responses;

                case (int)EventStatus.Avales:
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = errorMessageParty,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });

                    if ((party.SenderParty == senderCode || party.SenderParty == receiverCode) 
                        || (issuerAttorneyList != null && issuerAttorneyList.Contains(party.SenderParty)))
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC64",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC64"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    
                    //Valida receptor documento AR coincida con DIAN
                    if (party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG01",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG01"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;
                case (int)EventStatus.SolicitudDisponibilizacion:

                    //Valida existe cambio legitimo tenedor factura
                    bool validHolderExchang = false;
                    LogicalEventRadian logicalEventRadianRejected = new LogicalEventRadian();
                    HolderExchangeModel responseHolderExchange = logicalEventRadianRejected.RetrieveSenderHolderExchange(nitModel.DocumentKey, xmlParserCude);
                    if (responseHolderExchange != null)
                    {
                        validHolderExchang = true;
                        senderCode = !string.IsNullOrWhiteSpace(responseHolderExchange.PartyLegalEntity) ? responseHolderExchange.PartyLegalEntity : string.Empty;
                    }

                    if (party.SenderParty != senderCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = validHolderExchang ? errorCodeMessage.errorCodeB : errorCodeMessage.errorCode,
                            ErrorMessage = validHolderExchang ? errorCodeMessage.errorMessageB : errorCodeMessage.errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    //Valida receptor documento AR coincida con DIAN
                    if (party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG01",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG01"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;

                case (int)EventStatus.EndosoPropiedad:
                case (int)EventStatus.EndosoGarantia:
                case (int)EventStatus.EndosoProcuracion:

                    var receiverNameEndoso = xmlParserCude.Fields["ReceiverName"].ToString();
                    var receiverCodeEndoso = xmlParserCude.Fields["ReceiverCode"].ToString();

                    if (party.ListId != "2") // No informa SenderParty es un endoso en blanco entonces no valida emisor documento
                    {
                        if (party.SenderParty != senderCode)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = errorCodeMessage.errorCode,
                                ErrorMessage = errorCodeMessage.errorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    // EndosoPropiedad
                    if (Convert.ToInt16(party.ResponseCode) == (int)EventStatus.EndosoPropiedad
                        && (availabilityCustomizationId == "362" || availabilityCustomizationId == "364"))
                    {
                        if (partyLegalEntityName != receiverNameEndoso)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAG03",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG03_037"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }

                        if (partyLegalEntityCompanyID != receiverCodeEndoso)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAG04",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG04_037"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                       
                        var receiverPartyLegalEntityCompanyID = xmlParserCude.Fields["ReceiverPartyLegalEntityCompanyID"].ToString();
                        var receiverPartyLegalEntityName = xmlParserCude.Fields["ReceiverPartyLegalEntityName"].ToString();

                        if (receiverNameEndoso != receiverPartyLegalEntityName)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAG13",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG13"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }

                        if (receiverCodeEndoso != receiverPartyLegalEntityCompanyID)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAG14",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG14"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }

                    return responses;

                case (int)EventStatus.InvoiceOfferedForNegotiation:
                    if (party.SenderParty != senderCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeMessage.errorCode,
                            ErrorMessage = errorCodeMessage.errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    //Valida receptor documento AR coincida con el Endosatario
                    if (party.ReceiverParty != receiverCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG04a",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG04a"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;

                case (int)EventStatus.NegotiatedInvoice:
                case (int)EventStatus.AnulacionLimitacionCirculacion:
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = errorMessageParty,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });

                    if ((party.SenderParty == senderCode || party.SenderParty == receiverCode)
                        || (issuerAttorneyList != null && issuerAttorneyList.Contains(party.SenderParty)))
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC64",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC64"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    // Valida receptor documento AR coincida con DIAN
                    if (!string.IsNullOrWhiteSpace(party.ReceiverParty) && party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG04",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG04"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;

                case (int)EventStatus.TerminacionMandato:
                    //Revocación es información del mandante
                    if (party.CustomizationID == "441")
                    {
                        if (party.SenderParty != senderAttorney)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = errorCodeMessage.errorCode,
                                ErrorMessage = errorCodeMessage.errorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        // Valida receptor documento AR coincida con DIAN
                        if (!string.IsNullOrWhiteSpace(party.ReceiverParty) && party.ReceiverParty != "800197268")
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAG04",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG04"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    //Renuncia
                    else if (party.CustomizationID == "442")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });

                        //Renuncia es información del mandatario
                        if (party.SenderParty != issuerAttorney)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = errorCodeMessage.errorCode,
                                ErrorMessage = errorCodeMessage.errorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                       
                        if (!string.IsNullOrWhiteSpace(party.ReceiverParty) && party.ReceiverParty != "800197268")
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAG04",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG04"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }                                                 
                        
                    }
                    return responses;

                //NotificacionPagoTotalParcial
                case (int)EventStatus.NotificacionPagoTotalParcial:
                    if (party.SenderParty == receiverCode || party.SenderParty == senderCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeMessage.errorCode,
                            ErrorMessage = errorCodeMessage.errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    // Valida receptor documento AR coincida con DIAN
                    if (party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG01",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG01"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;
                //Valor Informe 3 dias pago
                case (int)EventStatus.ValInfoPago:

                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = errorMessageParty,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });

                    if (party.SenderParty != receiverCode)
                    {
                        var valid = ValidateBuyThreeDay(party.TrackId, party.SenderParty, nitModel.DocumentTypeId, (int)EventStatus.ValInfoPago);
                        if (valid != null)
                        {
                            responses.Add(valid);
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }

                    // Valida receptor documento AR coincida con DIAN
                    if (party.ReceiverParty != nitModel.ReceiverCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG04",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG04_046"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    // Valida nombre receptor documento Adquirente/Deudor/aceptante"
                    if (receiverNameCude != receiverNameCufe)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG01",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG01_046"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }                 
                    
                    break;
            }
            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion


        #region ValidatePayment
        private List<ValidateListResponse> ValidatePayment(XmlParser xmlParserCude, NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            //valor actual total factura TV
            string valueActualInvoice = nitModel.ValorActualTituloValor;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validPayment = false;

            //Valor pago
            XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
            int totalValueSender = 0;
            for (int i = 0; i < valueListSender.Count; i++)
            {
                string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                totalValueSender += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }

            if (nitModel.CustomizationId == "452")
            {
                //Valida Total valor pagado igual al valor actual del titulo valor
                if (totalValueSender != Int32.Parse(valueActualInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
                {
                    validPayment = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAF19c",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19c"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Valida Total valor pagado no supera el valor actual del titulo valor
            if (totalValueSender > Int32.Parse(valueActualInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
            {
                validPayment = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAF19b",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19b"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            if (validPayment)
                return responses;

            return null;
        }
        #endregion

        #region ValidateEndoso
        private List<ValidateListResponse> ValidateEndoso(XmlParser xmlParserCufe, XmlParser xmlParserCude, NitModel nitModel, string eventCode, double newAmountTV)
        {
            DateTime startDate = DateTime.UtcNow;
            //valor total Endoso Electronico AR
            string valueTotalEndoso = nitModel.ValorTotalEndoso;
            string valuePriceToPay = nitModel.PrecioPagarseFEV;
            string valueDiscountRateEndoso = nitModel.TasaDescuento;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validEndoso = false;           
            bool.TryParse(Environment.GetEnvironmentVariable("ValidateManadatory"), out bool ValidateManadatory);

            //Valida informacion Endoso en propiedad                       
            if ((Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad))
            {
                //Valida informacion Endoso                           
                if (String.IsNullOrEmpty(valuePriceToPay) || String.IsNullOrEmpty(valueDiscountRateEndoso))
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateManadatory,
                        ErrorCode = "AAI07b",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI07b"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }

                //Calculo valor de la negociación
                int resultNegotiationValue = (Int32.Parse(valueTotalEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) * (100 - Int32.Parse(valueDiscountRateEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)));
                resultNegotiationValue = resultNegotiationValue / 100;

                //Se debe comparar el valor de negociación contra el saldo(Nuevo Valor en disponibilización)
                if (Int32.Parse(valuePriceToPay, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != resultNegotiationValue)
                {
                    validEndoso = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateManadatory,
                        ErrorCode = "AAI07b",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI07b"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            if (xmlParserCude.Fields["listID"].ToString() != "2")
            {
                XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
                int totalValueSender = 0;
                for (int i = 0; i < valueListSender.Count; i++)
                {
                    string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                    totalValueSender += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }

                XmlNodeList valueListReceiver = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']");
                int totalValueReceiver = 0;
                for (int i = 0; i < valueListReceiver.Count; i++)
                {
                    string valueStockAmount = valueListReceiver.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                    totalValueReceiver += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }

                if (Int32.Parse(valueTotalEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueSender)
                {
                    validEndoso = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateManadatory,
                        ErrorCode = "AAF19",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

                if (Int32.Parse(valueTotalEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueReceiver)
                {
                    validEndoso = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateManadatory,
                        ErrorCode = "AAG20",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG20"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            if (validEndoso)
                return responses;

            return null;
        }
        #endregion

        #region ValidateEndosoNota
        private List<ValidateListResponse> ValidateEndosoNota(List<GlobalDocValidatorDocumentMeta> documentMetaList, XmlParser xmlParserCude, string eventCode)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validEndoso = false;

            if (eventCode == "040")
            {
                // busca un Endoso en Procuración...
                var documentMeta = documentMetaList.Where(x => x.EventCode == "039").OrderByDescending(x => x.SigningTimeStamp).FirstOrDefault();
                if (documentMeta != null)
                {
                    var document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
                    if (document == null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH07",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        return responses;
                    }

                    if (string.IsNullOrWhiteSpace(xmlParserCude.NoteMandato))
                    {
                        validEndoso = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAD11a",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAD11a_040"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }

            if (validEndoso)
                return responses;

            return null;
        }
        #endregion

        #region ValidateFacultityAttorney
        private List<ValidateListResponse> ValidateFacultityAttorney(NitModel nitModel, RequestObjectParty party, XmlParser xmlParserCude)
        {
            DateTime startDate = DateTime.UtcNow;
            string issueAtorney = xmlParserCude.ProviderCode;
            string eventCode = party.ResponseCode;
            string cufe = party.TrackId;
            string senderCode = xmlParserCude.Fields["SenderCode"].ToString();
            string noteMandato = xmlParserCude.NoteMandato;
            string noteMandato2 = xmlParserCude.NoteMandato2;
            string softwareId = xmlParserCude.Fields["SoftwareId"].ToString();
            ErrorCodeMessage errorCodeMessage = getErrorCodeMessage(eventCode);
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validError = false;

            //Valida exista informacion mandato - Mandatario - CUFE para eventos disitintos a Mandato 043
            if (Convert.ToInt32(eventCode) != (int)EventStatus.Mandato && !validError)
            {
                if (Convert.ToInt32(eventCode) == (int)EventStatus.TerminacionMandato)
                {
                    var referenceCudeMandato = TableManagerGlobalDocReferenceAttorney.FindByPartition<GlobalDocReferenceAttorney>(party.TrackId);
                    if (referenceCudeMandato != null)
                    {
                        foreach (var cufeMandato in referenceCudeMandato)
                            cufe = cufeMandato.RowKey;
                    }
                }

                var docsReferenceAttorney = TableManagerGlobalDocReferenceAttorney.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(cufe, senderCode);

                //No existe Mandato para el CUFE referenciado se valida si es Mandato Abierto ListID = 3
                if (docsReferenceAttorney == null || !docsReferenceAttorney.Any())
                {
                    docsReferenceAttorney = TableManagerGlobalDocReferenceAttorney.FindDocumentSenderCodeIssueAttorney<GlobalDocReferenceAttorney>(issueAtorney, senderCode);
                    if (docsReferenceAttorney == null || !docsReferenceAttorney.Any())
                    {
                        validError = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC36",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC36"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
                else
                {
                    //Valida asociacion mandato abierto
                    docsReferenceAttorney = TableManagerGlobalDocReferenceAttorney.FindDocumentSenderCodeIssueAttorney<GlobalDocReferenceAttorney>(issueAtorney, senderCode);
                    if (docsReferenceAttorney == null || !docsReferenceAttorney.Any())
                    {
                        validError = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC36",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC36"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }

                if (docsReferenceAttorney != null)
                {
                    //Valida existan permisos para firmar evento por mandatario
                    foreach (var docReferenceAttorney in docsReferenceAttorney)
                    {
                        validError = false;
                        if (docReferenceAttorney.IssuerAttorney == issueAtorney)
                        {
                            if ((String.IsNullOrEmpty(docReferenceAttorney.EndDate)
                                || (DateTime.ParseExact(docReferenceAttorney.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) >= DateTime.Now)
                                ) && docReferenceAttorney.Active)
                            {
                                //Valida se encuetre habilitado Modo Operacion RadianOperation
                                var globalRadianOperation = TableManagerGlobalRadianOperations.FindhByPartitionKeyRadianStatus<GlobalRadianOperations>(
                                    docReferenceAttorney.IssuerAttorney, false, softwareId);
                                if (globalRadianOperation == null)
                                {
                                    validError = true;
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "AAH62b",
                                        ErrorMessage = "El número de documento no corresponde a un participante habilitado en la plataforma RADIAN (PT/Factor/SNE/FE).",
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                                else
                                {
                                    switch (docReferenceAttorney.Actor)
                                    {
                                        case "FE":
                                            if (!globalRadianOperation.ElectronicInvoicer)
                                            {
                                                validError = true;
                                                responses.Add(new ValidateListResponse
                                                {
                                                    IsValid = false,
                                                    Mandatory = true,
                                                    ErrorCode = "LGC65",
                                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC65"),
                                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                });
                                            }
                                            break;
                                        case "PT":
                                            if (!globalRadianOperation.TecnologicalSupplier)
                                            {
                                                validError = true;
                                                responses.Add(new ValidateListResponse
                                                {
                                                    IsValid = false,
                                                    Mandatory = true,
                                                    ErrorCode = "LGC59",
                                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC59"),
                                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                });
                                            }
                                            break;
                                        case "F":
                                            if (!globalRadianOperation.Factor)
                                            {
                                                validError = true;
                                                responses.Add(new ValidateListResponse
                                                {
                                                    IsValid = false,
                                                    Mandatory = true,
                                                    ErrorCode = "LGC60",
                                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC60"),
                                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                });
                                            }
                                            break;
                                        case "SNE":
                                            if (!globalRadianOperation.NegotiationSystem)
                                            {
                                                validError = true;
                                                responses.Add(new ValidateListResponse
                                                {
                                                    IsValid = false,
                                                    Mandatory = true,
                                                    ErrorCode = "LGC61",
                                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC61"),
                                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                });
                                            }
                                            break;
                                    }
                                }

                                if (!validError)
                                {
                                    string[] tempFacultityCode = docReferenceAttorney.FacultityCode.Split(';');
                                    bool validForAttorneyFacultity = false;
                                    foreach (string codeFacultity in tempFacultityCode)
                                    {
                                        //Valida permisos/facultades firma para el evento emitido
                                        var filter = $"{codeFacultity}-{docReferenceAttorney.Actor}";                                        
                                        var attorneyFacultity = TableManagerGlobalAttorneyFacultity.FindDocumentFaculitityEvent<GlobalAttorneyFacultity>(eventCode);
                                        attorneyFacultity = attorneyFacultity.Where(t => t.PartitionKey == filter).ToList();

                                        if (attorneyFacultity != null && attorneyFacultity.Count > 0)
                                        {
                                            //Valida exista note mandatario
                                            if (noteMandato == null || !noteMandato.Contains("OBRANDO EN NOMBRE Y REPRESENTACION DE"))
                                            {
                                                if (noteMandato2 == null || !noteMandato2.Contains("OBRANDO EN NOMBRE Y REPRESENTACION DE"))
                                                {
                                                    validError = true;
                                                    validForAttorneyFacultity = false;
                                                    responses.Add(new ValidateListResponse
                                                    {
                                                        IsValid = false,
                                                        Mandatory = true,
                                                        ErrorCode = eventCode == "035" ? errorCodeMessage.errorCodeNoteA : errorCodeMessage.errorCodeNote,
                                                        ErrorMessage = eventCode == "035" ? errorCodeMessage.errorMessageNoteA : errorCodeMessage.errorMessageNote,
                                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                    });
                                                    break;
                                                }
                                            }

                                            //Si mandatario tiene permisos/facultades y esta habilitado para emitir documentos
                                            if (!validError)
                                                return null;
                                           
                                        }
                                        else
                                            validForAttorneyFacultity = true;
                                    }

                                    if (validForAttorneyFacultity)
                                    {                                       
                                        validError = true;
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = false,
                                            Mandatory = true,
                                            ErrorCode = (Convert.ToInt32(eventCode) >= 30 && Convert.ToInt32(eventCode) <= 34) ? errorCodeMessage.errorCodeFETV : errorCodeMessage.errorCodeMandato,
                                            ErrorMessage = (Convert.ToInt32(eventCode) >= 30 && Convert.ToInt32(eventCode) <= 34) ? errorCodeMessage.errorMessageFETV : errorCodeMessage.errorMessageMandato,
                                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                        });
                                        break;                                        
                                    }
                                }
                            }
                            else
                            {
                                validError = true;
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "LGC35",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC35"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }

                        if (validError)
                            return responses;
                    }
                }

                if (validError)
                    return responses;
            }
            if (validError)
                return responses;

            return null;
        }
        #endregion

        #region ValidateValPago
        private ValidateListResponse ValidateBuyThreeDay(string trackId, string SenderParty, string documentTypeId, int eventCode)
        {
            DateTime startDate = DateTime.UtcNow;
            GlobalDocValidatorDocument document1 = null;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Servicio
            List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(trackId.ToLower());
            List<GlobalDocValidatorDocumentMeta> documentMeta = (InvoiceWrapper.Any()) ? InvoiceWrapper[0].Documents.Select(x => x.DocumentMeta).ToList() : null;
               
            foreach (var document in documentMeta)
            {
                document1 = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(document.Identifier, document.Identifier);
                if (document1 != null)
                {
                    if (documentMeta.Where(t => t.EventCode == "037").ToList().Count == decimal.Zero)
                    {
                        if (documentMeta.Where(t => t.EventCode == "036").ToList().Count > decimal.Zero)
                        {
                            if (SenderParty == document.SenderCode && document.CustomizationID == "361" || document.CustomizationID == "362")
                            {
                                return null;
                            }
                            else
                            {
                                return new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "LGC51",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC51"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                };
                            }
                        }
                        else
                        {
                            return new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC51",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC51"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            };
                        }
                    }
                    else
                    {
                        if (SenderParty == document.ReceiverCode && document.EventCode == "037")
                        {
                            return null;
                        }
                        else
                        {
                            return new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAF03",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF03"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            };
                        }
                    }

                }
            }
            return null;
        }
        #endregion
        #region IsBase64
        private bool IsBase64(string base64String)
        {
            var rs = !string.IsNullOrEmpty(base64String) && !string.IsNullOrWhiteSpace(base64String) && base64String.Length != 0 && base64String.Length % 4 == 0 && !base64String.Contains(" ");
            return rs;
        }
        #endregion

        #region ValidateReferenceAttorney
        private List<ValidateListResponse> ValidateCufeReferenceAttorney(XmlParser xmlParser, XmlNamespaceManager ns)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            RequestObjectSigningTime dataSigningtime = new RequestObjectSigningTime();
            NitModel nitModel = new NitModel();
            bool validate = false;
            bool validateID = false;
            bool validateReference = false;
            bool validateSigningTime = false;
            int attorneyLimit = Convert.ToInt32(ConfigurationManager.GetValue("MAX_Attorney"));            
            bool.TryParse(Environment.GetEnvironmentVariable("ValidateManadatory"), out bool ValidateManadatory);

            string listID = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:ResponseCode", ns).Item(0)?.Attributes["listID"].Value;
            string companyId = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:SenderParty/cac:PartyTaxScheme/cbc:CompanyID", ns).Item(0)?.InnerText.ToString();
            string customizationID = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cbc:CustomizationID", ns).Item(0)?.InnerText.ToString();
            XmlNodeList cufeListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse[1]/cac:DocumentReference/cbc:ID", ns);
            XmlNodeList cufeListResponseRefeerence = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse[2]/cac:DocumentReference/cbc:ID", ns);
            XmlNodeList cufeListDocumentResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse", ns);
            XmlNodeList cufeListDocumentResponseReference = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse[1]/cac:DocumentReference", ns);
            int countCufeListDocumentResponse = cufeListDocumentResponse.Count - 1;
            int countCufeListDocumentResponseReference = cufeListDocumentResponseReference.Count;
            string xmlID2 = string.Empty;
            string xmlUUID2 = string.Empty;

            //Valida cantidad de CUFEs referenciados
            if (cufeListResponse.Count > attorneyLimit || cufeListResponseRefeerence.Count > attorneyLimit)
            {
                validate = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC58",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC58"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else if (cufeListResponse.Count != cufeListResponseRefeerence.Count && (customizationID == "431" || customizationID == "432"))
            {
                validate = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAL04",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL04"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else if (countCufeListDocumentResponse != countCufeListDocumentResponseReference && (customizationID == "433" || customizationID == "434"))
            {
                validate = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAL04",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL04"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {

                //Compara ID seccion DocumentReference 1 /  DocumentReference 2
                for (int i = 0; i < cufeListResponse.Count; i++)
                {
                    var xmlID = cufeListResponse.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse[1]/cac:DocumentReference/cbc:ID", ns).Item(i)?.InnerText.ToString();
                    if (customizationID == "431" || customizationID == "432")
                        xmlID2 = cufeListResponseRefeerence.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse[2]/cac:DocumentReference/cbc:ID", ns).Item(i)?.InnerText.ToString();
                    else
                        xmlID2 = cufeListDocumentResponse.Item(i + 1).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:DocumentReference/cbc:ID", ns).Item(i)?.InnerText.ToString();

                    if (!String.Equals(xmlID, xmlID2))
                    {
                        validateID = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAL05",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL05"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validateID = false;
                }

                //Compara UUID seccion DocumentReference 1 /  DocumentReference 2
                for (int i = 0; i < cufeListResponse.Count; i++)
                {
                    var xmlUUID = cufeListResponse.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse[1]/cac:DocumentReference/cbc:UUID", ns).Item(i)?.InnerText.ToString();
                    if (customizationID == "431" || customizationID == "432")
                        xmlUUID2 = cufeListResponseRefeerence.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse[2]/cac:DocumentReference/cbc:UUID", ns).Item(i)?.InnerText.ToString();
                    else
                        xmlUUID2 = cufeListDocumentResponse.Item(i + 1).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:DocumentReference/cbc:UUID", ns).Item(i)?.InnerText.ToString();


                    if (!String.Equals(xmlUUID, xmlUUID2))
                    {
                        validate = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAL07",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL07"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validate = false;
                }
            }

            //Valida documentos referenciados
            for (int i = 0; i < cufeListResponse.Count && !validateID && !validate; i++)
            {
                var xmlID = cufeListResponse.Item(i).SelectNodes("//cac:DocumentReference/cbc:ID", ns).Item(i)?.InnerText.ToString();
                var xmlUUID = cufeListResponse.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:DocumentReference/cbc:UUID", ns).Item(i)?.InnerText.ToString();
                var xmlDocumentTypeCode = cufeListResponse.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:DocumentReference/cbc:DocumentTypeCode", ns).Item(i)?.InnerText.ToString();

                //Valida CUFE referenciado existe en sistema DIAN                
                var resultValidateCufe = ValidateDocumentReferencePrev(xmlUUID, xmlID, "043", xmlDocumentTypeCode);
                foreach (var itemCufe in resultValidateCufe)
                {
                    if (!itemCufe.IsValid)
                    {
                        validateReference = true;
                        validate = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = itemCufe.IsValid,
                            Mandatory = true,
                            ErrorCode = itemCufe.ErrorCode,
                            ErrorMessage = itemCufe.ErrorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }

                
                var docHolderExchange = TableManagerGlobalDocHolderExchange.FindhByCufeExchange<GlobalDocHolderExchange>(xmlUUID.ToLower(), true);
                if (docHolderExchange != null)
                {
                    //Existe mas de un legitimo tenedor requiere un mandatario
                    string[] endosatarios = docHolderExchange.PartyLegalEntity.Split('|');
                    if (endosatarios.Length == 1)
                    {
                        if (docHolderExchange.PartyLegalEntity != companyId)
                        {
                            validateReference = true;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAL07",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL07"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    else
                    {
                        validateReference = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = "Factura cuenta con mas de un Legitimo tenedor, no es posible crear un mandato",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
                else
                {
                    var documentMetaCUFE = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(xmlUUID.ToLower(), xmlUUID.ToLower());
                    if (documentMetaCUFE != null)
                    {
                        if (documentMetaCUFE.SenderCode != companyId)
                        {
                            validateReference = true;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAL07",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL07"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                }


                if (validateReference)
                    break;
            }

            dataSigningtime.EventCode = "043";
            dataSigningtime.SigningTime = xmlParser.SigningTime;
            dataSigningtime.DocumentTypeId = "96";
            dataSigningtime.CustomizationID = customizationID;
            dataSigningtime.EndDate = "";

            //Valida La fecha debe ser mayor o igual al evento de la factura referenciada
            for (int i = 0; i < cufeListResponse.Count && !validateID && !validate; i++)
            {
                ValidatorEngine validatorEngine = new ValidatorEngine();
                dataSigningtime.TrackId = cufeListResponse.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:DocumentReference/cbc:UUID", ns).Item(i)?.InnerText.ToString();
                var xmlBytesCufe = validatorEngine.GetXmlFromStorageAsync(dataSigningtime.TrackId);
                var xmlParserCufe = new XmlParser(xmlBytesCufe.Result);
                if (!xmlParserCufe.Parser())
                    throw new Exception(xmlParserCufe.ParserError);


                var resultValidateSignInTime = ValidateSigningTime(dataSigningtime, xmlParserCufe, nitModel);
                foreach (var itemSingInTIme in resultValidateSignInTime)
                {
                    if (!itemSingInTIme.IsValid)
                    {
                        validateSigningTime = true;
                        validate = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = itemSingInTIme.IsValid,
                            Mandatory = true,
                            ErrorCode = "DC24r",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24r"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
                if (validateSigningTime)
                    break;
            }

            if (validate || validateID)
                return responses;

            return null;
        }
        #endregion

        #region Validate Reference Attorney
        public List<ValidateListResponse> ValidateReferenceAttorney(XmlParser xmlParser, string trackId, XmlNamespaceManager ns)
        {
            int attorneyLimit = Convert.ToInt32(ConfigurationManager.GetValue("MAX_Attorney"));
            bool validate = true;
            DateTime startDate = DateTime.UtcNow;
            RequestObjectSigningTime dataSigningtime = new RequestObjectSigningTime();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            string senderCode = xmlParser.FieldValue("SenderCode", true).ToString();
            string providerCode = xmlParser.FieldValue("ProviderCode", true).ToString();

            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                     
            XmlNodeList AttachmentBase64List = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:LineResponse/cac:LineReference/cac:DocumentReference/cac:Attachment/cbc:EmbeddedDocumentBinaryObject",ns);
                        
            string issuerPartyCode = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:IssuerParty/cac:PowerOfAttorney/cbc:ID",ns).Item(0)?.InnerText.ToString();
            XmlNodeList cufeList = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse", ns);
            XmlNodeList cufeListReference = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse[2]/cac:DocumentReference", ns);
            string customizationID = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cbc:CustomizationID", ns).Item(0)?.InnerText.ToString();
            string operacionMandato = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cbc:CustomizationID/@schemeID", ns).Item(0)?.InnerText.ToString();
            string listID = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:ResponseCode", ns).Item(0)?.Attributes["listID"].Value;
            dataSigningtime.EventCode = "043";
            dataSigningtime.SigningTime = xmlParser.SigningTime;
            dataSigningtime.DocumentTypeId = "96";
            dataSigningtime.CustomizationID = customizationID;
            dataSigningtime.EndDate = "";
            string factorTemp = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:IssuerParty/cac:PowerOfAttorney/cac:AgentParty/cac:PartyIdentification/cbc:ID", ns).Item(0)?.InnerText.ToString();
            string description = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:IssuerParty/cac:PowerOfAttorney/cbc:Description", ns).Item(0)?.InnerText.ToString();
            string senderId = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:SenderParty/cac:PowerOfAttorney/cac:AgentParty/cac:PartyIdentification/cbc:ID", ns).Item(0)?.InnerText.ToString();
            string senderPowerOfAttorney = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:SenderParty/cac:PowerOfAttorney/cbc:ID", ns).Item(0)?.InnerText.ToString();
            string descriptionSender = xmlParser.XmlDocument.DocumentElement.SelectNodes("/sig:ApplicationResponse/cac:SenderParty/cac:PowerOfAttorney/cbc:Description", ns).Item(0)?.InnerText.ToString();

            bool sendTestSet = false;
            string modoOperacion = string.Empty;
            string softwareId = xmlParser.Fields["SoftwareId"].ToString();

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento ValidateReferenceAttorney referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            //Validaciones previas secciones DocumentResponse / DocumentReference 1 y 2
            if (listID != "3")
            {
                var validateCufeReferenceAttorney = ValidateCufeReferenceAttorney(xmlParser, ns);
                if (validateCufeReferenceAttorney != null)
                {
                    validate = false;
                    foreach (var item in validateCufeReferenceAttorney)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = item.IsValid,
                            Mandatory = item.Mandatory,
                            ErrorCode = item.ErrorCode,
                            ErrorMessage = item.ErrorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }

            //Validacion senderParty igual a  senderParty / PowerOfAttorney ID
            if (!string.IsNullOrWhiteSpace(senderPowerOfAttorney))
            {
                long number1 = 0;
                bool valNumber = long.TryParse(senderPowerOfAttorney, out number1);

                if (valNumber && senderCode != senderPowerOfAttorney)
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAF32",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF32"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            long numberDesc = 0;
            bool valNumberDesc = long.TryParse(descriptionSender, out numberDesc);

            if (!string.IsNullOrWhiteSpace(descriptionSender) || !valNumberDesc)
            {
                //Validacion descripcion Mandante
                switch (senderId)
                {
                    case "Mandante-FE":
                        if (descriptionSender != "Mandante Facturador Electrónico")
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAF35",
                                ErrorMessage = "No fue informado el literal “Mandante Facturador Electrónico” de acuerdo con el campo “Descripcion” de la lista 13.2.5 Tipo de Mandante",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        break;
                    case "Mandante-LT":
                        if (descriptionSender != "Mandante Legitimo Tenedor")
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAF35",
                                ErrorMessage = "No fue informado el literal “Mandante Legitimo Tenedor” de acuerdo con el campo “Descripcion” de la lista 13.2.5 Tipo de Mandante",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        break;
                    case "Mandante-AV":
                        if (descriptionSender != "Mandante Aval")
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAF35",
                                ErrorMessage = "No fue informado el literal “Mandante Aval” de acuerdo con el campo “Descripcion” de la lista 13.2.5 Tipo de Mandante",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        break;
                    case "Mandante-AD":
                        if (descriptionSender != "Mandante Adquirente/Deudor")
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAF35",
                                ErrorMessage = "No fue informado el literal “Mandante Adquirente/Deudor” de acuerdo con el campo “Descripcion” de la lista 13.2.5 Tipo de Mandante",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        break;
                }

            }

            //Valida descripcion Mandatario 
            switch (factorTemp)
            {
                case "M-FE":
                    modoOperacion = "FE";
                    if (description != "Mandatario Facturador Electrónico")
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH65",
                            ErrorMessage = "No fue informado el literal “Mandatario Facturador Electrónico” de acuerdo con el campo “Descripcion” de la lista 13.2.8 Tipo de Mandatario",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case "M-SN-e":
                    modoOperacion = "SNE";
                    if (description != "Mandatario Sistema de Negociación Electrónica")
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH65",
                            ErrorMessage = "No fue informado el literal “Mandatario Sistema de Negociación Electrónica” de acuerdo con el campo “Descripcion” de la lista 13.2.8 Tipo de Mandatario",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case "M-Factor":
                    modoOperacion = "F";
                    if (description != "Mandatario Factor")
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH65",
                            ErrorMessage = "No fue informado el literal “Mandatario Factor” de acuerdo con el campo “Descripcion” de la lista 13.2.8 Tipo de Mandatario",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case "M-PT":
                    modoOperacion = "PT";
                    if (description != "Mandatario Proveedor Tecnológico")
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH65",
                            ErrorMessage = "No fue informado el literal “Mandatario Proveedor Tecnológico” de acuerdo con el campo “Descripcion” de la lista 13.2.8 Tipo de Mandatario",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
            }

            //Obtiene informacion del CUDE
            var documentMetaCUFE = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (documentMetaCUFE != null)
                sendTestSet = documentMetaCUFE.SendTestSet;

            //Valida se encuetre habilitado Modo Operacion RadianOperation
            var globalRadianOperation = TableManagerGlobalRadianOperations.FindhByPartitionKeyRadianStatus<GlobalRadianOperations>(
                issuerPartyCode, false, softwareId);

            //Validacion habilitado Modo Operacion RadianOperation y providerID igual a  IssuerParty / PowerOfAttorney / ID  
            if (!sendTestSet)
            {
                if (globalRadianOperation == null)
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAH62b",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH62b"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                else
                {
                    if (!globalRadianOperation.TecnologicalSupplier && !globalRadianOperation.Factor
                        && !globalRadianOperation.NegotiationSystem && !globalRadianOperation.ElectronicInvoicer)
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH62b",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH62b"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }
          
            if(issuerPartyCode != providerCode)
            {
                validate = false;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAH62b",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH62b"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida existe Contrato de mandatos entre las partes
            if (operacionMandato == "2")
            {
                if (AttachmentBase64List.Count < 2)
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAH84",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH84"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            if (AttachmentBase64List.Count > 0)
            {
                for (int i = 0; i < AttachmentBase64List.Count && validate; i++)
                {
                    string AttachmentBase64 = AttachmentBase64List.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:LineResponse/cac:LineReference/cac:DocumentReference/cac:Attachment/cbc:EmbeddedDocumentBinaryObject", ns).Item(i)?.InnerText.ToString().Trim();

                    if (!IsBase64(AttachmentBase64))
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH84",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH84"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                    {
                        byte[] data = Convert.FromBase64String(AttachmentBase64);
                        var mimeType = GetMimeFromBytes(data);
                        if (mimeType != pdfMimeType)
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAH84",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH84"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                    }
                }
            }

            var facultitys = TableManagerGlobalAttorneyFacultity.FindAll<GlobalAttorneyFacultity>();
            //Si existen mas de 2 documentResposne
            int listReference = cufeListReference.Count == 1 ? cufeList.Count : cufeListReference.Count;

            //Grupo de información alcances para el mandato sobre los CUFE.
            for (int i = 1; i < listReference && i < attorneyLimit && validate; i++)
            {
                //Valida facultades madato 
                string[] tempCode = new string[0];
                string descriptionCode = string.Empty;

                if (cufeList.Count > 2)
                {
                    descriptionCode = cufeList.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:Description", ns).Item(i)?.InnerText.ToString();
                    string code = cufeList.Item(i).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:ResponseCode", ns).Item(i)?.InnerText.ToString();
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        tempCode = code.Split(';');
                    }
                }
                else
                {
                    descriptionCode = cufeList.Item(1).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:Description", ns).Item(1)?.InnerText.ToString();
                    string code = cufeList.Item(1).SelectNodes("/sig:ApplicationResponse/cac:DocumentResponse/cac:Response/cbc:ResponseCode", ns).Item(1)?.InnerText.ToString();
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        tempCode = code.Split(';');
                    }
                }

                bool codeExist = false;
                bool validDescriptionCode = false;
                foreach (string codeAttorney in tempCode)
                {
                    //Valida exitan codigos de facultades asignadas mandato
                    if (facultitys.SingleOrDefault(t => t.PartitionKey == codeAttorney) == null)
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAL02",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL02"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                        codeExist = true;

                    codeExist = false;

                    //Valida codigos facultades mandato General - Limitado
                    string[] tempCodeAttorney = codeAttorney.Split('-');
                    if (validate && modoOperacion == tempCodeAttorney[1])
                    {
                        if ((customizationID == "431" || customizationID == "432"))
                        {
                            if (tempCode.Length == 1 && tempCodeAttorney[0] == "ALL17") codeExist = true;

                            //Valida description code acorde al codigo ingresado de mandato general
                            if (!descriptionCode.Equals("Mandato por documento General"))
                            {
                                validate = false;
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "AAL03",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL03"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }

                        }
                        else if ((customizationID == "433" || customizationID == "434"))
                        {
                            if (tempCode.Contains("MR91-" + tempCodeAttorney[1])) codeExist = true;

                            //Valida description code acorde al codigo ingresado de mandato Limitado
                            if (!descriptionCode.Equals("Mandato por documento Limitado"))
                            {
                                validate = false;
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "AAL03",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL03"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                        else
                            codeExist = false;
                    }                   
                }

                if (!codeExist)
                {
                    if (validate)
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAL02",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL02"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                   
                    if (new string[] { "Mandato por documento General", "Mandato por documento Limitado" }.Contains(descriptionCode)) validDescriptionCode = true;
                    if (!validDescriptionCode)
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAL03",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL03"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }

            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion

        /// <summary>
        /// Get mime type from data
        /// </summary>
        /// <param name="pBC"></param>
        /// <param name="pwzUrl"></param>
        /// <param name="pBuffer"></param>
        /// <param name="cbSize"></param>
        /// <param name="pwzMimeProposed"></param>
        /// <param name="dwMimeFlags"></param>
        /// <param name="ppwzMimeOut"></param>
        /// <param name="dwReserved"></param>
        /// <returns></returns>
        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        static extern int FindMimeFromData(IntPtr pBC,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I1, SizeParamIndex=3)]
            byte[] pBuffer,
            int cbSize,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
            int dwMimeFlags,
            out IntPtr ppwzMimeOut,
            int dwReserved);


        /// <summary>
        /// Get bytes mime type
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetMimeFromBytes(byte[] data)
        {
            try
            {
                string mime = string.Empty;
                IntPtr outPtr = IntPtr.Zero;
                int ret = FindMimeFromData(IntPtr.Zero, null, data, data.Length, null, 0, out outPtr, 0);
                if (ret == 0 && outPtr != IntPtr.Zero)
                {
                    //todo: this leaks memory outPtr must be freed
                    return Marshal.PtrToStringUni(outPtr);
                }
                return mime;
            }
            catch
            {
                return "";
            }
        }

        #region Number range validation
        public List<ValidateListResponse> ValidateNumberingRange(NumberRangeModel numberRangeModel, string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            var invoiceAuthorization = numberRangeModel.InvoiceAuthorization;
            var softwareId = numberRangeModel.SoftwareId;
            var senderCode = numberRangeModel.SenderCode;

            GlobalTestSetResult testSetResult = null;
            List<GlobalTestSetResult> testSetResults = null;
            GlobalNumberRange range = null;            
            List<GlobalNumberRange> ranges = GetNumberRangeInstanceCache(senderCode);

            if (ConfigurationManager.GetValue("Environment") == "Hab")
            {
                testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(senderCode);

                if (softwareId == ConfigurationManager.GetValue("BillerSoftwareId"))
                    range = ranges.FirstOrDefault(r => r.Serie == "SETG");
                else
                {
                    var contributor = GetContributorInstanceCache(senderCode);
                    testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{contributor?.TypeId}|{softwareId}");
                    if ((contributor?.TypeId == (int)ContributorType.Biller || contributor?.TypeId == (int)ContributorType.Provider) && testSetResult?.OperationModeId == (int)OperationMode.Own)
                        range = ranges.FirstOrDefault(r => r.Serie == "SETP");
                    else if ((contributor?.TypeId == (int)ContributorType.Biller || contributor?.TypeId == (int)ContributorType.Provider) && testSetResult?.OperationModeId == (int)OperationMode.Provider)
                        range = ranges.FirstOrDefault(r => r.Serie == "SETT");
                }
            }

            var documentType = documentMeta?.DocumentTypeId;
            if (ConfigurationManager.GetValue("Environment") == "Prod" || ConfigurationManager.GetValue("Environment") == "Test")
            {                
                if (Convert.ToInt32(documentType) == (int)DocumentType.DocumentSupportInvoice)
                {
                    var rk = $"{documentMeta?.Serie}|{documentType}|{documentMeta?.InvoiceAuthorization}";
                    range = ranges?.FirstOrDefault(r => r.PartitionKey == documentMeta.SenderCode && r.RowKey == rk);
                }
                else
                {
                    if (new string[] { "01", "02", "04", "09", "11" }.Contains(documentType)) documentType = "01";
                    var rk = $"{documentMeta?.Serie}|{documentType}|{documentMeta?.InvoiceAuthorization}";
                    range = ranges?.FirstOrDefault(r => r.PartitionKey == documentMeta.SenderCode && r.RowKey == rk);
                }
            }

            // If dont found range return
            if (range == null)
            {
                if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
                {                                       
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "DSAB05b",
                        ErrorMessage = "Número de la autorización de la numeración no corresponde a un número de autorización de este contribuyente vendedor para este Proveedor de Autorización",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });

                    return responses;
                }
                else
                {
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAD05e", ErrorMessage = "Número de factura no existe para el número de autorización.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    return responses;
                }

            }
           
            // Check invoice authorization Factura Electronica
            if (range.ResolutionNumber == invoiceAuthorization)
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = "FAB05b",
                    ErrorMessage = "Número de la resolución que autoriza la numeración corresponde a un número de resolución de este contribuyente emisor para este Proveedor de Autorización.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            else
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "FAB05b",
                    ErrorMessage = "Número de la resolución que autoriza la numeración no corresponde a un número de resolución de este contribuyente emisor para este Proveedor de Autorización.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                // Check software
                if (range.SoftwareId == softwareId)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB05c", ErrorMessage = "El indentificador del software corresponde al rango de numeración informado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB05c", ErrorMessage = "El indentificador del software no corresponde al rango de numeración informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }

            //
            DateTime.TryParse(numberRangeModel.StartDate, out DateTime _startDate);
            int.TryParse(_startDate.ToString("yyyyMMdd"), out int fromDateNumber);
            if (range.ValidDateNumberFrom == fromDateNumber)
            {
                if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "DSAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }
            else
            {
                if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "DSAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado NO corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado no corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }


            //Valida fecha de emision posterior a la fecha de final de autorizacion Documento soporte
            if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
            {

                //Valida numero documento este dentro del rango autorizado
                string validateSerie = numberRangeModel.SerieAndNumber;
                bool validateNumberID = false;
                if (!validateSerie.Contains(numberRangeModel.Serie)) validateNumberID = true;
               
                long num = 0;
                Match m = Regex.Match(validateSerie, "(\\d+)");
                if (m.Success)
                {
                    long.TryParse(numberRangeModel.StartNumber, out long fromNumberID);
                    long.TryParse(numberRangeModel.EndNumber, out long endNumberID);
                    num = long.Parse(m.Value);
                    if (num < fromNumberID || num > endNumberID) validateNumberID = true;                  
                }

                if(validateNumberID)
                {
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "DSAD05d", ErrorMessage = "Número de documento soporte no está contenido en el rango de numeración autorizado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "DSAD05e", ErrorMessage = "Número de documento soporte no existe para el número de autorización.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }

                DateTime.TryParse(numberRangeModel.EmissionDate, out DateTime numberEmisionDate);
                int.TryParse(numberEmisionDate.ToString("yyyyMMdd"), out int issueDateNumberEmision);

                DateTime.TryParse(numberRangeModel.StartDate, out DateTime startNumberEmision);
                int.TryParse(startNumberEmision.ToString("yyyyMMdd"), out int dateStartNumberEmision);
                if (dateStartNumberEmision > issueDateNumberEmision)
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "DSAB07a",
                        ErrorMessage = "Fecha de emisión anterior a la fecha de inicio de la autorización de la numeración ",                        
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                else
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "DSAB07a",
                        ErrorMessage = "Fecha emision referenciada correctamente",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });


                DateTime.TryParse(numberRangeModel.EndDate, out DateTime endNumberEmision);
                int.TryParse(endNumberEmision.ToString("yyyyMMdd"), out int dateNumberEmision);
                if (dateNumberEmision < issueDateNumberEmision)
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "DSAB08a",
                        ErrorMessage = "Fecha de emisión posterior a la fecha final de la autorización de numeración",                        
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                else
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "DSAB08a",
                        ErrorMessage = "Fecha emision referenciada correctamente",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
            }

            //
            DateTime.TryParse(numberRangeModel.EndDate, out DateTime endDate);
            int.TryParse(endDate.ToString("yyyyMMdd"), out int toDateNumber);
            string errorCodeRange = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice ? "DSAB08b" : "FAB08b";
            if (range.ValidDateNumberTo == toDateNumber)
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = errorCodeRange,
                    ErrorMessage = "Fecha final del rango de numeración informado corresponde a la fecha final de los rangos vigente para el contribuyente.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            else
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = errorCodeRange,
                    ErrorMessage = "Fecha final del rango de numeración informado no corresponde a la fecha final de los rangos vigente para el contribuyente.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });


            if (string.IsNullOrEmpty(range.Serie)) range.Serie = "";
            string errorCodeSerie = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice ? "DSAB10b" : "FAB10b";
            if (range.Serie == numberRangeModel.Serie)
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = errorCodeSerie,
                    ErrorMessage = "El prefijo corresponde al prefijo autorizado en la resolución.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            else
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = errorCodeSerie,
                    ErrorMessage = "El prefijo no corresponde al prefijo de la autorización de numeración",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

            long.TryParse(numberRangeModel.StartNumber, out long fromNumber);
            string errorCodeModel = (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice) ? "DSAB11b" : "FAB11b";

            string messageCodeModel = (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
                ? "Valor inicial del rango de numeración informado no corresponde a un valor inicial de los rangos vigentes para el contribuyente vendedor"
                : "Valor inicial del rango de numeración informado no corresponde a un valor inicial de los rangos vigente para el contribuyente emisor.";

            if (range.FromNumber == fromNumber)
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = errorCodeModel,
                    ErrorMessage = "Valor inicial del rango de numeración informado corresponde a un valor inicial de los rangos vigente para el contribuyente emisor.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            else
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = errorCodeModel,
                    ErrorMessage = messageCodeModel,
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

            long.TryParse(numberRangeModel.EndNumber, out long endNumber);
            string errorCodeModel2 = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice ? "DSAB12b" : "FAB12b";
            string messageCodeModel2 = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice
                ? "Valor final del rango de numeración informado no corresponde a un valor final de los rangos vigentes para el contribuyente vendedor"
                : "Valor final del rango de numeración informado no corresponde a un valor final de los rangos vigentes para el contribuyente emisor.";

            if (range.ToNumber == endNumber)
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = errorCodeModel2,
                    ErrorMessage = "Valor final del rango de numeración informado corresponde a un valor final de los rangos vigentes para el contribuyente emisor.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            else
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = errorCodeModel2,
                    ErrorMessage = messageCodeModel2,
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });


            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion

        #region Sinature validations
        public List<ValidateListResponse> ValidateSign(IEnumerable<X509Certificate> chainCertificates, IEnumerable<X509Crl> crls)
        {
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();

            if (chainCertificates == null)
                throw new ArgumentNullException(nameof(chainCertificates));

            var certificate = GetPrimaryCertificate();

            if (certificate == null)
            {
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD06", ErrorMessage = "Cadena de confianza del certificado digital no se pudo validar. [Missing X509Certificate node]", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                return validateResponses;
            }

            if (DateTime.Now < certificate.NotBefore)
            {
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD07", ErrorMessage = $"Certificado aún no se encuentra vigente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                return validateResponses;
            }

            if (DateTime.Now > certificate.NotAfter)
            {
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD07", ErrorMessage = $"Certificado se encuentra expirado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                return validateResponses;
            }

            bool.TryParse(Environment.GetEnvironmentVariable("RejectUntrustedCertificate"), out bool rejectUntrustedCertificate);

            if (certificate.IsTrusted(chainCertificates))
                validateResponses.Add(new ValidateListResponse { IsValid = true, Mandatory = rejectUntrustedCertificate, ErrorCode = "ZD05", ErrorMessage = "Cadena de confianza del certificado digital correcta.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = rejectUntrustedCertificate, ErrorCode = "ZD05", ErrorMessage = ConfigurationManager.GetValue("UnTrustedCertificateMessage"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            if (certificate.IsRevoked(crls))
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD04", ErrorMessage = "Certificado de la firma revocado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                validateResponses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "ZD04", ErrorMessage = "Certificado de la firma se encuentra vigente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });


            return validateResponses;
        }
        #endregion

        #region ValidateSignXades
        public List<ValidateListResponse> ValidateSignXades()
        {
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();
            var xadesVerifyDsl = new XadesVerify();
            var results = xadesVerifyDsl.PerformAndGetResults(_xmlDocument);
            validateResponses.AddRange(results.Select(r => new ValidateListResponse { IsValid = r.IsValid, Mandatory = true, ErrorCode = "ZE02", ErrorMessage = r.Message, ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds }));
            return validateResponses;
        }
        #endregion

        #region Software validation
        public ValidateListResponse ValidateSoftware(SoftwareModel softwareModel, string trackId, NominaModel nominaModel = null)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorMessage = $"Huella no corresponde a un software autorizado para este OFE." };
            response.ErrorCode = "FAB27b";
            if (documentMeta.DocumentTypeId == "91")
                response.ErrorCode = "CAB27b";
            if (documentMeta.DocumentTypeId == "92")
                response.ErrorCode = "DAB27b";
            if (documentMeta.DocumentTypeId == "96")
                response.ErrorCode = "AAB27b";
            if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll)
                response.ErrorCode = "NIE020";
            if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                response.ErrorCode = nominaModel.TipoNota == "2" ? "NIAE233" : "NIAE020";
            if ((Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice))
                response.ErrorCode = "DSAB27b";

            if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                || Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                response.ErrorMessage = "Se debe indicar el Software Security Code según la definición establecida.";


            var number = (softwareModel != null) ? softwareModel.SerieAndNumber : nominaModel.SerieAndNumber;
            var softwareId = (softwareModel != null) ? softwareModel.SoftwareId : nominaModel.ProveedorSoftwareID;
            var SoftwareSecurityCode = (softwareModel != null) ? softwareModel.SoftwareSecurityCode : nominaModel.ProveedorSoftwareSC;

            var billerSoftwareId = ConfigurationManager.GetValue("BillerSoftwareId");
            var billerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin");

            string hash = "";
            if (softwareId == billerSoftwareId)
                hash = $"{billerSoftwareId}{billerSoftwarePin}{number}".EncryptSHA384();
            else if (!string.IsNullOrWhiteSpace(softwareId))
            {
                var software = GetSoftwareInstanceCache(softwareId);
                if (software == null)
                {
                    response.ErrorCode = "FAB24b";
                    response.ErrorMessage = "El identificador del software asignado cuando el software se activa en el Sistema de Facturación Electrónica no corresponde a un software autorizado para este OFE.";
                    if (documentMeta.DocumentTypeId == "91")
                        response.ErrorCode = "CAB24b";
                    if (documentMeta.DocumentTypeId == "92")
                        response.ErrorCode = "DAB24b";
                    if (documentMeta.DocumentTypeId == "96")
                        response.ErrorCode = "AAB24b";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll)
                        response.ErrorCode = "NIE019";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                        response.ErrorCode = nominaModel.TipoNota == "2" ? "NIAE232" : "NIAE019";
                    if ((Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice))
                        response.ErrorCode = "DSAB24b";

                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                         || Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                        response.ErrorMessage = "Identificador del software asignado cuando el software se activa en el Sistema de Documento Soporte de Pago de Nómina Electrónica, debe corresponder a un software autorizado para este Emisor";

                    response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    return response;
                }
                else if (software.StatusId == (int)SoftwareStatus.Inactive)
                {
                    response.ErrorCode = "FAB24c";
                    response.ErrorMessage = "Identificador del software informado se encuentra inactivo.";
                    if (documentMeta.DocumentTypeId == "91")
                        response.ErrorCode = "CAB24c";
                    if (documentMeta.DocumentTypeId == "92")
                        response.ErrorCode = "DAB24c";
                    if (documentMeta.DocumentTypeId == "96")
                        response.ErrorCode = "AAB24c";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll)
                        response.ErrorCode = "NIE019";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                        response.ErrorCode = nominaModel.TipoNota == "2" ? "NIAE232" : "NIAE019";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
                        response.ErrorCode = "DSAB24c";

                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                        || Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                        response.ErrorMessage = "Identificador del software asignado cuando el software se activa en el Sistema de Documento Soporte de Pago de Nómina Electrónica, debe corresponder a un software autorizado para este Emisor";

                    response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    return response;
                }

                if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                      || Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                    hash = $"{software.PartitionKey}{software.Pin}{number}".EncryptSHA384();
                else
                    hash = $"{software.Id}{software.Pin}{number}".EncryptSHA384();
           
            }

            if (SoftwareSecurityCode.ToLower() == hash)
            {
                response.IsValid = true;
                response.ErrorMessage = "Huella del software correcta.";
            }

            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion

        #region Tax level code validation
        public List<ValidateListResponse> ValidateTaxLevelCodes(Dictionary<string, string> dictionary)
        {
            DateTime startDate = DateTime.UtcNow;
            var responses = new List<ValidateListResponse>();

            var typeListInstance = GetTypeListInstanceCache();
            var typeListvalues = typeListInstance.Value.Split(';');

            //Get xpath values
            var xpathValues = GetXpathValues(dictionary);

            //Tipo documento
            var typeDocument = xpathValues["InvoiceTypeCode"];

            if (Convert.ToInt32(typeDocument) == (int)DocumentType.ImportDocumentInvoice)
            {
                //receiver tax level code validation
                var isValid = true;
                var senderTaxLevelCodes = xpathValues["ReceiverTaxLevelCodes"].Split(';');
                foreach (var code in senderTaxLevelCodes)
                    if (!typeListvalues.Contains(code))
                    {
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "DIAK26", ErrorMessage = "Responsabilidad informada para el importador no válido según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        isValid = false;
                        break;
                    }
                if (isValid && senderTaxLevelCodes.Any())
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "DIAK26", ErrorMessage = "Responsabilidad informada para el importador válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            }
            else if(Convert.ToInt32(typeDocument) == (int)DocumentType.DocumentSupportInvoice)
            {
                //Sender tax level code validation
                var isValid = true;
                var senderTaxLevelCodes = xpathValues["SenderTaxLevelCodes"].Split(';');
                foreach (var code in senderTaxLevelCodes)
                    if (!typeListvalues.Contains(code))
                    {
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = false, ErrorCode = "DSAJ26", ErrorMessage = "Responsabilidad informada por vendedor no válido según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        isValid = false;
                        break;
                    }
                if (isValid && senderTaxLevelCodes.Any())
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "DSAJ26", ErrorMessage = "Responsabilidad informada por vendedor válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });


                //receiver tax level code validation
                isValid = true;
                var receiverTaxLevelCodes = xpathValues["ReceiverTaxLevelCodes"].Split(';');
                foreach (var code in receiverTaxLevelCodes)
                    if (!typeListvalues.Contains(code))
                    {
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "DSAK26", ErrorMessage = "Responsabilidad informada para receptor no válido según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        isValid = false;
                        break;
                    }
                if (isValid && receiverTaxLevelCodes.Any())
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "DSAK26", ErrorMessage = "Responsabilidad informada para receptor válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });


                //delivery tax level code validation
                isValid = true;
                var deliveryTaxLevelCodes = xpathValues["DeliveryTaxLevelCodes"].Split(';');
                foreach (var code in deliveryTaxLevelCodes)
                    if (!string.IsNullOrEmpty(code) && !typeListvalues.Contains(code))
                    {
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "DSAM37", ErrorMessage = "Responsabilidad informada para transportador no válido según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        isValid = false;
                        break;
                    }
                if (isValid && deliveryTaxLevelCodes.Any(d => !string.IsNullOrEmpty(d)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "DSAM37", ErrorMessage = "Responsabilidad informada para transportador válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                isValid = true;
                //sheldHolderTaxLevelCodeItems sender
                var sheldHolderTaxLevelCodeItems = xpathValues["PartyTaxSchemeTaxLevelCodes"].Split('|');
                foreach (var item in sheldHolderTaxLevelCodeItems)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var sheldHolderTaxLevelCodes = item.Split(';');
                        foreach (var code in sheldHolderTaxLevelCodes)
                            if (!typeListvalues.Contains(code))
                            {
                                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = false, ErrorCode = "DSAJ62", ErrorMessage = "Responsabilidad informada por participantes no válido según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                                isValid = false;
                                break;
                            }
                    }
                }
                   
                if (isValid && sheldHolderTaxLevelCodeItems.Any(s => !string.IsNullOrEmpty(s)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = false, ErrorCode = "DSAJ62", ErrorMessage = "Responsabilidad informada por participantes válido según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }
            else
            {
                //Sender tax level code validation
                var isValid = true;
                var senderTaxLevelCodes = xpathValues["SenderTaxLevelCodes"].Split(';');
                foreach (var code in senderTaxLevelCodes)
                    if (!typeListvalues.Contains(code))
                    {
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ26", ErrorMessage = "Responsabilidad informada por emisor no valida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        isValid = false;
                        break;
                    }
                if (isValid && senderTaxLevelCodes.Any())
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ26", ErrorMessage = "Responsabilidad informada por emisor válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                //receiver tax level code validation
                isValid = true;
                var additionalAccountId = xpathValues["AdditionalAccountIds"];
                var receiverTaxLevelCodes = xpathValues["ReceiverTaxLevelCodes"].Split(';');
                foreach (var code in receiverTaxLevelCodes)
                    if (!string.IsNullOrEmpty(code) && !typeListvalues.Contains(code))
                    {
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAK26", ErrorMessage = "Responsabilidad informada para receptor no valida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        isValid = false;
                        break;
                    }
                if (isValid && receiverTaxLevelCodes.Any(r => !string.IsNullOrEmpty(r)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAK26", ErrorMessage = "Responsabilidad informada para receptor válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                //delivery tax level code validation
                isValid = true;
                var deliveryTaxLevelCodes = xpathValues["DeliveryTaxLevelCodes"].Split(';');
                foreach (var code in deliveryTaxLevelCodes)
                    if (!string.IsNullOrEmpty(code) && !typeListvalues.Contains(code))
                    {
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = false, ErrorCode = "FAM37", ErrorMessage = "Responsabilidad informada para transportista no válido según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        isValid = false;
                        break;
                    }
                if (isValid && deliveryTaxLevelCodes.Any(d => !string.IsNullOrEmpty(d)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = false, ErrorCode = "FAM37", ErrorMessage = "Responsabilidad informada para transportista válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                isValid = true;
                var sheldHolderTaxLevelCodeItems = xpathValues["SheldHolderTaxLevelCodes"].Split('|');
                foreach (var item in sheldHolderTaxLevelCodeItems)
                    if (!string.IsNullOrEmpty(item))
                    {
                        var sheldHolderTaxLevelCodes = item.Split(';');
                        foreach (var code in sheldHolderTaxLevelCodes)
                            if (!typeListvalues.Contains(code))
                            {
                                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = false, ErrorCode = "FAJ62", ErrorMessage = "Responsabilidad informada por participantes no válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                                isValid = false;
                                break;
                            }
                    }
                if (isValid && sheldHolderTaxLevelCodeItems.Any(s => !string.IsNullOrEmpty(s)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = false, ErrorCode = "FAJ62", ErrorMessage = "Responsabilidad informada por participantes válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }


            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion

        #region Private methods
        private GlobalContributor GetContributorInstanceCache(string code)
        {
            var contributorInstanceCacheTimePolicyInMinutes = !String.IsNullOrEmpty(ConfigurationManager.GetValue("ContributorInstanceCacheTimePolicyInMinutes")) ? Int32.Parse(ConfigurationManager.GetValue("ContributorInstanceCacheTimePolicyInMinutes")) : CacheTimePolicy24HoursInMinutes;
            var itemKey = $"contributor-{code}";
            GlobalContributor contributor = null;
            var cacheItem = InstanceCache.ContributorInstanceCache.GetCacheItem(itemKey);

            if (cacheItem == null)
            {
                contributor = contributorTableManager.Find<GlobalContributor>(code, code);
                if (contributor == null) return null;
                CacheItemPolicy policy = new CacheItemPolicy
                {                    
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(contributorInstanceCacheTimePolicyInMinutes)
                };
                InstanceCache.ContributorInstanceCache.Set(new CacheItem(itemKey, contributor), policy);
            }
            else
                contributor = (GlobalContributor)cacheItem.Value;

            return contributor;
        }
        #endregion

        private List<GlobalNumberRange> GetNumberRangeInstanceCache(string senderCode)
        {
            var env = ConfigurationManager.GetValue("Environment");
            var numberRangesInstanceCacheTimePolicyInMinutes = !String.IsNullOrEmpty(ConfigurationManager.GetValue("NumberRangesInstanceCacheTimePolicyInMinutes")) ? Int32.Parse(ConfigurationManager.GetValue("NumberRangesInstanceCacheTimePolicyInMinutes")) : CacheTimePolicy1HourInMinutes;
            List<GlobalNumberRange> numberRanges = new List<GlobalNumberRange>();
            var cacheItemKey = $"number-range-{senderCode}";
            if (env == "Hab")
                cacheItemKey = "SET";
            var cacheItem = InstanceCache.NumberRangesInstanceCache.GetCacheItem(cacheItemKey);
            if (cacheItem == null)
            {
                if (env == "Hab")
                    numberRanges = numberRangeTableManager.FindByPartition<GlobalNumberRange>("SET");

                if (env == "Prod" || env == "Test")
                    numberRanges = numberRangeTableManager.FindByPartition<GlobalNumberRange>(senderCode);
                CacheItemPolicy policy = new CacheItemPolicy
                {                    
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(numberRangesInstanceCacheTimePolicyInMinutes)
                };
                InstanceCache.NumberRangesInstanceCache.Set(new CacheItem(cacheItemKey, numberRanges), policy);
            }
            else
                numberRanges = (List<GlobalNumberRange>)cacheItem.Value;

            if (env == "Prod")
            {
                if (!numberRanges.Any(r => r.State == (long)NumberRangeState.Authorized))
                    return numberRanges;

                numberRanges = numberRanges.Where(n => n.State == (long)NumberRangeState.Authorized).ToList();
            }

            return numberRanges;
        }
        private GlobalSoftware GetSoftwareInstanceCache(string id)
        {
            var itemKey = id;
            GlobalSoftware software = null;
            var softwareInstanceCacheTimePolicyInMinutes = !String.IsNullOrEmpty(ConfigurationManager.GetValue("SoftwareInstanceCacheTimePolicyInMinutes")) ? Int32.Parse(ConfigurationManager.GetValue("SoftwareInstanceCacheTimePolicyInMinutes")) : CacheTimePolicy24HoursInMinutes;
            var cacheItem = InstanceCache.SoftwareInstanceCache.GetCacheItem(itemKey);
            if (cacheItem == null)
            {
                software = softwareTableManager.Find<GlobalSoftware>(itemKey, itemKey);
                if (software == null) return null;
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(softwareInstanceCacheTimePolicyInMinutes)
                };
                InstanceCache.SoftwareInstanceCache.Set(new CacheItem(itemKey, software), policy);
            }
            else
                software = (GlobalSoftware)cacheItem.Value;

            return software;
        }
        private GlobalTypeList GetTypeListInstanceCache()
        {
            GlobalTypeList typeList = null;
            List<GlobalTypeList> typesList;
            var typesListInstanceCacheTimePolicyInMinutes = !String.IsNullOrEmpty(ConfigurationManager.GetValue("TypesListInstanceCacheTimePolicyInMinutes")) ? Int32.Parse(ConfigurationManager.GetValue("TypesListInstanceCacheTimePolicyInMinutes")) : CacheTimePolicy24HoursInMinutes;
            var cacheItem = InstanceCache.TypesListInstanceCache.GetCacheItem("TypesList");
            if (cacheItem == null)
            {
                typesList = typeListTableManager.FindByPartition<GlobalTypeList>("new-dian-ubl21");
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(typesListInstanceCacheTimePolicyInMinutes)
                };
                InstanceCache.TypesListInstanceCache.Set(new CacheItem("TypesList", typesList), policy);
            }
            else
                typesList = (List<GlobalTypeList>)cacheItem.Value;

            typeList = typesList.FirstOrDefault(t => t.Name == "Tipo Responsabilidad");
            return typeList;

        }
        private X509Certificate GetPrimaryCertificate()
        {
            var x509CertificateString = _xmlDocument.GetElementsByTagName("ds:X509Certificate")[0] != null ? _xmlDocument.GetElementsByTagName("ds:X509Certificate")[0].InnerText : string.Empty;

            var primaryCertificate = new X509Certificate2(Convert.FromBase64String(x509CertificateString));
            return DotNetUtilities.FromX509Certificate(primaryCertificate);
        }
        private Dictionary<string, string> GetXpathValues(Dictionary<string, string> dictionary)
        {
            var newDictionary = new Dictionary<string, string>();
            foreach (var item in dictionary)
            {
                try
                {
                    if (_xmlDocument.SelectNodes(item.Value, _ns).Count == 1)
                        newDictionary[item.Key] = _xmlDocument.SelectSingleNode(item.Value, _ns)?.InnerText;
                    else
                    {
                        var values = string.Empty;
                        var nodes = _xmlDocument.SelectNodes(item.Value, _ns);
                        foreach (XmlNode node in nodes)
                        {
                            if (string.IsNullOrEmpty(values))
                                values = node.InnerText;
                            else
                                values = $"{values}|{node.InnerText}";
                        }
                        newDictionary[item.Key] = values;
                    }
                }
                catch (Exception ex)
                {
                    newDictionary[item.Key] = ex.Message;
                }
            }
            return newDictionary;
        }
        private static decimal TruncateDecimal(decimal value, int preision)
        {
            decimal step = (decimal)Math.Pow(10, preision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }

        #region ValidateDigitCode
        public bool ValidateDigitCode(string code, int digit)
        {
            try
            {
                int[] cousins = new int[] { 0, 3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71 };
                int dv, actualCousin, _totalOperacion = 0, residue, totalDigits = code.Length;

                for (int i = 0; i < totalDigits; i++)
                {
                    actualCousin = int.Parse(code.Substring(i, 1));
                    _totalOperacion += actualCousin * cousins[totalDigits - i];
                }
                residue = _totalOperacion % 11;
                if (residue > 1)
                    dv = 11 - residue;
                else
                    dv = residue;

                return dv == digit;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Validación de la Sección DocumentReference - CUFE Informado
        //Validación de la Sección DocumentReference - CUFE Informado TASK 804
        //Validación de la Sección DocumentReference - CUDE  del evento referenciado TASK 729
        public List<ValidateListResponse> ValidateDocumentReferencePrev(string trackId, string idDocumentReference, string eventCode,
            string documentTypeIdRef, string issuerPartyCode = null, string issuerPartyName = null)
        {
            string messageTypeId = (Convert.ToInt32(eventCode) == (int)EventStatus.Mandato)
                ? ConfigurationManager.GetValue("ErrorMessage_AAH09_043")
                : ConfigurationManager.GetValue("ErrorMessage_AAH09");
            string errorCodeReglaUUID = "AAH07";

            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            DateTime startDate = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(trackId))
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = errorCodeReglaUUID,
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            if (Convert.ToInt32(eventCode) == (int)EventStatus.TerminacionMandato)
            {
                //Valida referencia evento terminacion de mandato
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = "100",
                    ErrorMessage = "Evento ValidateDocumentReferencePrev referenciado correctamente",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

                var referenceAttorneyResult = TableManagerGlobalDocReferenceAttorney.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(trackId?.ToLower());
                if (referenceAttorneyResult == null)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAH07",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                else
                {
                    //Valida ID documento Invoice/AR coincida con el CUFE/CUDE referenciado
                    if (referenceAttorneyResult.SerieAndNumber != idDocumentReference)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH06",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH06_043"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    //Valida DocumentTypeCode coincida con el documento informado
                    if ("96" != documentTypeIdRef)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH09",
                            ErrorMessage = "No corresponde al literal '96'",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }
            else
            {
                //Valida exista CUFE/CUDE en sistema DIAN
                var documentMeta = documentMetaTableManager.FindpartitionKey<GlobalDocValidatorDocumentMeta>(trackId?.ToLower()).FirstOrDefault();
                if (documentMeta != null)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = "Evento ValidateDocumentReferencePrev referenciado correctamente",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });

                    //Valida se encuentre aprobado el documento referenciado
                    var approved = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier, documentMeta?.PartitionKey);
                    if(approved == null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeReglaUUID,
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    //Valida ID documento Invoice/AR coincida con el CUFE/CUDE referenciado
                    if (documentMeta.SerieAndNumber != idDocumentReference)
                    {
                        string message = (Convert.ToInt32(eventCode) == (int)EventStatus.Mandato
                            || Convert.ToInt32(eventCode) == (int)EventStatus.TerminacionMandato)
                            ? ConfigurationManager.GetValue("ErrorMessage_AAH06_043")
                            : ConfigurationManager.GetValue("ErrorMessage_AAH06");

                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH06",
                            ErrorMessage = message,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    //Valida DocumentTypeCode coincida con el documento informado
                    if (documentMeta.DocumentTypeId != documentTypeIdRef)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH09",
                            ErrorMessage = messageTypeId,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    if(Convert.ToInt32(eventCode) == (int)EventStatus.InvoiceOfferedForNegotiation 
                        && documentMeta.DocumentTypeId != "96")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH06",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH06"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    if (Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad ||
                       Convert.ToInt32(eventCode) == (int)EventStatus.EndosoGarantia ||
                       Convert.ToInt32(eventCode) == (int)EventStatus.EndosoProcuracion)
                    {
                        //Valida número de identificación informado igual al número del adquiriente en la factura referenciada
                        if (documentMeta.ReceiverCode != issuerPartyCode)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAH26b",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH26b"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        //Valida nombre o razon social informado igual al del adquiriente en la factura referenciada
                        if (documentMeta.ReceiverName != issuerPartyName)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAH25b",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH25b"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }

                        var responseListEndoso = ValidateTransactionCufe(trackId.ToLower());
                        if (responseListEndoso != null)
                        {
                            foreach (var item in responseListEndoso)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = item.IsValid,
                                    Mandatory = item.Mandatory,
                                    ErrorCode = item.ErrorCode,
                                    ErrorMessage = item.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                }
                else
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = errorCodeReglaUUID,
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            return responses;
        }
        #endregion

        #region validation to emition to event
        public List<ValidateListResponse> ValidateEmitionEventPrev(RequestObjectEventPrev eventPrev, XmlParser xmlParserCufe, XmlParser xmlParserCude, NitModel nitModel)
        {
            string eventCode = eventPrev.EventCode;
            string successfulMessage = "Evento ValidateEmitionEventPrev referenciado correctamente";
            DateTime startDate = DateTime.UtcNow;
            GlobalDocValidatorDocument document = null;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            ErrorCodeMessage errorCodeMessage = getErrorCodeMessage(eventCode);

                       
            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = successfulMessage,
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });
           
            //Servicio
            List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(eventPrev.TrackId.ToLower());

            List<GlobalDocValidatorDocumentMeta> documentMeta = (InvoiceWrapper.Any()) ? InvoiceWrapper[0].Documents.Select(x => x.DocumentMeta).ToList() : null;
                       
            //Valida si el documento AR transmitido ya se encuentra aprobado
            switch (Convert.ToInt32(eventPrev.EventCode))
            {
                case (int)EventStatus.Rejected:
                    //Valida eventos previos Rechazo de la FEV
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianRejected = new LogicalEventRadian();
                        var eventRadianRejected = logicalEventRadianRejected.ValidateRejectedEventPrev(documentMeta);
                        if (eventRadianRejected != null)
                        {
                            foreach (var itemEventRadianRejected in eventRadianRejected)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianRejected.IsValid,
                                    Mandatory = itemEventRadianRejected.Mandatory,
                                    ErrorCode = itemEventRadianRejected.ErrorCode,
                                    ErrorMessage = itemEventRadianRejected.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC03",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC03"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    
                    break;
                case (int)EventStatus.Receipt:
                    //Valida eventos previos Constancia de recibo del Bien de la FEV
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianReceipt = new LogicalEventRadian();
                        var eventRadianReceipt = logicalEventRadianReceipt.ValidateReceiptEventPrev(documentMeta);
                        if (eventRadianReceipt != null)
                        {
                            foreach (var itemeventRadianReceipt in eventRadianReceipt)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemeventRadianReceipt.IsValid,
                                    Mandatory = itemeventRadianReceipt.Mandatory,
                                    ErrorCode = itemeventRadianReceipt.ErrorCode,
                                    ErrorMessage = itemeventRadianReceipt.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC09",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC09"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    
                    break;
                case (int)EventStatus.Accepted:
                    //Valida eventos previos Aceptacion expresa de la FEV
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianAccepted = new LogicalEventRadian();
                        var eventRadianAccepted = logicalEventRadianAccepted.ValidateAcceptedEventPrev(documentMeta);
                        if (eventRadianAccepted != null)
                        {
                            foreach (var itemeventRadianAccepted in eventRadianAccepted)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemeventRadianAccepted.IsValid,
                                    Mandatory = itemeventRadianAccepted.Mandatory,
                                    ErrorCode = itemeventRadianAccepted.ErrorCode,
                                    ErrorMessage = itemeventRadianAccepted.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    else
                    {

                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC12",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC12"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });

                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC13",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC13"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    break;
                case (int)EventStatus.AceptacionTacita:
                    //Valida eventos previos Aceptacion Tacita de la FEV
                    if (documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianTacitAcceptance = new LogicalEventRadian();
                        var eventRadianTacitAcceptance = logicalEventRadianTacitAcceptance.ValidateTacitAcceptanceEventPrev(documentMeta);
                        if (eventRadianTacitAcceptance != null)
                        {
                            foreach (var itemEventRadianTacitAcceptance in eventRadianTacitAcceptance)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianTacitAcceptance.IsValid,
                                    Mandatory = itemEventRadianTacitAcceptance.Mandatory,
                                    ErrorCode = itemEventRadianTacitAcceptance.ErrorCode,
                                    ErrorMessage = itemEventRadianTacitAcceptance.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC14",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC14"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    break;
                case (int)EventStatus.Avales:
                    //Valida eventos previos Aval
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianAval = new LogicalEventRadian();
                        var eventRadianAval = logicalEventRadianAval.ValidateEndorsementEventPrev(documentMeta, xmlParserCufe, xmlParserCude);
                        if (eventRadianAval != null)
                        {
                            foreach (var itemEventRadianAval in eventRadianAval)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianAval.IsValid,
                                    Mandatory = itemEventRadianAval.Mandatory,
                                    ErrorCode = itemEventRadianAval.ErrorCode,
                                    ErrorMessage = itemEventRadianAval.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    
                    break;
                case (int)EventStatus.SolicitudDisponibilizacion:
                    //Valida eventos previos Solicitud Disponibilizacion
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianDisponibilizacion = new LogicalEventRadian();
                        var eventRadianDisponibilizacion = logicalEventRadianDisponibilizacion.ValidateAvailabilityRequestEventPrev(documentMeta, xmlParserCufe, xmlParserCude, nitModel);
                        if (eventRadianDisponibilizacion != null)
                        {
                            foreach (var itemEventRadianDisponibilizacion in eventRadianDisponibilizacion)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianDisponibilizacion.IsValid,
                                    Mandatory = itemEventRadianDisponibilizacion.Mandatory,
                                    ErrorCode = itemEventRadianDisponibilizacion.ErrorCode,
                                    ErrorMessage = itemEventRadianDisponibilizacion.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    
                    break;
                case (int)EventStatus.EndosoPropiedad:
                    //Valida eventos previos Endoso en Propiedad
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianEndosoPropiedad = new LogicalEventRadian();
                        var eventRadianEndosoPropiedad = logicalEventRadianEndosoPropiedad.ValidatePropertyEndorsement(documentMeta, eventPrev, xmlParserCufe, xmlParserCude, nitModel);
                        if (eventRadianEndosoPropiedad != null)
                        {
                            foreach (var itemEventRadianEndosoPropiedad in eventRadianEndosoPropiedad)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianEndosoPropiedad.IsValid,
                                    Mandatory = itemEventRadianEndosoPropiedad.Mandatory,
                                    ErrorCode = itemEventRadianEndosoPropiedad.ErrorCode,
                                    ErrorMessage = itemEventRadianEndosoPropiedad.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    
                    break;
                case (int)EventStatus.EndosoGarantia:
                    //Valida eventos previos Endoso en Garantia
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianEndosoGarantia = new LogicalEventRadian();
                        var eventRadianEndosoGarantia = logicalEventRadianEndosoGarantia.ValidateEndorsementGatantia(documentMeta, eventPrev, xmlParserCufe, xmlParserCude, nitModel);
                        if (eventRadianEndosoGarantia != null)
                        {
                            foreach (var itemEventRadianEndosoGarantia in eventRadianEndosoGarantia)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianEndosoGarantia.IsValid,
                                    Mandatory = itemEventRadianEndosoGarantia.Mandatory,
                                    ErrorCode = itemEventRadianEndosoGarantia.ErrorCode,
                                    ErrorMessage = itemEventRadianEndosoGarantia.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                   
                    break;
                case (int)EventStatus.EndosoProcuracion:
                    //Valida eventos previos Endoso en Procuracion
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianEndosoProcuracion = new LogicalEventRadian();
                        var eventRadianEndosoProcuracion = logicalEventRadianEndosoProcuracion.ValidateEndorsementProcurement(documentMeta, eventPrev, xmlParserCufe, xmlParserCude, nitModel);
                        if (eventRadianEndosoProcuracion != null)
                        {
                            foreach (var itemEventRadianEndosoProcuracion in eventRadianEndosoProcuracion)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianEndosoProcuracion.IsValid,
                                    Mandatory = itemEventRadianEndosoProcuracion.Mandatory,
                                    ErrorCode = itemEventRadianEndosoProcuracion.ErrorCode,
                                    ErrorMessage = itemEventRadianEndosoProcuracion.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                   
                    break;
                case (int)EventStatus.InvoiceOfferedForNegotiation:
                    //Valida eventos previos Cancelacion Endosos
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianCancelaEndoso = new LogicalEventRadian();
                        var eventRadianCancelaEndoso = logicalEventRadianCancelaEndoso.ValidateEndorsementCancell(documentMeta, eventPrev, xmlParserCude);
                        if (eventRadianCancelaEndoso != null)
                        {
                            foreach (var itemEventRadianCancelaEndoso in eventRadianCancelaEndoso)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianCancelaEndoso.IsValid,
                                    Mandatory = itemEventRadianCancelaEndoso.Mandatory,
                                    ErrorCode = itemEventRadianCancelaEndoso.ErrorCode,
                                    ErrorMessage = itemEventRadianCancelaEndoso.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                   
                    break;
                case (int)EventStatus.NegotiatedInvoice:
                    //Valida eventos previos Limitacion de Circulacion
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianNegotiatedInvoice = new LogicalEventRadian();
                        var eventRadianNegotiatedInvoice = logicalEventRadianNegotiatedInvoice.ValidateNegotiatedInvoice(documentMeta);
                        if (eventRadianNegotiatedInvoice != null)
                        {
                            foreach (var itemEventRadianNegotiatedInvoice in eventRadianNegotiatedInvoice)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianNegotiatedInvoice.IsValid,
                                    Mandatory = itemEventRadianNegotiatedInvoice.Mandatory,
                                    ErrorCode = itemEventRadianNegotiatedInvoice.ErrorCode,
                                    ErrorMessage = itemEventRadianNegotiatedInvoice.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                   
                    break;
                case (int)EventStatus.AnulacionLimitacionCirculacion:
                    //Valida eventos previos Anulacion de la Limitacion de Circulacion
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianNegotiatedInvoiceCancell = new LogicalEventRadian();
                        var eventRadianNegotiatedInvoiceCancell = logicalEventRadianNegotiatedInvoiceCancell.ValidateNegotiatedInvoiceCancell(documentMeta);
                        if (eventRadianNegotiatedInvoiceCancell != null)
                        {
                            foreach (var itemEventRadianNegotiatedInvoiceCancell in eventRadianNegotiatedInvoiceCancell)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianNegotiatedInvoiceCancell.IsValid,
                                    Mandatory = itemEventRadianNegotiatedInvoiceCancell.Mandatory,
                                    ErrorCode = itemEventRadianNegotiatedInvoiceCancell.ErrorCode,
                                    ErrorMessage = itemEventRadianNegotiatedInvoiceCancell.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                   
                    break;
                case (int)EventStatus.Mandato:
                    //Valida eventos previos Mandato / El mandato no cuenta con eventos previos
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = successfulMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    break;
                case (int)EventStatus.TerminacionMandato:
                    //Valida eventos previos Terminacion de Mandato
                    LogicalEventRadian logicalEventRadianMandatoCancell = new LogicalEventRadian();
                    var eventRadianMandatoCancell = logicalEventRadianMandatoCancell.ValidateMandatoCancell(eventPrev);
                    if (eventRadianMandatoCancell != null)
                    {
                        foreach (var itemEventRadianMandatoCancell in eventRadianMandatoCancell)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianMandatoCancell.IsValid,
                                Mandatory = itemEventRadianMandatoCancell.Mandatory,
                                ErrorCode = itemEventRadianMandatoCancell.ErrorCode,
                                ErrorMessage = itemEventRadianMandatoCancell.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.NotificacionPagoTotalParcial:
                    //Valida eventos previos Pago parcial o Total
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianPartialPayment = new LogicalEventRadian();
                        var eventRadianPartialPayment = logicalEventRadianPartialPayment.ValidatePartialPayment(documentMeta, eventPrev, xmlParserCude, nitModel);
                        if (eventRadianPartialPayment != null)
                        {
                            foreach (var itemEventRadianPartialPayment in eventRadianPartialPayment)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianPartialPayment.IsValid,
                                    Mandatory = itemEventRadianPartialPayment.Mandatory,
                                    ErrorCode = itemEventRadianPartialPayment.ErrorCode,
                                    ErrorMessage = itemEventRadianPartialPayment.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    
                    break;
                case (int)EventStatus.ValInfoPago:
                    //Valida eventos previos Informacion Pago
                    if(documentMeta != null)
                    {
                        LogicalEventRadian logicalEventRadianPaymentInfo = new LogicalEventRadian();
                        var eventRadianPaymentInfo = logicalEventRadianPaymentInfo.ValidatePaymetInfo(documentMeta);
                        if (eventRadianPaymentInfo != null)
                        {
                            foreach (var itemEventRadianPaymentInfo in eventRadianPaymentInfo)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = itemEventRadianPaymentInfo.IsValid,
                                    Mandatory = itemEventRadianPaymentInfo.Mandatory,
                                    ErrorCode = itemEventRadianPaymentInfo.ErrorCode,
                                    ErrorMessage = itemEventRadianPaymentInfo.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }                   
                    break;

            }

            if(documentMeta != null)
            {
                foreach (var documentIdentifier in documentMeta)
                {
                    document = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(documentIdentifier.Identifier, documentIdentifier.Identifier, documentIdentifier.PartitionKey);

                    if (documentMeta.Count >= 2)
                    {
                        //Valida Evento registrado previamente para Fase I y Solicitud de primera disponibilizacion
                        if ((Convert.ToInt32(eventPrev.EventCode) >= 30 && Convert.ToInt32(eventPrev.EventCode) <= 34))
                        {
                            if (documentMeta.Any(t => t.EventCode == eventPrev.EventCode
                            && document != null && t.Identifier == document?.PartitionKey && string.IsNullOrEmpty(t.TestSetId)
                            ))
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "LGC01",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC01"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }                    
                }
            }
           
            return responses;
        }

        private ValidateListResponse ValidateAval(XmlParser xmlParserCufe, XmlParser xmlParserCude)
        {
            DateTime startDate = DateTime.UtcNow;
            string valueTotalInvoice = xmlParserCufe.TotalInvoice;

            XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
            int totalValueSender = 0;
            for (int i = 0; i < valueListSender.Count; i++)
            {
                string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                // Si no se reporta, el Avalista asume el valor del monto de quien respalda...
                if (string.IsNullOrWhiteSpace(valueStockAmount)) return null;

                totalValueSender += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

                // Si se reporta, pero en ceros (0), el Avalista asume el valor del monto de quien respalda...
                if (totalValueSender == 0) return null;
            }

            if (totalValueSender > Int32.Parse(valueTotalInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
            {
                return new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAH32c",
                    ErrorMessage = $"{(string)null} El valor reportado no es igual a la sumatoria del elemento SenderParty:CorporateStockAmount - IssuerParty:PartyLegalEntity:CorporateStockAmount",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                };
            }

            XmlNodeList valueListIssuerParty = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PartyLegalEntity']");
            // En caso de no indicarlo quedan garantizadas las obligaciones de todas las partes del título.
            if (valueListIssuerParty.Count <= 0) return null;

            int totalValueIssuerParty = 0;
            for (int i = 0; i < valueListIssuerParty.Count; i++)
            {
                string valueStockAmount = valueListIssuerParty.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                if (!string.IsNullOrWhiteSpace(valueStockAmount)) totalValueIssuerParty += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }

            if (totalValueIssuerParty == 0) return null;

            if (totalValueIssuerParty != totalValueSender)
            {
                return new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAH32c",
                    ErrorMessage = $"{(string)null} El valor reportado no es igual a la sumatoria del elemento SenderParty:CorporateStockAmount - IssuerParty:PartyLegalEntity:CorporateStockAmount",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                };
            }

            return null;
        }
        #endregion

        #region Validación de la Sección prerrequisitos Solicitud Disponibilizacion
        public List<ValidateListResponse> EventApproveCufe(NitModel dataModel, RequestObjectEventApproveCufe data)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();          

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento EventApproveCufe referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            List<InvoiceWrapper> invoiceWrapper = associateDocumentService.GetEventsByTrackId(data.TrackId.ToLower());
            if (invoiceWrapper.Any() && !invoiceWrapper[0].Invoice.IsInvoiceTV)
            {               
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC21",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC21"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

                return responses;
            }
                   
            return responses;
        }
        #endregion

        #region ValidateSigningTime
        public List<ValidateListResponse> ValidateSigningTime(RequestObjectSigningTime data, XmlParser dataModel, NitModel nitModel, string paymentDueDateFE = null,
            DateTime? signingTimeAvailability = null)
        {
            List<ValidateListResponse> responses = new List<ValidateListResponse>();            
            bool.TryParse(Environment.GetEnvironmentVariable("ValidateManadatory"), out bool ValidateManadatory);

            int businessDays = 0;
            DateTime startDate = DateTime.UtcNow;
            DateTime dateNow = DateTime.UtcNow.Date;
            DateTime signingTimeEvent = Convert.ToDateTime(data.SigningTime).Date;
            string errorMessageSign = "Evento ValidateSigningTime referenciado correctamente";

            if (signingTimeEvent > dateNow)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = ValidateManadatory,
                    ErrorCode = "DC24",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24"),
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            ErrorCodeMessage errorCodeMessage = getErrorCodeMessage(data.EventCode);
            string errorCodeRef = data.EventCode == "030" ? errorCodeMessage.errorCodeSigningTimeAcuse : errorCodeMessage.errorCodeSigningTimeRecibo;
            string errorMesaageRef = data.EventCode == "030" ? errorCodeMessage.errorMessageigningTimeAcuse : errorCodeMessage.errorMessageigningTimeRecibo;

            if (data.EventCode == "043")
            {
                errorCodeRef = "DC24r";
                errorMesaageRef = ConfigurationManager.GetValue("ErrorMessage_DC24r");
            }
            else if (data.EventCode != "030" && data.EventCode != "032")
            {
                errorCodeRef = "DC24q";
                errorMesaageRef = ConfigurationManager.GetValue("ErrorMessage_DC24q");
            }

            switch (int.Parse(data.EventCode))
            {
                case (int)EventStatus.Received:
                case (int)EventStatus.Receipt:
                case (int)EventStatus.InvoiceOfferedForNegotiation:
                case (int)EventStatus.Mandato:
                    DateTime dataSigningTime = Convert.ToDateTime(data.SigningTime);
                    DateTime modelSigningTime = Convert.ToDateTime(dataModel.SigningTime).AddHours(-5);
                    if (dataSigningTime >= modelSigningTime)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeRef,
                            ErrorMessage = errorMessageSign,
                            //ErrorMessage = errorMesaageRef,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case (int)EventStatus.Rejected:
                    businessDays = BusinessDaysHolidays.BusinessDaysUntil(Convert.ToDateTime(dataModel.SigningTime).AddHours(-5), Convert.ToDateTime(data.SigningTime));
                    responses.Add(businessDays > 3
                         ? new ValidateListResponse
                         {
                             IsValid = false,
                             Mandatory = ValidateManadatory,
                             ErrorCode = "DC24z",
                             ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24z"),
                             ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                         }
                        : new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.Accepted:
                    DateTime signingTimeAccepted = Convert.ToDateTime(data.SigningTime);
                    DateTime signingTimeReceipt = Convert.ToDateTime(dataModel.SigningTime).AddHours(-5);
                    businessDays = BusinessDaysHolidays.BusinessDaysUntil(signingTimeReceipt, signingTimeAccepted);
                    responses.Add(businessDays > 3
                        ? new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = ValidateManadatory,
                            ErrorCode = "DC24c",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24c"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.AceptacionTacita:
                    businessDays = BusinessDaysHolidays.BusinessDaysUntil(Convert.ToDateTime(dataModel.SigningTime).AddHours(-5), Convert.ToDateTime(data.SigningTime));
                    responses.Add(businessDays > 3
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = ValidateManadatory,
                            ErrorCode = "DC24e",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24e"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.ValInfoPago:
                    DateTime signingTimeEvento = Convert.ToDateTime(data.SigningTime).Date;
                    DateTime endDatePaymentDueDate = Convert.ToDateTime(data.EndDate).Date;
                    DateTime paymentDueDateFactura = Convert.ToDateTime(dataModel.PaymentDueDate).Date;
                    if (signingTimeEvento > paymentDueDateFactura)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = ValidateManadatory,
                            ErrorCode = "LGC55",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC55"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        if (endDatePaymentDueDate == paymentDueDateFactura)
                        {
                            businessDays = BusinessDaysHolidays.BusinessDaysUntil(signingTimeEvento, endDatePaymentDueDate);
                            if (businessDays == 3)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = true,
                                    Mandatory = true,
                                    ErrorCode = "100",
                                    ErrorMessage = errorMessageSign,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                            else if (businessDays < 3)
                            {
                                responses.Add(new ValidateListResponse
                                {

                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "LGC54",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC54"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                            else if (businessDays > 3)
                            {
                                responses.Add(new ValidateListResponse
                                {

                                    IsValid = false,
                                    Mandatory = ValidateManadatory,
                                    ErrorCode = "LGC56",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC56"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAH59",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH59"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }

                    break;
                case (int)EventStatus.SolicitudDisponibilizacion:
                    responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime).AddHours(-5)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DC24h",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24h"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });

                    // El evento debe incluir la fecha de Vencimiento de la Factura electrónica de Venta 
                    // (Debe validar el campo EndDate contra el campo  PaymentDueDate de la factura referenciada.)
                    if (!string.IsNullOrWhiteSpace(paymentDueDateFE))
                    {
                        responses.Add(Convert.ToDateTime(data.EndDate) == Convert.ToDateTime(paymentDueDateFE)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH59",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH59"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = "PaymentDueDate llega NULL",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case (int)EventStatus.NegotiatedInvoice:
                case (int)EventStatus.AnulacionLimitacionCirculacion:
                    if (nitModel.CustomizationId == "361" || nitModel.CustomizationId == "362" ||
                       nitModel.CustomizationId == "363" || nitModel.CustomizationId == "364")
                    {
                        responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime).AddHours(-5)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DC24t",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24t"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime).AddHours(-5)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DC24t",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24t"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case (int)EventStatus.Avales:
                    if (nitModel.CustomizationId == "361" || nitModel.CustomizationId == "362")
                    {
                        responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime).AddHours(-5)
                       ? new ValidateListResponse
                       {
                           IsValid = true,
                           Mandatory = true,
                           ErrorCode = "100",
                           ErrorMessage = errorMessageSign,
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       }
                       : new ValidateListResponse
                       {
                           IsValid = false,
                           Mandatory = true,
                           ErrorCode = "DC24g",
                           ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24g"),
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DC24g",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24g"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case (int)EventStatus.NotificacionPagoTotalParcial:
                    responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime).AddHours(-5)
                       ? new ValidateListResponse
                       {
                           IsValid = true,
                           Mandatory = true,
                           ErrorCode = "100",
                           ErrorMessage = errorMessageSign,
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       }
                       : new ValidateListResponse
                       {
                           IsValid = false,
                           Mandatory = true,
                           ErrorCode = "DC24x",
                           ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24x"),
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       });

                    // El evento debe incluir la fecha de Vencimiento de la Factura electrónica de Venta 
                    // (Debe validar el campo EndDate contra el campo  PaymentDueDate de la factura referenciada.)
                    DateTime endDatePaymentDueDateNotifica = Convert.ToDateTime(data.EndDate).Date;
                    DateTime paymentDueDateFacturaNotifica = Convert.ToDateTime(paymentDueDateFE).Date;

                    if (endDatePaymentDueDateNotifica == paymentDueDateFacturaNotifica)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH59",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH59"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    break;
                case (int)EventStatus.EndosoGarantia:
                case (int)EventStatus.EndosoProcuracion:
                case (int)EventStatus.EndosoPropiedad:
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = errorMessageSign,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });

                    DateTime signingTimeEndoso = Convert.ToDateTime(data.SigningTime);
                    DateTime signingTimeFEV = Convert.ToDateTime(dataModel.SigningTime).AddHours(-5);
                    string errorCode = string.Empty;
                    string errorMessage = string.Empty;
                    string errorMessageAvailability;
                    string errorCodeAvailability;
                    if ((int)EventStatus.EndosoPropiedad == Convert.ToInt32(data.EventCode))
                    {
                        errorCode = "DC24j";
                        errorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24j");
                        errorCodeAvailability = "DC24k";
                        errorMessageAvailability = ConfigurationManager.GetValue("ErrorMessage_DC24k");
                    }
                    else if ((int)EventStatus.EndosoGarantia == Convert.ToInt32(data.EventCode))
                    {
                        errorCode = "DC24l";
                        errorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24l");
                        errorCodeAvailability = "DC24m";
                        errorMessageAvailability = ConfigurationManager.GetValue("ErrorMessage_DC24m");
                    }
                    else
                    {
                        errorCode = "DC24o";
                        errorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24o");
                        errorCodeAvailability = "DC24p";
                        errorMessageAvailability = ConfigurationManager.GetValue("ErrorMessage_DC24p");
                    }

                    if (signingTimeEndoso.Date < signingTimeFEV.Date)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCode,
                            ErrorMessage = errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    // validación contra de la fecha de firma de la Disponibilización.
                    if (signingTimeEndoso < signingTimeAvailability.Value)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeAvailability,
                            ErrorMessage = errorMessageAvailability,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    // El evento debe incluir la fecha de Vencimiento de la Factura electrónica de Venta 
                    // (Debe validar el campo EndDate contra el campo  PaymentDueDate de la factura referenciada.)
                    DateTime endDatePaymentDueDateEndoso = Convert.ToDateTime(data.EndDate).Date;
                    DateTime paymentDueDateFacturaEndoso = Convert.ToDateTime(paymentDueDateFE).Date;

                    if (endDatePaymentDueDateEndoso != paymentDueDateFacturaEndoso)                   
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = false,
                            ErrorCode = "AAH59",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH59"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case (int)EventStatus.TerminacionMandato:
                    DateTime signingTime = Convert.ToDateTime(data.SigningTime);
                    //General por tiempo ilimitado_432 - limitado por tiempo ilimitado_434
                    if (nitModel.CustomizationId == "432" || nitModel.CustomizationId == "434") //que se mayor
                    {
                        DateTime dateMandato = Convert.ToDateTime(dataModel.SigningTime).AddHours(-5);
                        if (signingTime >= dateMandato)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageSign,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "DC24s",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24s"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    // General por tiempo limitado_431 - limitado por tiempo limitado_433
                    else if (nitModel.CustomizationId == "431" || nitModel.CustomizationId == "433")  //que sea menor
                    {
                        DateTime endDateMandato = Convert.ToDateTime(nitModel.ValidityPeriodEndDate);
                        if (signingTime <= endDateMandato)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageSign,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "DC24s",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24s"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = "Error en el Instrumento Mandato 043",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
            }

            return responses;
        }
        #endregion

        #region UpdateInTransactions
        public void UpdateInTransactions(string trackId, string eventCode)
        {
            //valida InTransaction eventos Endoso en propeidad, Garantia y procuración
            var arrayTasks = new List<Task>();
            if (Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad
            || Convert.ToInt32(eventCode) == (int)EventStatus.EndosoGarantia
            || Convert.ToInt32(eventCode) == (int)EventStatus.EndosoProcuracion)
            {
                GlobalDocValidatorDocumentMeta validatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                if (validatorDocumentMeta != null)
                {
                    validatorDocumentMeta.InTransaction = false;
                    arrayTasks.Add(documentMetaTableManager.InsertOrUpdateAsync(validatorDocumentMeta));
                }
            }
        }
        #endregion

        #region ValidateTransactionCufe
        private List<ValidateListResponse> ValidateTransactionCufe(string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            List<Task> arrayTasks = new List<Task>();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validTransaction = false;

            GlobalDocValidatorDocumentMeta validatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (validatorDocumentMeta != null)
            {
                if (!validatorDocumentMeta.SendTestSet)
                {
                    if (!validatorDocumentMeta.InTransaction)
                    {
                        validatorDocumentMeta.InTransaction = true;
                        arrayTasks.Add(
                            documentMetaTableManager.InsertOrUpdateAsync(validatorDocumentMeta));
                    }
                    else
                    {
                        validTransaction = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC63",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC63"),                            
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        return responses;
                    }
                }
            }

            if (validTransaction)
                return responses;

            return null;

        }
        #endregion


        #region validation for CBC ID
        public List<ValidateListResponse> ValidateSerieAndNumber(NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            List<ValidateListResponse> listResponses = new List<ValidateListResponse>();
            var documentReference = TableManagerGlobalDocRegisterProviderAR.FindDocumentRegisterAR<GlobalDocRegisterProviderAR>(nitModel.ProviderCode, nitModel.DocumentTypeId, nitModel.SerieAndNumber);

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento ValidateSerieAndNumber referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            if (documentReference.Count() > 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAD05b",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAD05b"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            var responseCheckDocument = CheckDocument(nitModel.SenderCode, nitModel.DocumentTypeId, nitModel.SerieAndNumber);
            if (responseCheckDocument != null)
            {
                foreach (var item in responseCheckDocument)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = item.IsValid,
                        Mandatory = item.Mandatory,
                        ErrorCode = item.ErrorCode,
                        ErrorMessage = item.ErrorMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            listResponses.AddRange(responses);

            return listResponses;
        }
        #endregion

        #region Error Code Message
        private class ErrorCodeMessage
        {
            public string errorCode = string.Empty;
            public string errorMessage = string.Empty;
            public string errorCodeB = string.Empty;
            public string errorMessageB = string.Empty;
            public string errorCodeNoteA = string.Empty;
            public string errorMessageNoteA = string.Empty;
            public string errorCodeNote = string.Empty;
            public string errorMessageNote = string.Empty;
            public string errorCodeFETV { get; set; }
            public string errorMessageFETV { get; set; }
            public string errorCodeReceiverFETV { get; set; }
            public string errorMessageReceiverFETV { get; set; }
            public string errorCodeSigningTimeAcuse { get; set; }
            public string errorMessageigningTimeAcuse { get; set; }
            public string errorCodeSigningTimeRecibo { get; set; }
            public string errorMessageigningTimeRecibo { get; set; }
            public string errorCodeEndoso { get; set; }
            public string errorMessageEndoso { get; set; }
            public string errorCodeMandato { get; set; }
            public string errorMessageMandato { get; set; }

        }

        private ErrorCodeMessage getErrorCodeMessage(string eventCode)
        {
            ErrorCodeMessage response = new ErrorCodeMessage()
            {
                errorCodeB = string.Empty,
                errorMessageB = string.Empty,
                errorCode = string.Empty,
                errorMessage = string.Empty,
                errorCodeNoteA = string.Empty,
                errorMessageNoteA = string.Empty,
                errorCodeNote = string.Empty,
                errorMessageNote = string.Empty,
                errorCodeFETV = string.Empty,
                errorMessageFETV = string.Empty,
                errorCodeReceiverFETV = string.Empty,
                errorMessageReceiverFETV = string.Empty,
                errorCodeSigningTimeAcuse = string.Empty,
                errorMessageigningTimeAcuse = string.Empty,
                errorCodeSigningTimeRecibo = string.Empty,
                errorMessageigningTimeRecibo = string.Empty,
                errorCodeEndoso = string.Empty,
                errorMessageEndoso = string.Empty,
                errorCodeMandato = string.Empty,
                errorMessageMandato = string.Empty
            };

            response.errorCodeNote = "AAD11";
            response.errorMessageNote = ConfigurationManager.GetValue("ErrorMessage_AAD11");
            response.errorMessageFETV = ConfigurationManager.GetValue("ErrorMessage_AAF01a");
            response.errorMessageReceiverFETV = ConfigurationManager.GetValue("ErrorMessage_AAG01a");
            response.errorMessageEndoso = ConfigurationManager.GetValue("ErrorMessage_LGC32");
            response.errorCodeMandato = "LGC36";
            response.errorMessageMandato = ConfigurationManager.GetValue("ErrorMessage_LGC36");

            //SenderPArty
            if (eventCode == "030") response.errorCodeFETV = "AAF01a";
            if (eventCode == "031") response.errorCodeFETV = "AAF01b";
            if (eventCode == "032") response.errorCodeFETV = "AAF01c";
            if (eventCode == "033") response.errorCodeFETV = "AAF01d";
            if (eventCode == "034") response.errorCodeFETV = "AAF01e";
            //ReceiverParty
            if (eventCode == "030") response.errorCodeReceiverFETV = "AAG01a";
            if (eventCode == "031") response.errorCodeReceiverFETV = "AAG01b";
            if (eventCode == "032") response.errorCodeReceiverFETV = "AAG01c";
            if (eventCode == "033") response.errorCodeReceiverFETV = "AAG01d";

            //SigningTime
            if (eventCode == "030") response.errorCodeSigningTimeAcuse = "DC24a";
            if (eventCode == "030") response.errorMessageigningTimeAcuse = ConfigurationManager.GetValue("ErrorMessage_DC24a");

            if (eventCode == "032") response.errorCodeSigningTimeRecibo = "DC24b";
            if (eventCode == "032") response.errorMessageigningTimeRecibo = ConfigurationManager.GetValue("ErrorMessage_DC24b");

            //Endoso
            if (eventCode == "037") response.errorCodeEndoso = "LGC26";
            if (eventCode == "038") response.errorCodeEndoso = "LGC29";
            if (eventCode == "039") response.errorCodeEndoso = "LGC32";
           
            else if (eventCode == "035")
            {
                response.errorCode = "AAF01";
                response.errorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF01_035");
                response.errorCodeNoteA = "AAD11a";
                response.errorMessageNoteA = ConfigurationManager.GetValue("ErrorMessage_AAD11a");
            }
            else if (eventCode == "036")
            {
                response.errorCode = "AAF01a";
                response.errorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF01a_036");
                response.errorCodeB = "AAF01b";
                response.errorMessageB = ConfigurationManager.GetValue("ErrorMessage_AAF01b_036");
            }
            else if (eventCode == "037")
            {
                response.errorCode = "AAF01";
                response.errorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF01_037");
            }
            else if (eventCode == "040" || eventCode == "039" || eventCode == "038")
            {
                response.errorCode = "AAF01";
                response.errorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF01_038");
            }
            else if (eventCode == "041")
            {
                response.errorCode = "AAF01";
                response.errorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF01_041");
            }
            else if (eventCode == "043")
            {
                response.errorCode = "AAF01";
                response.errorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF01");
            }
            else if (eventCode == "044")
            {
                response.errorCode = "AAF01";
                response.errorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF01_044");
            }
            else if (eventCode == "045")
            {
                response.errorCode = "AAF01";
                response.errorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF01_045");
            }
            return response;
        }
        #endregion

        #region Reemplazado Predecesor

        //public List<ValidateListResponse> ValidateReplacePredecesor(string trackId, string companyId, string employeeId)
        //{
        //    DateTime startDate = DateTime.UtcNow;
        //    List<ValidateListResponse> responses = new List<ValidateListResponse>();
        //    var item = new ValidateListResponse
        //    {
        //        IsValid = false,
        //        Mandatory = true,
        //        ErrorCode = "NIAE191a",
        //        ErrorMessage = "Documento a Reemplazar no se encuentra recibido en la Base de Datos."
        //    };

        //    var adjustment = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
        //    if (adjustment == null)
        //    {
        //        item.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
        //        responses.Add(item);
        //        return responses;
        //    }

        //    var individualPayroll = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(adjustment.DocumentReferencedKey, adjustment.DocumentReferencedKey);
        //    if (individualPayroll == null)
        //    {
        //        item.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
        //        responses.Add(item);
        //        return responses;
        //    }

        //    // Control de cambios v9.3: Se valida que el Predecesor corresponda al mismo Empleador y Empleado.
        //    if(companyId != individualPayroll.SenderCode || employeeId != individualPayroll.ReceiverCode)
        //    {
        //        item.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
        //        responses.Add(item);
        //        return responses;
        //    }

        //    var document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(individualPayroll.Identifier, individualPayroll.Identifier);
        //    if (document != null)
        //    {
        //        item.IsValid = true;
        //        item.ErrorCode = "100";
        //        item.ErrorMessage = "Evento ValidateReplacePredecesor referenciado correctamente";
        //    }

        //    item.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
        //    responses.Add(item);
        //    return responses;
        //}

        public List<ValidateListResponse> ValidateReplacePredecesor(string trackId, XmlParseNomina xmlParser)
        {
            var companyId = xmlParser.globalDocPayrolls.Emp_NIT;
            var employeeId = xmlParser.globalDocPayrolls.NumeroDocumento;
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            int? noteType = xmlParser.globalDocPayrolls.TipoNota;
            var noteTypeResponseError = new ValidateListResponse
            {
                IsValid = false,
                Mandatory = true,
                ErrorCode = "NIAE214",
                ErrorMessage = "Se debe colocar el Codigo correspondiente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            };

            // validar tipo de Nota...
            if ((!noteType.HasValue)
                || (noteType.HasValue 
                    && noteType.Value != (int)IndividualPayrollAdjustmentNoteType.Replace 
                    && noteType.Value != (int)IndividualPayrollAdjustmentNoteType.Remove))
            {
                responses.Add(noteTypeResponseError);
                return responses;
            }

            // Solo se debe informar uno de los nodos, nos pueden estar al mismo tiempo 'Reemplazar' y 'Eliminar'.
            if (noteType.HasValue 
                && ((noteType.Value == (int)IndividualPayrollAdjustmentNoteType.Replace && xmlParser.HasRemoveNode)
                        || (noteType.Value == (int)IndividualPayrollAdjustmentNoteType.Remove && xmlParser.HasReplaceNode)))
            {
                responses.Add(noteTypeResponseError);
                return responses;
            }

            var errorCode = (noteType.Value == (int)IndividualPayrollAdjustmentNoteType.Replace) ? "NIAE191a" : "NIAE216a";

            // Replace
            var itemResponse = new ValidateListResponse
            {
                IsValid = false,
                Mandatory = true,
                ErrorCode = errorCode,
                ErrorMessage = "Documento a Reemplazar no se encuentra recibido en la Base de Datos."
            };

            var adjustment = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (adjustment == null)
            {
                itemResponse.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                responses.Add(itemResponse);
                return responses;
            }

            var individualPayroll = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(adjustment.DocumentReferencedKey, adjustment.DocumentReferencedKey);
            if (individualPayroll == null)
            {
                itemResponse.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                responses.Add(itemResponse);
                return responses;
            }

            // Control de cambios v9.3: Se valida que el Predecesor corresponda al mismo Empleador y Empleado.
            if(noteType.Value == (int)IndividualPayrollAdjustmentNoteType.Replace)
            {
                if (companyId != individualPayroll.SenderCode || employeeId != individualPayroll.ReceiverCode)
                {
                    itemResponse.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    responses.Add(itemResponse);
                    return responses;
                }
            }
            else
            {
                if (companyId != individualPayroll.SenderCode)
                {
                    itemResponse.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    responses.Add(itemResponse);
                    return responses;
                }
            }

            var document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(individualPayroll.Identifier, individualPayroll.Identifier);
            if (document != null)
            {
                itemResponse.IsValid = true;
                itemResponse.ErrorCode = "100";
                itemResponse.ErrorMessage = "Evento ValidateReplacePredecesor referenciado correctamente";
            }

            itemResponse.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            responses.Add(itemResponse);
            return responses;
        }

        #endregion

        #region Individual Payroll

        private List<ValidateListResponse> CheckIndividualPayrollDuplicity(string companyId, string serieAndNumber)
        {
            DateTime startDate = DateTime.UtcNow;

            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento CheckIndividualPayrollDuplicity referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            // Solo se podrá transmitir una única vez el número del documento para el trabajador.
            var payroll = payrollTableManager.GlobalPayrollByRowKey_Number<GlobalDocPayroll>(companyId, serieAndNumber);
            if (payroll != null)
            {
                responses.Clear();
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "90",
                    ErrorMessage = "Documento procesado anteriormente",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            return responses;
        }

        //private List<ValidateListResponse> CheckIndividualPayrollInSameMonth(string companyId, string employeeId, bool novelty, DateTime startPaymentDate)
        private List<ValidateListResponse> CheckIndividualPayrollInSameMonth(XmlParseNomina xmlParser)
        {
            var companyId = xmlParser.globalDocPayrolls.Emp_NIT;
            var employeeId = xmlParser.globalDocPayrolls.NumeroDocumento;
            var novelty = xmlParser.Novelty;
            
            DateTime startDate = DateTime.UtcNow;

            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento CheckIndividualPayrollInSameMonth referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            // Solo se podrá transmitir para cada trabajador 1 documento NominaIndividual mensual durante cada mes del año. Para el mismo Empleador.
            var payrolls = payrollTableManager.GlobalPayrollByRowKey_DocumentNumber<GlobalDocPayroll>(companyId, employeeId);
            if (payrolls == null || payrolls.Count <= 0) // No exiten nóminas para el empleado...
            {
                //Novedad XML true
                if (novelty)
                {
                    responses.Clear();
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "NIE199a",
                        ErrorMessage = "Elemento Novedad con valor “true” no puede ser recibido por primera vez, " +
                        "ya que no existe una Nómina Electrónica recibida para este trabajador reportada por este Emisor durante este mes.",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }
                else
                    return responses; // no existe para el mes actual
            }

            var startPaymentDate = xmlParser.globalDocPayrolls.FechaPagoInicio.Value;

            // Se valida contra la FechaPagoInicio...
            var payrollCurrentMonth = payrolls.FirstOrDefault(x => x.FechaPagoInicio.Value.Year == startPaymentDate.Year
                && x.FechaPagoInicio.Value.Month == startPaymentDate.Month);
            if (payrollCurrentMonth == null)
            {
                //Novedad XML true
                if (novelty)
                {
                    responses.Clear();
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "NIE199a",
                        ErrorMessage = "Elemento Novedad con valor “true” no puede ser recibido por primera vez, " +
                        "ya que no existe una Nómina Electrónica recibida para este trabajador reportada por este Emisor durante este mes.",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }
                else
                    return responses; // no existe para el mes actual
            }

            ////Novedad XML False
            //if (payrollCurrentMonth != null && !novelty)
            //{
            //    responses.Clear();
            //    responses.Add(new ValidateListResponse
            //    {
            //        IsValid = false,
            //        Mandatory = true,
            //        ErrorCode = "NIE199",
            //        ErrorMessage = "Únicamente pueden ser aceptados documentos “NominaIndividual” del mismo trabajador" +
            //        " durante el Mes indicado en el documento que posean como 'True' este elemento.",
            //        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            //    });
            //    return responses;
            //}

            // Control de cambios v9.3: Se valida que el CUNENov exista y corresponda al mismo Empleador y Empleado.
            if (novelty)
            {
                var errorCode = "NIE204a";
                var errorMessage = "Documento a Realizar la Novedad contractual no se encuentra recibido en la Base de Datos";
                var cune = xmlParser.globalDocPayrolls.CUNENov;

                var individualPayroll = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(cune, cune);
                if (individualPayroll == null)
                {
                    responses.Clear();
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = errorCode,
                        ErrorMessage = errorMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }

                if (companyId != individualPayroll.SenderCode || employeeId != individualPayroll.ReceiverCode)
                {
                    responses.Clear();
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = errorCode,
                        ErrorMessage = errorMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }

                var document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(individualPayroll.Identifier, individualPayroll.Identifier);
                if (document == null)
                {
                    responses.Clear();
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = errorCode,
                        ErrorMessage = errorMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }
            }

            return responses;
        }

        #endregion


        #region CheckDocument
        private List<ValidateListResponse> CheckDocument(string senderCode, string documentType, string serieAndNumber)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            var identifier = StringUtil.GenerateIdentifierSHA256($"{senderCode}{documentType}{serieAndNumber}");
            var document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(identifier, identifier);

            if (document != null)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "90",
                    ErrorMessage = "Documento procesado anteriormente",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
                return null;


            return responses;
        }
        #endregion

        #region RequestValidateSigningTime

        public List<ValidateListResponse> RequestValidateSigningTime(RequestObjectSigningTime data)
        {            
            var validateResponses = new List<ValidateListResponse>();
            ValidatorEngine validatorEngine = new ValidatorEngine();

            EventStatus code;
            string trackIdAvailability = null;
            string originalTrackId = data.TrackId;

            switch (int.Parse(data.EventCode))
            {
                case (int)EventStatus.Receipt:
                    code = EventStatus.Received;
                    break;
                case (int)EventStatus.SolicitudDisponibilizacion:
                    code = EventStatus.Accepted;
                    break;
                case (int)EventStatus.NotificacionPagoTotalParcial:
                case (int)EventStatus.NegotiatedInvoice:
                    code = EventStatus.SolicitudDisponibilizacion;
                    break;
                case (int)EventStatus.Avales:
                case (int)EventStatus.EndosoPropiedad:
                case (int)EventStatus.EndosoGarantia:
                case (int)EventStatus.EndosoProcuracion:
                    code = EventStatus.SolicitudDisponibilizacion;
                    break;
                default:
                    code = EventStatus.Receipt;
                    break;
            }

            if (Convert.ToInt32(data.EventCode) == (int)EventStatus.Rejected ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.Receipt ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.Accepted ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.AceptacionTacita ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.SolicitudDisponibilizacion ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
                )
            {
                bool existDisponibilizaExpresa = false;
                //Servicio GlobalDocAssociate
                string eventSearch = "0" + (int)code;
                List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(data.TrackId.ToLower());

                if (InvoiceWrapper.Any())
                {
                    var trackIdEvent = InvoiceWrapper[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventSearch
                    && int.Parse(x.DocumentMeta.DocumentTypeId) == (int)DocumentType.ApplicationResponse);
                    if (trackIdEvent != null)
                    {
                        existDisponibilizaExpresa = true;
                        data.TrackId = trackIdEvent.DocumentMeta.PartitionKey;
                    }
                }               

                // Validación de la Sección Signature - Fechas valida transmisión evento Solicitud Disponibilizacion
                if (Convert.ToInt32(data.EventCode) == (int)EventStatus.SolicitudDisponibilizacion && !existDisponibilizaExpresa)
                {
                    //Servicio GlobalDocAssociate
                    code = EventStatus.AceptacionTacita;
                    string eventSearchTacita = "0" + (int)code;
                    List<InvoiceWrapper> InvoiceWrapperTacita = associateDocumentService.GetEventsByTrackId(data.TrackId.ToLower());
                    if (InvoiceWrapperTacita.Any())
                        data.TrackId = InvoiceWrapperTacita[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventSearchTacita
                        && int.Parse(x.DocumentMeta.DocumentTypeId) == (int)DocumentType.ApplicationResponse).DocumentMeta.PartitionKey;                   
                }
            }
            else if (Convert.ToInt32(data.EventCode) == (int)EventStatus.NegotiatedInvoice || Convert.ToInt32(data.EventCode) == (int)EventStatus.Avales)
            {
                //Servicio GlobalDocAssociate
                string eventSearch = "0" + (int)code;
                List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(data.TrackId.ToLower());

                if (InvoiceWrapper.Any())
                    data.TrackId = InvoiceWrapper[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventSearch
                    && int.Parse(x.DocumentMeta.DocumentTypeId) == (int)DocumentType.ApplicationResponse
                    && (x.DocumentMeta.CustomizationID == "361" || x.DocumentMeta.CustomizationID == "362")).DocumentMeta.PartitionKey;                
            }
            else if (Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoPropiedad
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoGarantia
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoProcuracion)
            {
                //Servicio GlobalDocAssociate
                string eventSearch = "0" + (int)code;
                List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(data.TrackId.ToLower());

                if (InvoiceWrapper.Any())
                {
                    var respTrackIdAvailability = InvoiceWrapper[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventSearch
                    && int.Parse(x.DocumentMeta.DocumentTypeId) == (int)DocumentType.ApplicationResponse);
                    if (respTrackIdAvailability != null)
                    {
                        trackIdAvailability = respTrackIdAvailability.DocumentMeta.PartitionKey;
                    }
                }                
            }

            var xmlBytes = validatorEngine.GetXmlFromStorageAsync(data.TrackId);
            var xmlParser = new XmlParser(xmlBytes.Result);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            // Por el momento solo para el evento 036 se conserva el trackId original, con el fin de traer el PaymentDueDate del CUFE
            // y enviarlo al validator para una posterior validación contra la fecha de vencimiento del evento (036).
            string parameterPaymentDueDateFE = null;
            if (Convert.ToInt32(data.EventCode) == (int)EventStatus.SolicitudDisponibilizacion
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoPropiedad
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoGarantia
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoProcuracion)
            {
                var originalXmlBytes = validatorEngine.GetXmlFromStorageAsync(originalTrackId);
                var originalXmlParser = new XmlParser(originalXmlBytes.Result);
                if (!originalXmlParser.Parser())
                    throw new Exception(originalXmlParser.ParserError);

                parameterPaymentDueDateFE = originalXmlParser.PaymentDueDate;
            }

            DateTime? signingTimeAvailability = null;
            if ((Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoPropiedad
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoGarantia
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoProcuracion) && !string.IsNullOrWhiteSpace(trackIdAvailability))
            {
                var availabilityXmlBytes = validatorEngine.GetXmlFromStorageAsync(trackIdAvailability);
                var availabilityXmlParser = new XmlParser(availabilityXmlBytes.Result);
                if (!availabilityXmlParser.Parser())
                    throw new Exception(availabilityXmlParser.ParserError);

                signingTimeAvailability = Convert.ToDateTime(availabilityXmlParser.SigningTime);
            }

            var nitModel = xmlParser.Fields.ToObject<NitModel>();
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateSigningTime(data, xmlParser, nitModel, paymentDueDateFE: parameterPaymentDueDateFE,
                signingTimeAvailability: signingTimeAvailability));

            return validateResponses;
        } 

        #endregion


        #region RequestValidateParty

        public List<ValidateListResponse> RequestValidateParty(RequestObjectParty party)
        {           
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();
            ValidatorEngine validatorEngine = new ValidatorEngine();
            XmlParser xmlParserCufe = null;
            XmlParser xmlParserCude = null;
            string receiverCancelacion = String.Empty;
            string issuerAttorney = string.Empty;
            string senderAttorney = string.Empty;
            string trackIdAvailability = null;

            //Anulacion de endoso electronico, TerminacionLimitacion de Circulacion obtiene CUFE referenciado en el CUDE emitido
            if (Convert.ToInt32(party.ResponseCode) == (int)EventStatus.InvoiceOfferedForNegotiation ||
                Convert.ToInt32(party.ResponseCode) == (int)EventStatus.AnulacionLimitacionCirculacion)
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(party.TrackId, party.TrackId);
                if (documentMeta != null)
                {
                    //Obtiene el CUFE
                    party.TrackId = documentMeta.DocumentReferencedKey;
                    receiverCancelacion = documentMeta.ReceiverCode;
                }
            }

            //Obtiene XML Factura Electornica CUFE
            var xmlBytes = validatorEngine.GetXmlFromStorageAsync(party.TrackId);
            xmlParserCufe = new XmlParser(xmlBytes.Result);
            if (!xmlParserCufe.Parser())
                throw new Exception(xmlParserCufe.ParserError);

            //Obtiene XML ApplicationResponse CUDE
            var xmlBytesCude = validatorEngine.GetXmlFromStorageAsync(party.TrackIdCude.ToLower());
            xmlParserCude = new XmlParser(xmlBytesCude.Result);
            if (!xmlParserCude.Parser())
                throw new Exception(xmlParserCude.ParserError);

            var nitModel = xmlParserCufe.Fields.ToObject<NitModel>();
            bool valid = true;

            // ...
            List<string> issuerAttorneyList = null;
            var eventCode = int.Parse(party.ResponseCode);
            if (eventCode == (int)EventStatus.Avales || eventCode == (int)EventStatus.NegotiatedInvoice || eventCode == (int)EventStatus.AnulacionLimitacionCirculacion)
            {
                var attorneyList = TableManagerGlobalDocReferenceAttorney.FindDocumentReferenceAttorneyByCUFEList<GlobalDocReferenceAttorney>(party.TrackId);
                if (attorneyList != null && attorneyList.Count > 0)
                {
                    issuerAttorneyList = new List<string>();
                    // ForEach...
                    attorneyList.ForEach(item =>
                    {
                        if (!string.IsNullOrWhiteSpace(item.EndDate))
                        {
                            var endDate = Convert.ToDateTime(item.EndDate);
                            if (endDate.Date > DateTime.Now.Date) issuerAttorneyList.Add(item.IssuerAttorney);
                        }
                        else issuerAttorneyList.Add(item.IssuerAttorney);
                    });
                }
            }
            else if (Convert.ToInt32(party.ResponseCode) == (int)EventStatus.EndosoPropiedad)
            {
                string eventDisponibiliza = "0" + (int)EventStatus.SolicitudDisponibilizacion;
                List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(party.TrackId.ToLower());

                if (InvoiceWrapper.Any())
                    trackIdAvailability = InvoiceWrapper[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventDisponibiliza
                    && int.Parse(x.DocumentMeta.DocumentTypeId) == (int)DocumentType.ApplicationResponse).DocumentMeta.PartitionKey;               
            }

            string partyLegalEntityName = null, partyLegalEntityCompanyID = null, availabilityCustomizationId = null;
            if ((Convert.ToInt32(party.ResponseCode) == (int)EventStatus.EndosoPropiedad && !string.IsNullOrWhiteSpace(trackIdAvailability)))
            {
                var availabilityXmlBytes = validatorEngine.GetXmlFromStorageAsync(trackIdAvailability);
                var availabilityXmlParser = new XmlParser(availabilityXmlBytes.Result);
                if (!availabilityXmlParser.Parser())
                    throw new Exception(availabilityXmlParser.ParserError);

                partyLegalEntityName = availabilityXmlParser.Fields["PartyLegalEntityName"].ToString();
                partyLegalEntityCompanyID = availabilityXmlParser.Fields["PartyLegalEntityCompanyID"].ToString();
                availabilityCustomizationId = availabilityXmlParser.Fields["CustomizationId"].ToString();
            }


            if (eventCode == (int)EventStatus.TerminacionMandato)
            {
                var attorney = TableManagerGlobalDocReferenceAttorney.FindByPartition<GlobalDocReferenceAttorney>(party.TrackId).ToList();

                if (attorney != null && attorney.Count > 0)
                {
                    foreach (var item in attorney)
                    {
                        issuerAttorney = item.IssuerAttorney;
                        senderAttorney = item.SenderCode;
                    }
                }
            }

            //Valida existe cambio legitimo tenedor
            GlobalDocHolderExchange documentHolderExchange = documentHolderExchangeTableManager.FindhByCufeExchange<GlobalDocHolderExchange>(party.TrackId.ToLower(), true);
            if (documentHolderExchange != null)
            {
                //Existe mas de un legitimo tenedor requiere un mandatario
                string[] endosatarios = documentHolderExchange.PartyLegalEntity.Split('|');
                if (endosatarios.Length == 1)
                {
                    nitModel.SenderCode = documentHolderExchange.PartyLegalEntity;
                }
                else
                {
                    foreach (string endosatario in endosatarios)
                    {
                        GlobalDocReferenceAttorney documentAttorney = TableManagerGlobalDocReferenceAttorney.FindhByCufeSenderAttorney<GlobalDocReferenceAttorney>(party.TrackId.ToLower(), endosatario, xmlParserCude.ProviderCode);
                        if (documentAttorney == null)
                        {
                            valid = false;
                            validateResponses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC35",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC35"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    if (valid)
                    {
                        nitModel.SenderCode = party.SenderParty;
                    }
                }
            }
            if (valid)
            {
                //Enodsatario Anulacion endoso
                nitModel.ReceiverCode = receiverCancelacion != "" ? receiverCancelacion : nitModel.ReceiverCode;
                var validator = new Validator();
                validateResponses.AddRange(ValidateParty(nitModel, party, xmlParserCude, issuerAttorneyList,
                    issuerAttorney, senderAttorney, partyLegalEntityName, partyLegalEntityCompanyID, availabilityCustomizationId));
            }
            return validateResponses;
        }
        #endregion

        public async Task<byte[]> GetXmlFromStorageAsync(string trackId)
        {
            var TableManager = new TableManager("GlobalDocValidatorRuntime");
            var documentStatusValidation = TableManager.Find<GlobalDocValidatorRuntime>(trackId, "UPLOAD");
            if (documentStatusValidation == null)
                return null;

            var fileManager = new FileManager();
            var container = $"global";
            var fileName = $"docvalidator/{documentStatusValidation.Category}/{documentStatusValidation.Timestamp.Date.Year}/{documentStatusValidation.Timestamp.Date.Month.ToString().PadLeft(2, '0')}/{trackId}.xml";
            var xmlBytes = await fileManager.GetBytesAsync(container, fileName);

            return xmlBytes;
        }


        #region RequestValidateEmitionEventPrev

        public List<ValidateListResponse> RequestValidateEmitionEventPrev(RequestObjectEventPrev eventPrev)
        {
            var validateResponses = new List<ValidateListResponse>();
            ValidatorEngine validatorEngine = new ValidatorEngine();
            var nitModel = new NitModel();
            XmlParser xmlParserCufe = null;
            XmlParser xmlParserCude = null;

            //Anulacion de endoso electronico obtiene CUFE referenciado en el CUDE emitido
            if (Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.InvoiceOfferedForNegotiation ||
                Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.AnulacionLimitacionCirculacion)
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(eventPrev.TrackId, eventPrev.TrackId);
                if (documentMeta != null)
                {
                    //Obtiene el CUFE
                    eventPrev.TrackId = documentMeta.DocumentReferencedKey;
                    //Obtiene XML ApplicationResponse CUDE
                    var xmlBytesCude = validatorEngine.GetXmlFromStorageAsync(eventPrev.TrackIdCude);
                    xmlParserCude = new XmlParser(xmlBytesCude.Result);
                    if (!xmlParserCude.Parser())
                        throw new Exception(xmlParserCude.ParserError);
                }
            }
            //Obtiene información factura referenciada Endoso electronico, Solicitud Disponibilización AR CUDE
            if (Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.SolicitudDisponibilizacion || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndosoGarantia
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndosoPropiedad || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndosoProcuracion
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.Avales || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial)
            {
                //Obtiene XML Factura electronica CUFE
                var xmlBytes = validatorEngine.GetXmlFromStorageAsync(eventPrev.TrackId);
                xmlParserCufe = new XmlParser(xmlBytes.Result);
                if (!xmlParserCufe.Parser())
                    throw new Exception(xmlParserCufe.ParserError);

                //Obtiene XML ApplicationResponse CUDE
                var xmlBytesCude = validatorEngine.GetXmlFromStorageAsync(eventPrev.TrackIdCude);
                xmlParserCude = new XmlParser(xmlBytesCude.Result);
                if (!xmlParserCude.Parser())
                    throw new Exception(xmlParserCude.ParserError);

                nitModel = xmlParserCude.Fields.ToObject<NitModel>();
            }

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateEmitionEventPrev(eventPrev, xmlParserCufe, xmlParserCude, nitModel));

            return validateResponses;
        }

        #endregion      

        #region NewValidateEventRADIAN
        public List<ValidateListResponse> NewValidateEventRadianAsync(string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();            
            
            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento NewValidateEventRadianAsync referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });


            GlobalDocValidatorDocumentMeta documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (documentMeta != null)
            {
                bool validEventRadian = true;
                bool validEventPrev = true;
                bool validEventReference = true;
                string signingTimeStamp = documentMeta.SigningTimeStamp.ToString("dd MMMM yyyy hh:mm:ss tt");                
                RequestObjectEventApproveCufe eventApproveCufe = new RequestObjectEventApproveCufe();
                RequestObjectDocReference docReference = new RequestObjectDocReference();
                RequestObjectParty requestParty = new RequestObjectParty();
                RequestObjectEventPrev eventPrev = new RequestObjectEventPrev();
                RequestObjectSigningTime signingTime = new RequestObjectSigningTime();

                EventRadianModel eventRadian = new EventRadianModel(
                     documentMeta.DocumentReferencedKey,
                     documentMeta.PartitionKey,
                     documentMeta.EventCode,
                     documentMeta.DocumentTypeId,
                     documentMeta.ResponseCodeListID,
                     documentMeta.CustomizationID,
                     signingTimeStamp,
                     documentMeta.ValidityPeriodEndDate,
                     documentMeta.SenderCode,
                     documentMeta.ReceiverCode,
                     documentMeta.DocumentReferencedTypeId,
                     documentMeta.DocumentReferencedId,
                     documentMeta.IssuerPartyCode,
                     documentMeta.IssuerPartyName,
                     documentMeta.SendTestSet
                     );

                NitModel nitModel = new NitModel(
                    documentMeta.ResponseCodeListID,
                    documentMeta.ValidityPeriodEndDate,
                    documentMeta.DocumentReferencedTypeId,
                    documentMeta.IssuerPartyCode,
                    documentMeta.IssuerPartyName,
                    documentMeta.TechProviderCode,
                    documentMeta.SerieAndNumber,
                    documentMeta.DocumentTypeId,
                    documentMeta.SenderCode
                    );


                if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.AnulacionLimitacionCirculacion
                    || Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.InvoiceOfferedForNegotiation)
                {                   
                    eventRadian.TrackId = documentMeta.CancelElectronicEvent;                    
                }

                bool validaMandatoListID = (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.Mandato && documentMeta.ResponseCodeListID == "3") ? false : true;               
                responses = ValidateSerieAndNumber(nitModel);
                validateResponses.AddRange(responses);

                if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.SolicitudDisponibilizacion)
                {
                    EventRadianModel.SetValueEventAproveCufe(ref eventRadian, eventApproveCufe);                   
                    responses = EventApproveCufe(nitModel, eventApproveCufe);
                    validateResponses.AddRange(responses);
                }

                //Si es mandato 
                if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.Mandato
                    && validEventRadian)
                {
                    var xmlBytes = GetXmlFromStorageAsync(trackId);
                    var xmlParser = new XmlParser(xmlBytes.Result);
                    if (!xmlParser.Parser())
                        throw new Exception(xmlParser.ParserError);

                    validatorDocumentNameSpaces(xmlBytes.Result);

                    responses = ValidateReferenceAttorney(xmlParser, trackId, _ns);
                    foreach (var itemReferenceAttorney in responses)
                    {
                        if (itemReferenceAttorney.ErrorCode == "AAH07")
                            validEventReference = false;
                    }
                    validateResponses.AddRange(responses);
                }

                if (Convert.ToInt32(documentMeta.EventCode) != (int)EventStatus.Mandato)
                {
                    EventRadianModel.SetValuesDocReference(ref eventRadian, docReference);
                    responses = ValidateDocumentReferencePrev(docReference.TrackId, docReference.IdDocumentReference, 
                        docReference.EventCode, docReference.DocumentTypeIdRef, docReference.IssuerPartyCode,
                        docReference.IssuerPartyName);
                    foreach (var itemReference in responses)
                    {
                        if (!itemReference.IsValid)
                            validEventRadian = false;
                    }
                    validateResponses.AddRange(responses);
                }

                //Si Mandato contiene CUFEs Referenciados
                if (validaMandatoListID && validEventRadian && validEventReference)
                {
                    EventRadianModel.SetValuesValidateParty(ref eventRadian, requestParty);
                    EventRadianModel.SetValuesEventPrev(ref eventRadian, eventPrev);
                    EventRadianModel.SetValuesSigningTime(ref eventRadian, signingTime);
                    
                    responses = RequestValidateParty(requestParty);
                    validateResponses.AddRange(responses);

                    responses = RequestValidateEmitionEventPrev(eventPrev);
                    foreach (var itemResponsesTacita in responses)
                    {
                        if (itemResponsesTacita.ErrorCode == "LGC14" || itemResponsesTacita.ErrorCode == "LGC12"
                            || itemResponsesTacita.ErrorCode == "LGC05" || itemResponsesTacita.ErrorCode == "LGC24"
                            || itemResponsesTacita.ErrorCode == "LGC27" || itemResponsesTacita.ErrorCode == "LGC30"
                            || itemResponsesTacita.ErrorCode == "LGC38")
                            validEventPrev = false;
                    }
                    validateResponses.AddRange(responses);

                    if (validEventPrev)
                    {
                        responses = RequestValidateSigningTime(signingTime);                      
                        validateResponses.AddRange(responses);
                    }

                }

                UpdateInTransactions(documentMeta.DocumentReferencedKey, documentMeta.EventCode);

            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAH07",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
                validateResponses.AddRange(responses);
            }


            return validateResponses;
        }
        #endregion

        public void validatorDocumentNameSpaces(byte[] xmlBytes)
        {
            _xmlBytes = xmlBytes;
            _xmlDocument = new XmlDocument() { PreserveWhitespace = true };
            _xmlDocument.LoadXml(Encoding.UTF8.GetString(xmlBytes));

            var xmlReader = new XmlTextReader(new MemoryStream(xmlBytes)) { Namespaces = true };

            _document = new XPathDocument(xmlReader);
            _navigator = _document.CreateNavigator();

            _navNs = _document.CreateNavigator();
            _navNs.MoveToFollowing(XPathNodeType.Element);
            IDictionary<string, string> nameSpaceList = _navNs.GetNamespacesInScope(XmlNamespaceScope.All);

            _ns = new XmlNamespaceManager(_xmlDocument.NameTable);

            foreach (var nsItem in nameSpaceList)
            {
                if (string.IsNullOrEmpty(nsItem.Key))
                    _ns.AddNamespace("sig", nsItem.Value);
                else
                    _ns.AddNamespace(nsItem.Key, nsItem.Value);
            }
            _ns.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        }

    }
}
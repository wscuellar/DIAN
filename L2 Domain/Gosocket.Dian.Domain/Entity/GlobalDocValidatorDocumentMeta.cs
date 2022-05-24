using Microsoft.WindowsAzure.Storage.Table;
using System;

public class GlobalDocValidatorDocumentMeta : TableEntity
{
    public GlobalDocValidatorDocumentMeta() { }

    public GlobalDocValidatorDocumentMeta(string pk, string rk) : base(pk, rk)
    {

    }
    public string CancelElectronicEvent { get; set; }
    public string CustomizationID { get; set; }
    public string FileName { get; set; }
    public DateTime EmissionDate { get; set; }
    public string DocumentTypeId { get; set; }
    public string DocumentTypeName { get; set; }
    public string Identifier { get; set; }
    public string SenderCode { get; set; }
    public string SenderName { get; set; }
    public string SenderTypeCode { get; set; }
    public string SenderSchemeCode { get; set; }
    public string ReceiverCode { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverTypeCode { get; set; }
    public string ReceiverSchemeCode { get; set; }
    public string Number { get; set; }
    public string Serie { get; set; }
    public string SerieAndNumber { get; set; }
    public double TotalAmount { get; set; }
    public double FreeAmount { get; set; }
    public double TaxAmountIva { get; set; }
    public double TaxAmountIva5Percent { get; set; }
    public double TaxAmountIva14Percent { get; set; }
    public double TaxAmountIva16Percent { get; set; }
    public double TaxAmountIva19Percent { get; set; }
    public double TaxAmountIca { get; set; }
    public double TaxAmountIpc { get; set; }
    public string DocumentKey { get; set; }
    public string DocumentReferencedKey { get; set; }
    public string InvoiceAuthorization { get; set; }
    public string SoftwareId { get; set; }
    public string TechProviderCode { get; set; }
    public string TestSetId { get; set; }
    public string UblVersion { get; set; }
    public string ZipKey { get; set; }
    public string EventCode { get; set; }
    public string EventCodeDescription { get; set; }
    public bool InTransaction { get; set; }
    public DateTime SigningTimeStamp { get; set; }
    public double NewAmountTV { get; set; }
    public bool IsInvoiceTV { get; set; }
    public bool SendTestSet { get; set; }
    public bool? Novelty { get; set; }
    public string DocumentReferencedId { get; set; }
    public string DocumentReferencedTypeId { get; set; }
    public string ResponseCodeListID { get; set; }
    public string ValidityPeriodEndDate { get; set; }
    public string IssuerPartyCode { get; set; }
    public string IssuerPartyName { get; set; }
    public string NoteMandato { get; set; }
    public string NoteMandato2 { get; set; }
    public string ReceiverPartyLegalEntityCompanyID { get; set; }
    public string ReceiverPartyLegalEntityName { get; set; }
    public string PartyLegalEntityName { get; set; }
    public string PartyLegalEntityCompanyID { get; set; }
    public string PaymentDueDate { get; set; }
    public string FechaPagoNominaInicio { get; set; }
    public string FechaPagoNominaFin { get; set; }
    public string CuneNovedad { get; set; }

    /// <summary>
    /// Contiene el tipo de formato con el que se 
    /// generará la representación gráfica de la factura
    /// <para>Formato tipo carta | Formato tipo media carta | Formato tipo parametrizable</para>
    /// </summary>
    public string PrintFormatType { get; set; }
    /// <summary>
    /// Contiene las secciones que deben ser ocultadas al momento de generar
    /// la factura electrónica (Cuando el usuario selecciona el formato tipo parametrizable)
    /// </summary>
    public string SectionsToHide { get; set; }
    public string DocumentCurrencyCode { get; set; }
}
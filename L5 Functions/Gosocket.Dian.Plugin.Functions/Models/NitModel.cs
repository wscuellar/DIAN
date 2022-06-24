namespace Gosocket.Dian.Plugin.Functions.Models
{
    public class NitModel
    {
        public string SenderCode { get; set; }
        public string SenderSchemeCode { get; set; }
        public string SenderName { get; set; }
        public string SenderCodeDigit { get; set; }
        public string ProviderCode { get; set; }
        public string ProviderCodeDigit { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverCodeDigit { get; set; }
        public string ReceiverCodeSchemaValue { get; set; }
        public string ReceiverCode2 { get; set; }
        public string ReceiverCode2Digit { get; set; }
        public string ReceiverCode2SchemaValue { get; set; }
        public string SoftwareProviderCode { get; set; }
        public string SoftwareProviderCodeDigit { get; set; }
        public string IssuerPartySchemeAgencyCode { get; set; }
        public string IssuerPartySchemeCode { get; set; }
        public string IssuerPartyCode { get; set; }
        public string IssuerPartyName { get; set; }
        public string CustomizationId { get; set; }
        public string IssuerPartySchemeID { get; set; }
        public string IssuerPartyID { get; set; }
        public string ValorTotalEndoso { get; set; }
        public string PrecioPagarseFEV { get; set; }
        public string TasaDescuento { get; set; }
        public string MedioPago { get; set; }
        public string DocumentKey { get; set; }
        public string ResponseCode { get; set; }
        public string listID { get; set; }
        public string DocumentTypeIdRef { get; set; }
        public string DocumentTypeId { get; set; }
        public string SerieAndNumber { get; set; }
        public string ValidityPeriodEndDate { get; set; }
        public string ValorActualTituloValor { get; set; }
        public string ValorPendienteTituloValor { get; set; }
        public string SoftwareId { get; set; }
        public string AgentPartyPersonSchemeID { get; set; }
        public string AgentPartyPersonSchemeName { get; set; }
        public string ReceiverPartyLegalEntityName { get; set; }
        public string ReceiverPartyLegalEntityCompanyID { get; set; }
        public string SenderPartyPowerOfAttorneySchemeID { get; set; }
        public string SenderPartyPowerOfAttorneySchemeName { get; set; }
        public string SenderPartyPowerOfAttorneyID { get; set; }
        public string SenderPartyPersonSchemeID { get; set; }
        public string SenderPartyPersonSchemeName { get; set; }
        public string SenderPartyPersonID { get; set; }
        public string InformacionTransferenciaDerechos { get; set; }
        public string PrecioPagarseInfoTransDerechos { get; set; }
        public string FactordeDescuentoInfoTransDerechos { get; set; }
        public string MedioPagoInfoTransDerechos { get; set; }
        public string InformacionPagoTransferencia { get; set; }
        public string ValorPendienteInfoPagoTrans { get; set; }
        public string ResponseEffectiveDate { get; set; }
        public string ValorTotalInformacionProtesto { get; set; }
        public string ValorAceptadoInformacionProtesto { get; set; }
        public string ValorPendienteInformacionProtesto { get; set; }

        public NitModel() { }

        public NitModel(string _listID, string _validityPeriodEndDate, string _documentTypeIdRef, 
            string _issuerPartyCode, string _issuerPartyName, string _providerCode, string _serieAndNumber, string _documentTypeId,
            string _senderCode)
        {
            listID = _listID;
            ValidityPeriodEndDate = _validityPeriodEndDate;
            DocumentTypeIdRef = _documentTypeIdRef;
            IssuerPartyCode = _issuerPartyCode;
            IssuerPartyName = _issuerPartyName;
            ProviderCode = _providerCode;
            SerieAndNumber = _serieAndNumber;
            DocumentTypeId = _documentTypeId;
            SenderCode = _senderCode;
        }
    }
}

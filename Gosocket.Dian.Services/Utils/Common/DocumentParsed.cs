namespace Gosocket.Dian.Services.Utils.Common
{
    public class DocumentParsed
    {
        public string Cude { get; set; }
        public string DocumentKey { get; set; }
        public string DocumentTypeId { get; set; }
        public string Number { get; set; }
        public string ResponseCode { get; set; }
        public string ReceiverCode { get; set; }
        public string SenderCode { get; set; }
        public string Serie { get; set; }
        public string SerieAndNumber { get; set; }
        public string CustomizationId { get; set; }
        public string listID { get; set; }
        public string DocumentID { get; set; }
        public string UBLVersionID { get; set; }
        public string DocumentTypeIdRef { get; set; }
        public string IssuerPartyCode { get; set; }
        public string IssuerPartyName { get; set; }
        public string ProviderCode { get; set; }
        public string ValidityPeriodEndDate { get; set; }
        public string SigningTime { get; set; }


        public static void SetValues(ref DocumentParsed documentParsed)
        {
            documentParsed.Number = documentParsed.SerieAndNumber;
            documentParsed.DocumentKey = documentParsed?.DocumentKey?.ToString()?.ToLower();
            documentParsed.CustomizationId = documentParsed?.CustomizationId;
            documentParsed.listID = documentParsed?.listID;
            documentParsed.DocumentID = documentParsed?.DocumentID;
            documentParsed.DocumentTypeIdRef = documentParsed?.DocumentTypeIdRef;
            documentParsed.IssuerPartyName = documentParsed?.IssuerPartyName;
            documentParsed.IssuerPartyCode = documentParsed?.IssuerPartyCode;
            documentParsed.ProviderCode = documentParsed?.ProviderCode;
            documentParsed.ValidityPeriodEndDate = documentParsed?.ValidityPeriodEndDate;
        }
    }
}

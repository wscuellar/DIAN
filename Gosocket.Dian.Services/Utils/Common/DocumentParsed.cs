﻿namespace Gosocket.Dian.Services.Utils.Common
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

        public static void SetValues(ref DocumentParsed documentParsed)
        {
            documentParsed.Number = documentParsed.SerieAndNumber;
            documentParsed.DocumentKey = documentParsed?.DocumentKey?.ToString()?.ToLower();
            documentParsed.CustomizationId = documentParsed?.CustomizationId;
        }
    }
}

namespace Gosocket.Dian.Services.Utils.Common
{
    public class DocumentParsed
    {
        public string DocumentKey { get; set; }
        public string DocumentTypeId { get; set; }
        public string Number { get; set; }
        public string SenderCode { get; set; }
        public string Serie { get; set; }
        public string SerieAndNumber { get; set; }

        public static void SetValues(ref DocumentParsed documentParsed)
        {
            documentParsed.Number = documentParsed.SerieAndNumber;
            if (!string.IsNullOrEmpty(documentParsed.Serie)) documentParsed.Number = documentParsed.SerieAndNumber.Replace(documentParsed.Serie, string.Empty);

            documentParsed.DocumentKey = documentParsed?.DocumentKey?.ToString()?.ToLower();
        }
    }
}

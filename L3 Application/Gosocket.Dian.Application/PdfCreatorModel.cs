using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class PdfCreatorModel
    {
        //Sección Membrete
        public string PathLogo { get; set; }
        public string HeaderText { get; set; }
        public DateTime Printdate { get; set; }
        public string ElectronicInvoiceNumber { get; set; }
        public string CUFE { get; set; }
        public DateTime ElectronicInvoiceDate { get; set; }
        public string CurrentStatus { get; set; }

        //Información factura
        public string SenderName { get; set; }
        public string SenderCode { get; set; }
        public float TotalAmount { get; set; }
        public string ExpirationDate { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverCode { get; set; }

        //Events
        public ICollection<PdfCreatorEvent> PdfCreatorEvents { get; set; }

        //Documentos asociados
        public int TotalDocuments { get; set; }
        public string TotalEvents { get; set; }

        //Sección fin del pdf
        public string FooterText { get; set; }
        public DateTime FooterDate { get; set; }
        public string FinalText { get; set; }

    }
}

using System.ComponentModel;

namespace Gosocket.Dian.Functions.Models
{
    public class DocumentExported
    {
        [DisplayName("Tipo de Documento")]
        public string DocumentTypeName { get; set; }

        [DisplayName("Key")]
        public string DocumentKey { get; set; }

        [DisplayName("Folio")]
        public string Number { get; set; }

        [DisplayName("Prefijo")]
        public string Serie { get; set; }

        [DisplayName("Fecha Emisión")]
        public string EmissionDate { get; set; }

        [DisplayName("Fecha Recepción")]
        public string ReceptionDate { get; set; }

        [DisplayName("NIT Emisor")]
        public string SenderCode { get; set; }

        [DisplayName("Nombre Emisor")]
        public string SenderName { get; set; }

        [DisplayName("NIT Receptor")]
        public string ReceiverCode { get; set; }

        [DisplayName("Nombre Receptor")]
        public string ReceiverName { get; set; }

        [DisplayName("IVA")]
        public double TotalIVA { get; set; }

        [DisplayName("ICA")]
        public double TotalICA { get; set; }

        [DisplayName("IPC")]
        public double TotalIPC { get; set; }

        [DisplayName("Monto Total")]
        public double TotalAmount { get; set; }

        [DisplayName("Estado")]
        public string Status { get; set; }

        [DisplayName("Grupo")]
        public string Group { get; set; }


        //[DisplayName("PDF")]
        //public string PdfLink { get; set; }
    }
}

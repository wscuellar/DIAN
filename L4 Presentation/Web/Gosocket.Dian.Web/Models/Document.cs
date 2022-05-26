using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class SearchDocumentViewModel
    {
        [Display(Name = "CUFE")]
        public string DocumentKey { get; set; }

        [Display(Name = "NIT emisor")]
        public string SenderCode { get; set; }

        [Display(Name = "NIT receptor")]
        public string ReceiverCode { get; set; }

        [Display(Name = "Prefijo y folio")]
        public string SerieAndNumber { get; set; }

        [Display(Name = "Resultado")]
        public List<DocumentStatusViewModel> Statuses { get; set; }

        [Display(Name = "Tipo de documento")]
        public string DocumentTypeId { get; set; }

        [Display(Name = "Tipo de referencia")]
        public string ReferencesType { get; set; }

        [Display(Name = "Estado RADIAN")]
        public List<DocumentRadianStatusViewModel> RadianStatusList { get; set; }


        public int ContributorTypeId { get; set; }
        public int Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RadianStatus { get; set; }
        public int Page { get; set; }
        public bool IsNextPage { get; set; }
        public int MaxItemCount { get; set; }
        public List<DocumentViewModel> Documents { get; set; }
        public List<DocumentTypeModel> DocumentTypes { get; set; }

        public SearchDocumentViewModel()
        {
            MaxItemCount = 10;
            IsNextPage = false;
            Page = 0;
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); ;
            EndDate = DateTime.Now;
            Status = 0;
            DocumentTypeId = "00";
            ReferencesType = "00";
            Documents = new List<DocumentViewModel>();
            DocumentTypes = DocumentTypeModel.List();
            Statuses = DocumentStatusViewModel.List();
            RadianStatusList = DocumentRadianStatusViewModel.List();
        }
    }

    public class DocumentViewModel
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public List<DocumentTagViewModel> DocumentTags { get; set; }
        public List<EventViewModel> Events { get; set; }
        public List<ReferenceViewModel> References { get; set; }
        public TaxesDetailViewModel TaxesDetail { get; set; }
        public DateTime EmissionDate { get; set; }
        public string DocumentKey { get; set; }
        public string DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; }
        public string Number { get; set; }
        public string Serie { get; set; }
        public string SerieAndNumber { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string TechProviderCode { get; set; }
        public string TechProviderName { get; set; }
        public DateTime GenerationDate { get; set; }
        public DateTime ReceptionDate { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string RadianStatusName { get; set; }
        public double Amount { get; set; }
        public double TaxAmountIva5Percent { get; set; }
        public double TaxAmountIva19Percent { get; set; }
        public double TaxAmountIva { get; set; }
        public double TaxAmountIca { get; set; }
        public double TaxAmountIpc { get; set; }
        public double TotalAmount { get; set; }

        public DocumentViewModel()
        {
            DocumentTags = new List<DocumentTagViewModel>();
            Events = new List<EventViewModel>();
            References = new List<ReferenceViewModel>();
            TaxesDetail = new TaxesDetailViewModel();
        }
    }

    public class DocumentTagViewModel
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class DocumentTypeModel
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static List<DocumentTypeModel> List()
        {
            return new List<DocumentTypeModel>()
            {
                new DocumentTypeModel() { Code = "00", Name = "Todos..." },
                new DocumentTypeModel() { Code = "01", Name = "Factura electrónica" },
                new DocumentTypeModel() { Code = "02", Name = "Factura de exportación electrónica" },
                new DocumentTypeModel() { Code = "03", Name = "Factura de contingencia electrónica" },
                new DocumentTypeModel() { Code = "04", Name = "Factura de contingencia electrónica DIAN" },
                new DocumentTypeModel() { Code = "05", Name = "Documento Soporte" },
                new DocumentTypeModel() { Code = "07", Name = "Nota de crédito electrónica" },
                new DocumentTypeModel() { Code = "08", Name = "Nota de débito electrónica" },
                new DocumentTypeModel() { Code = "09", Name = "Factura AIU" },
                new DocumentTypeModel() { Code = "11", Name = "Factura Mandato" },
                new DocumentTypeModel() { Code = "20", Name = "Documento equivalente POS electrónico" },
                new DocumentTypeModel() { Code = "50", Name = "Documento equivalente Transporte Aereo" },
                new DocumentTypeModel() { Code = "32", Name = "Documento equivalente De Juegos localizado" },
                new DocumentTypeModel() { Code = "35", Name = "Documento equivalente Transporte Pasajeros Terrestre" },
                new DocumentTypeModel() { Code = "40", Name = "Documento equivalente Cobro Peaje" },
                new DocumentTypeModel() { Code = "27", Name = "Documento equivalente Espectaculos publicos" },
                new DocumentTypeModel() { Code = "60", Name = "Documento equivalente Servicios Publicos y Domiciliarios" },
                new DocumentTypeModel() { Code = "55", Name = "Documento equivalente Bolsa de valor y agropecuario" },
                new DocumentTypeModel() { Code = "45", Name = "Documento equivalente Extractos" },
                new DocumentTypeModel() { Code = "94", Name = "Documento equivalente Nota de ajuste" },
                new DocumentTypeModel() { Code = "030", Name = "Acuse de recibo de la FEV" },
                new DocumentTypeModel() { Code = "031", Name = "Reclamo de la FEV" },
                new DocumentTypeModel() { Code = "032", Name = "Recibo del bien o prestación del servicio" },
                new DocumentTypeModel() { Code = "033", Name = "Aceptacion expresa" },
                new DocumentTypeModel() { Code = "034", Name = "Aceptacion tácita" },
                new DocumentTypeModel() { Code = "101", Name = "Documento importación" }
               // new DocumentTypeModel() { Code = "102", Name = "Nomina Individual" },
                //new DocumentTypeModel() { Code = "103", Name = "Nomina Individual de ajuste" },
            };
        }
    }

    public class EventViewModel
    {
        public string DocumentKey { get; set; }
        public DateTime Date { get; set; }
        public int DateNumber { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string CustomizationID { get; set; }
        public string prefijo { get; set; }
    }

    public class ReferenceViewModel
    {
        public string DocumentTypeId { get; set; }
        public string DocumenTypeName { get; set; }
        public DateTime Date { get; set; }
        public int DateNumber { get; set; }
        public DateTime TimeStamp { get; set; }
        public string DocumentKey { get; set; }
        public string Description { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }

        //Especifica si se muestra en la lista de documentos
        //      referenciados por el presente documento
        public bool ShowAsReference { get; set; }
    }

    public class TaxesDetailViewModel
    {
        public double TaxAmountIva5Percent { get; set; }
        public double TaxAmountIva14Percent { get; set; }
        public double TaxAmountIva16Percent { get; set; }
        public double TaxAmountIva19Percent { get; set; }
        public double TaxAmountIva { get; set; }
        public double TaxAmountIca { get; set; }
        public double TaxAmountIpc { get; set; }
    }

    public class DocumentStatusViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static List<DocumentStatusViewModel> List()
        {
            return new List<DocumentStatusViewModel>()
            {
                new DocumentStatusViewModel() { Code = "0", Name = "Todos..." },
                new DocumentStatusViewModel() { Code = "1", Name = "Aprobado" },
                new DocumentStatusViewModel() { Code = "10", Name = "Aprobado con notificación" }
            };
        }
    }

    public class DocumentRadianStatusViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        
        public static List<DocumentRadianStatusViewModel> List()
        {
            return new List<DocumentRadianStatusViewModel>()
            {
                new DocumentRadianStatusViewModel() { Code = "0", Name = "Todos..." },
                new DocumentRadianStatusViewModel() { Code = "1", Name = "Titulo valor" },
                new DocumentRadianStatusViewModel() { Code = "2", Name = "Disponibilizada" },
                new DocumentRadianStatusViewModel() { Code = "3", Name = "Endosada" },
                new DocumentRadianStatusViewModel() { Code = "4", Name = "Pagada" },
                new DocumentRadianStatusViewModel() { Code = "5", Name = "Limitada" },
                new DocumentRadianStatusViewModel() { Code = "6", Name = "Factura Electronica" },               
                new DocumentRadianStatusViewModel() { Code = "7", Name = "Transferida"},
                new DocumentRadianStatusViewModel() { Code = "8", Name = "Protestada" },
                new DocumentRadianStatusViewModel() { Code = "9", Name = "No Aplica" }
            };
        }
    }
}
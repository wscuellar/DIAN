using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocAssociate : TableEntity
    {
        public GlobalDocAssociate() { }

        public GlobalDocAssociate(string pk, string rk) : base(pk, rk)
        {

        }

        public bool Active { get; set; }
        public string Identifier { get; set; }
        public string EventCode { get; set; }
        public string CustomizationID { get; set; }
        public DateTime? SigningTimeStamp { get; set; }
        public string SerieAndNumber { get; set; }
        public string TestSetId { get; set; }
        public bool SendTestSet { get; set; }

    }

    public class EventDocument
    {
        public EventDocument()
        {
            Notifications = new List<GlobalDocValidatorTracking>();
        }
        /// <summary>
        /// Identificador de la factura
        /// </summary>
        public string Cufe { get; set; }
        /// <summary>
        /// Si tiene validaciones o no (true/false)
        /// </summary>
        public bool IsNotifications { get; set; }
        /// <summary>
        /// Eventos asociado a la factura
        /// </summary>
        public GlobalDocAssociate Associate { get; set; }
        /// <summary>
        /// Informacion general del evento
        /// </summary>
        public GlobalDocValidatorDocumentMeta DocumentMeta { get; set; }
        /// <summary>
        /// Asociacion de referencia al mandato.
        /// </summary>
        public GlobalDocReferenceAttorney Attorney { get; set; }
        /// <summary>
        /// Informacion resumen de la validacion
        /// </summary>
        public GlobalDocValidatorDocument ValidateDocument { get; set; }
        /// <summary>
        /// Validaciones
        /// </summary>
        public List<GlobalDocValidatorTracking> Notifications { get; set; }

    }


    public class InvoiceWrapper
    {
        public string Cufe { get; set; }
        public GlobalDocValidatorDocumentMeta Invoice { get; set; }
        public List<EventDocument> Documents { get; set; }
    }

}

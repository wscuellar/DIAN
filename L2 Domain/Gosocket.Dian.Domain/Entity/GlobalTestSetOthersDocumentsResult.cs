using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalTestSetOthersDocumentsResult : TableEntity
    {
        public GlobalTestSetOthersDocumentsResult() { }

        public GlobalTestSetOthersDocumentsResult(string pk, string rk) : base(pk, rk) { }

        public int OtherDocElecContributorId { get; set; }
        public string SenderCode { get; set; }
        public string SoftwareId { get; set; }
        public string ContributorTypeId { get; set; }
        public string OperationModeId { get; set; }
        public string OperationModeName { get; set; }
        public int ElectronicDocumentId { get; set; }
        public int ProviderId { get; set; }

        //OthersDocuments
        public int OthersDocumentsRequired { get; set; }
        public int OthersDocumentsAcceptedRequired { get; set; }
        public int TotalOthersDocumentsSent { get; set; }
        public int OthersDocumentsAccepted { get; set; }
        public int OthersDocumentsRejected { get; set; }
        //End OthersDocuments

        //ElectronicPayrollAjustment
        /// <summary>
        /// Nomina electrónica de Ajuste
        /// </summary>
        public int ElectronicPayrollAjustmentRequired { get; set; }
        /// <summary>
        /// Nomina electrónica de Ajuste Aceptada requerida
        /// </summary>
        public int ElectronicPayrollAjustmentAcceptedRequired { get; set; }
        public int TotalElectronicPayrollAjustmentSent { get; set; }
        public int ElectronicPayrollAjustmentAccepted { get; set; }
        public int ElectronicPayrollAjustmentRejected { get; set; }
        //End ElectronicPayrollAjustment

        //TotalDocument
        public int TotalDocumentAcceptedRequired { get; set; }
        public int TotalDocumentRequired { get; set; }
        public int TotalDocumentSent { get; set; }
        public int TotalDocumentAccepted { get; set; }
        public int TotalDocumentsRejected { get; set; }
        //End TotalDocument

        public string StatusDescription { get; set; }
        public int Status { get; set; }
        public bool Deleted { get; set; }
        public string Id { get; set; }
        // Estado: En Proceso, etc.
        public string State { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RangeDate => (StartDate.HasValue && EndDate.HasValue) ? $"{StartDate:dd/MM/yyyy hh:mm:ss tt} - {EndDate:dd/MM/yyyy hh:mm:ss tt}" : "";

        public int? EquivalentElectronicDocumentId { get; set; }
    }
}
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class OthersElectronicDocAssociatedViewModel
    {
        public OthersElectronicDocAssociatedViewModel()
        {
            PageTable = 1;
            LegalRepresentativeList = new List<UserViewModel>();
        }

        public int Id { get; set; }
        public int Step { get; set; }

        public int ContributorId { get; set; }

        [Display(Name = "Tipo de participante")]
        public int ContributorTypeId { get; set; }

        [Display(Name = "NIT")]
        public string Nit { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Razón Social")]
        public string BusinessName { get; set; }

        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Display(Name = "Estado de aprobación")]
        public string State { get; set; }

        public string OperationMode { get; set; }
        public int OperationModeId { get; set; }
        public bool OperationModeIsFree => OperationModeId == (int)OtherDocElecOperationMode.FreeBiller;
        public string ElectronicDoc { get; set; }
        public int ElectronicDocId { get; set; }
        [Display(Name = "Tipo de participante")]
        public string ContributorType { get; set; }
        public string SoftwareId { get; set; }
        public Guid SoftwareIdBase { get; set; }
        public int ProviderId { get; set; }
        public OtherDocElecSoftwareViewModel Software { get; set; }
        public OtherDocElecNumberingRangeViewModel NumberingRange { get; set; }
        public GlobalTestSetOthersDocumentsResult GTestSetOthersDocumentsResult { get; set; }

        public bool EsSupportDocument { get; set; }
        public bool EsEquivalentDocument { get; set; }
        public bool EsElectronicDocNomina { get; set; }
        public bool EsElectronicDocNominaNoOFE { get; set; }
        public string TitleDoc1 { get; set; }
        public string TitleDoc2 { get; set; }

        public int PageTable { get; set; }
        public int CustomerTotalCount { get; internal set; }

        [Display(Name = "Estado de aprobación")]
        public OtherDocElecState StateSelect { get; set; }
        [Display(Name = "NIT de Emisor")]
        public string NitSearch { get; set; }
        public List<OtherDocElecCustomerListViewModel> Customers { get; set; }
        public List<UserViewModel> LegalRepresentativeList { get; set; }
    }

    public class OtherDocElecNumberingRangeViewModel
    {
        public OtherDocElecNumberingRangeViewModel(
            string prefix, 
            string resolucionNumber, 
            long numberFrom, 
            long numberTo, 
            string creationDate, 
            string expirationDate)
        {
            Prefix = prefix;
            ResolucionNumber = resolucionNumber;
            NumberFrom = numberFrom;
            NumberTo = numberTo;
            CreationDate = creationDate;
            ExpirationDate = expirationDate;
        }

        public string Prefix { get; set; }
        public string ResolucionNumber { get; set; }
        public long NumberFrom { get; set; }
        public long NumberTo { get; set; }
        public string CreationDate { get; set; }
        public string ExpirationDate { get; set; }
    }
}
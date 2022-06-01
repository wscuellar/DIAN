using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class ContributorTableViewModel
    {
        public ContributorTableViewModel()
        {
            Page = 0;
            Length = 10;
            Contributors = new List<ContributorViewModel>();
        }
        public int Page { get; set; }
        public int Length { get; set; }

        public bool SearchFinished { get; set; }
        public List<ContributorViewModel> Contributors { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "NIT empresa")]
        [Required(ErrorMessage = "NIT empresa es requerido.")]
        public string Code { get; set; }
        public string Type { get; set; }
    }
    public class ContributorViewModel
    {

        public ContributorViewModel()
        {
            CanEdit = false;
            TestSetResultRejected = new ContributorTestSetResultViewModel();
            AcceptanceStatuses = new List<ContributorAcceptanceStatusViewModel>();
            OperationModes = new List<OperationModeViewModel>();
            Providers = new List<ProviderViewModel>();
            ContributorFiles = new List<ContributorFileViewModel>();
            ContributorFileTypes = new List<ContributorFileTypeViewModel>();
            Softwares = new List<SoftwareViewModel>();
            ContributorTestSets = new List<GlobalTestSet>();
            ContributorOperations = new List<ContributorOperationsViewModel>();
            Users = new List<UserViewModel>();
        }

        public int Id { get; set; }

        [Display(Name = "NIT")]
        [Required(ErrorMessage = "NIT es requerido")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Razón social es requerida")]
        [Display(Name = "Razón social")]
        public string BusinessName { get; set; }

        [Display(Name = "Correo electrónico")]
        //[EmailAddress(ErrorMessage = "Debe ingresar un Correo Electrónico válido.")]
        public string Email { get; set; }

        [Display(Name = "Correo electrónico para recepción de facturas")]
        [EmailAddress(ErrorMessage = "Por favor registre un correo electrónico válido.")]
        public string ExchangeEmail { get; set; }

        [Display(Name = "Correo electrónico para recepción")]
        [EmailAddress(ErrorMessage = "Por favor registre un correo electrónico válido.")]
        public string NoOFeEmail { get; set; }

        [Display(Name = "Fecha inicio autorización")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Fecha término autorización")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Fecha de inicio producción")]
        public string ProductionDate { get; set; }

        public int? ContributorFileTypeId { get; set; }

        [Display(Name = "Modo de operación")]
        public int? OperationModeId { get; set; }

        [Display(Name = "Proveedor")]
        public int? ProviderId { get; set; }

        [Display(Name = "Tipo de facturador")]
        public string BillerType { get; set; }

        [Display(Name = "Fecha máxima de registro")]
        public string MaxRegisterDate { get; set; }

        [Display(Name = "Fecha máxima de inicio")]
        public string MaxStartDate { get; set; }

        [Display(Name = "Número de resolución")]
        public int ResolutionNumber { get; set; }

        [Display(Name = "Fecha de resolución")]
        public string ResolutionDate { get; set; }

        [Display(Name = "Código actividad principal")]
        public string PrincipalActivityCode { get; set; }

        public bool Status { get; set; }
        public bool Deleted { get; set; }

        [Display(Name = "Estado de aprobación")]
        public int AcceptanceStatusId { get; set; }

        [Display(Name = "Estado de aprobación")]
        public string AcceptanceStatusName { get; set; }

        public int ContributorTypeId { get; set; }

        public bool CanEdit { get; set; }

        public int? TestSetStatus { get; set; }

        public bool IsNew { get; set; }

        public ContributorTestSetResultViewModel TestSetResultRejected { get; set; }

        [Display(Name = "Software")]
        public Guid SoftwareId{ get; set; }
        public SoftwareViewModel Software { get; set; }

        public List<OperationModeViewModel> OperationModes { get; set; }

        public List<ProviderViewModel> Providers { get; set; }

        [Display(Name = "Softwares")]
        public List<SoftwareViewModel> Softwares { get; set; }

        public List<UserViewModel> Users { get; set; }

        public List<ContributorFileViewModel> ContributorFiles { get; set; }

        public List<ContributorAcceptanceStatusViewModel> AcceptanceStatuses { get; set; }

        [Display(Name = "Archivos no mandatorios")]
        public List<ContributorFileTypeViewModel> ContributorFileTypes { get; set; }

        [Display(Name = "Estado")]
        public List<ContributorFileStatusViewModel> FileStatuses { get; internal set; }
        public List<GlobalTestSet> ContributorTestSets { get; set; }
        public List<ContributorOperationsViewModel> ContributorOperations { get; set; }
        public List<TestSetResultViewModel> ContributorTestSetResults { get; set; }

        public List<OperationModeViewModel> GetOperationModes()
        {
            return new List<OperationModeViewModel>
            {
                new OperationModeViewModel{ Id = 1, Name = "Software gratuito" },
                new OperationModeViewModel{ Id = 2, Name = "Software propio" },
                new OperationModeViewModel{ Id = 3, Name = "Software de un proveedor tecnológico" }
            };
        }
    }

    public class ContributorAcceptanceStatusViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class ContributorFileViewModel
    {
        public Guid Id { get; set; }

        public ContributorFileTypeViewModel ContributorFileType { get; set; }

        public string FileName { get; set; }

        public bool Deleted { get; set; }

        public ContributorFileStatusViewModel ContributorFileStatus { get; set; }

        public string Comments { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime Updated { get; set; }

        public string CreatedBy { get; set; }

        public bool IsNew { get; set; }
    }

    public class ContributorFileHistoryViewModel
    {
        public string FileName { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string User { get; set; }
        public string Comments { get; set; }
    }

    public class ContributorFileStatusViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ContributorUploadFileViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public Guid FileId { get; set; }
        public int FileTypeId { get; set; }
        public string FileTypeName { get; set; }
    }

    public class AcceptanceStatusViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class ContributorTestSetViewModel
    {
        public Guid TestSetId { get; set; }
        public bool TestSetReplace { get; set; }

        [Display(Name = "NIT Proveedor Tecnológico")]
        public string PaCode { get; set; }

        [Display(Name = "Nombre del Set de Prueba")]
        public string Name { get; set; }

        [Display(Name = "Descripción del Set de Prueba")]
        public string Description { get; set; }

        [Display(Name = "Fecha de inicio del Set de Prueba")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Fecha de fin del Set de Prueba")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Total de Documentos del Set de Prueba")]
        public int DocumentsTotal { get; set; }

        [Display(Name = "Total de Documentos correctos del Set de Prueba")]
        public int DocumentsTotalOk { get; set; }

        public string Category { get; set; }
        public string DocumentTypeCode { get; set; }
        public DateTime Date { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Active { get; set; }
        public bool Completed { get; set; }
    }

    public class ContributorTestSetResultViewModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public int Status { get; set; }
    }
}




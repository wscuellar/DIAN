using ExpressiveAnnotations.Attributes;
using Gosocket.Dian.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class RequestAuthorizationBigContributorTableViewModel
    {
        public RequestAuthorizationBigContributorTableViewModel()
        {
            Page = 0;
            Length = 10;
            SearchFinished = false;
            Requests = new List<RequestAuthorizationBigContributorViewModel>();
        }

        public int Page { get; set; }
        public int Length { get; set; }
        public bool SearchFinished { get; set; }
        public List<RequestAuthorizationBigContributorViewModel> Requests { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "NIT Facturador")]
        public string ContributorCode { get; set; }
    }
    public class RequestAuthorizationBigContributorViewModel
    {
        public RequestAuthorizationBigContributorViewModel()
        {
            AuthorizationRequestAlreadyExist = false;
            StatusCode = (int)BigContributorAuthorizationStatus.Unregistered;
            StatusDescription = EnumHelper.GetEnumDescription(BigContributorAuthorizationStatus.Unregistered);
            Trackings = new List<RequestAuthorizationBigContributorTrackingViewModel>();
        }

        public DateTime RequestDate { get; set; }

        [Display(Name = "NIT Facturador")]
        public string ContributorCode { get; set; }

        [Display(Name = "Nombre Facturador")]
        public string ContributorName { get; set; }

        public int StatusCode { get; set; }
        [Display(Name = "Estado Solicitud")]
        public string StatusDescription { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Justificación Solicitud")]
        [Required(ErrorMessage = "Justificación de solicitud es requerida.")]
        public string Justification { get; set; }

        [Display(Name = "Cantidad Min. Facturas por Lote")]
        [Required(ErrorMessage = "Cantidad mínima de facturas por lote es requerida.")]
        [Range(2, 500, ErrorMessage = "Cantidad mínima de facturas por lote debe ser entre {1} y {2}.")]
        public int MinimumInvoices { get; set; }

        [Display(Name = "Cantidad Max. Facturas por Lote")]
        [Required(ErrorMessage = "Cantidad máxima de facturas por lote es requerida.")]
        [Range(2, 500, ErrorMessage = "Cantidad máxima de facturas por lote debe ser entre {1} y {2}.")]
        public int MaximumInvoices { get; set; }

        public bool AuthorizationRequestAlreadyExist { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Observaciones")]
        [RequiredIf("StatusCode == 4", ErrorMessage = "Debe ingrezar una observación si la solicitud es rechazada.")]
        public string Observations { get; set; }

        public List<RequestAuthorizationBigContributorTrackingViewModel> Trackings { get; set; }
    }

    public class RequestAuthorizationBigContributorTrackingViewModel
    {
        public string ContributorCode { get; set; }
        public string User { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string Observations { get; set; }
        public DateTime Date { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models
{
    public class DocValidatorModel
    {
        public DocValidatorModel()
        {
            Validations = new List<DocValidatorTrackingModel>();
            References = new List<ReferenceViewModel>();
        }
        public DocumentViewModel Document { get; set; }
        public List<DocValidatorTrackingModel> Validations { get; set; }
        public List<ReferenceViewModel> References { get; set; }
        public List<EventsViewModel> Events { get; set; }
        public Dictionary<int, string> IconsData { get; set; }
        public string LegitimateOwner { get; internal set; }
        public DateTime? DateInscription { get; internal set; }
    }

    public class DocValidatorTrackingModel
    {
        public int Priority { get; set; }
        public string ErrorMessage { get; set; }
        public bool Mandatory { get; set; }
        public bool IsValid { get; set; }
        public bool IsNotification { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
    }
}
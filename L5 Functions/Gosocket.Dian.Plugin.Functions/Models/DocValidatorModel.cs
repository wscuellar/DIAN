using System.Collections.Generic;

namespace Gosocket.Dian.Plugin.Functions.Models
{
    public  class DocValidatorModel
    {
        public DocValidatorModel()
        {
            Validations = new List<DocValidatorTrackingModel>();
            //References = new List<ReferenceViewModel>();
        }
        public DocumentViewModel Document { get; set; }
        public List<DocValidatorTrackingModel> Validations { get; set; }
        //public List<ReferenceViewModel> References { get; set; }
    }
}

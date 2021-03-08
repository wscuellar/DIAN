using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models
{
    public class ProviderViewModel
    {
        public ProviderViewModel()
        {
            Softwares = new List<SoftwareViewModel>();
        }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public List<SoftwareViewModel> Softwares { get; set; }
    }
}
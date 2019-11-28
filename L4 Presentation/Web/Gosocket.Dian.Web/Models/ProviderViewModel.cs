using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
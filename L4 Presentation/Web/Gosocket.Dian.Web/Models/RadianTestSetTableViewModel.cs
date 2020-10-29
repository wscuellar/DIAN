using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models
{
    public class RadianTestSetTableViewModel
    {
        public RadianTestSetTableViewModel()
        {
            RadianTestSets = new List<RadianTestSetViewModel>();
        }
        public List<RadianTestSetViewModel> RadianTestSets { get; set; }
    }
}
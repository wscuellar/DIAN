using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models
{
    public class MenuViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public List<SubMenuViewModel> Options { get; set; }
    }
}
using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models.FreeBiller
{
    public class MenuOptionsModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ParentId { get; set; }

        public int MenuLevel { get; set; }

        public bool IsActive { get; set; }

        public List<Children> Children { get; set; }

    }
    public class Children
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ParentId { get; set; }

        public int MenuLevel { get; set; }

        public bool IsActive { get; set; }

        public List<Children> grandchild { get; set; }
    }
}
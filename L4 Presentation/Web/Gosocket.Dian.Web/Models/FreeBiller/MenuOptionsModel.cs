namespace Gosocket.Dian.Web.Models.FreeBiller
{
    public class MenuOptionsModel
    {
        public int MenuId { get; set; }

        public string Name { get; set; }

        public int? FatherId { get; set; }

        public int Level { get; set; }

        public bool IsChecked { get; set; }
    }
}
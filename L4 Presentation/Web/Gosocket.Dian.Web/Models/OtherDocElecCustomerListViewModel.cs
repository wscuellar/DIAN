namespace Gosocket.Dian.Web.Models
{
    public class OtherDocElecCustomerListViewModel
    {
        public OtherDocElecCustomerListViewModel()
        {
            Lenght = 10;
            Page = 1;
        }
        public int Lenght { get; set; }
        public int Page { get; set; }
        public string Nit { get; set; }
        public string BussinessName { get; set; }
        public string State { get; set; }
    }
}
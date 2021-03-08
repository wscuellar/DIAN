namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianCustomerViewModel
    {
        public RadianCustomerViewModel()
        {
            Lenght = 10;
            Page = 1;
        }
        public int Lenght { get; set; }
        public int Page { get; set; }
        public string Nit { get; set; }
        public string BussinessName { get; set; }
        public string RadianState { get; set; }
    }
}
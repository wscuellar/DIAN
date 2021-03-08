namespace Gosocket.Dian.Domain.Entity
{
    public class RadianCustomerList
    {
        public RadianCustomerList()
        {
        }

        public int Id { get; set; }
        public string BussinessName { get; set; }
        public string Nit { get; set; }
        public string RadianState { get; set; }
        public int Page { get; set; }
        public int Length { get; set; }
    }
}

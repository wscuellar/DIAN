namespace Gosocket.Dian.Domain
{
    [System.ComponentModel.DataAnnotations.Schema.Table("AcceptanceStatus")]
    public class AcceptanceStatus
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}

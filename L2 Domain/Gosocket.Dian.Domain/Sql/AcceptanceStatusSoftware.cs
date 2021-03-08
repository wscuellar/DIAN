namespace Gosocket.Dian.Domain
{
    [System.ComponentModel.DataAnnotations.Schema.Table("AcceptanceStatusSoftware")]
    public class AcceptanceStatusSoftware
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        
        public string Name { get; set; }
    }
}

namespace Gosocket.Dian.Domain
{
    [System.ComponentModel.DataAnnotations.Schema.Table("ContributorType")]
    public class ContributorType
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        
        public string Name { get; set; }

    }

}

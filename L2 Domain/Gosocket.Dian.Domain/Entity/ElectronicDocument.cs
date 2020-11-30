using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain.Entity
{
    /// <summary>
    /// Otros documentos electronicos para configurar set de pruebas
    /// </summary>
    [Table("ElectronicDocument")]
    public class ElectronicDocument
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class SearchViewModel
    {
        [DataType(DataType.Text)]
        [Display(Name = "Ingrese CUFE o UUID")]
        [Required(ErrorMessage = "CUFE o UUID es requerido.")]
        public string DocumentKey { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace SistemaCitasConsultorioDental.Models
{
    public class Aseguradoras
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string aseguradora { get; set; }

    }
}

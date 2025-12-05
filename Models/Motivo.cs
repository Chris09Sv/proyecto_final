using System.ComponentModel.DataAnnotations;

namespace SistemaCitasConsultorioDental.Models
{
    public class Motivo
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Descripcion { get; set; }
    }
}
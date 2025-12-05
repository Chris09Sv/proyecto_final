using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCitasConsultorioDental.Models
{
    public class Dentista
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Apellido { get; set; }
        [Required]
        public string Especialidad { get; set; }
        [Required]
        public string Telefono { get; set; }
        [Required]
        public string Email { get; set; }
        [NotMapped]
        public string? ResumenHorario { get; set; }

        [Required]
        [MaxLength(11)]
        public string Identificacion { get; set; }
    }
}
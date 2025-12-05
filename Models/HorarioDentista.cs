using System.ComponentModel.DataAnnotations;

namespace SistemaCitasConsultorioDental.Models
{
    public class HorarioDentista
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DentistaId { get; set; }
        public Dentista? Dentista { get; set; }

        [Required]                  
        public DayOfWeek DiaSemana { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCitasConsultorioDental.Models
{
    public class Cita
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Paciente")]
        public int PacienteId { get; set; }
        public Paciente? Paciente { get; set; }

        [Required]
        [Display(Name = "Dentista")]
        public int DentistaId { get; set; }
        public Dentista? Dentista { get; set; }

        [Required]
        [Display(Name = "Motivo")]
        public int MotivoId { get; set; }
        public Motivo? Motivo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        [Required]
        [Range(10, 480)]
        [Display(Name = "Duración (min)")]
        public int DuracionMinutos { get; set; } = 30;

        [NotMapped]
        public DateTime Inicio => Fecha.Date + Hora;

        [NotMapped]
        public DateTime Fin => Inicio.AddMinutes(DuracionMinutos);

        [NotMapped]
        [Display(Name = "Días / Horas restantes")]
        public string TiempoRestante
        {
            get
            {
                var ahora = DateTime.Now;
                var dif = Inicio - ahora;
                if (dif.TotalSeconds <= 0) return "0 días 0 horas";

                return $"{dif.Days} días {dif.Hours} horas";
            }
        }

        [NotMapped]
        public string Estado
        {
            get
            {
                var ahora = DateTime.Now;
                if (ahora < Inicio) return "Vigente";
                if (ahora >= Inicio && ahora <= Fin) return "En proceso";
                return "Finalizado";
            }
        }
    }
}
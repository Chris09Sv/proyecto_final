using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace SistemaCitasConsultorioDental.Models
{
    public class Paciente
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }
        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; }
        [Required]
        [Phone]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener 10 dígitos.")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",ErrorMessage = "El correo electrónico no es válido.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria. Debe seleccionar una fecha de nacimiento valida.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        [CustomValidation(typeof(PacientesValidations), nameof(PacientesValidations.ValidarEdadMinima))]
        public DateTime FechaNacimiento { get; set; }
        [Required]
        [MaxLength(200)]
        [MinLength(30)]
        public string? Direccion { get; set; }


        [MaxLength(11)]
        public string? Identificacion { get; set; }

        public bool SinSeguro { get; set; }

        public string? NumeroSeguroSocial { get; set; }
        public string? SeguroMedico { get; set; }
        [NotMapped]
        public DateTime? FechaUltimaCita { get; set; }

    }

 
    public static class PacientesValidations
    {
         public static ValidationResult ValidarEdadMinima(object value, ValidationContext context)
                {
                    if (value is DateTime fechaNacimiento)
                    {
                        var edadMinima = 8; 
                        var hoy = DateTime.Today;
                        var edad = hoy.Year - fechaNacimiento.Year;
                        if (fechaNacimiento > hoy.AddYears(-edad)) edad--;
                        if (edad < edadMinima)
                        {
                            return new ValidationResult($"El paciente debe tener al menos {edadMinima} años.");
                        }
                    }
            return ValidationResult.Success;
                }

    }
}


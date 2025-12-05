using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaCitasConsultorioDental.Models;

namespace SistemaCitasConsultorioDental.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Paciente> Paciente { get; set; } = default!;
        public DbSet<Aseguradoras> Aseguradora { get; set; } = default!;

        public DbSet<Dentista> Dentista { get; set; } = default!;
        public DbSet<Motivo> Motivo { get; set; } = default!;
        public DbSet<Cita> Cita { get; set; } = default!;
        public DbSet<HorarioDentista> HorarioDentista { get; set; }


    }
}

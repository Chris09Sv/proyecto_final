using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaCitasConsultorioDental.Data;
using SistemaCitasConsultorioDental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SistemaCitasConsultorioDental.Controllers
{
    [Authorize]

    public class PacientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PacientesController(ApplicationDbContext context)
        {
            _context = context;
        }


        public void CargarCombos()
        {
            ViewBag.Aseguradoras = _context.Aseguradora
            .Select(c => new SelectListItem
            {
                Value = c.aseguradora.ToString(),
                Text = c.aseguradora
            }).ToList();
        }
        public bool ExistePacienteSimilar(string? nombre, string? apellido,DateTime fechaNacimiento)
        {
            if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(apellido))
                return false;

            var termino = $"{nombre} {apellido}".Trim().ToLower();

            return _context.Paciente.Any(p => (p.Nombre + " " + p.Apellido).ToLower().Contains(termino) && Math.Abs(EF.Functions.DateDiffYear(p.FechaNacimiento, fechaNacimiento)) <= 2 );
        }




        public Paciente? GetIdentificacion(string? identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
                return null;

            var key = identificacion.Trim();

            return _context.Paciente
                           .Where(p => p.Identificacion == key)
                           .FirstOrDefault();
        }

        public Paciente? GetEmail(string? Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
                return null;

            var key = Email.Trim();

            return _context.Paciente
                           .Where(p => p.Email == key)
                           .FirstOrDefault();
        }


        public Paciente? GetSeguridadSocial(string? SeguridadSocial,string? proveedor)
        {
            if (string.IsNullOrWhiteSpace(SeguridadSocial))
                return null;

            var key = SeguridadSocial.Trim();

            return _context.Paciente
                           .Where(p => p.NumeroSeguroSocial == key)
                           .FirstOrDefault();
        }

        private void ValidarDuplicados(Paciente paciente)
        {
            if (!string.IsNullOrWhiteSpace(paciente.Identificacion) &&     paciente.Identificacion != "00000000000")
            {
                var identificacion = GetIdentificacion(paciente.Identificacion);
                if (identificacion != null)
                {
                    ModelState.AddModelError("Identificacion", "La identificación ya existe en el sistema.");
                }
            }
            var email = GetEmail(paciente.Email);
            if (email != null)
            {
                ModelState.AddModelError("Email", "El correo electrónico ya existe en el sistema.");
            }

            var seguridadSocial = GetSeguridadSocial(paciente.NumeroSeguroSocial, paciente.SeguroMedico);
            if (seguridadSocial != null)
            {
                ModelState.AddModelError("NumeroSeguroSocial", "El número de seguro social ya existe en el sistema.");
            }

        }

        // GET: Pacientes
        public async Task<IActionResult> Index()
        {

            var paciente = await _context.Paciente.AsNoTracking()
                .Select(p => new Paciente
                {
                    Id =  p.Id,
                    Nombre = p.Nombre,
                    Apellido = p.Apellido,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    FechaNacimiento = p.FechaNacimiento,
                    Direccion = p.Direccion,
                    Identificacion = p.Identificacion,
                    NumeroSeguroSocial = p.NumeroSeguroSocial,
                    SeguroMedico = p.SeguroMedico,
                    SinSeguro = p.SinSeguro,
                    FechaUltimaCita = _context.Cita
                        .Where(c => c.PacienteId ==  p.Id)
                        .OrderByDescending(c => c.Fecha)
                        .ThenByDescending(c => c.Hora)
                        .Select(c => (DateTime?)c.Fecha)
                        .FirstOrDefault()
                }).ToListAsync();



            return View(paciente);
        }

        // GET: Pacientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paciente = await _context.Paciente
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paciente == null)
            {
                return NotFound();
            }

            return View(paciente);
        }

        public IActionResult Create()
        {
            ViewBag.EsEdicion = false;
            CargarCombos();
            return View();
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Paciente paciente,bool confirmarDuplicado = false   )
            {



            var hoy = DateTime.Today;
            var edad = hoy.Year - paciente.FechaNacimiento.Year;
            if (paciente.FechaNacimiento > hoy.AddYears(-edad))
                edad--;
            if (paciente.SinSeguro)
            {
                if (string.IsNullOrWhiteSpace(paciente.NumeroSeguroSocial))
                    paciente.NumeroSeguroSocial = "00000000000";

                paciente.SeguroMedico = "SIN SEGURO";
            }

            if (edad >= 18)
            {
                if (string.IsNullOrWhiteSpace(paciente.Identificacion))
                {
                    ModelState.AddModelError("Identificacion", "La identificación es obligatoria para mayores de edad.");
                }
            }
            else 
            {
                if (string.IsNullOrWhiteSpace(paciente.Identificacion))
                {
                    paciente.Identificacion = "00000000000";
                }
            }

            ValidarDuplicados(paciente);
            if (!ModelState.IsValid)
                return View(paciente);

            if (!confirmarDuplicado && ExistePacienteSimilar(paciente.Nombre, paciente.Apellido,paciente.FechaNacimiento))
                {
                    ViewBag.MostrarConfirmacionDuplicado = true;
                    ViewBag.MensajeDuplicado = "Ya existe un paciente con un nombre y apellido similares. ¿Desea continuar de todos modos?";
                    return View(paciente);
                }


            _context.Add(paciente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            CargarCombos();
            if (id == null)
                return NotFound();

            var paciente = await _context.Paciente.FindAsync(id);
            if (paciente == null)
                return NotFound();

            ViewBag.EsEdicion = true;
            return View(paciente);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Telefono,Email,FechaNacimiento,Direccion,Identificacion,NumeroSeguroSocial,SeguroMedico")] Paciente paciente)
        {
            if (id != paciente.Id)
            {
                return NotFound();
            }
            var oldPaciente =  _context.Paciente.Where(x=>x.Id==id).FirstOrDefault() ;

            if (oldPaciente.Identificacion != paciente.Identificacion)
            {
                var identificacion = GetIdentificacion(paciente.Identificacion);
                if (identificacion != null)
                {
                    ModelState.AddModelError("Identificacion", "La identificación ya existe en el sistema.");
                }
            }
            if (oldPaciente.Email != paciente.Email)
            {
                var email = GetEmail(paciente.Email);
                if (email != null)
                {
                    ModelState.AddModelError("Email", "El correo electrónico ya existe en el sistema.");
                }

            
            }

            var hoy = DateTime.Today;
            var edad = hoy.Year - paciente.FechaNacimiento.Year;
            if (paciente.FechaNacimiento > hoy.AddYears(-edad))
                edad--;
            if (paciente.SinSeguro)
            {
                if (string.IsNullOrWhiteSpace(paciente.NumeroSeguroSocial))
                    paciente.NumeroSeguroSocial = "00000000000";

                paciente.SeguroMedico = "SIN SEGURO";
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EsEdicion = true;
                return View(paciente);
            }
            
            if (ModelState.IsValid)
            {

                try
                {
                    oldPaciente.Nombre = paciente.Nombre;
                    oldPaciente.Apellido = paciente.Apellido;
                    oldPaciente.Telefono = paciente.Telefono;
                    oldPaciente.Email = paciente.Email;
                    oldPaciente.FechaNacimiento = paciente.FechaNacimiento;
                    oldPaciente.Direccion = paciente.Direccion;
                    oldPaciente.Identificacion = paciente.Identificacion;
                    oldPaciente.NumeroSeguroSocial = paciente.NumeroSeguroSocial;
                    oldPaciente.SeguroMedico = paciente.SeguroMedico;
                    oldPaciente.SinSeguro = paciente.SinSeguro;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PacienteExists(paciente.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            return RedirectToAction(nameof(Index));

        }

        // GET: Pacientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paciente = await _context.Paciente
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paciente == null)
            {
                return NotFound();
            }

            return View(paciente);
        }

        // POST: Pacientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paciente = await _context.Paciente.FindAsync(id);
            if (paciente != null)
            {
                _context.Paciente.Remove(paciente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PacienteExists(int id)
        {
            return _context.Paciente.Any(e => e.Id == id);
        }
    }
}

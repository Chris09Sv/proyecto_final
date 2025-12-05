using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaCitasConsultorioDental.Data;
using SistemaCitasConsultorioDental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaCitasConsultorioDental.Controllers
{
    [Authorize]

    public class DentistasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DentistasController(ApplicationDbContext context)
        {
            _context = context;
        }


        private static string NombreDia(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sábado",
                DayOfWeek.Sunday => "Domingo",
                _ => dia.ToString()
            };
        }
        // GET: Dentistas
        public async Task<IActionResult> Index()
        {
            var dentistas = await _context.Dentista.AsNoTracking().ToListAsync();
            var horarios = await _context.HorarioDentista.AsNoTracking().ToListAsync();

            var horariosPorDentista = horarios
            .GroupBy(h => h.DentistaId)
            .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var d in dentistas)
            {
                if (horariosPorDentista.TryGetValue(d.Id, out var hs))
                {
                    var partes = hs
                        .OrderBy(h => h.DiaSemana)
                        .ThenBy(h => h.HoraInicio)
                        .Select(h =>
                            $"{NombreDia(h.DiaSemana)} " +
                            $"{h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm}");

                    d.ResumenHorario = string.Join(", ", partes);
                }
                else
                {
                    d.ResumenHorario = "Sin horario definido";
                }
            }

            return View(dentistas);
        }

        // GET: Dentistas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dentista = await _context.Dentista
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dentista == null)
            {
                return NotFound();
            }

            return View(dentista);
        }

        // GET: Dentistas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dentistas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,Especialidad,Identificacion,Telefono,Email")] Dentista dentista)
        {

            if (ModelState.IsValid)
            {
                var identificacionExistente = await _context.Dentista
                    .AnyAsync(d => d.Identificacion == dentista.Identificacion);
                if (identificacionExistente)
                {
                    ModelState.AddModelError("Identificacion", "La identificación ya está en uso por otro dentista.");
                    return View(dentista);
                }
                var emailExistente = await _context.Dentista
                    .AnyAsync(d => d.Email == dentista.Email);
                if (emailExistente)
                {
                    ModelState.AddModelError("Email", "El correo electrónico ya está en uso por otro dentista.");
                    return View(dentista);
                }

                    _context.Add(dentista);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dentista);
        }

        // GET: Dentistas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dentista = await _context.Dentista.FindAsync(id);
            if (dentista == null)
            {
                return NotFound();
            }
            return View(dentista);
        }

        // POST: Dentistas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Especialidad,Telefono,Email")] Dentista dentista)
        {
            if (id != dentista.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var identificacionExistente = await _context.Dentista
                        .AnyAsync(d => d.Identificacion == dentista.Identificacion && d.Id != dentista.Id);
                   
                    if (identificacionExistente)
                    {
                        ModelState.AddModelError("Identificacion", "La identificación ya está en uso por otro dentista.");
                        return View(dentista);
                    }
                    var emailExistente = await _context.Dentista
                        .AnyAsync(d => d.Email == dentista.Email && d.Id != dentista.Id);
                    if (emailExistente)
                    {
                        ModelState.AddModelError("Email", "El correo electrónico ya está en uso por otro dentista.");
                        return View(dentista);
                    }



                    _context.Update(dentista);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DentistaExists(dentista.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(dentista);
        }

        // GET: Dentistas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dentista = await _context.Dentista
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dentista == null)
            {
                return NotFound();
            }

            return View(dentista);
        }

        // POST: Dentistas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dentista = await _context.Dentista.FindAsync(id);
            if (dentista != null)
            {
                _context.Dentista.Remove(dentista);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DentistaExists(int id)
        {
            return _context.Dentista.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaCitasConsultorioDental.Data;
using SistemaCitasConsultorioDental.Models;

namespace SistemaCitasConsultorioDental.Controllers
{
    [Authorize]

    public class HorariosDentistasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HorariosDentistasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HorariosDentistas
        public async Task<IActionResult> Index()
        {
            var lista = await _context.HorarioDentista
                .Include(h => h.Dentista)
                .OrderBy(h => h.Dentista.Apellido)
                .ThenBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();

            return View(lista);
        }

        // GET: HorariosDentistas/Create
        public IActionResult Create()
        {
            CargarCombos();
            return View(new HorarioDentista());
        }

        // POST: HorariosDentistas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HorarioDentista horario)
        {
            if (horario.HoraInicio >= horario.HoraFin)
            {
                ModelState.AddModelError(string.Empty,
                    "La hora de inicio debe ser menor que la hora de fin.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View(horario);
            }

            _context.Add(horario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CargarCombos();

            var cita = await _context.HorarioDentista.FindAsync(id);

            if (cita == null)
            {
                return NotFound();
            }
            return View(cita);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DentistaId,DiaSemana,HoraInicio,HoraFin")] HorarioDentista horario)

        {
            if (id != horario.Id)
            {
                return NotFound();
            }


            if (!ModelState.IsValid)
            {
                return View(horario);
            }
            else
            {

                try
                {
                    _context.Update(horario);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                catch (DbUpdateConcurrencyException)
                {
                    bool exists = _context.HorarioDentista.Any(e => e.Id == horario.Id);
                    if (!exists)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
        }



        private void CargarCombos()
        {
            ViewData["DentistaId"] = new SelectList(
                _context.Dentista.ToList(),
                "Id",
                "Apellido"
            );

            var dias = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(d => new SelectListItem
                {
                    Value = d.ToString(),
                    Text = d.ToString()
                })
                .ToList();

            ViewBag.DiasSemana = dias;
        }
    }
}

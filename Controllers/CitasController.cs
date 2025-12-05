using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SistemaCitasConsultorioDental.Data;
using SistemaCitasConsultorioDental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SistemaCitasConsultorioDental.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        


        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public Motivo? GetMotivo(string? motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return null;

            var key = motivo.Trim().ToLower();

            return _context.Motivo
                           .Where(p => p.Descripcion.ToLower().Trim() == key)
                           .FirstOrDefault();
        }

        private async Task<List<Cita>> GetCitaAsync(int dentistaId, DateTime fecha)
        {
            var dia = fecha.Date;

            return await _context.Cita
              .Where(c => c.DentistaId == dentistaId && c.Fecha.Date == dia)
              .ToListAsync();
        }


        // GET: Citas
        public async Task<IActionResult> Hoy()
        {
            var hoy = DateTime.Today;
            var dia = hoy.DayOfWeek;

            var ahora = DateTime.Now;
            var citasHoy = await _context.Cita
            .Include(c => c.Paciente)
            .Include(c => c.Dentista)
            .Include(c => c.Motivo)
            .Where(c => c.Fecha.Date == hoy)
            .ToListAsync();

            ViewBag.CitasHoy = citasHoy.Count;


            ViewBag.TotalPacientes = await _context.Paciente.CountAsync();



            ViewBag.CitasVigentes = citasHoy.Where(c => c.Estado != "Finalizado").Count();

            var dentistasDisponiblesQuery =
                from d in _context.Dentista
                join h in _context.HorarioDentista
                    on d.Id equals h.DentistaId
                where h.DiaSemana == dia
                      && h.HoraInicio <= ahora.TimeOfDay
                      && h.HoraFin > ahora.TimeOfDay
                select d.Id;

            ViewBag.TotalDentistas = await dentistasDisponiblesQuery
                .Distinct()
                .CountAsync();



            var citasEnCurso = citasHoy
                .OrderBy(c => c.Hora)
                .ToList();

            return View(citasEnCurso);
        }

        // GET: Citas
        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var dia = hoy.DayOfWeek;

            var ahora = DateTime.Now;


            var citasHoy = await _context.Cita
            .Include(c => c.Paciente)
            .Include(c => c.Dentista)
            .Include(c => c.Motivo)
            .ToListAsync();


            ViewBag.CitasHoy = citasHoy.Where(c=>c.Fecha.Date == hoy).Count();
            ViewBag.TotalPacientes = await _context.Paciente.CountAsync();
            ViewBag.CitasVigentes = citasHoy.Where(c => c.Estado != "Finalizado").Count();
            

            var dentistasDisponiblesQuery =
                from d in _context.Dentista
                join h in _context.HorarioDentista
                    on d.Id equals h.DentistaId
                //where h.DiaSemana == dia
                //      && h.HoraInicio <= ahora.TimeOfDay
                //      && h.HoraFin > ahora.TimeOfDay
                select d.Id;

            ViewBag.TotalDentistas = await dentistasDisponiblesQuery
                .Distinct()
                .CountAsync();

            var citasEnCurso = citasHoy
                .OrderBy(c => c.Hora)
                .ToList();

            return View(citasEnCurso);
        }


        // GET: Citas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Cita
                .Include(c => c.Paciente)
                .Include(c => c.Dentista)
                .Include(c => c.Motivo)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

     
            return View(cita);
        }


        [HttpGet]
        public IActionResult BuscarPaciente(string termino)
        {

            var query = _context.Paciente.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(termino) ||
                    p.Apellido.Contains(termino) ||
                    p.Identificacion.Contains(termino) ||
                    p.Email.Contains(termino));
            }


            var _paciente =  query
                .Select(p => new Paciente
                {
                    Id = p.Id,
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
                        .Where(c => c.PacienteId == p.Id)
                        .OrderByDescending(c => c.Fecha)
                        .ThenByDescending(c => c.Hora)
                        .Select(c => (DateTime?)c.Fecha)
                        .FirstOrDefault()
                }).ToList();

            var pacientes = _paciente
                .Select(p => new
                {
                    id = p.Id,
                    text = p.Nombre + " " + p.Apellido + " (" + p.Identificacion + ")" + (p.FechaUltimaCita.HasValue
                ? "  última cita: " + p.FechaUltimaCita.Value.ToString("dd/MM/yyyy")
                : "")
                })
                .Take(10)
                .ToList();

            return Json(pacientes);
        }



        [HttpGet]
        public IActionResult DentistasPorFecha(DateTime fecha)
        {
            var diaSemana = fecha.DayOfWeek;
            var dentistas = from d in _context.Dentista
                            join h in _context.HorarioDentista
                                on d.Id equals h.DentistaId
                            where h.DiaSemana == diaSemana
                            select new
                            {
                                id = d.Id,
                                nombreCompleto = d.Nombre + " " + d.Apellido
                            };
            return Json(dentistas.Distinct().ToList());
        }
        [HttpGet]
        public IActionResult HorariosDisponibles(int dentistaId, DateTime fecha, int duracionMinutos)
        {
            var diaSemana = fecha.DayOfWeek;

            var horario = _context.HorarioDentista
                .FirstOrDefault(h => h.DentistaId == dentistaId && h.DiaSemana == diaSemana);

            if (horario == null)
            {
                return Json(new
                {
                    trabajaDe = "N/A",
                    trabajaHasta = "N/A",
                    slots = new List<string>()
                });
            }

            var inicioJornada = fecha.Date + horario.HoraInicio;
            var finJornada = fecha.Date + horario.HoraFin;

            var inicioDia = fecha.Date;
            var finDia = inicioDia.AddDays(1);

            var citasMismoDia = _context.Cita
                .Where(c =>
                    c.DentistaId == dentistaId &&
                    c.Fecha >= inicioDia &&
                    c.Fecha < finDia)
                .AsEnumerable() 
                .Where(c => c.Estado != "Cancelada") 
                .ToList();

            var duracion = TimeSpan.FromMinutes(duracionMinutos <= 0 ? 30 : duracionMinutos);
            var slots = new List<string>();
            var actual = inicioJornada;
            var ahora = DateTime.Now;


            while (actual + duracion <= finJornada)
            {
                if (fecha.Date == ahora.Date && actual <= ahora)
                {
                    actual = actual.Add(duracion);
                    continue;
                }

                var finSlot = actual + duracion;

                var _cita = citasMismoDia.Any(c =>
                {
                    var inicioCita = c.Inicio;
                    var finCita = c.Fin;
                    return actual < finCita && finSlot > inicioCita;
                });

                if (!_cita)
                {
                    slots.Add(actual.ToString("HH:mm"));
                }

                actual = actual.Add(duracion);
            }

            return Json(new
            {
                trabajaDe = horario.HoraInicio.ToString(@"hh\:mm"),
                trabajaHasta = horario.HoraFin.ToString(@"hh\:mm"),
                slots
            });
        }



        [HttpGet]
        public IActionResult BuscarDentistas(string termino)
        {
            IQueryable<Dentista> query = _context.Dentista;

            if (!string.IsNullOrWhiteSpace(termino))
            {
                query = query.Where(d =>
                    d.Nombre.Contains(termino) ||
                    d.Apellido.Contains(termino));
            }

            var dentistas = query
                .Select(d => new
                {
                    id = d.Id,
                    text = d.Nombre + " " + d.Apellido   
                })
                .Take(10)
                .ToList();

            return Json(dentistas);
        }

        [HttpGet]
        public IActionResult HorarioDentista(int dentistaId, DateTime fecha)
        {
            var diaSemana = fecha.DayOfWeek;

            var horario = _context.HorarioDentista
                .FirstOrDefault(h => h.DentistaId == dentistaId && h.DiaSemana == diaSemana);

            if (horario == null)
            {
                return Json(new
                {
                    trabajaDe = "N/A",
                    trabajaHasta = "N/A",
                    citasOcupadas = new List<object>()
                });
            }

            var inicioDia = fecha.Date;
            var finDia = inicioDia.AddDays(1);

            var citasMismoDia = _context.Cita
                .Where(c =>
                    c.DentistaId == dentistaId &&
                    c.Fecha >= inicioDia &&
                    c.Fecha < finDia)
                .AsEnumerable()               
                .Where(c => c.Estado != "Cancelada");

            var citasOcupadas = citasMismoDia
                .Select(c => new
                {
                    horaInicio = c.Inicio.ToString("HH:mm"),
                    horaFin = c.Fin.ToString("HH:mm")
                })
                .ToList();

            return Json(new
            {
                trabajaDe = horario.HoraInicio.ToString(@"hh\:mm"),
                trabajaHasta = horario.HoraFin.ToString(@"hh\:mm"),
                citasOcupadas
            });
        }


        // GET: Citas/Create


        private bool Conflicto(int dentistaId, DateTime fecha, TimeSpan hora, int duracionMinutos)
        {
            var inicioSeleccionado = fecha.Date + hora;
            var finSeleccionado = inicioSeleccionado.AddMinutes(duracionMinutos);

            var citasMismoDia = _context.Cita
                .Where(c =>
                    c.DentistaId == dentistaId &&
                    c.Fecha >= fecha.Date &&
                    c.Fecha < fecha.Date.AddDays(1))
                .AsEnumerable() 
                .Where(c => c.Estado != "Cancelada");


            return citasMismoDia.Any(c =>
            {
                var inicio = c.Fecha.Date + c.Hora;
                var fin = inicio.AddMinutes(c.DuracionMinutos);
                return inicioSeleccionado < fin && finSeleccionado > inicio;
            });
        }

        public IActionResult Create()
        {


            var cita = new Cita
            {
                Fecha = DateTime.Today,
                DuracionMinutos = 30  
            };
            ViewBag.Motivos = _context.Motivo
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Descripcion
                })
                .ToList();            
            return View(cita);
        }

        // POST: Citas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PacienteId,DentistaId,MotivoId,Fecha,Hora,DuracionMinutos")] Cita cita)
        {
            cita.DuracionMinutos = 30;

            if (!ModelState.IsValid)
            {
                ViewBag.Motivos = _context.Motivo
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Descripcion
                    })
                    .ToList();

                return View(cita);
            }

            if (Conflicto(cita.DentistaId, cita.Fecha, cita.Hora, cita.DuracionMinutos))
            {
                ModelState.AddModelError(string.Empty, "El horario seleccionado ya está reservado para este dentista.");

                ViewBag.Motivos = _context.Motivo
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Descripcion
                    })
                    .ToList();

                return View(cita);
            }

            _context.Cita.Add(cita);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        
        // GET: Citas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Cita
                .Include(c => c.Paciente)
                .Include(c => c.Dentista)
                .Include(c => c.Motivo)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            var inicioCita = cita.Fecha.Date + cita.Hora;
            if (inicioCita <= DateTime.Now)
            {
                ViewBag.Motivos = _context.Motivo
                    .Select(m => new SelectListItem 
                    {
                        Value = m.Id.ToString(),
                        Text = m.Descripcion
                    })
                    .ToList();

                ViewBag.Error = "No puede editar una cita en el pasado.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Motivos = _context.Motivo
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Descripcion
                })
                .ToList();

            return View(cita);
        }

        // POST: Citas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PacienteId,DentistaId,MotivoId,Fecha,Hora,DuracionMinutos")] Cita cita)
        {

            cita.DuracionMinutos = 30;

            if (!ModelState.IsValid)
            {
                ViewBag.Motivos = _context.Motivo
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Descripcion
                    })
                    .ToList();

                return View(cita);
            }

            try
            {
                if (Conflicto(cita.DentistaId, cita.Fecha, cita.Hora, cita.DuracionMinutos))
                {
                    ModelState.AddModelError(string.Empty, "El horario seleccionado ya está reservado para este dentista.");

                    ViewBag.Motivos = _context.Motivo
                        .Select(m => new SelectListItem
                        {
                            Value = m.Id.ToString(),
                            Text = m.Descripcion
                        })
                        .ToList();

                    return View(cita);
                }
                var citaDb = await _context.Cita.FindAsync(id);
=                citaDb.MotivoId = cita.MotivoId;
                citaDb.Fecha = cita.Fecha;
                citaDb.Hora = cita.Hora;
                citaDb.DuracionMinutos = cita.DuracionMinutos;



                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));


            }


            catch (DbUpdateConcurrencyException)
                {
                    if (!CitaExists(cita.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
        }

        // GET: Citas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Cita
                .Include(c => c.Dentista)
                .Include(c => c.Motivo)
                .Include(c => c.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        // POST: Citas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cita = await _context.Cita.FindAsync(id);
            if (cita != null)
            {
                _context.Cita.Remove(cita);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CitaExists(int id)
        {
            return _context.Cita.Any(e => e.Id == id);
        }
    }
}

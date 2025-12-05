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

    public class MotivosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MotivosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Motivos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Motivo.ToListAsync());
        }

        // GET: Motivos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var motivo = await _context.Motivo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (motivo == null)
            {
                return NotFound();
            }

            return View(motivo);
        }

        // GET: Motivos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Motivos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion")] Motivo motivo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(motivo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(motivo);
        }

        // GET: Motivos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var motivo = await _context.Motivo.FindAsync(id);
            if (motivo == null)
            {
                return NotFound();
            }
            return View(motivo);
        }

        // POST: Motivos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion")] Motivo motivo)
        {
            if (id != motivo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(motivo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MotivoExists(motivo.Id))
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
            return View(motivo);
        }

        // GET: Motivos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var motivo = await _context.Motivo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (motivo == null)
            {
                return NotFound();
            }

            return View(motivo);
        }

        // POST: Motivos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var motivo = await _context.Motivo.FindAsync(id);
            if (motivo != null)
            {
                _context.Motivo.Remove(motivo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MotivoExists(int id)
        {
            return _context.Motivo.Any(e => e.Id == id);
        }
    }
}

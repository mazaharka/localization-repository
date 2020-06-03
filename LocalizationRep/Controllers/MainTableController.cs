using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LocalizationRep.Data;
using LocalizationRep.Models;

namespace LocalizationRep.Controllers
{
    public class MainTableController : Controller
    {
        private readonly LocalizationRepContext _context;

        public MainTableController(LocalizationRepContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string sectionSearch, string searchString)
        {
            var localizationRepContext = _context.MainTable.Include(m => m.Section);
            IQueryable<string> genreQuery = from m in _context.MainTable
                                            orderby m.Section.ID
                                            select m.Section.Title;

            var mainTableItem = from m in _context.MainTable.Include(m => m.Section)
                                select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                mainTableItem = mainTableItem.Where(s => s.Section.Title.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(sectionSearch))
            {
                mainTableItem = mainTableItem.Where(x => x.Section.Title == sectionSearch);
            }

            var localizationSectionVM = new SectionSearchViewModel
            {
                Sections = new SelectList(await genreQuery.Distinct().ToListAsync()),
                MainTables = await mainTableItem.ToListAsync()
            };

            return View(localizationSectionVM);
        }

        // GET: MainTable/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainTable = await _context.MainTable
                .Include(m => m.Section)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (mainTable == null)
            {
                return NotFound();
            }

            return View(mainTable);
        }

        // GET: MainTable/Create
        public IActionResult Create()
        {
            ViewData["SectionID"] = new SelectList(_context.Section, "ID", "Title");
            return View();
        }

        // POST: MainTable/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CommonID,SectionID,IOsID,AndroidID,TextRU,TextEN,TextUA,IsFreezing")] MainTable mainTable)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mainTable);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SectionID"] = new SelectList(_context.Section, "ID", "Title", mainTable.SectionID);
            return View(mainTable);
        }

        // GET: MainTable/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainTable = await _context.MainTable.FindAsync(id);
            if (mainTable == null)
            {
                return NotFound();
            }
            ViewData["SectionID"] = new SelectList(_context.Section, "ID", "Title", mainTable.SectionID);
            return View(mainTable);
        }

        // POST: MainTable/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,CommonID,SectionID,IOsID,AndroidID,TextRU,TextEN,TextUA,IsFreezing")] MainTable mainTable)
        {
            if (id != mainTable.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mainTable);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MainTableExists(mainTable.ID))
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
            ViewData["SectionID"] = new SelectList(_context.Section, "ID", "Title", mainTable.SectionID);
            return View(mainTable);
        }

        // GET: MainTable/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainTable = await _context.MainTable
                .Include(m => m.Section)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (mainTable == null)
            {
                return NotFound();
            }

            return View(mainTable);
        }

        // POST: MainTable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mainTable = await _context.MainTable.FindAsync(id);
            _context.MainTable.Remove(mainTable);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MainTableExists(int id)
        {
            return _context.MainTable.Any(e => e.ID == id);
        }
    }
}

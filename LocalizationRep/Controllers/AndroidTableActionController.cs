using System;
using System.Linq;
using System.Threading.Tasks;
using LocalizationRep.Data;
using LocalizationRep.Models;
using LocalizationRep.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LocalizationRep.Controllers
{
    public class AndroidTableActionController : Controller
    {


        private readonly LocalizationRepContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly FileActionXML FAXML;
        private readonly FileActionJSON FAJSON;



        public AndroidTableActionController(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            FAXML = new FileActionXML(context, appEnvironment);
            FAJSON = new FileActionJSON(context, appEnvironment);
            _context = context;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> Index(string sectionSearch, string searchString)
        {
            IQueryable<string> genreQuery = from m in _context.MainTable
                                            orderby m.Section.ID
                                            select m.Section.Title;

            var androidTable = from m in _context.AndroidTable
                                .Include(m => m.Section)
                                select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                androidTable = androidTable.Where(s => s.Section.Title.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(sectionSearch))
            {
                androidTable = androidTable.Where(x => x.Section.Title == sectionSearch);
            }

            var androidTableSearchVM = new AndroidTableSearchViewModel
            {
                Sections = new SelectList(await genreQuery.Distinct().ToListAsync()),
                AndroidTable = await androidTable.ToListAsync(),
            };


            IQueryable<string> stylesUnique = from s in _context.StyleJsonKeyModel
                                              orderby s.StyleName
                                              select s.StyleName;

            ViewBag.Head = stylesUnique.AsQueryable().Distinct();
            return View(androidTableSearchVM);
        }

        //// GET: AndroidTableAction
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.AndroidTable.Include(m => m.Section).ToListAsync());
        //}

        // POST: AndroidTableAction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("ID,SectionID,StringNumber,AndroidID,NodeInnerText,CommentValue,CommonID,Section")] AndroidTable androidTableItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(androidTableItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //ViewData["SectionID"] = new SelectList(_context.Section, "ID", "Title", notMatchedItem.SectionID);
            return View(androidTableItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateItem(string commonID)
        {
            FAXML.UpdateAndroidItemSection(commonID, Request.Form["androidItemSection"][0]);

            return RedirectToAction("Index");
        }

        public IActionResult ChangeSectionByPack()
        {
            FAXML.ChangeSectionByPack();

            return RedirectToAction("Index");
        }
        

        public IActionResult UpdateXMLFromFile(string path)
        {
            FAXML.ReadXMLToListNotMatch(path);

            return RedirectToAction("Index");
        }


        public IActionResult ReadAndCompareXMLBetweenTable()
        {
            FAXML.ReadAndCompareXMLBetweenTable();

            return RedirectToAction("Index");
        }

        public IActionResult CompareCommonIDBetweenMainTableAndAndroid()
        {
            FAXML.CompareCommonIDBetweenMainTableAndAndroid();

            return RedirectToAction("Index");
        }

        public IActionResult CreateEntitesFromNullCommandIdInAndroidTables()
        {
            FAXML.CreateEntitesFromNullCommandIdInAndroidTables();

            return RedirectToAction("Index");
        }

        public IActionResult FillCommonIdInAndroidTableAccordingMainTable()
        {
            FAXML.FillCommonIdInAndroidTableAccordingMainTable();

            return RedirectToAction("Index");
        }


        public IActionResult FillInMissingTextsInAndroidTable()
        {
            FAXML.FillInMissingTextsInAndroidTable();

            return RedirectToAction("Index");
        }

        public IActionResult FillInMainTableAndroidInfo()
        {
            FAXML.FillInMainTableAndroidInfo();

            return RedirectToAction("Index");
        }

        public IActionResult DeleteAllAndroidItemsFromMainTable()
        {
            ActionWithDataBase.DeleteAllAndroidItemsFromMainTable(_context);

            return RedirectToAction("Index");
        }

        //чтение файла xml
        public IActionResult ReadFileXML()
        {
            FAXML.ReadFileXMLAction();

            return RedirectToAction("Index");
        }


        public IActionResult DeleteDublicateAction()
        {

            FAXML.DeleteDublicate();

            return RedirectToAction("Index");
        }

        public IActionResult EraseAndoridCommentInMainTable()
        {

            FAXML.RemoveAndoridCommentInMainTable();

            return RedirectToAction("Index");
        }

        public IActionResult RemoveAllAndoridIdMatchesInMainTable()
        {

            FAXML.RemoveAllAndoridIdMatchesInMainTable();

            return RedirectToAction("Index");
        }
    }
}

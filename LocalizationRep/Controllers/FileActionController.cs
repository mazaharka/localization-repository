using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocalizationRep.Data;
using LocalizationRep.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalizationRep.Utilities;

namespace LocalizationRep.Controllers
{
    public class FileActionController : Controller
    {
        private readonly LocalizationRepContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly FileActionXML FAXML;
        private readonly FileActionJSON FAJSON;



        public FileActionController(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            FAXML = new FileActionXML(context, appEnvironment);
            FAJSON = new FileActionJSON(context, appEnvironment);
            _context = context;
            _appEnvironment = appEnvironment;
        }

        // GET: FileAction
        public async Task<IActionResult> Index()
        {

            isHaveErrorsToUserView();

            return View(await _context.FileModel.ToListAsync());
        }


        // GET: FileAction/NotMatched
        public async Task<IActionResult> NotMatched()
        {
            return View(await _context.AndroidTable.Include(m => m.Section).ToListAsync());
        }

        // POST: FileAction/NotMatched
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotMatched([Bind("ID,SectionID,StringNumber,AndroidID,NodeInnerText,CommentValue,CommonID,Section")] AndroidTable notMatchedItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notMatchedItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //ViewData["SectionID"] = new SelectList(_context.Section, "ID", "Title", notMatchedItem.SectionID);
            return View(notMatchedItem);
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                string path = Path.Combine(_appEnvironment.WebRootPath, FileActionHelpers.UploadPath + uploadedFile.FileName);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                if (!_context.FileModel.Where(s => s.Path == path).Any())
                {
                    FileModel file = new FileModel { Name = uploadedFile.FileName, Path = path, TypeOfLoad = FileActionHelpers.TypeOfLoad.UPLOAD.ToString() };
                    _context.FileModel.Add(file);
                }
                _context.SaveChanges();

            }
            UpDateInfoFilesInDB();

            return RedirectToAction("Index");
        }

        public IActionResult UpdateFileInDatabase(string filename)
        {
            switch (Path.GetExtension(filename))
            {
                case ".csv":
                    CSVActionController.UpdateFromCsv(FileActionCSV.ReadUploadedCSV(filename), _context);
                    break;
                case ".json":
                    FAJSON.UpdateFromJsonToDbLocalizedText(filename);
                    break;
                case ".xml":
                    FAXML.ReadXMLToListNotMatch(filename); //filename = path
                    break;
                default:
                    break;
            }
            return RedirectToAction("Index");
        }

        public IActionResult UpdateXMLFromFile(string path)
        {
            FAXML.ReadXMLToListNotMatch(path);

            return RedirectToAction("Index");
        }

        // POST: FileAction/UpDateInfoFilesInDB
        [HttpPost]
        public IActionResult UpDateInfoFilesInDB()
        {
            ActionWithDataBase.UpDateInfoFilesInDBAction(_context, _appEnvironment);

            return RedirectToAction("Index");
        }

        // GET: FileAction/UpdateSectionFromFile
        public IActionResult UpdateSectionFromFile()
        {
            AddUniqeSectionToDbTable(FileActionHelpers.GetFilesInfo(_appEnvironment));

            return RedirectToAction("Index");
        }

        // GET: FileAction/EraseAllSectionFromDb
        [Obsolete]
        public IActionResult EraseAllSectionFromDb(string whatIMustRemove)
        {
            ActionWithDataBase.EraseAllSectionFromDBAction(_context, whatIMustRemove);
            UpDateInfoFilesInDB();

            return RedirectToAction("Index");
        }

        // GET: FileAction/PopulateDBWithValuesFromFiles
        public IActionResult PopulateDBWithValuesFromFiles()
        {
            if (_context.Section.Count() != 0)
            {
                Initialize(FileActionHelpers.GetFilesInfo(_appEnvironment));
            }
            else
            {
                ErrorsToUserView("Section", "Нет секций в базе. Обнови секции, по-братски!");
            }

            return RedirectToAction("Index");
        }

        public void Initialize(List<FileModel> FilesName)
        {
            foreach (var item in FilesName)
            {
                if (Path.GetExtension(item.Path) == ".json")
                {
                    using StreamReader sr = new StreamReader(item.Path);
                    string line;
                    string inFile = "";

                    while ((line = sr.ReadLine()) != null)
                    {
                        inFile += line;
                    }

                    FAJSON.JsonToDb(Path.GetFileNameWithoutExtension(item.Name), inFile);
                    sr.Close();
                }
            }
        }

        [HttpPost]
        public IActionResult AddUniqeSectionToDbTable(List<FileModel> FilesName)
        {
            ActionWithDataBase.AddUniqueSection(_context, FilesName);

            return RedirectToAction("Index");
        }

        public void GetAllSectors()
        {
            foreach (Sections section in _context.Section)
                Console.WriteLine(section.Title);
        }

        /// <summary>
        /// скачать файл
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task<IActionResult> Download(string fullpath)
        {
            var path = fullpath;

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, FileActionHelpers.GetContentType(path), Path.GetFileName(path));
        }


        public void ErrorsToUserView(string name, string message)
        {
            ViewData[name] = message;
        }

        private void isHaveErrorsToUserView()
        {
            if (_context.Section.Count() != 0)
            {
                ErrorsToUserView("Section", "");
            }
            else
            {
                ErrorsToUserView("Section", "Нет разделов в базе. Обнови разделы, по-братски!");
            }
        }

        //формирование файла json
        public IActionResult CreateFileJsonFromDb()
        {
            FAJSON.CreateJSONFiles();

            return RedirectToAction("Index");
        }

       


        //формирование файла xml
        public IActionResult CreateFileXMLFromDb()
        {
            FAXML.CreateXMLFileFromDB();
            return RedirectToAction("Index");
        }

        //формирование файла csv
        public IActionResult CreateFileCSVFromDb(string nameFileCsv)
        {
            FileActionCSV.CreateFileCSV(nameFileCsv, _context);

            return RedirectToAction("Index");
        }

        //чтение файла csv
        public List<CsvFileModel> ReadUploadedCSVFile(string nameFileCsv)
        {
            return FileActionCSV.ReadUploadedCSV(nameFileCsv);
        }

        public IActionResult RemoveFileFromServer(string fullpath)
        {
            FileActionHelpers.RemoveFile(fullpath, _context);

            return RedirectToAction("Index");
        }

        public IActionResult ChangeStyleName()
        {
            ActionWithDataBase.ChangeStyleName(_context);

            return RedirectToAction("Index");
        }

        //public IActionResult ReadAndCompareXMLBetweenTable()
        //{
        //    FAXML.ReadAndCompareXMLBetweenTable();

        //    return RedirectToAction("NotMatched");
        //}

        //public IActionResult CompareCommonIDBetweenMainTableAndAndroid()
        //{
        //    FAXML.CompareCommonIDBetweenMainTableAndAndroid();

        //    return RedirectToAction("NotMatched");
        //}

        //public IActionResult CreateEntitesFromNullCommandIdInAndroidTables()
        //{
        //    FAXML.CreateEntitesFromNullCommandIdInAndroidTables();

        //    return RedirectToAction("NotMatched");
        //}

        //public IActionResult FillCommonIdInAndroidTableAccordingMainTable()
        //{
        //    FAXML.FillCommonIdInAndroidTableAccordingMainTable();

        //    return RedirectToAction("NotMatched");
        //}


        //public IActionResult FillInMissingTextsInAndroidTable()
        //{
        //    FAXML.FillInMissingTextsInAndroidTable();

        //    return RedirectToAction("NotMatched");
        //}

        //public IActionResult FillInMainTableAndroidInfo()
        //{
        //    FAXML.FillInMainTableAndroidInfo();

        //    return RedirectToAction("NotMatched");
        //}

        //public IActionResult DeleteAllAndroidItemsFromMainTable()
        //{
        //    ActionWithDataBase.DeleteAllAndroidItemsFromMainTable(_context);

        //    return RedirectToAction("NotMatched");
        //}

        ////чтение файла xml
        //public IActionResult ReadFileXML()
        //{
        //    FAXML.ReadFileXMLAction();

        //    return RedirectToAction("NotMatched");
        //}


        //public IActionResult DeleteDublicateAction()
        //{

        //    FAXML.DeleteDublicate();

        //    return RedirectToAction("NotMatched");
        //}

        //public IActionResult EraseAndoridCommentInMainTable()
        //{

        //    FAXML.RemoveAndoridCommentInMainTable();

        //    return RedirectToAction("NotMatched");
        //}

        //public IActionResult RemoveAllAndoridIdMatchesInMainTable()
        //{

        //    FAXML.RemoveAllAndoridIdMatchesInMainTable();

        //    return RedirectToAction("NotMatched");
        //}

    }
}


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
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LocalizationRep.Controllers
{
    public class FileActionController : Controller
    {
        private readonly LocalizationRepContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly FileActionXML FAXML;



        public FileActionController(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            FAXML = new FileActionXML(context, appEnvironment);
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
            return View(await _context.NotMatchedItem.ToListAsync());
        }

        // POST: FileAction/NotMatched
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotMatched([Bind("ID,StringNumber,AndroidID,NodeInnerText,CommentValue,CommonID")] NotMatchedItem notMatchedItem)
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
                    break;
                case ".xml":
                    break;
                default:
                    break;
            }
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
            switch (whatIMustRemove)
            {
                case "Files":
                    _context.FileModel.RemoveRange(_context.FileModel);
                    break;
                case "Sections":
                    _context.Section.RemoveRange(_context.Section);
                    break;
                case "MainTable":
                    _context.MainTable.RemoveRange(_context.MainTable);
                    break;
                default:

                    _context.FileModel.RemoveRange(_context.FileModel);
                    _context.Section.RemoveRange(_context.Section);
                    _context.LangKeyModel.RemoveRange(_context.LangKeyModel);
                    _context.LangValue.RemoveRange(_context.LangValue);
                    _context.StyleJsonKeyModel.RemoveRange(_context.StyleJsonKeyModel);
                    _context.MainTable.RemoveRange(_context.MainTable);
                    break;
            }
            _context.SaveChanges();
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
                ErrorsToUserVIew("Section", "Нет секций в базе. Обнови секции, по-братски!");
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

                    JsonToDb(Path.GetFileNameWithoutExtension(item.Name), inFile);
                    sr.Close();
                }
            }
        }

        [HttpPost]
        public IActionResult AddUniqeSectionToDbTable(List<FileModel> FilesName)
        {
            foreach (var fileСontentsItem in FilesName)
            {
                bool next = true;
                if (fileСontentsItem.TypeOfLoad.Equals(FileActionHelpers.TypeOfLoad.UPLOAD.ToString()))
                {

                    foreach (Sections section in _context.Section)
                    {
                        if (fileСontentsItem.Name.Equals(section.Title))
                        {
                            next = false;
                            break;
                        }
                    }

                    string shortNameUn = GetUniqueShortName(Path.GetFileNameWithoutExtension(fileСontentsItem.Name).ToUpper());
                    int randomNumber = 0;
                    foreach (Sections section in _context.Section)
                    {

                        while (section.ShortName.Equals(shortNameUn))
                        {
                            shortNameUn = shortNameUn.Replace(shortNameUn.Substring(3, 1), randomNumber.ToString());
                            randomNumber++;
                        }
                    }


                    if (next && Path.GetExtension(fileСontentsItem.Path) == ".json")
                    {
                        _context.Section.AddRange(
                                   new Sections
                                   {
                                       Title = Path.GetFileNameWithoutExtension(fileСontentsItem.Name).ToString(),
                                       LastIndexOfCommonID = "0000",
                                       ShortName = shortNameUn
                                   });
                        _context.SaveChanges();
                    }
                }
            }


            return RedirectToAction("Index");
        }

        public string GetUniqueShortName(string fileName)
        {
            string returnString = fileName;
            string pattern = @"[AEYUIO]";
            if (fileName.Length > 4)
            {
                returnString = fileName.Remove(1) + Regex.Replace(fileName.Substring(1), pattern, "");

                if (returnString.Length > 4)
                {
                    returnString = returnString.Remove(4);
                }
                else if (returnString.Length <= 4)
                {
                    while (returnString.Length < 4)
                    {
                        returnString += "0";
                    }
                }
            }
            else if (fileName.Length < 4)
            {
                while (fileName.Length < 4)
                {
                    returnString += "0";
                }
            }


            return returnString;
        }

        public void GetAllSectors()
        {
            foreach (Sections section in _context.Section)
                Console.WriteLine(section.Title);
        }

        public void JsonToDb(string key, string itemOfJSONInAllString)
        {
            string CommonID;
            MainTable mainTable = new MainTable();
            StyleJsonKeyModel styleJsonKeyModel = new StyleJsonKeyModel();
            LangKeyModel langKeyModel = new LangKeyModel();
            LangValue langValue = new LangValue();
            Dictionary<string, Dictionary<string, Dictionary<string, object>>> Localiz = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
            Dictionary<string, string> jsonHaveAlsoPrular = new Dictionary<string, string>();
            string jsonHaveOnlySingle = "!jsonHaveOnlySingle";
            var sections = _context.Section.Where(t => t.Title == key).ToList();
            try
            {
                Localiz = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>(itemOfJSONInAllString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (sections.Count != 0)
            {
                foreach (var parse in Localiz)
                {
                    if (_context.MainTable.FirstOrDefault(s => s.IOsID == parse.Key) == null)
                    {
                        //CommonID = CommonIDGetNext(key);
                        try
                        {
                            mainTable = new MainTable
                            {
                                SectionID = sections.Find(s => s.Title == key).ID,
                                //CommonID = CommonID,
                                IOsID = parse.Key
                            };
                            _context.MainTable.Add(mainTable);
                            _context.SaveChanges();
                        }
                        catch (InvalidOperationException ex)
                        {
                            Console.WriteLine("Нет секций в базе. Обнови секции, по-братски!\n" + ex);
                        }
                        List<LangKeyModel> langKeyModels = new List<LangKeyModel>();
                        List<StyleJsonKeyModel> styleJsonKeyModels = new List<StyleJsonKeyModel>();

                        string styleNameKey = "default";
                        bool flag = false;
                        foreach (var langKey in parse.Value)
                        {
                            styleNameKey = langKey.Key;
                            styleJsonKeyModel = new StyleJsonKeyModel
                            {
                                StyleName = styleNameKey,
                                //LangKeyModels = null,
                                MainTables = mainTable
                            };
                            _context.StyleJsonKeyModel.Add(styleJsonKeyModel);
                            _context.SaveChanges();

                            langKeyModels.Clear();
                            LangValue langValues = new LangValue();
                            string itemLangName = "ru/en/ua";

                            foreach (var itemLangKey in langKey.Value)
                            {
                                flag = false;
                                itemLangName = itemLangKey.Key;
                                if (itemLangKey.Value.ToString().Contains("%@"))
                                {
                                    flag = true;
                                }
                                if (itemLangKey.Value.ToString().Split('"').Count() == 2)
                                {
                                    jsonHaveAlsoPrular = JsonConvert.DeserializeObject<Dictionary<string, string>>(itemLangKey.Value.ToString());
                                    langValues = new LangValue
                                    {
                                        Single = jsonHaveAlsoPrular.Values.First(),
                                        Prular = jsonHaveAlsoPrular.Values.Last()
                                    };
                                }
                                else
                                {
                                    jsonHaveOnlySingle = itemLangKey.Value.ToString();
                                    langValues = new LangValue
                                    {
                                        Single = jsonHaveOnlySingle
                                    };
                                }
                                try
                                {
                                    langKeyModel = new LangKeyModel
                                    {
                                        LangName = itemLangName,
                                        LangValue = langValues,
                                        StyleJsonKeyModel = styleJsonKeyModel
                                    };
                                    langKeyModels.Add(langKeyModel);
                                    _context.LangKeyModel.Add(langKeyModel);
                                    _context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("LangKeyModel not create" + ex);
                                }
                            }

                            styleJsonKeyModel.StyleName = styleNameKey;
                            styleJsonKeyModel.LangKeyModels = langKeyModels.GetRange(langKeyModels.Count - 3, 3);
                            _context.StyleJsonKeyModel.Update(styleJsonKeyModel);
                            _context.SaveChanges();
                            styleJsonKeyModels.Add(styleJsonKeyModel);
                            _context.SaveChanges();
                        }

                        mainTable.StyleJsonKeyModel = styleJsonKeyModels;
                        mainTable.CommonID = CommonIDGetNext(key, flag);
                        _context.MainTable.Update(mainTable);
                        _context.SaveChanges();
                    }
                }
                _context.SaveChanges();
            }
        }

        private string CommonIDGetNext(string sectionKey, bool flag)
        {
            string CommonID = "";
            string Zero = "0000";
            int NextNumb;
            foreach (var section in _context.Section)
            {
                if (section.Title == sectionKey)
                {
                    if (section.ShortName != null)
                    {
                        CommonID = section.ShortName.Trim();
                    }
                    try
                    {
                        if (flag)
                        {
                            NextNumb = int.Parse(section.LastIndexOfCommonID) + 2;
                        }
                        else
                        {
                            NextNumb = int.Parse(section.LastIndexOfCommonID) + 1;
                        }

                        CommonID += Zero.Remove(Zero.Length - NextNumb.ToString().Length) + NextNumb.ToString();
                        section.LastIndexOfCommonID = CommonID.Remove(0, 4);
                        _context.SaveChanges();
                        break;
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine("Насяльника, как буква сыфра складывать будем? Сыфра нэту! " + ex);
                    }
                }
            }
            return CommonID;
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


        public void ErrorsToUserVIew(string name, string message)
        {
            ViewData[name] = message;
        }

        private void isHaveErrorsToUserView()
        {
            if (_context.Section.Count() != 0)
            {
                ErrorsToUserVIew("Section", "");
            }
            else
            {
                ErrorsToUserVIew("Section", "Нет разделов в базе. Обнови разделы, по-братски!");
            }
        }

        //формирование файла json
        public IActionResult CreateFileJsonFromDb()
        {
            // указываем путь к файлу
            string pathJsonFile;
            List<JsonKeyModel> jsonKeyModels = new List<JsonKeyModel>();

            foreach (var item in _context.MainTable)
            {
                if (!item.IsFreezing)
                {
                    var sections = _context.Section.Where(t => t.ID == item.SectionID).ToList();
                    List<LangKeyModel> langKeyModels = new List<LangKeyModel>
                    {
                        //new LangKeyModel { LangName = "ru", IE = item.TextRU },
                        //new LangKeyModel { LangName = "en", IE = item.TextEN },
                        //new LangKeyModel { LangName = "ua", IE = item.TextUA }
                    };
                    //jsonKeyModels.Add(new JsonKeyModel { JsonKey = item.IOsID, JsonValue = langKeyModels });
                }
            }
            foreach (var item in _context.Section)
            {
                try
                {
                    var mainTable = _context.MainTable.Where(m => m.SectionID == item.ID).ToList();
                    pathJsonFile = FileActionHelpers.DownloadPath + item.Title + ".json";

                    //начало фала
                    using (StreamWriter sw = new StreamWriter(pathJsonFile, false, System.Text.Encoding.Default))
                    {
                        sw.WriteLine("{");
                        sw.Close();
                    }

                    using (StreamWriter sw = new StreamWriter(pathJsonFile, true, System.Text.Encoding.Default))
                    {
                        var last = jsonKeyModels.Last();
                        foreach (var jsonKeyModel in jsonKeyModels)
                        {
                            string keyJson = "    \"" + jsonKeyModel.JsonKey + "\": {";
                            sw.WriteLine(keyJson);
                            //var lastJV = jsonKeyModel.JsonValue.Last();
                            //foreach (var JsonValues in jsonKeyModel.JsonValue)
                            {
                                //string comma = JsonValues.Equals(lastJV) ? "" : ",";
                                //sw.WriteLine(String.Format("        \"{0}\": \"{1}\"" + comma, JsonValues.LangName, JsonValues.IE));
                            }
                            sw.WriteLine(jsonKeyModel.Equals(last) ? "    }" : "    },"); //найти форматер стандартный из c#
                        }
                        sw.Close();
                    }

                    //окончание файла
                    using (StreamWriter sw = new StreamWriter(pathJsonFile, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine("}");
                        sw.Close();
                    }
                    Console.WriteLine("Запись выполнена");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return RedirectToAction("Index");
        }

        //чтение файла xml
        public IActionResult ReadFileXML()
        {
            //var list =
            FAXML.ReadFileXMLAction();
            //ViewBag.List = list;
            return RedirectToAction("Index");
        }


        public IActionResult DeleteDublicateAction()
        {

            FAXML.DeleteDublicate();

            return RedirectToAction("Index");
        }


        //формирование файла xml
        public void CreateFileXMLFromDb()
        {

        }

        //формирование файла csv
        public IActionResult CreateFileCSVFromDb(string nameFileCsv)
        {
            FileActionCSV.CreateFileCSV(nameFileCsv, _context);

            return RedirectToAction("Index");
        }

        //чтение файла csv
        public List<CsvFileModel> ReadUploadedCSVFile(string fileName)
        {
            return FileActionCSV.ReadUploadedCSV(fileName);
        }

        public IActionResult RemoveFileFromServer(string fullpath)
        {
            FileActionHelpers.RemoveFile(fullpath, _context);

            return RedirectToAction("Index");
        }

    }
}


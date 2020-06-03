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
using CsvHelper;
using System.Globalization;
using System.Collections;
using System.Text.Json;
using System.Xml;

namespace LocalizationRep.Controllers
{
    public class FileActionController : Controller
    {
        private readonly LocalizationRepContext _context;
        private readonly IWebHostEnvironment _appEnvironment;

        private readonly Dictionary<string, string> fileСontents = new Dictionary<string, string>();
        //private readonly Dictionary<string, string> FileInfo = new Dictionary<string, string>();
        private readonly List<FileModel> FileInfo = new List<FileModel>();

        public FileActionController(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
        }

        // GET: FileAction
        public async Task<IActionResult> Index()
        {
            isHaveErrorsToUserView();

            return View(await _context.FileModel.ToListAsync());
        }


        /// <summary>
        /// Загрузка файла на сервер, в корневую папку - подпапка "Files"
        /// Добавление информации о файле в таблицу FileModel базы данных
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddFile(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                // путь к папке Files
                string path = Path.Combine(_appEnvironment.WebRootPath, "/Files/upload/" + uploadedFile.FileName);
                // сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                if (!_context.FileModel.Where(s => s.Path == path).Any())
                {
                    FileModel file = new FileModel { Name = uploadedFile.FileName, Path = path };
                    _context.FileModel.Add(file);
                }
                _context.SaveChanges();

            }
            UpDateInfoFilesInDB();
            //UpdateFileInDatabase(uploadedFile);
            return RedirectToAction("Index");
        }


        public IActionResult UpdateFileInDatabase(string filename)
        {
            //List<CsvFileModel> st;
            switch (Path.GetExtension(filename))
            {
                case ".csv":
                    UpdateFromCsv(ReadUploadedCSVFile(filename));
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

        private void UpdateFromCsv(List<CsvFileModel> csvFiles)
        {
            foreach (var item in csvFiles)
            {
                var entity = _context.MainTable.FirstOrDefault(e => e.CommonID == item.CommonID); //Where(m => m.CommonID == item.CommonID);
                if (entity != null)
                {
                    entity.TextEN = item.TextEN;
                    entity.TextRU = item.TextRU;
                    entity.TextUA = item.TextUA;

                    _context.MainTable.Update(entity);

                    _context.SaveChanges();
                }
            }
        }

        // POST: FileAction/UpDateInfoFilesInDB
        [HttpPost]
        public IActionResult UpDateInfoFilesInDB()
        {
            List<FileModel> filesOnSrv = GetFilesInfo();
            FileModel file;

            if (filesOnSrv.Count == 0)
            {
                _context.FileModel.RemoveRange(_context.FileModel);
            }
            else
            {
                foreach (var item in filesOnSrv)
                {
                    if (!_context.FileModel.Where(s => s.Path == item.Path).Any())
                    {
                        file = new FileModel { ID = _context.FileModel.Count(), Name = item.Name, Path = item.Path, TypeOfLoad = item.TypeOfLoad };
                        _context.FileModel.Add(file);
                    }

                }
            }
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Метод обновления информации в базе данных о разделах
        /// </summary>
        /// <returns></returns>
        // GET: FileAction/UpdateSectionFromFile
        public IActionResult UpdateSectionFromFile()
        {
            AddUniqeSectionToDbTable(GetFilesInfo());

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Метод получающий данные о файлах(путь расположения) в переменную FileInfo
        /// Также фильтрует файлы по расширениям json и xml
        /// </summary>
        private List<FileModel> GetFilesInfo()
        {
            string filePathUpload = Path.Combine(_appEnvironment.WebRootPath, "Files/upload/");
            string filePathDownload = Path.Combine(_appEnvironment.WebRootPath, "Files/download/");
            string[] DocumentsUpload = Directory.GetFiles(filePathUpload);
            string[] DocumentsDownload = Directory.GetFiles(filePathDownload);

            foreach (var item in DocumentsUpload)
            {
                if (Path.GetExtension(item) == ".json" || Path.GetExtension(item) == ".xml" || Path.GetExtension(item) == ".csv")
                {
                    //FileInfo.Add(new FileModel { Name = Path.GetFileNameWithoutExtension(item), Path = item, TypeOfLoad = "upload" });
                    FileInfo.Add(new FileModel { Name = Path.GetFileName(item), Path = item, TypeOfLoad = "upload" });
                }
            }

            foreach (var item in DocumentsDownload)
            {
                if (Path.GetExtension(item) == ".json" || Path.GetExtension(item) == ".xml" || Path.GetExtension(item) == ".csv")
                {
                    //FileInfo.Add(new FileModel { Name = Path.GetFileNameWithoutExtension(item), Path = item, TypeOfLoad = "download" });
                    FileInfo.Add(new FileModel { Name = Path.GetFileName(item), Path = item, TypeOfLoad = "download" });
                }
            }

            return FileInfo;
        }

        /// <summary>
        /// Метод очищающий все таблицы базы данных
        /// TODO разделить на отдельные методы для очистки каждой таблицы
        /// </summary>
        /// <returns></returns>
        ///
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
                    _context.MainTable.RemoveRange(_context.MainTable);
                    break;
            }
            //очистка таблицы Section
            //if (_context.Section.Any() || _context.MainTable.Any() || _context.FileModel.Any())
            //{
            //    _context.FileModel.RemoveRange(_context.FileModel);
            //    _context.Section.RemoveRange(_context.Section);
            //    _context.MainTable.RemoveRange(_context.MainTable);

            //}
            _context.SaveChanges();
            UpDateInfoFilesInDB();
            return RedirectToAction("Index");
        }



        // GET: FileAction/PopulateDBWithValuesFromFiles
        public IActionResult PopulateDBWithValuesFromFiles()
        {
            if (_context.Section.Count() != 0)
            {
                Initialize(GetFilesInfo());
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

                    JsonToDb(Path.GetFileNameWithoutExtension(item.Name), inFile.Remove(0, 1).Remove(inFile.Length - 2, 1).Split('}'));
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
                if (fileСontentsItem.TypeOfLoad.Equals("upload"))
                {
                    foreach (Sections section in _context.Section)
                    {
                        if (fileСontentsItem.Name.Equals(section.Title))
                        {
                            next = false;
                            continue;
                        }
                    }
                    if (next && Path.GetExtension(fileСontentsItem.Path) == ".json")
                    {
                        _context.Section.AddRange(
                                   new Sections
                                   {
                                       Title = Path.GetFileNameWithoutExtension(fileСontentsItem.Name).ToString(),
                                       LastIndexOfCommonID = "0000",
                                       ShortName = fileСontentsItem.Name.Remove(4, fileСontentsItem.Name.Length - 4).ToUpper()
                                   });
                    }
                }
            }
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public void GetAllSectors()
        {
            foreach (Sections section in _context.Section)
                Console.WriteLine(section.Title);
        }

        public void JsonToDb(string key, string[] itemOfJSONInAllString)
        {
            string IOsID = "", AndroidID = "", TextRU = "", TextEN = "", TextUA = "";
            bool next = true;
            var sections = _context.Section.Where(t => t.Title == key).ToList();
            Dictionary<string, Dictionary<string, string>> tmp = ParceJsonToDb(key, itemOfJSONInAllString);
            if (sections.Count != 0)
            {
                foreach (var parse in tmp)
                {
                    foreach (MainTable mainTable in _context.MainTable) //TODO переделать на _context.Maintable.FirstDefault()
                    {
                        if (parse.Key.Equals(mainTable.IOsID))
                        {
                            next = false;
                            break;
                        }
                    }
                    if (next)
                    {
                        IOsID = parse.Key;
                        foreach (var item in parse.Value)
                        {
                            switch (item.Key)
                            {
                                case "uk":
                                    TextUA = item.Value;
                                    break;
                                case "ru":
                                    TextRU = item.Value;
                                    break;
                                case "en":
                                    TextEN = item.Value;
                                    break;
                                //case "en":
                                //    TextEN = item.Value;
                                //    break;
                                //case "en":
                                //    TextEN = item.Value;
                                //    break;
                                default:
                                    TextRU = "non";
                                    TextEN = "non";
                                    TextUA = "non";
                                    break;
                            }
                        }
                        try
                        {
                            _context.MainTable.AddRange(
                            new MainTable
                            {
                                SectionID = sections.First().ID,

                                CommonID = CommonIDGetNext(key),
                                IOsID = IOsID,
                                AndroidID = AndroidID,

                                TextRU = TextRU,
                                TextEN = TextEN,
                                TextUA = TextUA,

                                IsFreezing = false

                            });
                        }
                        catch (InvalidOperationException ex)
                        {
                            Console.WriteLine("Нет секций в базе. Обнови секции, по-братски!\n" + ex);
                        }
                    }
                }
                _context.SaveChanges();
            }
        }

        private string CommonIDGetNext(string sectionKey)
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
                        NextNumb = int.Parse(section.LastIndexOfCommonID) + 1;
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

        public Dictionary<string, Dictionary<string, string>> ParceJsonToDb(string sector, string[] itemOfJSONInAllString)
        {
            Dictionary<string, Dictionary<string, string>> tempJSON = new Dictionary<string, Dictionary<string, string>>();
            Char[] elementForTrimStrings = new Char[] { ' ', '"', ':', ',', '{' };
            string[] tempString = new string[13];

            foreach (var item in itemOfJSONInAllString)
            {
                if (item == "")
                {
                    continue;
                }

                string[] tempItem = item.Split("\"");
                List<string> trimedString = new List<string>();
                int key = 0;

                for (int i = 0; i < tempItem.Length; i++)
                {
                    if (tempItem[i].Trim(elementForTrimStrings) != "")
                    {
                        trimedString.Add(tempItem[i].Trim(elementForTrimStrings));
                    }
                }

                Array.Clear(tempString, 0, tempString.Length);
                tempString = new string[trimedString.Count];
                if (tempString.Length != 0)
                {
                    foreach (var str in trimedString)
                    {
                        tempString[key] = str;
                        key++;
                    }

                    Dictionary<string, string> pairs = new Dictionary<string, string>();

                    for (int i = 1; i < tempString.Length - 1; i += 2)
                    {
                        pairs.Add(tempString[i], tempString[i + 1]);
                    }

                    tempJSON.Add(tempString[0], pairs);
                }
            }

            return tempJSON;
        }
        /// <summary>
        /// скачать файл
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task<IActionResult> Download(string fullpath)
        {
            //if (filename == null)
            //    return Content("filename not present");

            var path = fullpath; // Path.Combine(
                           //Directory.GetCurrentDirectory(),
                           //"wwwroot/Files/download", filename);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".json","application/json"},
                {".xml","application/xml"},
                {".csv","text/csv"}
            };
        }

        public void ErrorsToUserVIew(string id, string message)
        {
            ViewData[id] = message;
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
            string pathJsonFile;// = "wwwroot/Files/download/" + nameJsonFile + ".json";
            List<JsonKeyModel> jsonKeyModels = new List<JsonKeyModel>();

            foreach (var item in _context.MainTable)
            {
                if (!item.IsFreezing)
                {
                    var sections = _context.Section.Where(t => t.ID == item.SectionID).ToList();
                    List<LangKeyModel> langKeyModels = new List<LangKeyModel>
                    {
                        new LangKeyModel { LangName = "ru", IE = item.TextRU },
                        new LangKeyModel { LangName = "en", IE = item.TextEN },
                        new LangKeyModel { LangName = "ua", IE = item.TextUA }
                    };
                    jsonKeyModels.Add(new JsonKeyModel { JsonKey = item.IOsID, JsonValue = langKeyModels });
                }
            }
            foreach (var item in _context.Section)
            {

                try
                {
                    var mainTable = _context.MainTable.Where(m => m.SectionID == item.ID).ToList();
                    pathJsonFile = "wwwroot/Files/download/" + item.Title + ".json";

                    //начало фала
                    using (StreamWriter sw = new StreamWriter(pathJsonFile, false, System.Text.Encoding.Default))
                    {
                        sw.WriteLine("{");
                    }

                    using (StreamWriter sw = new StreamWriter(pathJsonFile, true, System.Text.Encoding.Default))
                    {
                        var last = jsonKeyModels.Last();
                        foreach (var jsonKeyModel in jsonKeyModels)
                        {
                            string keyJson = "    \"" + jsonKeyModel.JsonKey + "\": {";
                            sw.WriteLine(keyJson);
                            var lastJV = jsonKeyModel.JsonValue.Last();
                            foreach (var JsonValues in jsonKeyModel.JsonValue)
                            {
                                string comma = JsonValues.Equals(lastJV) ? "" : ",";
                                sw.WriteLine(String.Format("        \"{0}\": \"{1}\"" + comma, JsonValues.LangName, JsonValues.IE));
                            }
                            sw.WriteLine(jsonKeyModel.Equals(last) ? "    }" : "    },");
                        }
                        sw.Close();
                    }

                    //окончание файла
                    using (StreamWriter sw = new StreamWriter(pathJsonFile, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine("}");
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
        public IActionResult ReadFileXML(string path)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);

            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode xnode in xRoot)
            {
                if (xnode.NodeType != XmlNodeType.Comment)
                {
                    if (xnode.Attributes.Count > 0)
                    {
                        XmlNode attr = xnode.Attributes.GetNamedItem("name");
                        if (attr != null)
                        {
                            Console.WriteLine(attr.Value + " = " + xnode.InnerText);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("\n<--" + xnode.Value.ToString() + "-->");
                }
            }

            return RedirectToAction("Index");
        }
        //формирование файла xml
        public void CreateFileXMLFromDb()
        {

        }

        //формирование файла csv
        public IActionResult CreateFileCSVFromDb(string nameFileCsv)
        {
            string pathCsvFile = "wwwroot/Files/download/" + nameFileCsv + ".csv";
            List<CsvFileModel> csvFileModel = new List<CsvFileModel>();

            foreach (var item in _context.MainTable)
            {
                if (!item.IsFreezing)
                {
                    var sections = _context.Section.Where(t => t.ID == item.SectionID).ToList();
                    csvFileModel.Add(new CsvFileModel { CommonID = item.CommonID, SectorName = sections.First().Title, TextRU = item.TextRU, TextEN = item.TextEN, TextUA = item.TextUA });
                }
            }

            using StreamWriter streamReader = new StreamWriter(pathCsvFile);
            using CsvWriter csvReader = new CsvWriter(streamReader, CultureInfo.InvariantCulture);
            csvReader.Configuration.Delimiter = ";";
            csvReader.WriteRecords(csvFileModel);

            return RedirectToAction("Index");
        }

        //чтение файла csv
        public List<CsvFileModel> ReadUploadedCSVFile(string FileName)
        {
            string pathCsvFile = "wwwroot/Files/download/" + FileName;
            var list = new List<CsvFileModel>();
            string line;
            try
            {
                using StreamReader streamReader = new StreamReader(pathCsvFile);
                while ((line = streamReader.ReadLine()) != null)
                {
                    string[] lineOfCsvFile = line.Split(';');
                    if (lineOfCsvFile.Length == 5)
                    {
                        list.Add(new CsvFileModel { CommonID = lineOfCsvFile[0], SectorName = lineOfCsvFile[1], TextEN = lineOfCsvFile[2], TextRU = lineOfCsvFile[3], TextUA = lineOfCsvFile[4] });
                    }
                }
                streamReader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return list;
        }

        public IActionResult RemoveFileFromServer(string fullpath)
        {
            RemoveFile(fullpath);
            return RedirectToAction("Index");
        }

        public void RemoveFile(string fullpath)
        {
            var entity = _context.FileModel.FirstOrDefault(e => e.Path == fullpath);
            System.IO.File.Delete(fullpath);
            _context.FileModel.Remove(entity);
            _context.SaveChanges();
        }
    }
}


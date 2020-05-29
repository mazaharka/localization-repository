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

namespace LocalizationRep.Controllers
{
    public class FileActionController : Controller
    {
        LocalizationRepContext _context;
        IWebHostEnvironment _appEnvironment;

        private readonly Dictionary<string, string> fileСontents = new Dictionary<string, string>();
        private readonly Dictionary<string, string> FileInfo = new Dictionary<string, string>();


        public FileActionController(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            return View(_context.FileModel.ToList());
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
                string path = "/Files/" + uploadedFile.FileName;
                // сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                FileModel file = new FileModel { Name = uploadedFile.FileName, Path = path };
                _context.FileModel.Add(file);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Метод обновления информации в базе данных о разделах
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateSectionFromFile()
        {
            GetFilesInfo();
            AddUniqeSectionToDbTable(FileInfo);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Метод получающий данные о файлах(путь расположения) в переменную FileInfo
        /// Также фильтрует файлы по расширениям json и xml
        /// </summary>
        private void GetFilesInfo()
        {
            string filePath = Path.Combine(_appEnvironment.WebRootPath, "Files");
            string[] Documents = Directory.GetFiles(filePath);

            foreach (var item in Documents)
            {
                if (Path.GetExtension(item) == ".json" || Path.GetExtension(item) == ".xml")
                {
                    FileInfo.Add(Path.GetFileNameWithoutExtension(item), item);
                }
            }
        }

        /// <summary>
        /// Метод очищающий все таблицы базы данных
        /// TODO разделить на отдельные методы для очистки каждой таблицы
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EraseAllSectionFromDb()
        {
            //очистка таблицы Section
            if (_context.Section.Any() || _context.MainTable.Any())
            {
                _context.FileModel.RemoveRange(_context.FileModel);
                _context.Section.RemoveRange(_context.Section);
                _context.MainTable.RemoveRange(_context.MainTable);
            }
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult PopulateDBWithValuesFromFiles()
        {
            GetFilesInfo();
            Initialize(FileInfo);

            return RedirectToAction("Index");
        }

        public void Initialize(Dictionary<string, string> FilesName)
        {
            foreach (var item in FilesName)
            {
                using StreamReader sr = new StreamReader(item.Value);
                string line;
                string inFile = "";
                while ((line = sr.ReadLine()) != null)
                {
                    inFile += line;
                    Console.WriteLine("inFile = " + inFile + "\nline = " + line + "\n");
                    //Console.WriteLine(sr.ReadLine());
                }

                fileСontents.Add(item.Key, inFile.Remove(0, 1).Remove(inFile.Length - 2, 1)); // Trim(new char[] { '{', '}' }));
                sr.Close();
            }

            //AddUniqeSectionToDbTable();

            //string[] itemOfJSONInAllString;
            foreach (var fileItem in fileСontents)
            {
                //itemOfJSONInAllString = fileItem.Value
                //    .Split('}');
                JsonToDb(fileItem.Key, fileItem.Value.Split('}'));
            }


        }

        [HttpPost]
        public IActionResult AddUniqeSectionToDbTable(Dictionary<string, string> FilesName)
        {

            foreach (var fileСontentsItem in FilesName)
            {
                bool next = true;
                foreach (Sections section in _context.Section)
                {
                    if (fileСontentsItem.Key.Equals(section.Title))
                    {
                        Console.WriteLine(fileСontentsItem.Key + " EQUALS " + section.Title);
                        next = false;
                        continue;
                    }
                    else
                    {
                        Console.WriteLine(fileСontentsItem.Key + " NOT EQ " + section.Title);
                    }
                }
                if (next)
                {
                    _context.Section.AddRange(
                               new Sections
                               {
                                   Title = fileСontentsItem.Key.ToString(),
                                   LastIndexOfCommonID = "0000"
                               });
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public void GetAllSectors()
        {
            // Извлечь всех заказчиков и отобразить их имена в консоли
            foreach (Sections section in _context.Section)
                Console.WriteLine(section.Title);
        }

        public void JsonToDb(string key, string[] itemOfJSONInAllString)
        {
            string IOsID = "", AndroidID = "", TextRU = "", TextEN = "", TextUA = "";
            bool next = true;
            //GetAllSectors();
            var sections = _context.Section.Where(t => t.Title == key).ToList();
            Dictionary<string, Dictionary<string, string>> tmp = ParceJsonToDb(key, itemOfJSONInAllString);

            foreach (var parse in tmp)
            {
                foreach (MainTable mainTable in _context.MainTable)
                {
                    if (parse.Key.Equals(mainTable.IOsID))
                    {
                        Console.WriteLine(parse.Key + " EQUALS " + mainTable.IOsID);
                        next = false;
                        break;
                    }
                    else
                    {
                        Console.WriteLine(parse.Key + " NOT EQ " + mainTable.IOsID);
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
                            default:
                                TextRU = "non";
                                TextEN = "non";
                                TextUA = "non";
                                break;
                        }
                    }

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
            }
            _context.SaveChanges();
        }

        private string CommonIDGetNext(string sectionKey)
        {
            string CommonID = "";
            string Zero = "0000";

            foreach (var section in _context.Section)
            {
                if (section.Title == sectionKey)
                {
                    CommonID = section.ShortName;
                    int NextNumb = int.Parse(section.LastIndexOfCommonID) + 1;
                    CommonID += Zero.Remove(Zero.Length - NextNumb.ToString().Length) + NextNumb.ToString();
                    section.LastIndexOfCommonID = CommonID.Remove(0, 4);
                    _context.SaveChanges();
                    break;
                }
            }
            return CommonID;
        }

        public Dictionary<string, Dictionary<string, string>> ParceJsonToDb(string sector, string[] itemOfJSONInAllString)
        {
            Dictionary<string, Dictionary<string, string>> tempJSON = new Dictionary<string, Dictionary<string, string>>();
            //string pattern = @"\""\w*(-\w*)*\"": {";
            //string pattern = @": {";

            //string patternRUexcess = @"\""ru\"": {";
            //string patternENexcess = @"\""en\"": {";
            //string patternUKexcess = @"\""uk\"": {";

            //string patternRU = @"\""ru\""";
            //string patternEN = @"\""en\""";
            //string patternUK = @"\""uk\""";

            Char[] elementForTrimKeys = new Char[] { ' ', '"', ':', '{' };
            Char[] elementForTrimStrings = new Char[] { ' ', '"', ':', ',', '{' };
            string[] tempString = new string[13];

            foreach (var item in itemOfJSONInAllString)
            {
                if (item == "")
                {
                    continue;
                }

                //Console.WriteLine();
                //Console.WriteLine("Sector:{0} ", sector);
                //Console.WriteLine();


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
                    //Console.WriteLine();
                }
            }
            return tempJSON;
        }

        public ActionResult DownloadFile(string filePath)
        {
            GetFilesInfo();
            string fullName = ""; //FileInfo.Values; // Server.MapPath("~" + filePath);

            byte[] fileBytes = GetFile(filePath);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filePath);
        }

        byte[] GetFile(string s)
        {
            FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new IOException(s);
            return data;
        }
    }
}


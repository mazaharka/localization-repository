using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationRep.Data;
using LocalizationRep.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LocalizationRep.Utilities
{
    public class FileActionJSON
    {
        private readonly LocalizationRepContext _context;
        private readonly IWebHostEnvironment _appEnvironment;

        public FileActionJSON(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
        }

        public void JsonToDb(string key, string itemOfJSONInAllString)
        {
            //string CommonID;
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
                        try
                        {
                            mainTable = new MainTable
                            {
                                SectionID = sections.Find(s => s.Title == key).ID,
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
                        foreach (var langKey in parse.Value)
                        {
                            styleNameKey = langKey.Key;
                            styleJsonKeyModel = new StyleJsonKeyModel
                            {
                                StyleName = styleNameKey,
                                MainTables = mainTable
                            };
                            _context.StyleJsonKeyModel.Add(styleJsonKeyModel);
                            _context.SaveChanges();

                            langKeyModels.Clear();
                            LangValue langValues = new LangValue();
                            string itemLangName = "ru/en/ua";

                            foreach (var itemLangKey in langKey.Value)
                            {
                                itemLangName = itemLangKey.Key;

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
                        mainTable.CommonID = ActionWithDataBase.CommonIDGetNext(_context, key);
                        _context.MainTable.Update(mainTable);
                        _context.SaveChanges();
                    }
                }
                _context.SaveChanges();
            }
        }



        public void UpdateFromJsonToDbLocalizedText(string nameFileJSON)
        {
            string pathFileFSON = "wwwroot/Files/upload/" + nameFileJSON;
            string itemOfJSONInAllString = "";
            if (Path.GetExtension(pathFileFSON) == ".json")
            {
                using StreamReader sr = new StreamReader(pathFileFSON);
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    itemOfJSONInAllString += line;
                }
                sr.Close();
            }

            MainTable mainTable = new MainTable();
            StyleJsonKeyModel styleJsonKeyModel = new StyleJsonKeyModel();
            LangKeyModel langKeyModel = new LangKeyModel();
            LangValue langValue = new LangValue();

            Dictionary<string, Dictionary<string, string>> Localized = new Dictionary<string, Dictionary<string, string>>();

            try
            {
                Localized = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(itemOfJSONInAllString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            var sections = _context.Section.ToList();
            if (sections.Count != 0)
            {
                foreach (var parse in Localized)
                {
                    if (_context.MainTable.FirstOrDefault(s => s.IOsID == parse.Key) == null)
                    {
                        MainTable item = _context.MainTable.First(s => s.CommonID == parse.Key);

                        string SectorName = "";
                        string Style = "";
                        string TextRUSingle = "";
                        string TextENSingle = "";
                        string TextUASingle = "";

                        foreach (var langKey in parse.Value)
                        {
                            switch (langKey.Key)
                            {
                                case "SectorName":
                                    SectorName = langKey.Value;
                                    break;
                                case "Style":
                                    Style = langKey.Value;
                                    break;
                                case "TextRUSingle":
                                    TextRUSingle = langKey.Value;
                                    break;
                                case "TextENSingle":
                                    TextENSingle = langKey.Value;
                                    break;
                                case "TextUASingle":
                                    TextUASingle = langKey.Value;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    _context.SaveChanges();
                }
            }
        }

        public void CreateJSONFiles()
        {
            // указываем путь к файлу
            string pathJsonFile;
            List<JsonKeyModel> jsonKeyModels = new List<JsonKeyModel>();


            var sections = _context.Section.ToList();
            sections.RemoveAt(sections.Count() - 1);
            foreach (var section in sections)
            {


                var mainTableItems = (from m in _context.MainTable
                    .Include(m => m.Section)
                    .Include(m => m.StyleJsonKeyModel)
                        .ThenInclude(s => s.LangKeyModels)
                            .ThenInclude(l => l.LangValue)
                                      where m.Section.ID == section.ID && m.IOsID != null
                                      select m).ToList();
                var it = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
                Dictionary<string, Dictionary<string, Dictionary<string, object>>> Localized = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

                foreach (var mainTableItem in mainTableItems)
                {
                    var itt = new Dictionary<string, Dictionary<string, object>>();
                    var ittt = new Dictionary<string, object>();

                    var styleJsonKeyModelItems = mainTableItem.StyleJsonKeyModel.ToList();

                    foreach (var styleJsonKeyModelItem in styleJsonKeyModelItems)
                    {

                        var langKeyModelItems = styleJsonKeyModelItem.LangKeyModels.ToList();
                        ittt = new Dictionary<string, object>();
                        foreach (var langKeyModelItem in langKeyModelItems)
                        {
                            ittt.Add(langKeyModelItem.LangName, langKeyModelItem.LangValue.Single);
                        }
                        itt.Add(styleJsonKeyModelItem.StyleName, ittt);
                    }
                    if (!it.ContainsKey(mainTableItem.IOsID))
                    {
                        it.Add(mainTableItem.IOsID, itt);
                        Localized.Add(mainTableItem.IOsID, itt);
                    }
                }

                string json = JsonConvert.SerializeObject(Localized, Formatting.Indented);
                pathJsonFile = "wwwroot/Files/download/iOs/" + section.Title + ".json";

                using StreamWriter sw = new StreamWriter(pathJsonFile, false, System.Text.Encoding.Default);
                sw.WriteLine(json);
                sw.Close();
            }
        }
    }
}

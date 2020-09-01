using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationRep.Data;
using LocalizationRep.Models;
using Microsoft.AspNetCore.Hosting;
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
    }
}

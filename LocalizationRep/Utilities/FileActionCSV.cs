using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationRep.Data;
using LocalizationRep.Models;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using System;

namespace LocalizationRep.Utilities
{
    public class FileActionCSV
    {
        public static void CreateFileCSV(string nameFileCsv, LocalizationRepContext _context)
        {
            string pathCsvFile = "wwwroot/Files/download/" + nameFileCsv + ".csv";
            List<CsvFileModel> csvFileModel = new List<CsvFileModel>();
            var mainTableItem = from m in _context.MainTable
                                .Include(m => m.Section)
                                .Include(m => m.StyleJsonKeyModel)
                                    .ThenInclude(s => s.LangKeyModels)
                                        .ThenInclude(l => l.LangValue)
                                select m;

            IQueryable<string> stylesUnique = from s in _context.StyleJsonKeyModel
                                              orderby s.StyleName
                                              select s.StyleName;

            IQueryable<string> langNameUnique = from s in _context.LangKeyModel
                                                orderby s.LangName
                                                select s.LangName;

            List<string> TextValues = new List<string>();
            foreach (var item in mainTableItem)
            {
                if (!item.IsFreezing)
                {
                    string sectionName = item.Section.Title;
                    var sections = _context.Section.Where(t => t.ID == item.SectionID).ToList();
                    foreach (var itemNode in item.StyleJsonKeyModel)
                    {

                        string textStyle = itemNode.StyleName;
                        string textRUSingle = itemNode.LangKeyModels.Where(lang => lang.LangName == "ru").First().LangValue.Single;
                        string textRUPrular = itemNode.LangKeyModels.Where(lang => lang.LangName == "ru").First().LangValue.Prular;
                        string textENSingle = itemNode.LangKeyModels.Where(lang => lang.LangName == "en").First().LangValue.Single;
                        string textENPrular = itemNode.LangKeyModels.Where(lang => lang.LangName == "en").First().LangValue.Prular;
                        string textUASingle = itemNode.LangKeyModels.Where(lang => lang.LangName == "uk").First().LangValue.Single;
                        string textUAPrular = itemNode.LangKeyModels.Where(lang => lang.LangName == "uk").First().LangValue.Prular;

                        csvFileModel.Add(new CsvFileModel
                        {
                            CommonID = item.CommonID,
                            SectorName = sectionName,
                            TextStyle = itemNode.StyleName,
                            TextRUSingle = itemNode.LangKeyModels.Where(lang => lang.LangName == "ru").First().LangValue.Single,
                            TextRUPrular = itemNode.LangKeyModels.Where(lang => lang.LangName == "ru").First().LangValue.Prular,
                            TextENSingle = itemNode.LangKeyModels.Where(lang => lang.LangName == "en").First().LangValue.Single,
                            TextENPrular = itemNode.LangKeyModels.Where(lang => lang.LangName == "en").First().LangValue.Prular,
                            TextUASingle = itemNode.LangKeyModels.Where(lang => lang.LangName == "uk").First().LangValue.Single,
                            TextUAPrular = itemNode.LangKeyModels.Where(lang => lang.LangName == "uk").First().LangValue.Prular
                        });
                    }
                }
            }
            using StreamWriter streamWriter = new StreamWriter(pathCsvFile);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.Configuration.Delimiter = ";";
            csvWriter.WriteRecords(csvFileModel);
        }


        public static List<CsvFileModel> ReadUploadedCSV(string fileName)
        {
            string pathCsvFile = FileActionHelpers.UploadPath + fileName;
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
                        list.Add(new CsvFileModel
                        {
                            CommonID = lineOfCsvFile[0],
                            SectorName = lineOfCsvFile[1],
                            TextStyle = lineOfCsvFile[2],
                            TextRUSingle = lineOfCsvFile[3],
                            TextRUPrular = lineOfCsvFile[4],
                            TextENSingle = lineOfCsvFile[5],
                            TextENPrular = lineOfCsvFile[6],
                            TextUASingle = lineOfCsvFile[7],
                            TextUAPrular = lineOfCsvFile[8]
                        });
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
    }
}

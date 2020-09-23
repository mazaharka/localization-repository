using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationRep.Data;
using LocalizationRep.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalizationRep.Controllers
{
    public static class CSVActionController
    {
        public static void UpdateFromCsv(List<CsvFileModel> csvFiles, LocalizationRepContext _context)
        {
            var mainTableItemItems = (from m in _context.MainTable
                                .Include(m => m.Section)
                                .Include(m => m.StyleJsonKeyModel)
                                    .ThenInclude(s => s.LangKeyModels)
                                        .ThenInclude(l => l.LangValue)
                                      select m).ToList();
            CsvFileModel itCSV = new CsvFileModel();

            foreach (var mainTableItemItem in mainTableItemItems)
            {
                itCSV = csvFiles.Where(s => s.CommonID == mainTableItemItem.CommonID).FirstOrDefault();

                if (itCSV != null)
                {
                    foreach (var styleJsonKeyModelItem in mainTableItemItem.StyleJsonKeyModel.Where(style => style.StyleName == itCSV.TextStyle).ToList())
                    {

                        //styleJsonKeyModelItem.StyleName = itCSV.TextStyle;

                        foreach (var langKeyModelItem in styleJsonKeyModelItem.LangKeyModels.ToList())
                        {
                            switch (langKeyModelItem.LangName)
                            {
                                case "ru":
                                    langKeyModelItem.LangValue.Single = itCSV.TextRUSingle;
                                    if (itCSV.TextRUPrular != null && itCSV.TextRUPrular != "" && itCSV.TextRUPrular != " ")
                                    {
                                        langKeyModelItem.LangValue.Prular = itCSV.TextRUPrular;
                                    }
                                    break;
                                case "en":
                                    langKeyModelItem.LangValue.Single = itCSV.TextENSingle;
                                    if (itCSV.TextENPrular != null && itCSV.TextENPrular != "" && itCSV.TextENPrular != " ")
                                    {
                                        langKeyModelItem.LangValue.Prular = itCSV.TextENPrular;
                                    }
                                    break;
                                case "uk":
                                    langKeyModelItem.LangValue.Single = itCSV.TextUASingle;
                                    if (itCSV.TextUAPrular != null && itCSV.TextUAPrular != "" && itCSV.TextUAPrular != " ")
                                    {
                                        langKeyModelItem.LangValue.Prular = itCSV.TextUAPrular;
                                    }
                                    break;
                                case "ua":
                                    langKeyModelItem.LangValue.Single = itCSV.TextUASingle;
                                    if (itCSV.TextUAPrular != null && itCSV.TextUAPrular != "" && itCSV.TextUAPrular != " ")
                                    {
                                        langKeyModelItem.LangValue.Prular = itCSV.TextUAPrular;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            _context.SaveChanges();
        }

        private static void RemoveSpaceItem(LangKeyModel langKeyModelItem)
        {
            if (langKeyModelItem.LangValue.Prular == "" || langKeyModelItem.LangValue.Prular == " ")
            {
                langKeyModelItem.LangValue.Prular = null;
            }
        }
    }
}
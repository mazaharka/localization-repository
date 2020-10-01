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
            foreach (var item in csvFiles)
            {

                //}
                //foreach (var mainTableItemItem in mainTableItemItems)
                //{
                MainTable mainTableItemItem = mainTableItemItems.Where(m => m.CommonID == item.CommonID).FirstOrDefault();
                    //itCSV = csvFiles.Where(s => s.CommonID == mainTableItemItem.CommonID).FirstOrDefault();

                if (mainTableItemItem != null)
                {
                    var sss = mainTableItemItem.StyleJsonKeyModel.Where(style => style.StyleName == item.TextStyle).ToList();
                    foreach (var styleJsonKeyModelItem in mainTableItemItem.StyleJsonKeyModel.Where(style => style.StyleName == item.TextStyle).ToList())
                    {

                        //styleJsonKeyModelItem.StyleName = itCSV.TextStyle;

                        foreach (var langKeyModelItem in styleJsonKeyModelItem.LangKeyModels.ToList())
                        {
                            switch (langKeyModelItem.LangName)
                            {
                                case "ru":
                                    langKeyModelItem.LangValue.Single = item.TextRUSingle;
                                    if (item.TextRUPrular != null && item.TextRUPrular != "" && item.TextRUPrular != " ")
                                    {
                                        langKeyModelItem.LangValue.Prular = item.TextRUPrular;
                                    }
                                    break;
                                case "en":
                                    langKeyModelItem.LangValue.Single = item.TextENSingle;
                                    if (item.TextENPrular != null && item.TextENPrular != "" && item.TextENPrular != " ")
                                    {
                                        langKeyModelItem.LangValue.Prular = item.TextENPrular;
                                    }
                                    break;
                                case "uk":
                                    langKeyModelItem.LangValue.Single = item.TextUASingle;
                                    if (item.TextUAPrular != null && item.TextUAPrular != "" && item.TextUAPrular != " ")
                                    {
                                        langKeyModelItem.LangValue.Prular = item.TextUAPrular;
                                    }
                                    break;
                                case "ua":
                                    langKeyModelItem.LangValue.Single = item.TextUASingle;
                                    if (item.TextUAPrular != null && item.TextUAPrular != "" && item.TextUAPrular != " ")
                                    {
                                        langKeyModelItem.LangValue.Prular = item.TextUAPrular;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            _context.LangKeyModel.Update(langKeyModelItem);
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
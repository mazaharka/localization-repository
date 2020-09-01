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
            //csvFiles.RemoveAt(0);



            //foreach (var itemcsv in csvFiles)
            //{
            //    var mainTableItemItem = (from m in _context.MainTable
            //                    .Include(m => m.Section)
            //                    .Include(m => m.StyleJsonKeyModel)
            //                        .ThenInclude(s => s.LangKeyModels)
            //                            .ThenInclude(l => l.LangValue)
            //                             where m.CommonID == itemcsv.CommonID
            //                             select m).First();

            //    var r = mainTableItemItem.StyleJsonKeyModel.ToList();

            //    bool f = true;
            //    if (f)
            //    {
            //        foreach (var styleJsonKeyModelItem in mainTableItemItem.StyleJsonKeyModel.ToList())
            //        {
            //            var ss = _context.StyleJsonKeyModel.Where(s => s.ID == styleJsonKeyModelItem.ID).FirstOrDefault();
            //            Console.WriteLine("styleID = " + ss.ID + " StyleName = " + ss.StyleName);

            //            switch (itemcsv.TextStyle)
            //            {
            //                case "dude":
            //                    ss.StyleName = "friendly";
            //                    f = false;
            //                    break;
            //                case "official":
            //                    ss.StyleName = "business";
            //                    f = false;
            //                    break;
            //                case "common":
            //                    ss.StyleName = "neutral";
            //                    f = false;
            //                    break;
            //                default:
            //                    break;
            //            }
            //            Console.WriteLine("styleID = " + ss.ID + " StyleName = " + ss.StyleName);
            //            _context.StyleJsonKeyModel.Update(ss);
            //            if (f)
            //            {
            //                break;
            //            }
            //        }
            //    }
            //}

            //_context.SaveChanges();
            //Dictionary<string, Dictionary<string, Dictionary<string, string>>> csvItems = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            //Dictionary<string, Dictionary<string, string>> styleAndLocalText = new Dictionary<string, Dictionary<string, string>>();
            //Dictionary<string, string> localNameAndLocalText = new Dictionary<string, string>();

            //foreach (var item in csvFiles)
            //{
            //    localNameAndLocalText.Add("ru", item.TextRUSingle);
            //    localNameAndLocalText.Add("en", item.TextENSingle);
            //    localNameAndLocalText.Add("uk", item.TextUASingle);
            //    styleAndLocalText.Add(item.TextStyle, localNameAndLocalText);
            //    csvItems.Add(item.CommonID, styleAndLocalText);
            //}
            var mainTableItemItems = (from m in _context.MainTable
                                .Include(m => m.Section)
                                .Include(m => m.StyleJsonKeyModel)
                                    .ThenInclude(s => s.LangKeyModels)
                                        .ThenInclude(l => l.LangValue)
                                      select m).ToList();
            foreach (var mainTableItemItem in mainTableItemItems)
            {
                var itCSV = csvFiles.Where(s => s.CommonID == mainTableItemItem.CommonID);
                if (itCSV != null)
                {
                    int i = 0;
                    foreach (var styleJsonKeyModelItem in mainTableItemItem.StyleJsonKeyModel.ToList())
                    {
                        var item = itCSV.ElementAt(i);
                        styleJsonKeyModelItem.StyleName = item.TextStyle;

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
                        }
                        i++;
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
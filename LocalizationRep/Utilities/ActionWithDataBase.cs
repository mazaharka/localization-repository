using System.Collections.Generic;
using System.Linq;
using LocalizationRep.Data;
using LocalizationRep.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace LocalizationRep.Utilities
{
    public class ActionWithDataBase
    {

        public static void UpDateInfoFilesInDBAction(LocalizationRepContext _context, IWebHostEnvironment _appEnvironment)
        {
            List<FileModel> filesOnSrv = FileActionHelpers.GetFilesInfo(_appEnvironment);
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
        }

        public static void ChangeStyleName(LocalizationRepContext _context)
        {
            var mainTableItemItem = (from m in _context.MainTable
                                .Include(m => m.Section)
                                .Include(m => m.StyleJsonKeyModel)
                                    .ThenInclude(s => s.LangKeyModels)
                                        .ThenInclude(l => l.LangValue)
                                     select m).ToList();

            foreach (var items in mainTableItemItem)
            {
                var styleJsonKeyModelItems = items.StyleJsonKeyModel.ToList();
                foreach (var styleJsonKeyModelItem in styleJsonKeyModelItems)
                {
                    var langKeyModelItems = styleJsonKeyModelItem.LangKeyModels.ToList();
                    switch (styleJsonKeyModelItem.StyleName)
                    {
                        case "dude":
                            styleJsonKeyModelItem.StyleName = "friendly";
                            break;
                        case "official":
                            styleJsonKeyModelItem.StyleName = "business";
                            break;
                        case "common":
                            styleJsonKeyModelItem.StyleName = "neutral";
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

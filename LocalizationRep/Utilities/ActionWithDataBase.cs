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
using System.Xml;
using LocalizationRep.Utilities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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
    }
}

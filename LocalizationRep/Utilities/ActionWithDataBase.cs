using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LocalizationRep.Controllers;
using LocalizationRep.Data;
using LocalizationRep.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace LocalizationRep.Utilities
{
    public class ActionWithDataBase
    {
        private readonly LocalizationRepContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly FileActionXML FAXML;
        private readonly FileActionJSON FAJSON;

        public ActionWithDataBase(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            FAXML = new FileActionXML(context, appEnvironment);
            FAJSON = new FileActionJSON(context, appEnvironment);
            _context = context;
            _appEnvironment = appEnvironment;
        }

        public void UpdateFileInDatabaseAction(string filename)
        {
            switch (Path.GetExtension(filename))
            {
                case ".csv":
                    CSVActionController.UpdateFromCsv(FileActionCSV.ReadUploadedCSV(filename), _context);
                    break;
                case ".json":
                    FAJSON.UpdateFromJsonToDbLocalizedText(filename);
                    break;
                case ".xml":
                    FAXML.ReadXMLToListNotMatch(filename);
                    break;
                default:
                    break;
            }
        }

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
                        file = new FileModel { Name = item.Name, Path = item.Path, TypeOfLoad = item.TypeOfLoad };
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

        public static void EraseAllSectionFromDBAction(LocalizationRepContext _context, string whatIMustRemove)
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
                    _context.LangKeyModel.RemoveRange(_context.LangKeyModel);
                    _context.LangValue.RemoveRange(_context.LangValue);
                    _context.StyleJsonKeyModel.RemoveRange(_context.StyleJsonKeyModel);
                    _context.MainTable.RemoveRange(_context.MainTable);
                    break;
            }
            _context.SaveChanges();
        }

        public static void AddUniqueSection(LocalizationRepContext _context, List<FileModel> FilesName)
        {
            string IOS = "ios";
            string tempFileName = "";
            var sections = _context.Section.ToList();
            List<string> sectionsToAdd = new List<string>();

            foreach (var sectionsItem in sections)
            {
                sectionsToAdd.Add(sectionsItem.Title);
            }



            foreach (var fileСontentsItem in FilesName)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(fileСontentsItem.Path);
                string directoryInfoParentName = directoryInfo.Parent.Name;
                if (fileСontentsItem.TypeOfLoad.Equals(FileActionHelpers.TypeOfLoad.UPLOAD.ToString()) && directoryInfoParentName.ToLower() == IOS.ToLower())
                {
                    tempFileName = Path.GetFileNameWithoutExtension(fileСontentsItem.Name);

                    if (!sectionsToAdd.Contains(tempFileName))
                    {
                        string shortNameUn = GetUniqueShortName(tempFileName.ToUpper());
                        int randomNumber = 0;
                        foreach (Sections section in _context.Section)
                        {

                            while (section.ShortName.Equals(shortNameUn))
                            {
                                shortNameUn = shortNameUn.Replace(shortNameUn.Substring(3, 1), randomNumber.ToString());
                                randomNumber++;
                            }
                        }
                        _context.Section.AddRange(
                                   new Sections
                                   {
                                       Title = tempFileName,
                                       LastIndexOfCommonID = "0000",
                                       ShortName = shortNameUn
                                   });
                        _context.SaveChanges();
                        sectionsToAdd.Add(tempFileName);
                    }
                }
            }
        }

        public static string CommonIDGetNext(LocalizationRepContext _context, string sectionKey)
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

        private static string GetUniqueShortName(string fileName)
        {
            string returnString = fileName;
            string pattern = @"[AEYUIO]";
            if (fileName.Length > 4)
            {
                returnString = fileName.Remove(1) + Regex.Replace(fileName.Substring(1), pattern, "");

                if (returnString.Length > 4)
                {
                    returnString = returnString.Remove(4);
                }
                else if (returnString.Length <= 4)
                {
                    while (returnString.Length < 4)
                    {
                        returnString += "0";
                    }
                }
            }
            else if (fileName.Length < 4)
            {
                while (returnString.Length < 4)
                {
                    returnString += "0";
                }
            }

            return returnString;
        }

        public static void DeleteAllAndroidItemsFromMainTable(LocalizationRepContext _context)
        {
            var mainTable = _context.MainTable
                                   .Include(m => m.Section)
                                   .Include(m => m.StyleJsonKeyModel)
                                        .ThenInclude(s => s.LangKeyModels)
                                            .ThenInclude(l => l.LangValue)
                                   .Where(m => m.Section.Title == "ANDROID");

            _context.MainTable.RemoveRange(mainTable);
            _context.SaveChangesAsync();
        }
    }
}

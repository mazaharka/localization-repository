using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using LocalizationRep.Data;
using LocalizationRep.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using static LocalizationRep.Utilities.FileActionHelpers;

namespace LocalizationRep.Utilities
{
    public class FileActionXML
    {
        private readonly LocalizationRepContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
        Dictionary<string, XMLKeyModel> noMatchedItems = new Dictionary<string, XMLKeyModel>();

        public FileActionXML(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
        }

        public void ReadFileXMLAction()
        {

            List<XMLKeyModel> listNotMatchedItems = new List<XMLKeyModel>();
            List<XMLKeyModel> listNotMatchedItemsTemp = new List<XMLKeyModel>();
            var Files = FileActionHelpers.GetFilesInfo(_appEnvironment).Where(f => f.TypeOfLoad == TypeOfLoad.UPLOAD.ToString());

            foreach (var path in Files)
            {
                if (Path.GetExtension(path.Path) == ".xml")
                {
                    ReadAndCompareXMLFromFile(path.Path, noMatchedItems);
                }
            }

            var mainTableItem = from m in _context.MainTable
                                            .Include(m => m.Section)
                                            .Include(m => m.StyleJsonKeyModel)
                                                .ThenInclude(s => s.LangKeyModels)
                                                    .ThenInclude(l => l.LangValue)
                                                    .Where(m => m.AndroidID == null)
                                select m;

            foreach (var notMatchItem in noMatchedItems.Values)
            {
                bool flag = false;
                foreach (var item in mainTableItem)
                {
                    if (notMatchItem.AttributeValue == item.AndroidID)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    listNotMatchedItemsTemp.Add(notMatchItem);
                }
            }
            AddNotMatchedItemsToTable(listNotMatchedItemsTemp);
        }

        private void AddNotMatchedItemsToTable(List<XMLKeyModel> noMatchedItems)
        {
            foreach (var item in noMatchedItems)
            {
                NotMatchedItem notMatchedItem = new NotMatchedItem
                {
                    AndroidID = item.AttributeValue,
                    NodeInnerText = item.NodeInnerText,
                    CommentValue = item.CommentValue,
                    StringNumber = item.StringNumber
                };
                _context.NotMatchedItem.Add(notMatchedItem);
                _context.SaveChanges();
            }
        }

        private void ReadAndCompareXMLFromFile(string path, Dictionary<string, XMLKeyModel> noMatchedItems)
        {
            string comment = "";

            foreach (var items in ReadXMLFromFileToList(path))
            {
                bool isMatched = false;
                if (items.IsComment)
                {
                    comment = items.NodeInnerText;
                }
                if (!items.IsComment)
                {
                    var mainTableItem = from m in _context.MainTable
                                            .Include(m => m.Section)
                                            .Include(m => m.StyleJsonKeyModel)
                                                .ThenInclude(s => s.LangKeyModels)
                                                    .ThenInclude(l => l.LangValue)
                                            .Where(m => m.AndroidID == null)
                                        select m;

                    foreach (var iosRuString in mainTableItem)
                    {

                        if (IsMatchComplete(iosRuString, items, 70))
                        {
                            iosRuString.AndroidID = items.AttributeValue;
                            iosRuString.AndoridStringNumber = items.StringNumber;
                            isMatched = true;
                            break;
                        }
                    }

                    if (!isMatched && !noMatchedItems.Keys.Contains(items.AttributeValue))
                    {
                        noMatchedItems.Add(items.AttributeValue, items);
                    }
                }
            }
            Console.WriteLine("END compare" + Path.GetDirectoryName(path));

            _context.SaveChanges();
        }

        private bool IsMatchComplete(MainTable iosRuString, XMLKeyModel items, double matchEdge)
        {

            List<bool> resulting = new List<bool>();
            bool commonResult = false;

            IQueryable<string> stylesUnique = from s in _context.StyleJsonKeyModel
                                              orderby s.StyleName
                                              select s.StyleName;

            IQueryable<string> langNameUnique = from s in _context.LangKeyModel
                                                orderby s.LangName
                                                select s.LangName;


            stylesUnique = stylesUnique.AsQueryable().Distinct();
            langNameUnique = langNameUnique.AsQueryable().Distinct();

            foreach (var stylesUniqueItem in stylesUnique)
            {
                foreach (var langNameUniqueItem in langNameUnique)
                {
                    resulting.Add(CompareStrings(iosRuString.StyleJsonKeyModel.Where(s => s.StyleName == stylesUniqueItem).First().LangKeyModels.Where(c => c.LangName == langNameUniqueItem).First().LangValue.Single, items.NodeInnerText) > matchEdge);
                }
            }

            foreach (var item in resulting)
            {
                if (item)
                {
                    commonResult = true;
                }
            }

            return !iosRuString.IOsOnly && iosRuString.AndroidID == null && commonResult;
        }


        public static double CompareStrings(string ios, string android)
        {
            double matchCount = 0;
            double matchPercent = 0;
            double maxLength = Math.Max(ios.Length, android.Length);
            int minLength = Math.Min(ios.Length, android.Length);
            try
            {
                for (int i = 0; i < minLength; i++)
                {
                    if (ios[i] == android[i])
                    {
                        matchCount++;
                    }
                }
                if (matchCount != 0)
                {
                    matchPercent = matchCount / maxLength * 100;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return matchPercent;
        }

        private static List<XMLKeyModel> ReadXMLFromFileToList(string path)
        {
            string XMLComment = "";
            int StringNumberCounter = 0;
            List<XMLKeyModel> xmlKeyModels = new List<XMLKeyModel>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);

            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode xnode in xRoot)
            {
                if (xnode.NodeType == XmlNodeType.Comment)
                {
                    XMLComment = xnode.OuterXml;
                }
                else if (xnode.NodeType != XmlNodeType.Comment)
                {
                    if (xnode.Attributes.Count > 0)
                    {
                        XmlNode attr = xnode.Attributes.GetNamedItem("name");
                        if (attr != null)
                        {
                            xmlKeyModels.Add(new XMLKeyModel { StringNumber = StringNumberCounter, AttributeValue = attr.Value, NodeInnerText = xnode.InnerText, CommentValue = XMLComment });
                        }
                    }
                }
                StringNumberCounter++;
            }

            return xmlKeyModels;
        }

        public void DeleteDublicate()
        {
            List<NotMatchedItem> notMatchedItems = new List<NotMatchedItem>();

            var notMatchedItemsTable = _context.NotMatchedItem.ToList();

            var mainTableItem = _context.MainTable
                                           .Include(m => m.Section)
                                           .Include(m => m.StyleJsonKeyModel)
                                               .ThenInclude(s => s.LangKeyModels)
                                                   .ThenInclude(l => l.LangValue).Where(m => m.AndroidID != null).ToList();


            foreach (var notMatchedItem in notMatchedItemsTable)
            {
                bool flag = false;
                foreach (var item in mainTableItem)
                {
                    if (item.AndroidID.Equals(notMatchedItem.AndroidID))
                    {
                        _context.NotMatchedItem.Remove(notMatchedItem);
                        flag = true;
                    }
                }
                if (!flag)
                {
                    notMatchedItems.Add(notMatchedItem);
                }
            }
            _context.SaveChanges();
            
        }
    }
}

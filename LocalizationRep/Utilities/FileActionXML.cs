using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly char[] TrimXMLCommentChars = new Char[] { '<', '-', '>', '!', ' ' };
        private readonly IQueryable<string> stylesUnique;
        private readonly IQueryable<string> langNameUnique;
        public FileActionXML(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
            stylesUnique = _context.StyleJsonKeyModel.OrderBy(s => s.StyleName).Select(s => s.StyleName).AsQueryable().Distinct();
            langNameUnique = _context.LangKeyModel.OrderBy(s => s.LangName).Select(s => s.LangName).AsQueryable().Distinct();
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

            //var mainTableItem = from m in _context.MainTable
            //                                .Include(m => m.Section)
            //                                .Include(m => m.StyleJsonKeyModel)
            //                                    .ThenInclude(s => s.LangKeyModels)
            //                                        .ThenInclude(l => l.LangValue)
            //                                        .Where(m => m.AndroidID == null)
            //                    select m;

            foreach (var notMatchItem in noMatchedItems.Values)
            {
                bool flag = false;
                foreach (var item in GetAllValuesMainTable())
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
            var sectionAndroid = new Sections();

            if (_context.Section.Where(s => s.Title == "ANDROID") == null)
            {
                sectionAndroid = new Sections
                {
                    Title = "ANDROID",
                    LastIndexOfCommonID = "0000",
                    ShortName = "ANDR"
                };
            }
            
            _context.Section.Add(sectionAndroid);
            _context.SaveChanges();

            foreach (var item in noMatchedItems)
            {
                NotMatchedItem notMatchedItem = new NotMatchedItem
                {
                    AndroidID = item.AttributeValue,
                    NodeInnerText = item.NodeInnerText,
                    CommentValue = item.CommentValue.Trim(TrimXMLCommentChars),
                    StringNumber = item.StringNumber,
                    Section = sectionAndroid
                };
                _context.NotMatchedItem.Add(notMatchedItem);
                _context.SaveChanges();
            }
        }

        private void ReadAndCompareXMLFromFile(string path, Dictionary<string, XMLKeyModel> noMatchedItems)
        {
            string comment = "";
            string pattern = @"\%.\$?";

            IQueryable<string> stylesUnique = _context.StyleJsonKeyModel.OrderBy(s => s.StyleName).Select(s => s.StyleName);
            IQueryable<string> langNameUnique = _context.LangKeyModel.OrderBy(s => s.LangName).Select(s => s.LangName);



            stylesUnique = stylesUnique.AsQueryable().Distinct();
            langNameUnique = langNameUnique.AsQueryable().Distinct();

            foreach (var items in ReadXMLFromFileToList(path))
            {
                bool isMatched = false;
                if (items.IsComment)
                {
                    comment = items.NodeInnerText;
                }
                if (!items.IsComment)
                {



                    //var mainTableItem = from m in _context.MainTable
                    //                        .Include(m => m.Section)
                    //                        .Include(m => m.StyleJsonKeyModel)
                    //                            .ThenInclude(s => s.LangKeyModels)
                    //                                .ThenInclude(l => l.LangValue)
                    //                        .Where(m => m.AndroidID == null)
                    //                    select m;

                    foreach (var iosRuString in GetAllValuesMainTable())
                    {

                        if (IsMatchComplete(iosRuString, items, 70) && Regex.Matches(items.NodeInnerText, pattern).Count() == 0) // &&  !IsContainSpecialSymbol(items.NodeInnerText, pattern))
                        {
                            iosRuString.AndroidID = items.AttributeValue;
                            iosRuString.AndoridStringNumber = items.StringNumber;
                            iosRuString.AndroidXMLComment = items.CommentValue.Trim(TrimXMLCommentChars);
                            _context.MainTable.Update(iosRuString);
                            isMatched = true;
                            break;
                        }
                        if (Regex.Matches(items.NodeInnerText, pattern).Count() != 0)
                        {

                        }
                    }
                    //_context.SaveChanges();
                    //var s = Regex.Matches(items.NodeInnerText, pattern);
                    //var count = Regex.Matches(items.NodeInnerText, pattern).Count();

                    if (!isMatched && !noMatchedItems.Keys.Contains(items.AttributeValue))
                    {
                        noMatchedItems.Add(items.AttributeValue, items);
                    }
                    //if (Regex.Matches(items.NodeInnerText, pattern).Count() != 0)
                    //{
                    //    noMatchedItems.Add(items.AttributeValue, items);
                    //}
                }
            }
            Console.WriteLine("END compare" + Path.GetDirectoryName(path));

            _context.SaveChanges();
        }

        private bool IsContainSpecialSymbol(string input, string pattern)
        {
            var s = Regex.Matches(input, pattern);
            foreach (Match m in Regex.Matches(input, pattern))
            {
                Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);
            }
            return false;
        }
        private bool IsMatchComplete(MainTable iosRuString, XMLKeyModel items, double matchEdge)
        {

            List<bool> resulting = new List<bool>();
            bool commonResult = false;

            //IQueryable<string> stylesUnique = from s in _context.StyleJsonKeyModel
            //                                  orderby s.StyleName
            //                                  select s.StyleName;
            //IQueryable<string> langNameUnique = from s in _context.LangKeyModel
            //                                    orderby s.LangName
            //                                    select s.LangName;

            //IQueryable<string> stylesUnique = _context.StyleJsonKeyModel.OrderBy(s => s.StyleName).Select(s => s.StyleName);
            //IQueryable<string> langNameUnique = _context.LangKeyModel.OrderBy(s => s.LangName).Select(s => s.LangName);



            //stylesUnique = stylesUnique.AsQueryable().Distinct();
            //langNameUnique = langNameUnique.AsQueryable().Distinct();

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

        private List<XMLKeyModel> ReadXMLFromFileToList(string path)
        {
            string XMLComment = "";
            string Comment = "";
            int StringNumberCounter = 0;
            List<XMLKeyModel> xmlKeyModels = new List<XMLKeyModel>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);

            XmlElement xRoot = xDoc.DocumentElement;
            var commentAndroidXMLModelEntity = _context.CommentAndroidXMLModel.ToList();
            List<CommentAndroidXMLModel> commentAndroidXMLModels = new List<CommentAndroidXMLModel>();
            foreach (XmlNode xnode in xRoot)
            {
                if (xnode.NodeType == XmlNodeType.Comment)
                {
                    XMLComment = xnode.OuterXml;
                    Comment = XMLComment.Trim(TrimXMLCommentChars);

                    commentAndroidXMLModels.Add(new CommentAndroidXMLModel
                    {
                        CommentValue = XMLComment
                    });
                }
                else if (xnode.NodeType != XmlNodeType.Comment)
                {
                    if (xnode.Attributes.Count > 0)
                    {
                        XmlNode attr = xnode.Attributes.GetNamedItem("name");
                        if (attr != null)
                        {
                            xmlKeyModels.Add(new XMLKeyModel
                            {
                                StringNumber = StringNumberCounter,
                                AttributeValue = attr.Value,
                                NodeInnerText = xnode.InnerText,
                                CommentValue = XMLComment
                            });
                        }
                    }
                }
                StringNumberCounter++;
            }

            try
            {
                foreach (var item in commentAndroidXMLModels)
                {
                    if (!_context.CommentAndroidXMLModel.Any())
                    {
                        _context.CommentAndroidXMLModel.Add(item);
                    }
                    else
                    {
                        var t = _context.CommentAndroidXMLModel.Where(x => x.CommentValue == item.CommentValue);
                        if (t.Count() == 0)
                        {
                            _context.CommentAndroidXMLModel.Add(item);
                        }
                    }
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return xmlKeyModels;
        }

        public void RemoveAndoridCommentInMainTable()
        {
            //List<MainTable> mainTableItems = GetAllValuesMainTable();

            foreach (var mainTableItem in GetAllValuesMainTableAndroidIDNotNull())
            {
                mainTableItem.AndroidXMLComment = null;
                _context.MainTable.Update(mainTableItem);
            }
            _context.SaveChanges();
        }

        public void RemoveAllAndoridIdMatchesInMainTable()
        {
            //List<MainTable> mainTableItems = GetAllValuesMainTable();

            foreach (var mainTableItem in GetAllValuesMainTableAndroidIDNotNull())
            {
                mainTableItem.AndroidID = null;
                _context.MainTable.Update(mainTableItem);
            }
            _context.SaveChanges();
        }

        public void DeleteDublicate()
        {
            List<NotMatchedItem> notMatchedItems = new List<NotMatchedItem>();

            var notMatchedItemsTable = _context.NotMatchedItem.ToList();

            //var mainTableItem = _context.MainTable
            //                               .Include(m => m.Section)
            //                               .Include(m => m.StyleJsonKeyModel)
            //                                   .ThenInclude(s => s.LangKeyModels)
            //                                       .ThenInclude(l => l.LangValue).Where(m => m.AndroidID != null).ToList();


            foreach (var notMatchedItem in notMatchedItemsTable)
            {
                bool flag = false;
                foreach (var item in GetAllValuesMainTableAndroidIDNotNull())
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

        private List<MainTable> GetAllValuesMainTable()
        {
            return _context.MainTable
                                     .Include(m => m.Section)
                                     .Include(m => m.StyleJsonKeyModel)
                                         .ThenInclude(s => s.LangKeyModels)
                                             .ThenInclude(l => l.LangValue)
                                        .Where(m => m.AndroidID == null).ToList();
        }



        private List<MainTable> GetAllValuesMainTableAndroidIDNotNull()
        {
            return _context.MainTable
                                     .Include(m => m.Section)
                                     .Include(m => m.StyleJsonKeyModel)
                                         .ThenInclude(s => s.LangKeyModels)
                                             .ThenInclude(l => l.LangValue)
                                     .Where(m => m.AndroidID != null).ToList();
        }
    }
}
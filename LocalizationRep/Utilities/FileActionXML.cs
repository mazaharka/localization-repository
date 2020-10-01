using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
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
        private const string UK_NEUTRAL = "uk";
        private const string RU_NEUTRAL = "ru";
        private const string EN_NEUTRAL = "en";

        private const string UK_BUSINESS = "ru-rUA";
        private const string RU_BUSINESS = "ru-rBY";
        private const string EN_BUSINESS = "ru-rKG";

        private const string UK_FRIENDLY = "en-rAG";
        private const string RU_FRIENDLY = "en-rAU";
        private const string EN_FRIENDLY = "en-rAS";

        private readonly LocalizationRepContext _context;
        private readonly IWebHostEnvironment _appEnvironment;

        private readonly char[] TrimXMLCommentChars = new char[] { '<', '-', '>', '!', ' ' };
        private readonly IQueryable<string> stylesUnique;
        private readonly IQueryable<string> langNameUnique;


        public FileActionXML(LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
            //stylesUnique = _context.StyleJsonKeyModel.OrderBy(s => s.StyleName).Select(s => s.StyleName).AsQueryable().Distinct();
            //langNameUnique = _context.LangKeyModel.OrderBy(s => s.LangName).Select(s => s.LangName).AsQueryable().Distinct();
            stylesUnique = GetStyleTextUnique();
            langNameUnique = GetLangNameUnique();
        }

        //прочитать все XML файлы
        public void ReadFileXMLAction()
        {
            List<XMLKeyModel> listNotMatchedItems = new List<XMLKeyModel>();
            List<XMLKeyModel> listNotMatchedItemsTemp = new List<XMLKeyModel>();
            var Files = GetFilesInfo(_appEnvironment).Where(f => f.TypeOfLoad == TypeOfLoad.UPLOAD.ToString());

            foreach (var path in Files)
            {
                if (Path.GetExtension(path.Path) == ".xml")
                {
                    ReadXMLToListNotMatch(path.Path);
                }
            }
        }

        public void ReadXMLToListNotMatch(string path)
        {
            AndroidTable notMatchedItem = new AndroidTable();

            List<XMLKeyModel> xmlListItems = ReadXMLFromFileToList(path);
            List<AndroidTable> listToAdd = new List<AndroidTable>();
            AndroidTable androidItem = new AndroidTable();

            foreach (var item in xmlListItems)
            {
                if (!item.IsComment)
                {
                    if (!IsAndroidTableHaveSameID(item.AttributeValue))
                    {
                        androidItem = new AndroidTable
                        {
                            AndroidID = item.AttributeValue,
                            CommentValue = item.CommentValue.Trim(TrimXMLCommentChars),
                            StringNumber = item.StringNumber,
                            SectionID = AddAndroidSection().ID,
                            Section = AddAndroidSection()
                        };
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);


                        var sc = directoryInfo.Parent.Name;

                        switch (sc)
                        {
                            case "values-" + RU_NEUTRAL:
                                androidItem.RU_NEUTRAL = item.NodeInnerText;
                                break;
                            case "values-" + EN_NEUTRAL:
                                androidItem.EN_NEUTRAL = item.NodeInnerText;
                                break;
                            case "values":
                                androidItem.UK_NEUTRAL = item.NodeInnerText;
                                break;
                            case "values-" + RU_BUSINESS:
                                androidItem.RU_BUSINESS = item.NodeInnerText;
                                break;
                            case "values-" + EN_BUSINESS:
                                androidItem.EN_BUSINESS = item.NodeInnerText;
                                break;
                            case "values-" + UK_BUSINESS:
                                androidItem.UK_BUSINESS = item.NodeInnerText;
                                break;
                            case "values-" + RU_FRIENDLY:
                                androidItem.RU_FRIENDLY = item.NodeInnerText;
                                break;
                            case "values-" + EN_FRIENDLY:
                                androidItem.EN_FRIENDLY = item.NodeInnerText;
                                break;
                            case "values-" + UK_FRIENDLY:
                                androidItem.UK_FRIENDLY = item.NodeInnerText;
                                break;
                            default:
                                break;
                        }

                        _context.AndroidTable.Add(androidItem);
                    }
                    //else
                    //{
                    //    var androidItemExist = _context.AndroidTable.FirstOrDefault(b => b.AndroidID == item.AttributeValue);
                    //    DirectoryInfo directoryInfo = new DirectoryInfo(path);

                    //    switch (directoryInfo.Parent.Name)
                    //    {
                    //        case "values-" + RU_NEUTRAL:
                    //            androidItemExist.RU_NEUTRAL = item.NodeInnerText;
                    //            break;
                    //        case "values-" + EN_NEUTRAL:
                    //            androidItemExist.EN_NEUTRAL = item.NodeInnerText;
                    //            break;
                    //        case "values":
                    //            androidItemExist.UK_NEUTRAL = item.NodeInnerText;
                    //            break;
                    //        case "values-" + RU_BUSINESS:
                    //            androidItemExist.RU_BUSINESS = item.NodeInnerText;
                    //            break;
                    //        case "values-" + EN_BUSINESS:
                    //            androidItemExist.EN_BUSINESS = item.NodeInnerText;
                    //            break;
                    //        case "values-" + UK_BUSINESS:
                    //            androidItemExist.UK_BUSINESS = item.NodeInnerText;
                    //            break;
                    //        case "values-" + RU_FRIENDLY:
                    //            androidItemExist.RU_FRIENDLY = item.NodeInnerText;
                    //            break;
                    //        case "values-" + EN_FRIENDLY:
                    //            androidItemExist.EN_FRIENDLY = item.NodeInnerText;
                    //            break;
                    //        case "values-" + UK_FRIENDLY:
                    //            androidItemExist.UK_FRIENDLY = item.NodeInnerText;
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //    _context.AndroidTable.Update(androidItemExist);
                    //}
                }
            }

            _context.SaveChanges();
            Console.WriteLine("END compare" + Path.GetDirectoryName(path));
        }

        public void ReadAndCompareXMLBetweenTable()
        {
            string pattern = @"\%.\$?";

            IQueryable<string> stylesUnique = GetStyleTextUnique();
            IQueryable<string> langNameUnique = GetLangNameUnique();

            List<AndroidTable> androidItems = GetAllValuesAndroidWhereSectionIsAndroid();
            List<MainTable> allValuesMainTable = GetAllValuesMainTableAndroidIDIsNull();
            //List<MainTable> allValuesMainTableWithoutSymbols = GetAllValuesMainTableAndroidIDIsNull();

            List<AndroidTable> androidItemWNotMatched = new List<AndroidTable>();

            List<SearchModel> allMainTableItemsInList = GetAllAndroidNullMainTableItemsToList();
            List<SearchModel> allMainTableItemsInListWithoutSymbol = new List<SearchModel>();
            SearchModel mainTableItemNotNeeded = new SearchModel();

            foreach (var item in allMainTableItemsInList)
            {
                if (!IsMainTableHaveSpecialSymbols(pattern, item))
                {
                    allMainTableItemsInListWithoutSymbol.Add(item);
                }
            }

            Dictionary<string, string> AndroidIDPairsBothTable = new Dictionary<string, string>();
            foreach (var androidItem in androidItems)
            {
                if (!IsAndroidItemHaveSpecialSymbols(pattern, androidItem))
                {
                    foreach (var mainTableItem in allMainTableItemsInListWithoutSymbol)
                    {

                        if (IsMatchComplete(mainTableItem, androidItem, 70))
                        {
                            if (mainTableItem.AndroidID == null)
                            {
                                MainTable mainTable = _context.MainTable.Where(m => m.CommonID == mainTableItem.CommonID).FirstOrDefault();
                                mainTable.AndroidID = androidItem.AndroidID;
                                mainTable.AndoridStringNumber = androidItem.StringNumber;
                                mainTable.AndroidXMLComment = androidItem.CommentValue.Trim(TrimXMLCommentChars);
                                _context.MainTable.Update(mainTable);

                                androidItem.CommonID = mainTable.CommonID;
                                androidItem.Section = mainTable.Section;
                                androidItem.SectionID = mainTable.SectionID;
                                _context.AndroidTable.Update(androidItem);

                                break;
                            }
                            else if (mainTableItem.AndroidID != null)
                            {
                                MainTable mainTable = _context.MainTable.Where(m => m.CommonID == mainTableItem.CommonID).FirstOrDefault();

                                androidItem.CommonID = mainTable.CommonID;
                                androidItem.Section = mainTable.Section;
                                androidItem.SectionID = mainTable.SectionID;
                                _context.AndroidTable.Update(androidItem);
                            }
                            if (mainTableItem.AndroidID != androidItem.AndroidID)
                            {
                                Console.WriteLine(androidItem.AndroidID + " already exists item in mainTable " + mainTableItem.AndroidID);
                                AndroidIDPairsBothTable.Add(mainTableItem.AndroidID, androidItem.AndroidID);
                            }
                            allMainTableItemsInList.Remove(mainTableItem);
                        }
                    }
                }
                else
                {
                    androidItemWNotMatched.Add(androidItem);
                }
            }
            Console.WriteLine("END compare");

            XmlToDb(androidItemWNotMatched);

            _context.SaveChanges();
        }

        private static bool IsAndroidItemHaveSpecialSymbols(string pattern, AndroidTable androidItem)
        {
            List<string> vs = new List<string> {
                androidItem.RU_NEUTRAL,
                androidItem.EN_NEUTRAL,
                androidItem.UK_NEUTRAL,
                androidItem.RU_BUSINESS,
                androidItem.EN_BUSINESS,
                androidItem.UK_BUSINESS,
                androidItem.RU_FRIENDLY,
                androidItem.EN_FRIENDLY,
                androidItem.UK_FRIENDLY
            };
            foreach (var item in vs)
            {
                if (item != null)
                {
                    if (Regex.Matches(item, pattern).Count() != 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsMainTableHaveSpecialSymbols(string pattern, SearchModel mainTableItem)
        {

            List<string> vs = new List<string> {
                mainTableItem.TextRU_NEUTRAL,
                mainTableItem.TextEN_NEUTRAL,
                mainTableItem.TextUK_NEUTRAL,
                mainTableItem.TextRU_BUSINESS,
                mainTableItem.TextEN_BUSINESS,
                mainTableItem.TextUK_BUSINESS,
                mainTableItem.TextRU_FRIENDLY,
                mainTableItem.TextEN_FRIENDLY,
                mainTableItem.TextUK_FRIENDLY
            };

            foreach (var item in vs)
            {
                if (item != null)
                {
                    if (Regex.Matches(item, pattern).Count() != 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void CreateEntitesFromNullCommandIdInAndroidTables()
        {
            XmlToDb(GetNullAndroidIDValuesInAndroidTables());
        }

        private void XmlToDb(List<AndroidTable> androidItemWNotMatched)
        {
            MainTable mainTable = new MainTable();
            StyleJsonKeyModel styleJsonKeyModel = new StyleJsonKeyModel();
            LangKeyModel langKeyModel = new LangKeyModel();
            LangValue langValue = new LangValue();
            List<LangKeyModel> langKeyModels = new List<LangKeyModel>();

            List<StyleJsonKeyModel> styleJsonKeyModels = new List<StyleJsonKeyModel>();

            foreach (var androidItem in androidItemWNotMatched)
            {
                var s = _context.MainTable.FirstOrDefault(s => s.AndroidID == androidItem.AndroidID);

                if (_context.MainTable.FirstOrDefault(s => s.AndroidID == androidItem.AndroidID) == null)
                {
                    try
                    {
                        mainTable = new MainTable
                        {
                            Section = AddAndroidSection(),
                            SectionID = AddAndroidSection().ID,
                            AndroidID = androidItem.AndroidID,
                            AndoridStringNumber = androidItem.StringNumber,
                            AndroidXMLComment = androidItem.CommentValue
                        };
                        _context.MainTable.Add(mainTable);
                        _context.SaveChanges();
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine("Нет секций в базе. Обнови секции, по-братски!\n" + ex);
                    }

                    foreach (var styleName in GetStyleTextUnique())
                    {
                        styleJsonKeyModel = CreateStyleJsonKeyModel(androidItem, styleName, mainTable);

                        styleJsonKeyModels.Add(styleJsonKeyModel);

                        _context.StyleJsonKeyModel.Update(styleJsonKeyModel);
                        _context.SaveChanges();
                    }

                    mainTable.StyleJsonKeyModel = styleJsonKeyModels.GetRange(styleJsonKeyModels.Count - 3, 3);
                    mainTable.CommonID = ActionWithDataBase.CommonIDGetNext(_context, androidItem.Section.Title);
                    _context.MainTable.Update(mainTable);

                    androidItem.CommonID = mainTable.CommonID;
                    _context.AndroidTable.Update(androidItem);
                }
            }

            _context.SaveChanges();
        }
        public void UpdateAndroidItemSection(string commonID, string sectionName)
        {
            AndroidTable androidItem = GetAndroidTableItemByCommonId(commonID);
            MainTable mainTableAndroidItem = GetValueFromMainTabelByCommonID(commonID);

            if (androidItem.Section.Title != sectionName)
            {
                Sections newSection = _context.Section.FirstOrDefault(s => s.Title == sectionName);
                string newCommonID = ActionWithDataBase.CommonIDGetNext(_context, sectionName);
                androidItem.CommonID = newCommonID;
                androidItem.Section = newSection;
                androidItem.SectionID = newSection.ID;

                mainTableAndroidItem.CommonID = newCommonID;
                mainTableAndroidItem.Section = newSection;
                mainTableAndroidItem.SectionID = newSection.ID;

                _context.AndroidTable.Update(androidItem);
                _context.MainTable.Update(mainTableAndroidItem);

            }
            _context.SaveChanges();
        }


        public void AddAndroidTableItemToMainTable(string commonId, Sections section)
        {
            AndroidTable androidTableItem = GetAndroidTableItemByCommonId(commonId);
            MainTable mainTable = new MainTable();
            StyleJsonKeyModel styleJsonKeyModel = new StyleJsonKeyModel();

            string CommonID = ActionWithDataBase.CommonIDGetNext(_context, section.Title);


            if (_context.MainTable.FirstOrDefault(s => s.AndroidID == androidTableItem.AndroidID) == null)
            {
                try
                {
                    mainTable = new MainTable
                    {
                        Section = section,
                        SectionID = section.ID,
                        AndroidID = androidTableItem.AndroidID
                    };
                    _context.MainTable.Add(mainTable);
                    _context.SaveChanges();
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine("Нет секций в базе. Обнови секции, по-братски!\n" + ex);
                }

                foreach (var stylesUniqueItem in GetStyleTextUnique())
                {

                    styleJsonKeyModel = CreateStyleJsonKeyModel(androidTableItem, stylesUniqueItem, mainTable);

                    //_context.LangKeyModel.AddRange(langKeyModels.GetRange(langKeyModels.Count - 3, 3));
                    //styleJsonKeyModel.LangKeyModels = langKeyModels;
                    _context.StyleJsonKeyModel.Update(styleJsonKeyModel);
                    _context.SaveChanges();
                }

                //mainTable.StyleJsonKeyModel = styleJsonKeyModels;
                //mainTable.CommonID = ActionWithDataBase.CommonIDGetNext(_context, androidItem.Section.Title);
                _context.MainTable.Update(mainTable);
            }
        }

        private StyleJsonKeyModel CreateStyleJsonKeyModel(AndroidTable androidItem, string styleName, MainTable mainTable)
        {
            StyleJsonKeyModel styleJsonKeyModel = new StyleJsonKeyModel();
            List<LangKeyModel> langKeyModels = new List<LangKeyModel>();
            //MainTable mainTable = new MainTable();

            //IQueryable<string> stylesUnique = _context.StyleJsonKeyModel.OrderBy(s => s.StyleName).Select(s => s.StyleName);
            //stylesUnique = stylesUnique.AsQueryable().Distinct();

            //IQueryable<string> langNameUnique = GetLangNameUnique();

            styleJsonKeyModel = new StyleJsonKeyModel
            {
                StyleName = styleName,
                MainTables = mainTable
            };

            _context.StyleJsonKeyModel.Add(styleJsonKeyModel);
            _context.SaveChanges();

            switch (styleName)
            {
                case "neutral":

                    foreach (var langNameUniqueitem in GetLangNameUnique())
                    {
                        string singleText = "";
                        switch (langNameUniqueitem)
                        {
                            case "ru":
                                singleText = androidItem.RU_NEUTRAL;
                                break;
                            case "en":
                                singleText = androidItem.EN_NEUTRAL;
                                break;
                            case "uk":
                                singleText = androidItem.UK_NEUTRAL;
                                break;
                            case "":
                                singleText = androidItem.UK_NEUTRAL;
                                break;
                            default:
                                break;
                        }

                        langKeyModels.Add(new LangKeyModel
                        {
                            LangName = langNameUniqueitem,
                            LangValue = new LangValue
                            {
                                Single = singleText
                            },
                            StyleJsonKeyModel = styleJsonKeyModel
                        });
                    }

                    break;
                case "business":

                    foreach (var langNameUniqueitem in GetLangNameUnique())
                    {
                        string singleText = "";
                        switch (langNameUniqueitem)
                        {
                            case "ru":
                                singleText = androidItem.RU_BUSINESS;
                                break;
                            case "en":
                                singleText = androidItem.EN_BUSINESS;
                                break;
                            case "uk":
                                singleText = androidItem.UK_BUSINESS;
                                break;
                            case "":
                                singleText = androidItem.UK_BUSINESS;
                                break;
                            default:
                                break;
                        }

                        langKeyModels.Add(new LangKeyModel
                        {
                            LangName = langNameUniqueitem,
                            LangValue = new LangValue
                            {
                                Single = singleText
                            },
                            StyleJsonKeyModel = styleJsonKeyModel
                        });
                    }
                    break;
                case "friendly":

                    foreach (var langNameUniqueitem in GetLangNameUnique())
                    {
                        string singleText = "";
                        switch (langNameUniqueitem)
                        {
                            case "ru":
                                singleText = androidItem.RU_FRIENDLY;
                                break;
                            case "en":
                                singleText = androidItem.EN_FRIENDLY;
                                break;
                            case "uk":
                                singleText = androidItem.UK_FRIENDLY;
                                break;
                            case "":
                                singleText = androidItem.UK_FRIENDLY;
                                break;
                            default:
                                break;
                        }
                        langKeyModels.Add(new LangKeyModel
                        {
                            LangName = langNameUniqueitem,
                            LangValue = new LangValue
                            {
                                Single = singleText
                            },
                            StyleJsonKeyModel = styleJsonKeyModel
                        });

                    }
                    break;
                default:
                    break;
            }
            _context.LangKeyModel.AddRange(langKeyModels);
            _context.SaveChanges();

            return styleJsonKeyModel;
        }

        private IQueryable<string> GetLangNameUnique()
        {
            IQueryable<string> langNameUnique = _context.LangKeyModel.OrderBy(s => s.LangName).Select(s => s.LangName);
            langNameUnique = langNameUnique.AsQueryable().Distinct();
            return langNameUnique;
        }

        private IQueryable<string> GetStyleTextUnique()
        {

            IQueryable<string> stylesUnique = _context.StyleJsonKeyModel.OrderBy(s => s.StyleName).Select(s => s.StyleName);
            stylesUnique = stylesUnique.AsQueryable().Distinct();
            return stylesUnique;
        }

        //сравнение
        public void CompareCommonIDBetweenMainTableAndAndroid()
        {
            var androidTabelItems = GetAllValuesAndroid();

            foreach (var androidTabelItem in androidTabelItems)
            {
                var sv = _context.MainTable.Include(m => m.Section)
                                           .Include(m => m.StyleJsonKeyModel)
                                               .ThenInclude(s => s.LangKeyModels)
                                                   .ThenInclude(l => l.LangValue)
                                                   .FirstOrDefault(a => a.AndroidID == androidTabelItem.AndroidID);

                if (sv != null && sv.Section.ShortName != "ANDR")
                {
                    foreach (var itemStyleJsonKeyModel in sv.StyleJsonKeyModel)
                    {
                        foreach (var itemLangKeyModels in itemStyleJsonKeyModel.LangKeyModels)
                        {

                            switch (itemStyleJsonKeyModel.StyleName)
                            {
                                case "neutral":
                                    switch (itemLangKeyModels.LangName)
                                    {
                                        case "ru":
                                            androidTabelItem.RU_NEUTRAL = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "en":
                                            androidTabelItem.EN_NEUTRAL = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "uk":
                                            androidTabelItem.UK_NEUTRAL = itemLangKeyModels.LangValue.Single;
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case "business":
                                    switch (itemLangKeyModels.LangName)
                                    {
                                        case "ru":
                                            androidTabelItem.RU_BUSINESS = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "en":
                                            androidTabelItem.EN_BUSINESS = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "uk":
                                            androidTabelItem.UK_BUSINESS = itemLangKeyModels.LangValue.Single;
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case "friendly":
                                    switch (itemLangKeyModels.LangName)
                                    {
                                        case "ru":
                                            androidTabelItem.RU_FRIENDLY = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "en":
                                            androidTabelItem.EN_FRIENDLY = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "uk":
                                            androidTabelItem.UK_FRIENDLY = itemLangKeyModels.LangValue.Single;
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    androidTabelItem.CommonID = _context.MainTable.FirstOrDefault(a => a.AndroidID == androidTabelItem.AndroidID).CommonID;
                    androidTabelItem.Section = _context.MainTable.FirstOrDefault(a => a.AndroidID == androidTabelItem.AndroidID).Section;
                    androidTabelItem.SectionID = _context.MainTable.FirstOrDefault(a => a.AndroidID == androidTabelItem.AndroidID).SectionID;
                    _context.AndroidTable.Update(androidTabelItem);
                }
            }

            _context.SaveChanges();

        }

        private bool IsMatchComplete(SearchModel iosItem, AndroidTable androidItem, double matchEdge)
        {
            List<string> textsInAllLocals = new List<string> {
                iosItem.TextRU_NEUTRAL,
                iosItem.TextEN_NEUTRAL,
                iosItem.TextUK_NEUTRAL,
                iosItem.TextRU_BUSINESS,
                iosItem.TextEN_BUSINESS,
                iosItem.TextUK_BUSINESS,
                iosItem.TextRU_FRIENDLY,
                iosItem.TextEN_FRIENDLY,
                iosItem.TextUK_FRIENDLY
            };

            if (!IsMainTableHaveSameAndroidID(androidItem.AndroidID))
            {
                //foreach (var itemAndroid in textsInAllLocalsAndroidItem)
                //{
                foreach (var itemMain in textsInAllLocals)
                {
                    if (itemMain != null && androidItem.RU_NEUTRAL != null)
                    {
                        if (CompareStrings(itemMain, androidItem.RU_NEUTRAL) > matchEdge)
                        {
                            return true;
                        }
                    }
                }
                //}
            }
            return false; // count <= 1 && count != 0;
        }

        public Dictionary<string, Dictionary<string, List<string>>> GetDictionaryLocalizationValueSplitToList()
        {
            Dictionary<string, Dictionary<string, List<string>>> DictionaryLocalizationValueSplitToList = new Dictionary<string, Dictionary<string, List<string>>>();
            List<string> RU_NEUTRAL = new List<string>();
            List<string> EN_NEUTRAL = new List<string>();
            List<string> UK_NEUTRAL = new List<string>();

            List<string> RU_BUSINESS = new List<string>();
            List<string> EN_BUSINESS = new List<string>();
            List<string> UK_BUSINESS = new List<string>();

            List<string> RU_FRIENDLY = new List<string>();
            List<string> EN_FRIENDLY = new List<string>();
            List<string> UK_FRIENDLY = new List<string>();

            foreach (var styleItem in GetStyleTextUnique())
            {
                foreach (var LangItem in GetLangNameUnique())
                {
                    foreach (var item in GetAllValuesMainTable())
                    {

                        
                    }
                }
            }

            return DictionaryLocalizationValueSplitToList;
        }

        private List<SearchModel> GetAllAndroidNullMainTableItemsToList()
        {
            List<SearchModel> searchModels = new List<SearchModel>();
            string ru_neutral = "", en_neutral = "", uk_neutral = "", ru_business = "", en_business = "", uk_business = "", ru_friendly = "", en_friendly = "", uk_friendly = "";

            var mainTableItems = GetAllValuesMainTableAndroidIDIsNull();

            foreach (var mainTableItem in mainTableItems)
            {
                if (mainTableItem.AndroidID == null)
                {
                    foreach (var itemStyleJsonKeyModel in mainTableItem.StyleJsonKeyModel)
                    {
                        foreach (var itemLangKeyModels in itemStyleJsonKeyModel.LangKeyModels)
                        {

                            switch (itemStyleJsonKeyModel.StyleName)
                            {
                                case "neutral":
                                    switch (itemLangKeyModels.LangName)
                                    {
                                        case "ru":
                                            ru_neutral = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "en":
                                            en_neutral = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "uk":
                                            uk_neutral = itemLangKeyModels.LangValue.Single;
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case "business":
                                    switch (itemLangKeyModels.LangName)
                                    {
                                        case "ru":
                                            ru_business = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "en":
                                            en_business = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "uk":
                                            uk_business = itemLangKeyModels.LangValue.Single;
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case "friendly":
                                    switch (itemLangKeyModels.LangName)
                                    {
                                        case "ru":
                                            ru_friendly = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "en":
                                            en_friendly = itemLangKeyModels.LangValue.Single;
                                            break;
                                        case "uk":
                                            uk_friendly = itemLangKeyModels.LangValue.Single;
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                searchModels.Add(new SearchModel
                {
                    CommonID = mainTableItem.CommonID,
                    AndroidID = mainTableItem.AndroidID,
                    IOsID = mainTableItem.IOsID,
                    TextRU_NEUTRAL = ru_neutral,
                    TextEN_NEUTRAL = en_neutral,
                    TextUK_NEUTRAL = uk_neutral,
                    TextRU_BUSINESS = ru_business,
                    TextEN_BUSINESS = en_business,
                    TextUK_BUSINESS = uk_business,
                    TextRU_FRIENDLY = ru_friendly,
                    TextEN_FRIENDLY = en_friendly,
                    TextUK_FRIENDLY = uk_friendly
                });
            }

            return searchModels;
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
                            }); ;
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

        public void DeleteDublicate()
        {
            List<AndroidTable> notMatchedItems = new List<AndroidTable>();

            var notMatchedItemsTable = _context.AndroidTable.ToList();

            foreach (var notMatchedItem in notMatchedItemsTable)
            {
                bool flag = false;
                foreach (var item in GetAllValuesMainTableAndroidIDNotNull())
                {
                    if (item.AndroidID.Equals(notMatchedItem.AndroidID))
                    {
                        _context.AndroidTable.Remove(notMatchedItem);
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

        public void CreateXMLFileFromDB()
        {
            List<AndroidTable> allValuesAndroid = GetAllValuesAndroid();

            List<XMLKeyModel> ForAndroidFromDB = new List<XMLKeyModel>();

            foreach (var androidTableItems in allValuesAndroid)
            {
                if (androidTableItems.CommonID != null)
                {
                    var androidTableItem = androidTableItems.RU_BUSINESS;


                    {
                        foreach (var itemStyleJsonKeyModel in GetStyleTextUnique())
                        {
                            foreach (var itemLangKeyModel in GetLangNameUnique())
                            {
                                //var m = _context.MainTable.Where(s => s.StyleJsonKeyModel.Where);
                                switch (itemStyleJsonKeyModel)
                                {
                                    case "neutral":
                                        switch (itemLangKeyModel)
                                        {
                                            case "ru": androidTableItem = androidTableItems.RU_NEUTRAL; break;
                                            case "en": androidTableItem = androidTableItems.EN_NEUTRAL; break;
                                            case "uk": androidTableItem = androidTableItems.UK_NEUTRAL; break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case "business":
                                        switch (itemLangKeyModel)
                                        {
                                            case "ru": androidTableItem = androidTableItems.RU_BUSINESS; break;
                                            case "en": androidTableItem = androidTableItems.EN_BUSINESS; break;
                                            case "uk": androidTableItem = androidTableItems.UK_BUSINESS; break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case "friendly":
                                        switch (itemLangKeyModel)
                                        {
                                            case "ru": androidTableItem = androidTableItems.RU_FRIENDLY; break;
                                            case "en": androidTableItem = androidTableItems.EN_FRIENDLY; break;
                                            case "uk": androidTableItem = androidTableItems.UK_FRIENDLY; break;
                                            default:
                                                break;
                                        }
                                        break;
                                    default:
                                        break;
                                }

                                ForAndroidFromDB.Add(
                                    new XMLKeyModel()
                                    {
                                        CommonId = androidTableItems.CommonID,
                                        StringNumber = androidTableItems.StringNumber,

                                        AttributeValue = androidTableItems.AndroidID,
                                        NodeInnerText = androidTableItem,

                                        LanguageName = itemLangKeyModel,
                                        StyleText = itemStyleJsonKeyModel,

                                        CommentValue = androidTableItems.CommentValue
                                    }
                                );
                            }
                        }
                    }
                }
            }

            SortedDictionary<int, XMLKeyModel> RU_NEUTRAL_ARRAY = new SortedDictionary<int, XMLKeyModel>();
            SortedDictionary<int, XMLKeyModel> UK_NEUTRAL_ARRAY = new SortedDictionary<int, XMLKeyModel>();
            SortedDictionary<int, XMLKeyModel> EN_NEUTRAL_ARRAY = new SortedDictionary<int, XMLKeyModel>();

            SortedDictionary<int, XMLKeyModel> RU_BUSINESS_ARRAY = new SortedDictionary<int, XMLKeyModel>();
            SortedDictionary<int, XMLKeyModel> UK_BUSINESS_ARRAY = new SortedDictionary<int, XMLKeyModel>();
            SortedDictionary<int, XMLKeyModel> EN_BUSINESS_ARRAY = new SortedDictionary<int, XMLKeyModel>();

            SortedDictionary<int, XMLKeyModel> RU_FRIENDLY_ARRAY = new SortedDictionary<int, XMLKeyModel>();
            SortedDictionary<int, XMLKeyModel> UK_FRIENDLY_ARRAY = new SortedDictionary<int, XMLKeyModel>();
            SortedDictionary<int, XMLKeyModel> EN_FRIENDLY_ARRAY = new SortedDictionary<int, XMLKeyModel>();

            XmlSerializer formatter = new XmlSerializer(typeof(XmlEntities));
            XmlSerializerNamespaces emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            foreach (var item in ForAndroidFromDB)
            {
                try
                {
                    switch (item.StyleText)
                    {
                        case "neutral":
                            switch (item.LanguageName)
                            {
                                case "ru":
                                    RU_NEUTRAL_ARRAY.Add(item.StringNumber, item);
                                    break;
                                case "uk":
                                    UK_NEUTRAL_ARRAY.Add(item.StringNumber, item);
                                    break;
                                case "en":
                                    EN_NEUTRAL_ARRAY.Add(item.StringNumber, item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "friendly":
                            switch (item.LanguageName)
                            {
                                case "ru":
                                    RU_FRIENDLY_ARRAY.Add(item.StringNumber, item);
                                    break;
                                case "uk":
                                    UK_FRIENDLY_ARRAY.Add(item.StringNumber, item);
                                    break;
                                case "en":
                                    EN_FRIENDLY_ARRAY.Add(item.StringNumber, item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "business":
                            switch (item.LanguageName)
                            {
                                case "ru":
                                    RU_BUSINESS_ARRAY.Add(item.StringNumber, item);
                                    break;
                                case "uk":
                                    UK_BUSINESS_ARRAY.Add(item.StringNumber, item);
                                    break;
                                case "en":
                                    EN_BUSINESS_ARRAY.Add(item.StringNumber, item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(item.AttributeValue + " key already exists " + item.StringNumber + "\n" + ex);
                }

            }

            using FileStream fsRU_NEUTRAL = new FileStream("wwwroot/Files/download/Android/values-" + RU_NEUTRAL + "/strings.xml", FileMode.OpenOrCreate);
            using FileStream fsEN_NEUTRAL = new FileStream("wwwroot/Files/download/Android/values-" + EN_NEUTRAL + "/strings.xml", FileMode.OpenOrCreate);
            using FileStream fsUK_NEUTRAL = new FileStream("wwwroot/Files/download/Android/values/strings.xml", FileMode.OpenOrCreate);

            using FileStream fsRU_BUSINESS = new FileStream("wwwroot/Files/download/Android/values-" + RU_BUSINESS + "/strings.xml", FileMode.OpenOrCreate);
            using FileStream fsEN_BUSINESS = new FileStream("wwwroot/Files/download/Android/values-" + EN_BUSINESS + "/strings.xml", FileMode.OpenOrCreate);
            using FileStream fsUK_BUSINESS = new FileStream("wwwroot/Files/download/Android/values-" + UK_BUSINESS + "/strings.xml", FileMode.OpenOrCreate);

            using FileStream fsRU_FRIENDLY = new FileStream("wwwroot/Files/download/Android/values-" + RU_FRIENDLY + "/strings.xml", FileMode.OpenOrCreate);
            using FileStream fsEN_FRIENDLY = new FileStream("wwwroot/Files/download/Android/values-" + EN_FRIENDLY + "/strings.xml", FileMode.OpenOrCreate);
            using FileStream fsUK_FRIENDLY = new FileStream("wwwroot/Files/download/Android/values-" + UK_FRIENDLY + "/strings.xml", FileMode.OpenOrCreate);

            formatter.Serialize(fsRU_NEUTRAL, GetSortedArrayOfEntities(RU_NEUTRAL_ARRAY), emptyNamespaces);
            formatter.Serialize(fsEN_NEUTRAL, GetSortedArrayOfEntities(EN_NEUTRAL_ARRAY), emptyNamespaces);
            formatter.Serialize(fsUK_NEUTRAL, GetSortedArrayOfEntities(UK_NEUTRAL_ARRAY), emptyNamespaces);

            formatter.Serialize(fsRU_BUSINESS, GetSortedArrayOfEntities(RU_BUSINESS_ARRAY), emptyNamespaces);
            formatter.Serialize(fsEN_BUSINESS, GetSortedArrayOfEntities(EN_BUSINESS_ARRAY), emptyNamespaces);
            formatter.Serialize(fsUK_BUSINESS, GetSortedArrayOfEntities(UK_BUSINESS_ARRAY), emptyNamespaces);

            formatter.Serialize(fsRU_FRIENDLY, GetSortedArrayOfEntities(RU_FRIENDLY_ARRAY), emptyNamespaces);
            formatter.Serialize(fsEN_FRIENDLY, GetSortedArrayOfEntities(EN_FRIENDLY_ARRAY), emptyNamespaces);
            formatter.Serialize(fsUK_FRIENDLY, GetSortedArrayOfEntities(UK_FRIENDLY_ARRAY), emptyNamespaces);

            fsRU_NEUTRAL.Close();
            fsEN_NEUTRAL.Close();
            fsUK_NEUTRAL.Close();

            fsRU_BUSINESS.Close();
            fsEN_BUSINESS.Close();
            fsUK_BUSINESS.Close();

            fsRU_FRIENDLY.Close();
            fsEN_FRIENDLY.Close();
            fsUK_FRIENDLY.Close();
        }

        private static XmlEntities GetSortedArrayOfEntities(SortedDictionary<int, XMLKeyModel> ARRAY)
        {
            List<ElementString> elementStrings = new List<ElementString>();
            foreach (var item in ARRAY.OrderBy(i => i.Key))
            {
                elementStrings.Add(new ElementString { AttributeValue = item.Value.AttributeValue, Text = item.Value.NodeInnerText });
            }
            XmlEntities xmlEntitiesArray = new XmlEntities() { ElementString = elementStrings };

            return xmlEntitiesArray;
        }

        private Sections AddAndroidSection()
        {
            Sections sectionAndroid = new Sections();
            if (_context.Section.FirstOrDefault(s => s.Title == "ANDROID") == null)
            {
                sectionAndroid = new Sections
                {
                    Title = "ANDROID",
                    LastIndexOfCommonID = "0000",
                    ShortName = "ANDR"
                };
                _context.Section.Add(sectionAndroid);
                _context.SaveChanges();
            }
            else
            {
                sectionAndroid = _context.Section.FirstOrDefault(s => s.Title == "ANDROID");
            }
            return sectionAndroid;
        }

        public void FillInMissingTextsInAndroidTable()
        {
            var androidTabelItems = GetAllValuesAndroid();

            foreach (var androidTabelItem in androidTabelItems)
            {
                if (androidTabelItem.RU_NEUTRAL != null)
                {
                    if (androidTabelItem.RU_BUSINESS == null)
                    {
                        androidTabelItem.RU_BUSINESS = androidTabelItem.RU_NEUTRAL;
                    }
                    if (androidTabelItem.RU_FRIENDLY == null)
                    {
                        androidTabelItem.RU_FRIENDLY = androidTabelItem.RU_NEUTRAL;
                    }

                    if (androidTabelItem.UK_NEUTRAL == null)
                    {
                        androidTabelItem.UK_NEUTRAL = androidTabelItem.RU_NEUTRAL;
                    }
                    if (androidTabelItem.UK_BUSINESS == null)
                    {
                        androidTabelItem.UK_BUSINESS = androidTabelItem.RU_NEUTRAL;
                    }
                    if (androidTabelItem.UK_FRIENDLY == null)
                    {
                        androidTabelItem.UK_FRIENDLY = androidTabelItem.RU_NEUTRAL;
                    }
                }
                else
                {
                    if (androidTabelItem.RU_NEUTRAL == null)
                    {
                        if (androidTabelItem.UK_NEUTRAL != null)
                        {
                            if (androidTabelItem.RU_NEUTRAL == null)
                            {
                                androidTabelItem.RU_NEUTRAL = androidTabelItem.UK_NEUTRAL;
                            }
                            if (androidTabelItem.RU_BUSINESS == null)
                            {
                                androidTabelItem.RU_BUSINESS = androidTabelItem.UK_NEUTRAL;
                            }
                            if (androidTabelItem.RU_FRIENDLY == null)
                            {
                                androidTabelItem.RU_FRIENDLY = androidTabelItem.UK_NEUTRAL;
                            }

                            if (androidTabelItem.UK_BUSINESS == null)
                            {
                                androidTabelItem.UK_BUSINESS = androidTabelItem.UK_NEUTRAL;
                            }
                            if (androidTabelItem.UK_FRIENDLY == null)
                            {
                                androidTabelItem.UK_FRIENDLY = androidTabelItem.UK_NEUTRAL;
                            }
                        }
                    }
                }

                if (androidTabelItem.EN_NEUTRAL != null)
                {
                    if (androidTabelItem.EN_BUSINESS == null)
                    {
                        androidTabelItem.EN_BUSINESS = androidTabelItem.EN_NEUTRAL;
                    }
                    if (androidTabelItem.EN_FRIENDLY == null)
                    {
                        androidTabelItem.EN_FRIENDLY = androidTabelItem.EN_NEUTRAL;
                    }
                }
                else
                {
                    if (androidTabelItem.EN_NEUTRAL == null)
                    {
                        if (androidTabelItem.UK_NEUTRAL != null)
                        {
                            if (androidTabelItem.EN_NEUTRAL == null)
                            {
                                androidTabelItem.EN_NEUTRAL = androidTabelItem.UK_NEUTRAL;
                            }
                            if (androidTabelItem.EN_BUSINESS == null)
                            {
                                androidTabelItem.EN_BUSINESS = androidTabelItem.UK_NEUTRAL;
                            }
                            if (androidTabelItem.EN_FRIENDLY == null)
                            {
                                androidTabelItem.EN_FRIENDLY = androidTabelItem.UK_NEUTRAL;
                            }
                        }
                    }
                }
                _context.AndroidTable.Update(androidTabelItem);
            }
            _context.SaveChanges();
        }

        private bool IsAndroidTableHaveSameID(string androidId)
        {
            return _context.AndroidTable.FirstOrDefault(b => b.AndroidID == androidId) != null;
        }

        private bool IsMainTableHaveSameAndroidID(string androidId)
        {
            return _context.MainTable.FirstOrDefault(b => b.AndroidID == androidId) != null;
        }

        private List<MainTable> GetAllValuesMainTableAndroidIDIsNull()
        {
            return _context.MainTable
                                     .Include(m => m.Section)
                                     .Include(m => m.StyleJsonKeyModel)
                                         .ThenInclude(s => s.LangKeyModels)
                                             .ThenInclude(l => l.LangValue)
                                        .Where(m => m.AndroidID == null).ToList();
        }

        private List<MainTable> GetAllValuesMainTable()
        {
            return _context.MainTable
                                     .Include(m => m.Section)
                                     .Include(m => m.StyleJsonKeyModel)
                                         .ThenInclude(s => s.LangKeyModels)
                                             .ThenInclude(l => l.LangValue).ToList();
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

        private MainTable GetValueFromMainTabelByCommonID(string commonId)
        {
            return _context.MainTable
                                     .Include(m => m.Section)
                                     .Include(m => m.StyleJsonKeyModel)
                                         .ThenInclude(s => s.LangKeyModels)
                                             .ThenInclude(l => l.LangValue)
                                     .Where(m => m.CommonID == commonId).FirstOrDefault();
        }

        private MainTable GetValueFromMainTabelByAndroidID(string androidId)
        {
            return _context.MainTable
                                     .Include(m => m.Section)
                                     .Include(m => m.StyleJsonKeyModel)
                                         .ThenInclude(s => s.LangKeyModels)
                                             .ThenInclude(l => l.LangValue)
                                     .Where(m => m.AndroidID == androidId).FirstOrDefault();
        }

        private List<AndroidTable> GetAllValuesAndroid()
        {
            return _context.AndroidTable
                                     .Include(m => m.Section).ToList();
        }

        private List<AndroidTable> GetAllValuesAndroidWhereSectionIsAndroid()
        {
            return _context.AndroidTable
                                     .Include(m => m.Section)
                                     .Where(m => m.Section.Title == "ANDROID")
                                     .ToList();
        }

        private List<AndroidTable> GetNullAndroidIDValuesInAndroidTables()
        {
            return _context.AndroidTable
                                     .Include(m => m.Section)
                                     .Where(m => m.CommonID == null).ToList();
        }

        public AndroidTable GetAndroidTableItemByCommonId(string commonId)
        {
            return _context.AndroidTable
                                     .Include(m => m.Section)
                                     .Where(m => m.CommonID == commonId).FirstOrDefault();
        }

        public void RemoveAndoridCommentInMainTable()
        {
            foreach (var mainTableItem in GetAllValuesMainTableAndroidIDNotNull())
            {
                mainTableItem.AndroidXMLComment = null;
                _context.MainTable.Update(mainTableItem);
            }
            _context.SaveChanges();
        }

        public void RemoveAllAndoridIdMatchesInMainTable()
        {
            foreach (var mainTableItem in GetAllValuesMainTableAndroidIDNotNull())
            {
                mainTableItem.AndroidID = null;
                _context.MainTable.Update(mainTableItem);
            }
            _context.SaveChanges();
        }

        public void FillCommonIdInAndroidTableAccordingMainTable()
        {

            foreach (var androidItem in GetNullAndroidIDValuesInAndroidTables())
            {
                MainTable mainTableItem = GetValueFromMainTabelByAndroidID(androidItem.AndroidID);
                androidItem.CommonID = mainTableItem.CommonID;
                mainTableItem.AndoridStringNumber = androidItem.StringNumber;
                mainTableItem.AndroidXMLComment = androidItem.CommentValue;

                _context.AndroidTable.Update(androidItem);
                _context.MainTable.Update(mainTableItem);
            }

            _context.SaveChanges();
        }


        public void UpdateAndroidTableAccordingMainTable()
        {

            foreach (var androidItem in GetAllValuesAndroid())
            {
                MainTable mainTableItem = GetValueFromMainTabelByAndroidID(androidItem.AndroidID);
                if (mainTableItem != null)
                {


                    foreach (var style in mainTableItem.StyleJsonKeyModel)
                    {
                        foreach (var item in style.LangKeyModels)
                        {
                            try
                            {
                                switch (style.StyleName)
                                {
                                    case "neutral":
                                        switch (item.LangName)
                                        {
                                            case "ru":
                                                androidItem.RU_NEUTRAL = item.LangValue.Single;
                                                break;
                                            case "uk":
                                                androidItem.UK_NEUTRAL = item.LangValue.Single;
                                                break;
                                            case "en":
                                                androidItem.EN_NEUTRAL = item.LangValue.Single;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case "friendly":
                                        switch (item.LangName)
                                        {
                                            case "ru":
                                                androidItem.RU_FRIENDLY = item.LangValue.Single;
                                                break;
                                            case "uk":
                                                androidItem.UK_FRIENDLY = item.LangValue.Single;
                                                break;
                                            case "en":
                                                androidItem.EN_FRIENDLY = item.LangValue.Single;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case "business":
                                        switch (item.LangName)
                                        {
                                            case "ru":
                                                androidItem.RU_BUSINESS = item.LangValue.Single;
                                                break;
                                            case "uk":
                                                androidItem.UK_BUSINESS = item.LangValue.Single;
                                                break;
                                            case "en":
                                                androidItem.EN_BUSINESS = item.LangValue.Single;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(item + " key already exists " + item + "\n" + ex);
                            }
                        }
                    }



                    androidItem.CommonID = mainTableItem.CommonID;
                    mainTableItem.AndoridStringNumber = androidItem.StringNumber;
                    mainTableItem.AndroidXMLComment = androidItem.CommentValue;

                    _context.AndroidTable.Update(androidItem);
                    _context.MainTable.Update(mainTableItem);
                }
            }

            _context.SaveChanges();
        }

        public void FillInMainTableAndroidInfo()
        {
            foreach (var androidItem in GetAllValuesAndroid())
            {
                try
                {
                    MainTable mainTableItem = GetValueFromMainTabelByAndroidID(androidItem.AndroidID);

                    mainTableItem.AndoridStringNumber = androidItem.StringNumber;
                    mainTableItem.AndroidXMLComment = androidItem.CommentValue;


                    _context.MainTable.Update(mainTableItem);
                }
                catch (Exception)
                {

                }
            }

            _context.SaveChanges();
        }

        public void ChangeSectionByPack()
        {
            var android = _context.AndroidTable
                                     .Include(m => m.Section)
                                     .Where(m => m.Section.Title == "ANDROID").ToList();

            foreach (var item in android)
            {
                try
                {
                    switch (item.CommentValue)
                    {
                        case "Cash U": UpdateAndroidItemSection(item.CommonID, "cashU"); break;
                        case "Feature Auth Common":
                        case "Auth greetings":
                        case "Auth access code":
                        case "Auth by biometry": UpdateAndroidItemSection(item.CommonID, "authorization"); break;
                        case "transfers": UpdateAndroidItemSection(item.CommonID, "transfers"); break;
                        case "mainProducts": UpdateAndroidItemSection(item.CommonID, "mainProducts"); break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("No XML comment");
                }

            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationRep.Data;
using LocalizationRep.Models;
using Microsoft.AspNetCore.Hosting;

namespace LocalizationRep.Utilities
{
    public class FileActionHelpers
    {
        public static string UploadPath = "Files/upload/";
        public static string DownloadPath = "Files/download/";


        public enum TypeOfLoad
        {
            UPLOAD, DOWNLOAD
        }
        public static void RemoveFile(string fullpath, LocalizationRepContext _context)
        {
            var entity = _context.FileModel.FirstOrDefault(e => e.Path == fullpath);
            File.Delete(fullpath);
            _context.FileModel.Remove(entity);
            _context.SaveChanges();
        }

        public FileActionHelpers()
        {
        }

        /// <summary>
        /// Метод получающий данные о файлах(путь расположения) в переменную FileInfo
        /// Также фильтрует файлы по расширениям json и xml
        /// </summary>
        public static List<FileModel> GetFilesInfo(IWebHostEnvironment _appEnvironment)
        {
            List<FileModel> FileInformation = new List<FileModel>();

            string filePathUpload = Path.Combine(_appEnvironment.WebRootPath, UploadPath);
            string filePathDownload = Path.Combine(_appEnvironment.WebRootPath, DownloadPath);

            FileInformation = GetFileInfoByPath(filePathUpload, TypeOfLoad.UPLOAD.ToString());
            FileInformation.AddRange(GetFileInfoByPath(filePathDownload, TypeOfLoad.DOWNLOAD.ToString()));

            return FileInformation;
        }

        private static List<FileModel> GetFileInfoByPath(string filePath, string TypeOfLoad)
        {
            List<FileModel> FileInformation = new List<FileModel>();

            List<string[]> DocumentsIOs = new List<string[]>();
            List<string[]> DocumentsAndroid = new List<string[]>();

            foreach (var item in Directory.EnumerateDirectories(filePath))
            {
                if (item.Contains("iOs"))
                {
                    DocumentsIOs.Add(Directory.GetFiles(item));
                }
                else
                {
                    foreach (var itemAn in Directory.EnumerateDirectories(item))
                    {
                        DocumentsAndroid.Add(Directory.GetFiles(itemAn));
                    }
                }
            }

            FileInformation = SetFileInfo(DocumentsIOs, DocumentsAndroid, TypeOfLoad);
            return FileInformation;
        }

        private static List<FileModel> SetFileInfo(List<string[]> DocumentsIOs, List<string[]> DocumentsAndroid, string TypeOfLoad)
        {
            List<FileModel> FileInformation = new List<FileModel>();
            Dictionary<string, Dictionary<string, List<string[]>>> DocumentsInfo = new Dictionary<string, Dictionary<string, List<string[]>>>();
            Dictionary<string, List<string[]>> DocumentsUploadInfo = new Dictionary<string, List<string[]>>
            {
                { "iOs", DocumentsIOs },
                { "Android", DocumentsAndroid }
            };

            DocumentsInfo.Add(TypeOfLoad, DocumentsUploadInfo);

            foreach (var documentsInfo in DocumentsInfo)
            {
                foreach (var documents in documentsInfo.Value)
                {
                    foreach (var documentsString in documents.Value)
                    {
                        foreach (var item in documentsString)
                        {
                            if (Path.GetExtension(item) == ".json" || Path.GetExtension(item) == ".xml" || Path.GetExtension(item) == ".csv")
                            {
                                FileInformation.Add(new FileModel { Name = Path.GetFileName(item), Path = item, TypeOfLoad = documentsInfo.Key });
                            }
                        }
                    }
                }
            }
            return FileInformation;
        }

        public static string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return types[ext];
        }

        private static Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".json","application/json"},
                {".xml","application/xml"},
                {".csv","text/csv"}
            };
        }
    }
}

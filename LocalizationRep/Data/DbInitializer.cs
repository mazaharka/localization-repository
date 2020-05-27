//using LocalizationRep.Models;
//using System;
//using System.Linq;

//namespace LocalizationRep.Data
//{
//    public static class DbInitializer
//    {
//        public static void Initialize(LocalizationRepContext context)
//        {
//            context.Database.EnsureCreated();

//            if (context.MainTable.Any())
//            {
//                return;
//            }

//            var students = new MainTable[]
//            {
//            new MainTable{SectionID = 1 ,AndroidID = "localAuth", IOsID = "local_Auth", TextRU = "Местная авторизация", TextEN = "", TextUA = "", IsFreezing = false},
//            new MainTable{SectionID = 1 ,AndroidID = "localAuthTitle", IOsID = "local_Auth_Title", TextRU = "", TextEN = "Заголовок", TextUA = "", IsFreezing = false},
//            new MainTable{SectionID = 1 ,AndroidID = "localAuthDescription", IOsID = "local_Auth_Description", TextRU = "Описание", TextEN = "", TextUA = "", IsFreezing = false}
//            };
//            foreach (MainTable s in students)
//            {
//                context.MainTable.Add(s);
//            }
//            context.SaveChanges();

//            var sections = new Sections[]
//            {
//            new Sections{ID = 1, Title = "Auth"},
//            new Sections{ID = 2, Title = "Desktop"},
//            new Sections{ID = 3, Title = "Reauth"},
//            };
//            foreach (Sections c in sections)
//            {
//                context.Section.Add(c);
//            }
//            context.SaveChanges();
//        }
//    }
//}
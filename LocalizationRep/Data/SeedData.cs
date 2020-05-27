using System;
using System.Linq;
using LocalizationRep.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalizationRep.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new LocalizationRepContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<LocalizationRepContext>>()))
            {
                // Look for any movies.
                if (context.MainTable.Any())
                {
                    return;   // DB has been seeded
                }

                context.MainTable.AddRange(
                    new MainTable
                    {
                        SectionID = 1,
                        AndroidID = "localAuth",
                        IOsID = "local_Auth",
                        TextRU = "Местная авторизация",
                        TextEN = "",
                        TextUA = "",
                        IsFreezing = false
                    },

                    new MainTable
                    {
                        SectionID = 2,
                        AndroidID = "localAuthTitle",
                        IOsID = "local_Auth_Title",
                        TextRU = "",
                        TextEN = "Заголовок",
                        TextUA = "",
                        IsFreezing = false
                    }
                );
                context.SaveChanges();

                if (context.Section.Any())
                {
                    return;
                }

                context.Section.AddRange(
                    new Sections
                    {
                        ID = 1,
                        Title = "Auth"
                    },

                    new Sections
                    {
                        ID = 2,
                        Title = "Desktop"
                    },

                    new Sections
                    {
                        ID = 3,
                        Title = "Reauth"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}

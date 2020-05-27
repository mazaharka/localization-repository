using Microsoft.EntityFrameworkCore;
using LocalizationRep.Models;

namespace LocalizationRep.Data
{
    public class LocalizationRepContext : DbContext
    {
        public LocalizationRepContext()
        {
        }

        public LocalizationRepContext(DbContextOptions<LocalizationRepContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<MainTable> MainTable { get; set; }
        public DbSet<Sections> Section { get; set; }
        public DbSet<FileModel> FileModel { get; set; }
    }
}

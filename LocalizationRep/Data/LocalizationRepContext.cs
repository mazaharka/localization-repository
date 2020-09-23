using Microsoft.EntityFrameworkCore;
using LocalizationRep.Models;

namespace LocalizationRep.Data
{
    public class LocalizationRepContext : DbContext
    {
        public LocalizationRepContext()
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        public LocalizationRepContext(DbContextOptions<LocalizationRepContext> options) : base(options)
        {
            Database.EnsureCreated();
            //Database.Migrate();
        }

        public DbSet<FileModel> FileModel { get; set; }
        public DbSet<MainTable> MainTable { get; set; }
        public DbSet<Sections> Section { get; set; }
        public DbSet<StyleJsonKeyModel> StyleJsonKeyModel { get; set; }
        public DbSet<LangKeyModel> LangKeyModel { get; set; }
        public DbSet<LangValue> LangValue { get; set; }
        public DbSet<AndroidTable> AndroidTable { get; set; }
        public DbSet<CommentAndroidXMLModel> CommentAndroidXMLModel { get; set; }
    }
}

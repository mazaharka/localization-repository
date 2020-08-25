using CsvHelper.Configuration;

namespace LocalizationRep.Models
{
    public class CsvFileMap : ClassMap<CsvFileModel>
    {
        public CsvFileMap()
        {
            Map(m => m.CommonID).Name("CommonID");
            Map(m => m.SectorName).Name("SectorName");
            Map(m => m.TextStyle).Name("TextStyle");
            Map(m => m.TextRUSingle).Name("TextRUSingle");
            Map(m => m.TextRUPrular).Name("TextRUPrular");
            Map(m => m.TextENSingle).Name("TextENSingle");
            Map(m => m.TextENPrular).Name("TextENPrular");
            Map(m => m.TextUASingle).Name("TextUASingle");
            Map(m => m.TextUAPrular).Name("TextUAPrular");
        }
    }
}

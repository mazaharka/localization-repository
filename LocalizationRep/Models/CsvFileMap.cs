using System;
using CsvHelper.Configuration;

namespace LocalizationRep.Models
{
    public class CsvFileMap : ClassMap<CsvFileModel>
    {
        public CsvFileMap()
        {
            Map(m => m.CommonID).Name("CommonID");
            Map(m => m.SectorName).Name("SectorName");
            Map(m => m.TextRU).Name("TextRU");
            Map(m => m.TextEN).Name("TextEN");
            Map(m => m.TextUA).Name("TextUA");
        }
    }
}

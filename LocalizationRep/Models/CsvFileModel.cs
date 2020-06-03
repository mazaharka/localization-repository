using CsvHelper.Configuration.Attributes;

namespace LocalizationRep.Models
{
    public class CsvFileModel
    {
        [Name("CommonID")]
        public string CommonID { get; set; }
        [Name("SectorName")]
        public string SectorName { get; set; }
        [Name("TextRU")]
        public string TextRU { get; set; }
        [Name("TextEN")]
        public string TextEN { get; set; }
        [Name("TextUA")]
        public string TextUA { get; set; }

    }
}

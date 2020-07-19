using CsvHelper.Configuration.Attributes;

namespace LocalizationRep.Models
{
    public class CsvFileModel
    {
        [Name("CommonID")]
        public string CommonID { get; set; }
        [Name("SectorName")]
        public string SectorName { get; set; }
        [Name("Style")]
        public string TextStyle { get; set; }
        [Name("TextRUSingle")]
        public string TextRUSingle { get; set; }
        [Name("TextRUPrular")]
        public string TextRUPrular { get; set; }
        [Name("TextENSingle")]
        public string TextENSingle { get; set; }
        [Name("TextENPrular")]
        public string TextENPrular { get; set; }
        [Name("TextUASingle")]
        public string TextUASingle { get; set; }
        [Name("TextUAPrular")]
        public string TextUAPrular { get; set; }

    }
}

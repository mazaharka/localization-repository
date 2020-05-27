namespace LocalizationRep.Models
{
    public class MainTable
    {
        public int ID { get; set; }

        public int SectionID { get; set; }

        public string CommonID { get; set; }
        public string IOsID { get; set; }
        public string AndroidID { get; set; }

        public string TextRU { get; set; }
        public string TextEN { get; set; }
        public string TextUA { get; set; }

        public bool IsFreezing { get; set; }

        public Sections Section { get; set; }
    }
}

namespace LocalizationRep.Models
{
    public class SearchModel
    {
        public string CommonID { get; set; }

        public string IOsID { get; set; }
        public string AndroidID { get; set; }

        public string TextStyle { get; set; }

        public string TextRU_NEUTRAL { get; set; }
        public string TextEN_NEUTRAL { get; set; }
        public string TextUK_NEUTRAL { get; set; }

        public string TextRU_BUSINESS { get; set; }
        public string TextEN_BUSINESS { get; set; }
        public string TextUK_BUSINESS { get; set; }

        public string TextRU_FRIENDLY { get; set; }
        public string TextEN_FRIENDLY { get; set; }
        public string TextUK_FRIENDLY { get; set; }
    }
}

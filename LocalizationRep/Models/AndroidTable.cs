namespace LocalizationRep.Models
{
    public class AndroidTable
    {

        public int ID { get; set; }

        public int SectionID { get; set; }

        public string AndroidID { get; set; }

        public int StringNumber { get; set; }

        //public string NodeInnerText { get; set; }

        public string RU_NEUTRAL { get; set; }
        public string UK_NEUTRAL { get; set; }
        public string EN_NEUTRAL { get; set; }

        public string RU_BUSINESS { get; set; }
        public string UK_BUSINESS { get; set; }
        public string EN_BUSINESS { get; set; }

        public string RU_FRIENDLY { get; set; }
        public string UK_FRIENDLY { get; set; }
        public string EN_FRIENDLY { get; set; }

        public string CommentValue { get; set; }

        public string CommonID { get; set; }

        public Sections Section { get; set; }

    }
}

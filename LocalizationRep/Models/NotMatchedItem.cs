namespace LocalizationRep.Models
{
    public class NotMatchedItem
    {
        public int ID { get; set; }

        public int SectionID { get; set; } 

        public int StringNumber { get; set; }
        public string AndroidID { get; set; }
        public string NodeInnerText { get; set; }
        public string CommentValue { get; set; }

        public string CommonID { get; set; }

        public Sections Section { get; set; }

    }
}

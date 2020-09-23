namespace LocalizationRep.Models
{
    public class XMLKeyModel
    {
        public string CommonId { get; set; }
        public int StringNumber { get; set; }
        public string AttributeValue { get; set; }
        public string NodeInnerText { get; set; }
        public double MatchPercentage { get; set; }
        public bool IsComment { get; set; }
        public string CommentValue { get; set; }
        public string StyleText { get; set; }
        public string LanguageName { get; set; }
    }
}

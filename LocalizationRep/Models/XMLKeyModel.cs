namespace LocalizationRep.Models
{
    public class XMLKeyModel
    {
        public int StringNumber { get; set; }
        public string AttributeValue { get; set; }
        public string NodeInnerText { get; set; }
        public double MatchPercentage { get; set; }
        public bool IsComment { get; set; }
        public string CommentValue { get; set; }
    }
}

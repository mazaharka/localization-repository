using System.Collections.Generic;

namespace LocalizationRep.Models
{
    public class Sections
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string ShortName { get; set; }
        public string LastIndexOfCommonID { get; set; }
        public string CommentAndroidXMLModelID { get; set; }

        public CommentAndroidXMLModel CommentAndroidXMLModel { get; set; }
        public ICollection<MainTable> MainTables { get; set; }
    }
}

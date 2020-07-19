using System.Collections.Generic;

namespace LocalizationRep.Models
{
    public class MainTable
    {
        public int ID { get; set; }

        public int SectionID { get; set; }

        public string CommonID { get; set; }
        public string IOsID { get; set; }
        public string AndroidID { get; set; }
        public int AndoridStringNumber { get; set; }

        public List<StyleJsonKeyModel> StyleJsonKeyModel { get; set; }

        public bool IOsOnly { get; set; }
        public bool AndroidOnly { get; set; }
        public bool IsFreezing { get; set; }

        public Sections Section { get; set; }

    }
}

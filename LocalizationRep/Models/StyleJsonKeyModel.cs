using System;
using System.Collections.Generic;

namespace LocalizationRep.Models
{
    public class StyleJsonKeyModel
    {
        public int ID { get; set; }
        public string StyleName { get; set; }
        public List<LangKeyModel> LangKeyModels { get; set; }

        public MainTable MainTables { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace LocalizationRep.Models
{
    public class LangKeyModel
    {
        public int ID { get; set; }
        public string LangName { get; set; }
        public LangValue LangValue { get; set; }

        public StyleJsonKeyModel StyleJsonKeyModel { get; set; }
    }
}

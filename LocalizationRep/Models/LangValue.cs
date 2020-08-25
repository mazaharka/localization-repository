using System;
using System.Collections.Generic;

namespace LocalizationRep.Models
{
    public class LangValue
    {
        public int ID { get; set; }
        public string Single { get; set; }
        public string Prular { get; set; } = null;

        public int LangKeyModelId { get; set; }
        public LangKeyModel LangKeyModel { get; set; }
    }
}

using System.Collections.Generic;

namespace LocalizationRep.Models
{
    public class JsonKeyModel
    {
        public string JsonKey { get; set; }
        public List<LangKeyModel> JsonValue { get; set; }
    }
}

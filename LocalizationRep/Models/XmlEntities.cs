using System.Collections.Generic;
using System.Xml.Serialization;

namespace LocalizationRep.Models
{
    [XmlRoot("resources")]
    public class XmlEntities
    {
        [XmlElement("string")]
        public List<ElementString> ElementString { get; set; }
    }

    public class ElementString
    {
        [XmlAttribute("name")]
        public string AttributeValue { get; set; }

        [XmlAttribute("id")]
        public string CommonId { get; set; }

        [XmlText()]
        public string Text { get; set; }
    }
}

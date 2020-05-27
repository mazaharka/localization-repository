using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LocalizationRep.Models
{
    public class SectionSearchViewModel
    {

        public List<MainTable> MainTables { get; set; }     // Список записей локализации.
        public SelectList Sections { get; set; }            // Объект SelectList со списком разделов. В этом списке пользователь может выбрать раздел переводов.
        public string SectionSearch { get; set; }     // Объект LocalizationSection, содержащий выбранный раздел.
        public string SearchString { get; set; }            // SearchString, содержащий текст, который пользователи вводят в поле поиска.

    }
}

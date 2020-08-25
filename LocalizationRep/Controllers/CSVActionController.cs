using System.Collections.Generic;
using System.Linq;
using LocalizationRep.Data;
using LocalizationRep.Models;

namespace LocalizationRep.Controllers
{
    public static class CSVActionController
    {
        public static void UpdateFromCsv(List<CsvFileModel> csvFiles, LocalizationRepContext _context)
        {
            foreach (var item in csvFiles)
            {
                var entity = _context.MainTable.FirstOrDefault(e => e.CommonID == item.CommonID);
                if (entity != null)
                {

                    //entity.TextEN = item.TextEN;
                    //entity.TextRU = item.TextRU;
                    //entity.TextUA = item.TextUA;

                    _context.MainTable.Update(entity);

                    _context.SaveChanges();
                }
            }
        }
    }
}

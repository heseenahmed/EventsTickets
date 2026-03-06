using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using Tickets.Application.Common.Interfaces;

namespace Tickets.Application.Common.Services
{
    public class ExcelService : IExcelService
    {
        public byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            
            // Insert data as a table (handles headers based on property names)
            worksheet.Cell(1, 1).InsertTable(data);
            
            // Auto-fit columns for better readability
            worksheet.Columns().AdjustToContents();
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

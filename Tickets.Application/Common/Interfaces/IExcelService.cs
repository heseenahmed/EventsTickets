using System.Collections.Generic;

namespace Tickets.Application.Common.Interfaces
{
    public interface IExcelService
    {
        byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mini_pos.Models;

namespace mini_pos.Services;

public interface ISalesRepository
{
    Task<bool> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details);
    Task<List<SalesReportItem>> GetSalesReportAsync(DateTime startDate, DateTime endDate);
}

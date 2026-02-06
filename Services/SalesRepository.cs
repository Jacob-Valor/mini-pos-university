using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;
using Serilog;

namespace mini_pos.Services;

public sealed class SalesRepository : ISalesRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public SalesRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using var saleCmd = new MySqlCommand(SqlQueries.InsertSale, connection, transaction);
            saleCmd.Parameters.AddWithValue("@exId", sale.ExchangeRateId);
            saleCmd.Parameters.AddWithValue("@cusId", sale.CustomerId);
            saleCmd.Parameters.AddWithValue("@empId", sale.EmployeeId);
            saleCmd.Parameters.Add(new MySqlParameter("@date", MySqlDbType.DateTime)
            {
                Value = sale.DateSale
            });
            saleCmd.Parameters.Add(new MySqlParameter("@sub", MySqlDbType.Decimal)
            {
                Precision = 12,
                Scale = 2,
                Value = sale.SubTotal
            });
            saleCmd.Parameters.Add(new MySqlParameter("@pay", MySqlDbType.Decimal)
            {
                Precision = 12,
                Scale = 2,
                Value = sale.Pay
            });
            saleCmd.Parameters.Add(new MySqlParameter("@change", MySqlDbType.Decimal)
            {
                Precision = 12,
                Scale = 2,
                Value = sale.Change
            });

            var saleIdObj = await saleCmd.ExecuteScalarAsync();
            int saleId = Convert.ToInt32(saleIdObj);

            foreach (var item in details)
            {
                await using var detailCmd = new MySqlCommand(SqlQueries.InsertSaleDetail, connection, transaction);
                detailCmd.Parameters.AddWithValue("@saleId", saleId);
                detailCmd.Parameters.AddWithValue("@prodId", item.ProductId);
                detailCmd.Parameters.AddWithValue("@qty", item.Quantity);
                detailCmd.Parameters.Add(new MySqlParameter("@price", MySqlDbType.Decimal)
                {
                    Precision = 12,
                    Scale = 2,
                    Value = item.Price
                });
                detailCmd.Parameters.Add(new MySqlParameter("@total", MySqlDbType.Decimal)
                {
                    Precision = 12,
                    Scale = 2,
                    Value = item.Total
                });
                await detailCmd.ExecuteNonQueryAsync();

                await using var stockCmd = new MySqlCommand(SqlQueries.UpdateStock, connection, transaction);
                stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                stockCmd.Parameters.AddWithValue("@prodId", item.ProductId);
                await stockCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            Log.Information("Sale created successfully. Amount: {Amount}", sale.SubTotal);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Log.Error(ex, "Error creating sale");
            return false;
        }
    }

    public async Task<List<SalesReportItem>> GetSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var items = new List<SalesReportItem>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.SalesReport, connection);

            var endDateTime = endDate.Date.AddDays(1).AddTicks(-1);
            command.Parameters.Add(new MySqlParameter("@start", MySqlDbType.DateTime)
            {
                Value = startDate.Date
            });
            command.Parameters.Add(new MySqlParameter("@end", MySqlDbType.DateTime)
            {
                Value = endDateTime
            });

            await using var reader = await command.ExecuteReaderAsync();

            int no = 1;
            while (await reader.ReadAsync())
            {
                items.Add(new SalesReportItem
                {
                    No = no++,
                    Barcode = reader.GetString("product_id"),
                    ProductName = reader.IsDBNull("product_name") ? "Unknown" : reader.GetString("product_name"),
                    Unit = reader.IsDBNull("unit") ? "" : reader.GetString("unit"),
                    Quantity = reader.GetInt32("total_qty"),
                    Price = reader.GetDecimal("price"),
                    Total = reader.GetDecimal("total_amount")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching sales report");
        }

        return items;
    }
}

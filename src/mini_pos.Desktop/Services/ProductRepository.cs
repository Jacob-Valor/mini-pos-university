using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;
using Serilog;

namespace mini_pos.Services;

public sealed class ProductRepository : IProductRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public ProductRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        var products = new List<Product>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.Products, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Barcode = reader.GetString("barcode"),
                    ProductName = reader.GetString("product_name"),
                    Unit = reader.GetString("unit"),
                    Quantity = reader.GetInt32("quantity"),
                    QuantityMin = reader.GetInt32("quantity_min"),
                    CostPrice = reader.GetDecimal("cost_price"),
                    RetailPrice = reader.GetDecimal("retail_price"),
                    BrandId = reader.IsDBNull("brand_id") ? "" : reader.GetString("brand_id"),
                    BrandName = reader.IsDBNull("brand_name") ? "" : reader.GetString("brand_name"),
                    CategoryId = reader.IsDBNull("category_id") ? "" : reader.GetString("category_id"),
                    CategoryName = reader.IsDBNull("category_name") ? "" : reader.GetString("category_name"),
                    Status = reader.GetString("status")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching products");
        }

        return products;
    }

    public async Task<Product?> GetProductByBarcodeAsync(string barcode)
    {
        ArgumentException.ThrowIfNullOrEmpty(barcode);

        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.ProductByBarcode, connection);
            command.Parameters.AddWithValue("@barcode", barcode);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Product
                {
                    Barcode = reader.GetString("barcode"),
                    ProductName = reader.GetString("product_name"),
                    Unit = reader.GetString("unit"),
                    Quantity = reader.GetInt32("quantity"),
                    QuantityMin = reader.GetInt32("quantity_min"),
                    CostPrice = reader.GetDecimal("cost_price"),
                    RetailPrice = reader.GetDecimal("retail_price"),
                    BrandId = reader.IsDBNull("brand_id") ? "" : reader.GetString("brand_id"),
                    BrandName = reader.IsDBNull("brand_name") ? "" : reader.GetString("brand_name"),
                    CategoryId = reader.IsDBNull("category_id") ? "" : reader.GetString("category_id"),
                    CategoryName = reader.IsDBNull("category_name") ? "" : reader.GetString("category_name"),
                    Status = reader.GetString("status")
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error finding product by barcode: {Barcode}", barcode);
            return null;
        }
    }

    public async Task<bool> ProductExistsAsync(string barcode)
    {
        ArgumentException.ThrowIfNullOrEmpty(barcode);

        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.ProductExists, connection);
            command.Parameters.AddWithValue("@id", barcode);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking product existence: {Barcode}", barcode);
            return false;
        }
    }

    public async Task<bool> AddProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertProduct, connection);

            command.Parameters.AddWithValue("@id", product.Barcode);
            command.Parameters.AddWithValue("@name", product.ProductName);
            command.Parameters.AddWithValue("@unit", product.Unit);
            command.Parameters.AddWithValue("@qty", product.Quantity);
            command.Parameters.AddWithValue("@min", product.QuantityMin);
            command.Parameters.Add(new MySqlParameter("@cost", MySqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 0,
                Value = product.CostPrice
            });
            command.Parameters.Add(new MySqlParameter("@price", MySqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 0,
                Value = product.RetailPrice
            });
            command.Parameters.AddWithValue("@brand", product.BrandId);
            command.Parameters.AddWithValue("@type", product.CategoryId);
            command.Parameters.AddWithValue("@status", product.Status);

            await command.ExecuteNonQueryAsync();
            Log.Information("Product added: {ProductName} ({Barcode})", product.ProductName, product.Barcode);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding product: {ProductName}", product.ProductName);
            return false;
        }
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateProduct, connection);
            command.Parameters.AddWithValue("@id", product.Barcode);
            command.Parameters.AddWithValue("@name", product.ProductName);
            command.Parameters.AddWithValue("@unit", product.Unit);
            command.Parameters.AddWithValue("@qty", product.Quantity);
            command.Parameters.AddWithValue("@min", product.QuantityMin);
            command.Parameters.Add(new MySqlParameter("@cost", MySqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 0,
                Value = product.CostPrice
            });
            command.Parameters.Add(new MySqlParameter("@price", MySqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 0,
                Value = product.RetailPrice
            });
            command.Parameters.AddWithValue("@brand", product.BrandId);
            command.Parameters.AddWithValue("@type", product.CategoryId);
            command.Parameters.AddWithValue("@status", product.Status);

            await command.ExecuteNonQueryAsync();
            Log.Information("Product updated: {ProductName} ({Barcode})", product.ProductName, product.Barcode);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating product: {Barcode}", product.Barcode);
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(string barcode)
    {
        ArgumentException.ThrowIfNullOrEmpty(barcode);

        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteProduct, connection);
            command.Parameters.AddWithValue("@id", barcode);
            await command.ExecuteNonQueryAsync();
            Log.Information("Product deleted: {Barcode}", barcode);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting product: {Barcode}", barcode);
            return false;
        }
    }
}

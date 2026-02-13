namespace mini_pos.Models;

/// <summary>
/// Represents a product in the Mini POS system.
/// Maps to the 'product' table in the database.
/// </summary>
public class Product
{
    /// <summary>
    /// Product barcode (primary key).
    /// </summary>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>
    /// Product name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Unit of measurement (e.g., "ຕຸກ", "ກ໋ອງ", "ຖົງ").
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Current quantity in stock.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Minimum quantity threshold for reorder alerts.
    /// </summary>
    public int QuantityMin { get; set; }

    /// <summary>
    /// Cost price (purchase price).
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Retail price (selling price).
    /// </summary>
    public decimal RetailPrice { get; set; }

    /// <summary>
    /// Brand ID (foreign key).
    /// </summary>
    public string BrandId { get; set; } = string.Empty;

    /// <summary>
    /// Brand name (from brand table join).
    /// </summary>
    public string BrandName { get; set; } = string.Empty;

    /// <summary>
    /// Category ID (foreign key).
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// Category name (from category table join).
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Product status (e.g., "ມີ" = available).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    // Legacy properties for backward compatibility - Use Barcode, ProductName, etc. instead
    /// <summary>
    /// [DEPRECATED] Use <see cref="Barcode"/> instead.
    /// </summary>
    [System.Obsolete("Use Barcode property instead")]
    public string Id { get => Barcode; set => Barcode = value; }

    /// <summary>
    /// [DEPRECATED] Use <see cref="ProductName"/> instead.
    /// </summary>
    [System.Obsolete("Use ProductName property instead")]
    public string Name { get => ProductName; set => ProductName = value; }

    /// <summary>
    /// [DEPRECATED] Use <see cref="QuantityMin"/> instead.
    /// </summary>
    [System.Obsolete("Use QuantityMin property instead")]
    public int MinQuantity { get => QuantityMin; set => QuantityMin = value; }

    /// <summary>
    /// [DEPRECATED] Use <see cref="RetailPrice"/> instead.
    /// </summary>
    [System.Obsolete("Use RetailPrice property instead")]
    public decimal SellingPrice { get => RetailPrice; set => RetailPrice = value; }

    /// <summary>
    /// [DEPRECATED] Use <see cref="BrandName"/> instead.
    /// </summary>
    [System.Obsolete("Use BrandName property instead")]
    public string Brand { get => BrandName; set => BrandName = value; }

    /// <summary>
    /// [DEPRECATED] Use <see cref="CategoryName"/> instead.
    /// </summary>
    [System.Obsolete("Use CategoryName property instead")]
    public string Type { get => CategoryName; set => CategoryName = value; }
}

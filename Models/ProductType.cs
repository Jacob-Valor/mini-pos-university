namespace mini_pos.Models;

/// <summary>
/// Represents a product category/type in the Mini POS system.
/// Maps to the 'category' table in the database.
/// </summary>
public class ProductType
{
    /// <summary>
    /// Category ID (e.g., "C001").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

namespace mini_pos.Models;

/// <summary>
/// Represents a product brand in the Mini POS system.
/// Maps to the 'brand' table in the database.
/// </summary>
public class Brand
{
    /// <summary>
    /// Brand ID (e.g., "B001").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Brand name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

namespace mini_pos.Models;

/// <summary>
/// Represents a customer entity (ລູກຄ້າ)
/// </summary>
public class Customer
{
    /// <summary>
    /// Unique customer identifier (ລະຫັດລູກຄ້າ)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Customer's first name (ຊື່)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Customer's surname (ນາມສະກຸນ)
    /// </summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Customer's gender - "ຊາຍ" (Male) or "ຍິງ" (Female)
    /// </summary>
    public string Gender { get; set; } = string.Empty;

    /// <summary>
    /// Customer's phone number (ເບີໂທລະສັບ)
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Customer's address (ທີ່ຢູ່)
    /// </summary>
    public string Address { get; set; } = string.Empty;
}

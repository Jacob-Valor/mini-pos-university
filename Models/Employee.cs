using System;

namespace mini_pos.Models;

/// <summary>
/// Represents an employee in the Mini POS system.
/// Contains personal information, contact details, and authentication credentials.
/// </summary>
public class Employee
{
    /// <summary>
    /// Gets or sets the unique employee identifier.
    /// Format: EMP### (e.g., EMP001, EMP002).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee's first name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee's surname (last name).
    /// </summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee's gender.
    /// Typical values: "ຊາຍ" (Male), "ຍິງ" (Female).
    /// </summary>
    public string Gender { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee's date of birth.
    /// Used for age verification and employee records.
    /// </summary>
    public DateTime DateOfBirth { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the employee's phone number.
    /// Format varies by region (e.g., 020-12345678 for Lao numbers).    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the province where the employee resides.
    /// Part of the employee's address information.
    /// </summary>
    public string Province { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the district where the employee resides.
    /// Part of the employee's address information.
    /// </summary>
    public string District { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the village where the employee resides.
    /// Part of the employee's address information.
    /// </summary>
    public string Village { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the province identifier.
    /// </summary>
    public string ProvinceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the district identifier.
    /// </summary>
    public string DistrictId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the village identifier.
    /// </summary>
    public string VillageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee's password for system authentication.
    /// Should be stored as a hashed value for security.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee's job position or role.
    /// Examples: "Admin", "Cashier", "Manager".
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path to the employee's profile image.
    /// Used for displaying employee photos in the UI.
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username for system login.
    /// </summary>
    public string Username { get; set; } = string.Empty;
}

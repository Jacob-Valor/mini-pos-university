using System;

namespace mini_pos.Models;

public class Employee
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; } = DateTime.Now;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}

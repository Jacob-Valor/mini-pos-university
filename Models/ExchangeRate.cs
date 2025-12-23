using System;

namespace mini_pos.Models;

public class ExchangeRate
{
    public int Id { get; set; }
    public decimal UsdRate { get; set; }
    public decimal ThbRate { get; set; }
    public DateTime CreatedDate { get; set; }
}

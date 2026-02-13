using System;
using System.Collections.Generic;

namespace mini_pos.Models;

public class Sale
{
    public int SaleId { get; set; }
    public int ExchangeRateId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime DateSale { get; set; } = DateTime.Now;
    public decimal SubTotal { get; set; }
    public decimal Pay { get; set; }
    public decimal Change { get; set; }
}

public class SaleDetail
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}

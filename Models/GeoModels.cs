using System;

namespace mini_pos.Models;

public class Province
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class District
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProvinceId { get; set; } = string.Empty;
}

public class Village
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
}

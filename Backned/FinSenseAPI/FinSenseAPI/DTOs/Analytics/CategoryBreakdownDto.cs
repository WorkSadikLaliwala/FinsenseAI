using System;

namespace FinSenseAPI.DTOs.Analytics;

public class CategoryBreakdownDto
{
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public double Percentage { get; set; }

}

using System;

namespace FinSenseAPI.DTOs.EMI;

public class EMIRequestDto
{
    public decimal Salary { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal LoanAmount { get; set; }
    public int TenureMonths { get; set; }
    public double InterestRatePerAnnum { get; set; }
    public Guid? SessionId { get; set; }

}

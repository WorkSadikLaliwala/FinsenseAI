using System;

namespace FinSenseAPI.DTOs.EMI;

public class EMIResponseDto
{
    public decimal EMIAmount { get; set; }
    public string Status { get; set; }
    public string Reason { get; set; }
    public string Recommendation { get; set; }
    public decimal SafeEMILimit { get; set; }
    public decimal DisposableIncome { get; set; }
    public double EMIAsPercentOfSalary { get; set; }

}

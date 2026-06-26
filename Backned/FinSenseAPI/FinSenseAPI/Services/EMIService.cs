using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Services.Interfaces;
using FinSenseAPI.DTOs.EMI;
using FinSenseAPI.Repositories.Interfaces;

namespace FinSenseAPI.Services;

public class EMIService : IEMIService
{
    private readonly ITransactionRepository _txRepo;

    public EMIService(ITransactionRepository txRepo)
    {
        _txRepo = txRepo;
    }

    public async Task<EMIResponseDto> CheckAffordabilityAsync(EMIRequestDto dto, CancellationToken ct)
    {
        decimal actualMonthlyExpenses = dto.MonthlyExpenses;

        //if (dto.SessionId.HasValue)
        //{
        //    var txs = await _txRepo.GetBySessionIdAsync(dto.SessionId.Value, ct);
        //    var debits = txs
        //        .Where(t => !string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase))
        //        .ToList();

        //    if (debits.Any())
        //        actualMonthlyExpenses = debits.Sum(t => t.Amount);
        //    // else keep dto.MonthlyExpenses as fallback
        //}

        var salary = dto.Salary;
        var disposable = salary - dto.MonthlyExpenses;
        if (disposable < 0) disposable = 0m;

        // EMI calculation
        var r = (decimal)dto.InterestRatePerAnnum / 12m / 100m;
        var n = dto.TenureMonths;
        decimal emi = 0m;

        if (n > 0 && r > 0)
        {
            var pow = (decimal)Math.Pow((double)(1 + r), n);
            emi = dto.LoanAmount * r * pow / (pow - 1);
        }
        else if (n > 0)
        {
            emi = dto.LoanAmount / n;
        }

        emi = Math.Round(emi, 2);

        // Safe limit = 40% of disposable income
        var safeLimit = Math.Round(disposable * 0.4m, 2);

        var emiPercent = salary > 0 ? Math.Round((double)(emi / salary * 100m), 2) : 0.0;

        var status = emiPercent switch
        {
            < 20 => "Safe",
            < 40 => "Risky",
            _ => "Danger"
        };

        var reason = status switch
        {
            "Safe" => "EMI is within a comfortable range of your salary.",
            "Risky" => $"EMI will consume {emiPercent}% of your salary. This is manageable but leaves little room for savings.",
            "Danger" => $"EMI will consume {emiPercent}% of your salary. This is too high and puts you at financial risk.",
            _ => string.Empty
        };

        var recommendation = status switch
        {
            "Safe" => "Proceed with confidence. You can comfortably afford this EMI.",
            "Risky" => $"Proceed with caution. Consider reducing loan amount or extending tenure to bring EMI under ₹{safeLimit:N0}.",
            "Danger" => $"Not recommended. Reduce loan amount significantly or extend tenure. Target EMI under ₹{safeLimit:N0}.",
            _ => string.Empty
        };

        return new EMIResponseDto
        {
            EMIAmount = emi,
            Status = status,
            Reason = reason,
            Recommendation = recommendation,
            SafeEMILimit = safeLimit,
            DisposableIncome = disposable,
            EMIAsPercentOfSalary = emiPercent
        };
    }
}

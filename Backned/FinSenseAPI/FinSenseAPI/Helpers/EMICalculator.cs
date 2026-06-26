using System;

namespace FinSenseAPI.Helpers;

public static class EMICalculator
{
    // principal: P, annualRate in percent (e.g., 12.5), tenureMonths: n
    public static decimal Calculate(decimal principal, double annualRate, int tenureMonths)
    {
        if (tenureMonths <= 0) return 0m;

        var monthlyRate = (decimal)(annualRate / 12.0 / 100.0);
        var P = principal;
        var r = monthlyRate;
        var n = tenureMonths;

        if (r == 0)
            return Math.Round(P / n, 2);

        var numerator = P * r * (decimal)Math.Pow((double)(1 + r), n);
        var denominator = (decimal)(Math.Pow((double)(1 + r), n) - 1);
        var emi = numerator / denominator;
        return Math.Round(emi, 2);
    }

    public static decimal GetSafeEMILimit(decimal salary)
    {
        return Math.Round(salary * 0.40m, 2);
    }
}

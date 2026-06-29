using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FinSenseAPI.Helpers;

public static class CsvParser
{
    public class ParsedTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
    }

    // Very small, permissive CSV parser supporting common date formats.
    public static async Task<List<ParsedTransaction>> ParseAsync(IFormFile file)
    {
        var results = new List<ParsedTransaction>();
        if (file == null) return results;

        using var stream = file.OpenReadStream();
        // Use a BOM-aware UTF8 reader so files with a UTF-8 BOM are handled correctly (header won't contain \uFEFF)
        using var reader = new StreamReader(
    stream,
    Encoding.UTF8,
    detectEncodingFromByteOrderMarks: true);
        string? line;
        var formats = new[] { "dd/MM/yyyy", "dd/MM/yy", "dd-MM-yyyy", "dd-MM-yy", "MM/dd/yyyy", "MM-dd-yyyy" };

        // Skip header if present
        var first = await reader.ReadLineAsync();
        if (first == null) return results;
        if (!first.Contains("date", StringComparison.OrdinalIgnoreCase))
        {
            // first line is data
            line = first;
        }
        else
        {
            line = await reader.ReadLineAsync();
        }

        //while (line != null)
        //{
        //    var parts = line.Split(',');
        //    if (parts.Length >= 3)
        //    {
        //        // try parse date from first column
        //        if (DateTime.TryParseExact(parts[0].Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
        //            || DateTime.TryParse(parts[0].Trim(), out dt))
        //        {
        //            var desc = parts[1].Trim();
        //            var amtText = parts[2].Trim().Replace("\u20B9", string.Empty).Replace("$", string.Empty);
        //            // Handle Indian number grouping (1,23,456.00) by removing commas before parsing
        //            var cleaned = amtText.Replace(",", string.Empty).Replace("\u00A0", string.Empty).Trim();
        //            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
        //            {
        //                var type = amt < 0 ? "Debit" : "Credit";
        //                results.Add(new ParsedTransaction { Date = dt, Description = desc, Amount = Math.Abs(amt), Type = type });
        //            }
        //        }
        //    }
        //
        //    line = await reader.ReadLineAsync();
        //}
        while (line != null)
        {
            var parts = line.Split(',');

            // Need at least 4 columns: Date, Description, Debit, Credit
            if (parts.Length >= 4)
            {
                if (DateTime.TryParseExact(parts[0].Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                    || DateTime.TryParse(parts[0].Trim(), new CultureInfo("en-IN"), DateTimeStyles.None, out dt))
                {
                    var desc = parts[1].Trim();

                    // Skip opening/closing balance rows
                    if (desc.Contains("Opening Balance", StringComparison.OrdinalIgnoreCase) ||
                        desc.Contains("Closing Balance", StringComparison.OrdinalIgnoreCase))
                    {
                        line = await reader.ReadLineAsync();
                        continue;
                    }

                    var debitText = parts[2].Trim().Replace(",", "").Replace("₹", "").Replace("$", "").Trim();
                    var creditText = parts[3].Trim().Replace(",", "").Replace("₹", "").Replace("$", "").Trim();

                    decimal debit = 0, credit = 0;
                    decimal.TryParse(debitText, NumberStyles.Any, CultureInfo.InvariantCulture, out debit);
                    decimal.TryParse(creditText, NumberStyles.Any, CultureInfo.InvariantCulture, out credit);

                    if (debit > 0)
                    {
                        results.Add(new ParsedTransaction { Date = dt, Description = desc, Amount = debit, Type = "Debit" });
                    }
                    else if (credit > 0)
                    {
                        results.Add(new ParsedTransaction { Date = dt, Description = desc, Amount = credit, Type = "Credit" });
                    }
                    // if both zero — skip row (empty/balance row)
                }
            }

            line = await reader.ReadLineAsync();
        }
        return results;
    }
}

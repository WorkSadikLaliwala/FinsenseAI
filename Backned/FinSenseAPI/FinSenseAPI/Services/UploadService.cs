using FinSenseAPI.DTOs.Analytics;
using FinSenseAPI.DTOs.Upload;
using FinSenseAPI.Helpers;
using FinSenseAPI.Models;
using FinSenseAPI.Prompts;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static FinSenseAPI.Services.UploadService;

namespace FinSenseAPI.Services;

public class UploadService : IUploadService
{
    private readonly IUploadSessionRepository _sessionRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IClaudeService _claude;

    public UploadService(IUploadSessionRepository sessionRepo, ITransactionRepository transactionRepo, IClaudeService claude)
    {
        _sessionRepo = sessionRepo;
        _transactionRepo = transactionRepo;
        _claude = claude;
    }

    public async Task<UploadResponseDto> ProcessUploadAsync(IFormFile file, Guid? userId, CancellationToken ct)
    {
        // Read file bytes once into a memory stream so we can both hash and parse without re-reading disk.
        string? fileHash = null;
        MemoryStream? ms = null;
        if (file != null)
        {
            ms = new System.IO.MemoryStream();
            await file.CopyToAsync(ms, ct);
            var bytes = ms.ToArray();
            fileHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
            ms.Position = 0; // reset for parsing
        }

        if (fileHash != null)
        {
            var existing = await _sessionRepo.GetByHashAndUserAsync(fileHash, userId, ct);
            if (existing != null)
            {
                // Load cached transactions and build full response
                var saved = await _transactionRepo.GetBySessionIdAsync(existing.Id, ct);
                var cachedTxDtos = saved.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Date = t.Date,
                    Description = t.Description,
                    Amount = t.Amount,
                    Type = t.Type,
                    Category = t.Category
                }).ToList();

                var summary = BuildSummary(saved);

                return new UploadResponseDto
                {
                    SessionId = existing.Id,
                    Transactions = cachedTxDtos,
                    Summary = summary,
                    IsGuest = existing.IsGuest
                };
            }
        }

        // create new session record
        var session = new UploadSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = file?.FileName ?? string.Empty,
            UploadedAt = DateTime.UtcNow,
            IsGuest = userId == null,
            FileHash = fileHash
        };

        if (session.IsGuest)
        {
            session.ExpiresAt = DateTime.UtcNow.AddDays(1);
        }

        await _sessionRepo.CreateAsync(session, ct);

        // If there is no file or it could not be read, return empty response (session recorded)
        if (ms == null)
        {
            return new UploadResponseDto
            {
                SessionId = session.Id,
                Transactions = new List<TransactionDto>(),
                Summary = null,
                IsGuest = session.IsGuest
            };
        }

        // Build a FormFile over the in-memory stream so CsvParser can consume it without re-reading disk

        var inMemoryFormFile = new FormFile(ms, 0, ms.Length, file?.Name ?? "file", file?.FileName ?? "upload.csv");

        // Parse CSV into lightweight parsed transactions
        var parsed = await Helpers.CsvParser.ParseAsync(inMemoryFormFile);

        var transactions = new List<Transaction>();

        foreach (var p in parsed)
        {
            transactions.Add(new Transaction
            {
                SessionId = session.Id,
                Date = (p.Date.Kind == DateTimeKind.Utc) ? p.Date : (p.Date.Kind == DateTimeKind.Local) ? p.Date.ToUniversalTime() : DateTime.SpecifyKind(p.Date, DateTimeKind.Utc),
                Description = p.Description,
                Amount = p.Amount,
                Type = p.Type,
                Category = "Others" // filled after categorization
            });
        }

        // If no transactions parsed, return empty result but keep session
        if (transactions.Count == 0)
        {
            return new UploadResponseDto
            {
                SessionId = session.Id,
                Transactions = new List<TransactionDto>(),
                Summary = null,
                IsGuest = session.IsGuest
            };
        }

       


        try
        {
            
            var descriptions = transactions
    .Select(t => t.Description)
    .Distinct()                    // deduplicate same merchant
    .ToList();
            var descriptionsJson = JsonSerializer.Serialize(descriptions);

            var prompt = ClaudePrompts.TransactionCategorizer(descriptionsJson);
            var claudeResponse = await _claude.CallAsync(prompt, "", 3300, ct);
            Console.WriteLine("GROQ RAW: " + claudeResponse);
            Console.WriteLine("GROQ RAW LENGTH: " + claudeResponse?.Length);
            Console.WriteLine("GROQ RAW END: " + claudeResponse?.Substring(Math.Max(0, (claudeResponse?.Length ?? 0) - 100)));
            if (JsonHelper.TryDeserialize<List<CategoryResult>>(claudeResponse, out var result) && result != null)
            {
                
                var categoryMap = result
                    .GroupBy(r => r.Description!)
                    .ToDictionary(g => g.Key, g => g.First().Category ?? "Others");

                // Map back to all transactions (including duplicates)
                foreach (var tx in transactions)
                {
                    tx.Category = categoryMap.TryGetValue(tx.Description!, out var cat)
                        ? cat
                        : "Others";
                }

            }
            else
            {
                foreach (var t in transactions) t.Category = "Others";
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Console.WriteLine("CATEGORIZATION ERROR: " + ex.Message);
            Console.WriteLine("STACK: " + ex.StackTrace);
            foreach (var t in transactions) t.Category = "Others";
        }
        // Persist transactions to DB in bulk
        await _transactionRepo.BulkInsertAsync(transactions, ct);

        // Re-load saved transactions to obtain database-generated ids (and any defaults)
        var savedTransactions = (await _transactionRepo.GetBySessionIdAsync(session.Id, ct)).ToList();

        // Build DTOs and summary
        var txDtos = savedTransactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            Date = t.Date,
            Description = t.Description,
            Amount = t.Amount,
            Type = t.Type,
            Category = t.Category
        }).ToList();

        var summaryDto = BuildSummary(savedTransactions);

        return new UploadResponseDto
        {
            SessionId = session.Id,
            Transactions = txDtos,
            Summary = summaryDto,
            IsGuest = session.IsGuest
        };
    }
    public async Task<System.Collections.Generic.IEnumerable<FinSenseAPI.DTOs.Upload.UploadHistoryItemDto>> GetHistoryAsync(Guid userId, CancellationToken ct)
    {
        var sessions = await _sessionRepo.GetByUserIdAsync(userId, ct);
        var list = sessions.Select(s => new FinSenseAPI.DTOs.Upload.UploadHistoryItemDto
        {
            UserId = s.UserId,
            SessionId = s.Id,
            FileName = s.FileName,
            UploadedAt = s.UploadedAt,
            TransactionCount = s.Transactions?.Count ?? 0
        }).ToList();

        return list;
    }

    private SpendingSummaryDto? BuildSummary(IEnumerable<Transaction> transactions)
    {
        if (transactions == null) return null;
        var txList = transactions.ToList();
        if (txList.Count == 0) return null;

        decimal totalIncome = 0m;
        decimal totalRefunds = 0m;
        decimal totalSpending = 0m;

        foreach (var t in txList)
        {
            if (string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase))
            {
                // treat descriptions containing "refund" as refunds
                //if (!string.IsNullOrEmpty(t.Description) && t.Description.Contains("refund", StringComparison.OrdinalIgnoreCase))
                //    totalRefunds += t.Amount;
                //else
                //    totalIncome += t.Amount;
                var refundKeywords = new[] { "refund", "reversal", "cashback", "transfer received", "upi cr", "neft cr" };

                if (!string.IsNullOrEmpty(t.Description) &&
                    refundKeywords.Any(k => t.Description.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    totalRefunds += t.Amount;
                else
                    totalIncome += t.Amount;
            }
            else if (string.Equals(t.Type, "Debit", StringComparison.OrdinalIgnoreCase))
            {
                totalSpending += t.Amount;
            }
        }

        // Only group debit transactions for category breakdown
        var byCategory = txList
            .Where(t => string.Equals(t.Type, "Debit", StringComparison.OrdinalIgnoreCase))
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? "Other" : t.Category)
            .Select(g => new CategoryBreakdownDto
            {
                Category = g.Key,
                Amount = g.Sum(x => x.Amount),
                Percentage = 0.0
            }).ToList();

        // compute percentages relative to totalSpending (if zero, base on totalIncome+refunds to avoid divide by zero)
        var denom = totalSpending > 0 ? totalSpending : (totalIncome + totalRefunds);
        if (denom == 0) denom = 1; // avoid div by zero; percentages will be meaningless but safe

        foreach (var c in byCategory)
        {
            c.Percentage = Math.Round((double)(c.Amount / denom) * 100.0, 2);
        }

        var topTx = txList
    .Where(t => string.Equals(t.Type, "Debit", StringComparison.OrdinalIgnoreCase))
    .OrderByDescending(t => t.Amount)
    .Take(5)
            .Select(t => new TransactionDto { Id = t.Id, Date = t.Date, Description = t.Description, Amount = t.Amount, Type = t.Type, Category = t.Category }).ToList();

        return new SpendingSummaryDto
        {
            TotalIncome = totalIncome,
            TotalRefunds = totalRefunds,
            TotalSpending = totalSpending,
            NetSavings = totalIncome + totalRefunds - totalSpending,
            ByCategory = byCategory,
            TopTransactions = topTx
        };
    }

    public class CategoryResult
    {
        public string? Description { get; set; }
        public string? Category { get; set; }
    }
}

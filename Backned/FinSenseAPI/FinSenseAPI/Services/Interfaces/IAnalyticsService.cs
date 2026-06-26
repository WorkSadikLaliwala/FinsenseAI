using System;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.DTOs.Analytics;

namespace FinSenseAPI.Services.Interfaces;

public interface IAnalyticsService
{
    Task<SpendingSummaryDto> GetSummaryAsync(Guid sessionId, CancellationToken ct);
}

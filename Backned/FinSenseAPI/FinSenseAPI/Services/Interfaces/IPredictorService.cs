using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.DTOs.Predictor;

namespace FinSenseAPI.Services.Interfaces;

public interface IPredictorService
{
    Task<PredictorResponseDto> PredictMonthEndAsync(PredictorRequestDto dto, CancellationToken ct);
}

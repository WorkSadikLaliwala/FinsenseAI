using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.DTOs.EMI;

namespace FinSenseAPI.Services.Interfaces;

public interface IEMIService
{
    Task<EMIResponseDto> CheckAffordabilityAsync(EMIRequestDto dto, CancellationToken ct);
}

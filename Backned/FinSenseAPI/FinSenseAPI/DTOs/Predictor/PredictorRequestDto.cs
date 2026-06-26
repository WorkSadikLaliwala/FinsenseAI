using System;

namespace FinSenseAPI.DTOs.Predictor;

public class PredictorRequestDto
{
    public Guid SessionId { get; set; }
    public DateTime CurrentDate { get; set; }

}

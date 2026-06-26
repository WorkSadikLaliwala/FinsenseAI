using Microsoft.AspNetCore.Http;

namespace FinSenseAPI.DTOs.Upload;

public class UploadRequestDto
{
    // Swashbuckle handles file uploads cleanly when the IFormFile is a property on a [FromForm] model
    public required IFormFile File { get; set; }
}

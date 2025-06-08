using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using System.IO;
using System.Threading.Tasks;
using ChatGPTcorpus.Services;
using System;
using System.Text.Json;

namespace ChatGPTcorpus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ZipService _zipService;
        private readonly ImportService _importService;

        public UploadController(ZipService zipService, ImportService importService)
        {
            _zipService = zipService;
            _importService = importService;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string metadata)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded." });

            // Save to a temp location
            var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Extract ZIP to the backend's Data/unzipped/{userId} directory
            var userId = Path.GetFileNameWithoutExtension(file.FileName); // crude userId from filename
            var extractPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "unzipped", userId);
            try
            {
                _zipService.ExtractZip(tempPath, extractPath);
                // Parse metadata if provided
                Dictionary<string, object>? metadataDict = null;
                if (!string.IsNullOrEmpty(metadata))
                {
                    metadataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata) ?? new Dictionary<string, object>();
                }
                // Import conversations and save to database
                var conversations = await _importService.ImportConversationsAsync(userId, metadataDict ?? new Dictionary<string, object>());
                return Ok(new { message = "File uploaded and conversations loaded!", fileName = file.FileName, conversations = conversations.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Failed to process uploaded file: {ex.Message}" });
            }
            finally
            {
                // Optionally delete the temp file
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }
            }
        }
    }
} 
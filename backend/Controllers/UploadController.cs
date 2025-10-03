using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using System.IO;
using System.Threading.Tasks;
using ChatGPTcorpus.Services;
using System;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace ChatGPTcorpus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ZipService _zipService;
        private readonly ImportService _importService;
        private readonly IHostEnvironment _hostEnvironment;

        public UploadController(ZipService zipService, ImportService importService, IHostEnvironment hostEnvironment)
        {
            _zipService = zipService;
            _importService = importService;
            _hostEnvironment = hostEnvironment;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string metadata)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded." });

            // Save to a temp location
            var originalFileName = Path.GetFileName(file.FileName);
            var tempPath = Path.Combine(Path.GetTempPath(), originalFileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Extract ZIP to the backend's Data/unzipped/{userId} directory
            var userId = Path.GetFileNameWithoutExtension(originalFileName); // crude userId from filename
            var extractPath = Path.Combine(_hostEnvironment.ContentRootPath, "Data", "unzipped", userId);
            var rawZipDir = Path.Combine(_hostEnvironment.ContentRootPath, "Data", "RawZips", userId);
            var importBatchId = Guid.NewGuid().ToString("N");
            try
            {
                Directory.CreateDirectory(rawZipDir);

                var preservedZipPath = Path.Combine(rawZipDir, $"{importBatchId}_{originalFileName}");
                if (!System.IO.File.Exists(preservedZipPath))
                {
                    System.IO.File.Copy(tempPath, preservedZipPath);
                }

                _zipService.ExtractZip(tempPath, extractPath);
                // Parse metadata if provided
                Dictionary<string, object>? metadataDict = null;
                if (!string.IsNullOrEmpty(metadata))
                {
                    metadataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata) ?? new Dictionary<string, object>();
                }
                // Import conversations and save to database
                var conversations = await _importService.ImportConversationsAsync(userId, importBatchId, metadataDict ?? new Dictionary<string, object>());
                return Ok(new { message = "File uploaded and conversations loaded!", fileName = file.FileName, conversations = conversations.Count, uploadId = importBatchId });
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

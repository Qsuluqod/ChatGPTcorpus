using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;
using Newtonsoft.Json.Linq;

// Custom factory to set correct content root
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        builder.UseContentRoot(projectDir);
        return base.CreateHost(builder);
    }
}

namespace ChatGPTcorpus.Tests
{
    public class UploadAndImportTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly string _testZipPath = Path.Combine("Data", "zip", "data_test.zip");
        private readonly string _rawConvDir = Path.Combine("Data", "RawConversations");

        public UploadAndImportTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task UploadZip_ExtractsAndImportsCorrectly()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userId = "data_test";
            var rawConvUserDir = Path.Combine(_rawConvDir, userId);
            var rawConvFile = Path.Combine(rawConvUserDir, "conversations.json");
            if (Directory.Exists(rawConvUserDir)) Directory.Delete(rawConvUserDir, true);

            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(_testZipPath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
            form.Add(fileContent, "file", Path.GetFileName(_testZipPath));

            // Act
            var response = await client.PostAsync("/api/upload", form);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("conversations loaded", responseString);
            Assert.True(File.Exists(rawConvFile));

            // Check that the file is not in unzipped anymore
            var unzippedDir = Path.Combine("Data", "unzipped", userId);
            Assert.False(Directory.Exists(unzippedDir));

            // Check that the imported conversations are correct
            var stats = await GetStatsAsync(client);
            Assert.True(stats["messages"]?.Value<int>() > 0);
            Assert.True(stats["uploads"]?.Value<int>() > 0);
        }

        [Fact]
        public async Task UploadZip_CanBeRepeatedWithoutLosingMessages()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userId = "data_test";
            var rawConvUserDir = Path.Combine(_rawConvDir, userId);
            if (Directory.Exists(rawConvUserDir)) Directory.Delete(rawConvUserDir, true);

            var baselineStats = await GetStatsAsync(client);
            var baselineMessages = baselineStats["messages"]?.Value<int>() ?? 0;
            var baselineUploads = baselineStats["uploads"]?.Value<int>() ?? 0;

            // Act
            var firstUploadStats = await UploadZipAndGetStatsAsync(client);
            var secondUploadStats = await UploadZipAndGetStatsAsync(client);

            // Assert
            Assert.True(firstUploadStats.messages > baselineMessages);
            Assert.Equal(firstUploadStats.messages, secondUploadStats.messages);
            Assert.Equal(baselineUploads + 1, firstUploadStats.uploads);
            Assert.Equal(baselineUploads + 2, secondUploadStats.uploads);
            Assert.True(File.Exists(Path.Combine(rawConvUserDir, "conversations.json")));
        }

        private static async Task<JObject> GetStatsAsync(HttpClient client)
        {
            var statsResponse = await client.GetAsync("/api/search/stats");
            statsResponse.EnsureSuccessStatusCode();
            var statsJson = await statsResponse.Content.ReadAsStringAsync();
            return JObject.Parse(statsJson);
        }

        private async Task<(int messages, int uploads)> UploadZipAndGetStatsAsync(HttpClient client)
        {
            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(_testZipPath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
            form.Add(fileContent, "file", Path.GetFileName(_testZipPath));

            var response = await client.PostAsync("/api/upload", form);
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode, $"Upload failed: {responseBody}");

            var stats = await GetStatsAsync(client);
            return (
                stats["messages"]?.Value<int>() ?? 0,
                stats["uploads"]?.Value<int>() ?? 0
            );
        }
    }
}

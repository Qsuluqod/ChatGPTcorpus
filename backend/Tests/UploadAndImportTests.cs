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
        builder.UseContentRoot(Path.GetFullPath(Path.Combine("..", "backend")));
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
            var statsResponse = await client.GetAsync("/api/search/stats");
            var statsJson = await statsResponse.Content.ReadAsStringAsync();
            var stats = JObject.Parse(statsJson);
            Assert.True((int)stats["contributions"] > 0);
            Assert.True((int)stats["messages"] > 0);
        }
    }
} 
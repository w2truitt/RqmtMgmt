using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using backend;

namespace backend.ApiTests
{
    public abstract class BaseApiTest : IClassFixture<TestWebApplicationFactory<Program>>
    {
        protected readonly HttpClient _client;
        protected readonly JsonSerializerOptions _jsonOptions;

        protected BaseApiTest(TestWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }
    }
}
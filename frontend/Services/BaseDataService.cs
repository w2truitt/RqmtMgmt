using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace frontend.Services
{
    /// <summary>
    /// Base class for all data services providing common HTTP client operations and centralized configuration.
    /// Eliminates code duplication and ensures consistent behavior across all data services.
    /// </summary>
    public abstract class BaseDataService
    {
        protected readonly HttpClient _http;
        protected readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the BaseDataService with the specified HTTP client.
        /// Provides centralized JSON serialization configuration for consistency.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        protected BaseDataService(HttpClient http)
        {
            _http = http;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        /// <summary>
        /// Performs a GET request and deserializes the response to the specified type.
        /// Provides consistent error handling across all data services.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="endpoint">The API endpoint to call.</param>
        /// <returns>The deserialized response object, or null if the request fails.</returns>
        protected async Task<T?> GetAsync<T>(string endpoint) where T : class
        {
            try
            {
                return await _http.GetFromJsonAsync<T>(endpoint, _jsonOptions);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>
        /// Performs a GET request and deserializes the response to a list of the specified type.
        /// Returns an empty list if the request fails for consistent behavior.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="endpoint">The API endpoint to call.</param>
        /// <returns>A list of items, or an empty list if the request fails.</returns>
        protected async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<T>>(endpoint, _jsonOptions) ?? new();
            }
            catch (HttpRequestException)
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Performs a POST request with the specified data and deserializes the response.
        /// Provides consistent error handling for create operations.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request data.</typeparam>
        /// <typeparam name="TResponse">The type of the response data.</typeparam>
        /// <param name="endpoint">The API endpoint to call.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <returns>The deserialized response object, or null if the request fails.</returns>
        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data) 
            where TResponse : class
        {
            try
            {
                var response = await _http.PostAsJsonAsync(endpoint, data, _jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                }
                return null;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>
        /// Performs a POST request with the specified data without expecting a response body.
        /// Useful for operations that only need to know success/failure.
        /// </summary>
        /// <typeparam name="T">The type of the request data.</typeparam>
        /// <param name="endpoint">The API endpoint to call.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <returns>True if the request was successful, false otherwise.</returns>
        protected async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(endpoint, data, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Performs a PUT request with the specified data and deserializes the response.
        /// Provides consistent error handling for update operations.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request data.</typeparam>
        /// <typeparam name="TResponse">The type of the response data.</typeparam>
        /// <param name="endpoint">The API endpoint to call.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <returns>The deserialized response object, or null if the request fails.</returns>
        protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data) 
            where TResponse : class
        {
            try
            {
                var response = await _http.PutAsJsonAsync(endpoint, data, _jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                }
                return null;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>
        /// Performs a PUT request with the specified data without expecting a response body.
        /// Useful for update operations that only need to know success/failure.
        /// </summary>
        /// <typeparam name="T">The type of the request data.</typeparam>
        /// <param name="endpoint">The API endpoint to call.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <returns>True if the request was successful, false otherwise.</returns>
        protected async Task<bool> PutAsync<T>(string endpoint, T data)
        {
            try
            {
                var response = await _http.PutAsJsonAsync(endpoint, data, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Performs a DELETE request to the specified endpoint.
        /// Provides consistent error handling for delete operations.
        /// </summary>
        /// <param name="endpoint">The API endpoint to call.</param>
        /// <returns>True if the request was successful, false otherwise.</returns>
        protected async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _http.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Builds a query string from the provided parameters.
        /// Handles URL encoding and null value filtering automatically.
        /// </summary>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>A properly formatted query string starting with '?' or empty string if no parameters.</returns>
        protected static string BuildQueryString(Dictionary<string, object?> parameters)
        {
            var validParams = parameters
                .Where(kvp => kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString()))
                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value!.ToString()!)}")
                .ToList();

            return validParams.Any() ? "?" + string.Join("&", validParams) : "";
        }
    }
}
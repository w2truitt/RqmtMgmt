namespace frontend.Models
{
    /// <summary>
    /// Generic result wrapper for service operations with success/failure state.
    /// </summary>
    public class ServiceResult<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public T? Data { get; init; }

        public static ServiceResult<T> SuccessResult(T data) => new()
        {
            Success = true,
            Data = data
        };

        public static ServiceResult<T> Failure(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}

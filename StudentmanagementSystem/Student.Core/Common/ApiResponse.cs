namespace Student.Core.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int StatusCode { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Request processed successfully", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string> { message },
                StatusCode = statusCode
            };
        }
    }
}

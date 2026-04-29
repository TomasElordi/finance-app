namespace api.DTOs;
public class ApiResponse<T>
{
    public required bool Success { get; set; }
    public T? Data { get; set;} 
    public string? Message { get; set; }

    public static ApiResponse<T> Ok(T data) => new ApiResponse<T> { Success = true, Data = data };
    public static ApiResponse<T> Fail(string message) => new ApiResponse<T> { Success = false, Message = message };
}
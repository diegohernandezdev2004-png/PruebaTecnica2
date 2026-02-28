namespace CursosAPI.Domain.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "") => new() { Data = data, Message = message, Success = true };
    public static ApiResponse<T> Error(string message) => new() { Message = message, Success = false };
}

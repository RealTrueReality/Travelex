namespace Travelex.Models;

public class ResultModel(bool isSuccess, string? message){
    public bool IsSuccess { get; } = isSuccess;
    public string? Message { get; } = message;


    public static ResultModel Success(string message) => new(true, message);
    public static ResultModel Success() => new(true,string.Empty);
    public static ResultModel Failure(string message) => new(false, message);
}
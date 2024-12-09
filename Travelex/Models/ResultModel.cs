namespace Travelex.Models;

public readonly struct ResultModel (bool IsSuccess, string Message){
    
    public static ResultModel Success(string message) => new(true, message);
    public static ResultModel Success() => new(true,string.Empty);
    public static ResultModel Failure(string message) => new(false, message);
}
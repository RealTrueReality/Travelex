using Travelex.Entities;

namespace Travelex.Models;

public class ResultDataModel<T>(bool isSuccess, string? message, T? data) {
    public bool IsSuccess { get; } = isSuccess;
    public string? Message { get; } = message;
    public T? Data { get; set; } = data;


    public static ResultDataModel<T> Success(T data) => new(true, string.Empty, data);
    public static ResultDataModel<T> Success(string message, T data) => new(true, message, data);
    public static ResultDataModel<T> Failure(T data) => new(false, string.Empty, data);
    public static ResultDataModel<T> Failure(string message) => new(false, message, default);
    public static ResultDataModel<T> Failure(string message, T data) => new(false, message, data);
}
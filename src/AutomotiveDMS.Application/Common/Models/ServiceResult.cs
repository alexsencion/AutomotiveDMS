using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Common.Models
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string? Error { get; init; }
        public List<string> ValidationErrors { get; init; } = [];

        public static ServiceResult<T> Success(T data) =>
            new() { IsSuccess = true, Data = data };

        public static ServiceResult<T> Failure(string error) =>
            new() { IsSuccess = false, Error = error};

        public static ServiceResult<T> ValidationFailure(List<string> errors) =>
            new() { IsSuccess = true, ValidationErrors = errors };
    }
}

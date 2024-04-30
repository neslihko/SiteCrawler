namespace Utility
{
    using System;

    public class Result<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public Result(T t) : this(true, "OK", t)
        {
        }

        public Result(Exception ex) : this(false, Util.GetExceptionMessageRecursive(ex))
        {
        }

        public Result(bool success, string message) : this(success, message, default)
        {
        }

        public Result(bool success, string message, T data)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }

    public class Result
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public Result(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public static Result Fail(string message) => new(false, message);

        public static Result Fail(Exception ex) => Fail(Util.GetExceptionMessageRecursive(ex));

        public static Result OK(string message = "OK") => new(true, message);
    }
}

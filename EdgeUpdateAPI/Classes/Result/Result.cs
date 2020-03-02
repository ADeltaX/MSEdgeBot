namespace EdgeUpdateAPI.Classes
{
    public class Result<T> : IResult<T>
    {

        public Result(bool success, T value)
        {
            Success = success;
            Value = value;
        }

        public Result(bool success, string message, ResultType result)
        {
            Success = success;
            ResultType = result;
            Message = message;
        }

        public Result(bool success, T value, ResultType result)
        {
            Success = success;
            Value = value;
            ResultType = result;
        }

        public bool Success { get; }
        public T Value { get; }
        public string Message { get; }
        public ResultType ResultType { get; }
    }
}

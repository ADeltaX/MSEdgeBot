namespace EdgeUpdateAPI.Classes
{
    public interface IResult<out T>
    {
        bool Success { get; }
        T Value { get; }
        string Message { get; }
        ResultType ResultType { get; }
    }
}

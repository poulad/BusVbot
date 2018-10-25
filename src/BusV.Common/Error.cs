namespace BusV
{
    public class Error
    {
        public string Code { get; }

        public string Message { get; }

        public Error(string code)
        {
            Code = code;
        }

        public Error(string code, string message)
            : this(code)
        {
            Message = message;
        }
    }
}

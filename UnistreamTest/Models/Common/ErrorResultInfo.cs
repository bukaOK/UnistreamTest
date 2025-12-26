namespace UnistreamTest.Models.Common
{
    public class ErrorResultInfo(ECommonErrorReasons code, string message)
    {
        public ECommonErrorReasons Code { get; } = code;

        public string Message { get; } = message;
    }
}

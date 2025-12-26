namespace UnistreamTest.Models.Common
{
    public class AppResult
    {
        public AppResult(bool success)
        {
            Success = success;
        }

        public AppResult(ErrorResultInfo? error)
        {
            Success = false;
            Error = error;
        }

        public bool Success { get; }

        public ErrorResultInfo? Error { get; }

        public static AppResult CreateFromDataResult<TData>(AppResult<TData> result)
        {
            return result.Success
                ? new AppResult(true)
                : new AppResult(result.Error);
        }

        public static AppResult CreateSuccess() => new(true);

        public static AppResult CreateError(ECommonErrorReasons reason, string message)
            => new(new ErrorResultInfo(reason, message));

        public static AppResult CreateValidationError(string message)
            => CreateError(ECommonErrorReasons.Validation, message);

        public static AppResult<TData> CreateSuccess<TData>(TData data) => new(data);

        public static AppResult<TData> CreateError<TData>(ECommonErrorReasons reason, string message)
        {
            return new AppResult<TData>(new ErrorResultInfo(reason, message));
        }

        public static AppResult CreateError(ErrorResultInfo errorResultInfo)
        {
            return new AppResult(errorResultInfo);
        }
    }

    public class AppResult<TData>
    {
        public AppResult(TData data)
        {
            Success = true;
            Data = data;
        }

        public AppResult(ErrorResultInfo? error)
        {
            Success = false;
            Error = error;
        }

        public AppResult(AppResult appResult)
        {
            Success = appResult.Success;
            Error = appResult.Error;
        }

        public bool Success { get; }

        public ErrorResultInfo? Error { get; }

        public TData? Data { get; }

        public static implicit operator AppResult<TData>(AppResult result)
        {
            return new AppResult<TData>(result);
        }

        public static implicit operator AppResult<TData>(ErrorResultInfo error)
        {
            return new AppResult<TData>(error);
        }

        public static implicit operator AppResult<TData>(TData result)
        {
            return new AppResult<TData>(result);
        }
    }
}

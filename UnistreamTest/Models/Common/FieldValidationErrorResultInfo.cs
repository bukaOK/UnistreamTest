namespace UnistreamTest.Models.Common
{
    public class FieldValidationErrorResultInfo : ErrorResultInfo
    {
        private readonly Dictionary<string, List<string>> _errors;

        public FieldValidationErrorResultInfo(): base(ECommonErrorReasons.Validation, "Проверьте правильность заполнения полей")
        {
            _errors = [];
        }

        public IReadOnlyDictionary<string, List<string>> Errors => _errors;

        public void AddError(string fieldName, string error)
        {
            if(!_errors.ContainsKey(fieldName))
                _errors[fieldName] = [];

            _errors[fieldName].Add(error);
        }
    }
}

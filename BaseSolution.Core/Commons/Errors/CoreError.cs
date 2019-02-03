using System.Collections.Generic;
using System.Linq;

namespace BaseSolution.Core.Commons.Errors
{
    public class CoreError
    {
        private readonly string _message;

        public int StatusCode { get; set; }

        public IDictionary<string, IEnumerable<string>> ValidationErrors { get; set; }

        public IEnumerable<string> Messages
        {
            get
            {
                if (ValidationErrors != null && ValidationErrors.Any())
                {
                    var listMessages = new List<string>();
                    foreach (var err in ValidationErrors)
                    {
                        if (err.Value != null && err.Value.Any())
                        {
                            listMessages.AddRange(err.Value);
                        }
                    }
                    return listMessages;
                }

                if (!string.IsNullOrEmpty(_message))
                {
                    return new List<string> { _message };
                }

                return new List<string>();
            }
        }

        public CoreError(int statusCode = 400)
        {
            ValidationErrors = new Dictionary<string, IEnumerable<string>>();
            StatusCode = statusCode;
        }

        public CoreError(string message, int statusCode = 400)
        {
            _message = message;
            StatusCode = statusCode;
        }

        public CoreError(IEnumerable<CoreValidationError> validationErrors, int statusCode = 400)
        {
            AddErrors(validationErrors);
            StatusCode = statusCode;
        }

        public void AddError(CoreValidationError validationError)
        {
            if (ValidationErrors == null)
            {
                ValidationErrors = new Dictionary<string, IEnumerable<string>>();
            }
            var fieldName = validationError.Field ?? "Generic";
            if (ValidationErrors.ContainsKey(fieldName))
            {
                var value = ValidationErrors[fieldName];
                var enumerable = value as string[] ?? value.ToArray();
                if (value != null && enumerable.Any())
                {
                    enumerable.Append(validationError.Message);
                }
                else
                {
                    ValidationErrors[fieldName] = new List<string> { validationError.Message };
                }
            }
            else
            {
                ValidationErrors[fieldName] = new List<string> { validationError.Message };
            }
        }

        public void AddErrors(IEnumerable<CoreValidationError> validationErrors)
        {
            var errors = validationErrors as IList<CoreValidationError> ?? validationErrors.ToList();
            if (!errors.Any()) return;
            foreach (var err in errors)
            {
                AddError(err);
            }
        }
    }
}

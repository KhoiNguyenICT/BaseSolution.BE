using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BaseSolution.Core.Commons.Errors
{
    public class CoreException : Exception
    {
        public CoreError Error { get; set; }

        public string SerializedErrors => JsonConvert.SerializeObject(Error, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

        public CoreException()
        {
        }

        public CoreException(string field, string message)
        {
            Error = new CoreError(new List<CoreValidationError> { new CoreValidationError(field, message) });
        }

        public CoreException(string message)
        {
            Error = new CoreError(new List<CoreValidationError> { new CoreValidationError(message) });
        }

        public CoreException(CoreValidationError validationError)
        {
            Error = new CoreError(new List<CoreValidationError> { validationError });
        }

        public CoreException(IEnumerable<CoreValidationError> validationErrors)
        {
            Error = new CoreError(validationErrors);
        }
    }
}

using BaseSolution.Core.Commons.Extensions;

namespace BaseSolution.Core.Commons.Errors
{
    public class CoreValidationError
    {
        public CoreValidationError(string field, string message)
        {
            Field = field?.ToCamelCasing();
            Message = message;
        }

        public CoreValidationError(string message)
        {
            Message = message;
        }

        public string Field { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return $"Field = {Field} | Message = {Message}";
        }
    }
}

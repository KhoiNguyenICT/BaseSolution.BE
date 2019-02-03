using System.Collections.Generic;
using BaseSolution.Core.Commons.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BaseSolution.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected virtual void ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = new List<CoreValidationError>();
                foreach (var item in ModelState)
                {
                    if (item.Value.ValidationState == ModelValidationState.Invalid)
                        validationErrors.Insert(0, new CoreValidationError(item.Key, item.Value.Errors[0].ErrorMessage));
                }
                throw new CoreException(validationErrors);
            }
        }
    }
}
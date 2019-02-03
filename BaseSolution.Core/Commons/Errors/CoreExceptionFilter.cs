using System;
using System.IO;
using BaseSolution.Core.Commons.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BaseSolution.Core.Commons.Errors
{
    public class CoreExceptionFilter : ExceptionFilterAttribute
    {
        private readonly IStringLocalizer _localizer;

        public CoreExceptionFilter(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        public override void OnException(ExceptionContext context)
        {
            CoreError dgmError;
            switch (context.Exception)
            {
                case CoreException exception:
                    var ex = exception;
                    context.Exception = null;
                    dgmError = ex.Error;
                    break;
                case UnauthorizedAccessException _:
                    dgmError = new CoreError("Unauthorized access", StatusCodes.Status401Unauthorized);
                    break;
                default:
                    var env = (IHostingEnvironment)context.HttpContext.RequestServices.GetService(typeof(IHostingEnvironment));
                    var msg = "An unhandled error occurred.";
                    string stack = null;
                    if (!env.IsProduction())
                    {
                        msg = context.Exception.Message;
                        stack = context.Exception.StackTrace;
                    }

                    if (context.Exception is InvalidDataException invalidDataException)
                    {
                        if (invalidDataException.Message.StartsWith("Multipart body length limit"))
                        {
                            msg = string.Format(_localizer["MultipartBodyLengthLimit_Message"], DefineValueList.FormOptionsMultipartBodyLengthLimit / 1048576);
                            stack = null;
                        }
                    }

                    if (context.Exception is Microsoft.AspNetCore.Server.Kestrel.Core.BadHttpRequestException badHttpRequestException)
                    {
                        if (badHttpRequestException.Message.StartsWith("Request body too large"))
                        {
                            msg = string.Format(_localizer["MultipartBodyLengthLimit_Message"], DefineValueList.FormOptionsMultipartBodyLengthLimit / 1048576);
                            stack = null;
                        }
                    }
                    dgmError = new CoreError($"{msg} {stack}".Trim());
                    break;
            }

            if (context.HttpContext.Request.Path.Value.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.HttpContext.Response.StatusCode = dgmError.StatusCode;
                context.Result = new JsonResult(dgmError, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }

            base.OnException(context);
        }
    }
}

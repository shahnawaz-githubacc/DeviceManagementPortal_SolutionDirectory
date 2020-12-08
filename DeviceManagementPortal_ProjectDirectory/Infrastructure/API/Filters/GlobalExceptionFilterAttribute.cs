using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure.API.Filters
{
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public GlobalExceptionFilterAttribute()
        {
            
        }

        public override void OnException(ExceptionContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<GlobalExceptionFilterAttribute>))
                            as ILogger<GlobalExceptionFilterAttribute>;

            logger.LogError("DeviceManagemenPortal : GlobalExceptionFilter : Start");
            logger.LogError($"DeviceManagemenPortal : GlobalExceptionFilter : Exception Type : {context.Exception.GetType().Name}");
            logger.LogError($"DeviceManagemenPortal : GlobalExceptionFilter : Request Path : {context.HttpContext.Request.Path}");
            logger.LogError($"DeviceManagemenPortal : GlobalExceptionFilter : Request Method : {context.HttpContext.Request.Method}");
            logger.LogError($"DeviceManagemenPortal : GlobalExceptionFilter : Request Content Type : {context.HttpContext.Request.ContentType}");
            logger.LogError($"DeviceManagemenPortal : GlobalExceptionFilter : Model State Is Valid : {context.ModelState?.IsValid}");

            if (context.HttpContext.Request.RouteValues?.Count > 0)
            {
                foreach (var routeValue in context.HttpContext.Request.RouteValues) 
                {
                    logger.LogError($"DeviceManagemenPortal : GlobalExceptionFilter : Route Key : {routeValue.Key}, Route Value : {routeValue.Value}.");
                }
            }
            
            logger.LogError("DeviceManagemenPortal : GlobalExceptionFilter : Request Method : {0}", context.HttpContext.Request.QueryString.HasValue ? context.HttpContext.Request.QueryString.Value : string.Empty);
            logger.LogError($"DeviceManagemenPortal : GlobalExceptionFilter : Exception Message : {context.Exception.Message}");
            logger.LogError($"DeviceManagemenPortal : GlobalExceptionFilter : Exception Stack Trace : {context.Exception.StackTrace}");

            if (context.Exception is NotImplementedException)
            {
                context.Result = BuildActionResult("Method not implemented.", StatusCodes.Status501NotImplemented);
            }
            else if (context.Exception is ArgumentOutOfRangeException)
            {
                context.Result = BuildActionResult("Bad Request", StatusCodes.Status400BadRequest);
            }
            else if (context.Exception is DBConcurrencyException)
            {
                context.Result = BuildActionResult("Conflict occured in Database while saving data. Please try again later.", StatusCodes.Status409Conflict);
            }
            else
            {
                // Generic exception.
                context.Result = BuildActionResult("Internal Server Error", StatusCodes.Status500InternalServerError);
            }
            context.ExceptionHandled = true;
            logger.LogError("DeviceManagemenPortal : GlobalExceptionFilter : End");
        }

        private IActionResult BuildActionResult(string message, int statusCode)
        {
            return new ObjectResult(message)
            {
                StatusCode = statusCode
            };
        }
    }
}

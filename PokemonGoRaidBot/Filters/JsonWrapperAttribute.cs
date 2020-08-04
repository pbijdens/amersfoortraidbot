using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RaidBot.Filters
{
    public class JsonWrapperAttribute : ActionFilterAttribute
    {
        private ILogger _logger;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _logger = (context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(GetType().Name);

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                _logger.LogError($"Exception in {filterContext.Controller?.GetType().Name} ({filterContext.HttpContext?.Request}): {filterContext.Exception.GetType().Name} {filterContext.Exception.Message} {filterContext.Exception.StackTrace}");

                filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                filterContext.HttpContext.Response.ContentType = "application/json";
                filterContext.ExceptionHandled = true;
                filterContext.Result = new JsonResult(new
                {
                    success = false,
                    error = (null != filterContext.Exception) ? $"{filterContext.Exception.GetType().Name} {filterContext.Exception.Message} {filterContext.Exception.StackTrace}" : "null",
                    message = filterContext?.Exception?.Message
                },
                new Newtonsoft.Json.JsonSerializerSettings
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented
                });
            }

            base.OnActionExecuted(filterContext);
        }
    }
}

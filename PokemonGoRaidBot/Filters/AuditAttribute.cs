using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace RaidBot.Filters
{
    public class AuditAttribute : ActionFilterAttribute
    {
        public const string StopWatchItemKey = "$AuditAttribute.OnActionExecuting()";

        public static ConcurrentDictionary<string, Stopwatch> _inProgressOperations = new ConcurrentDictionary<string, Stopwatch>();

        private ILogger _logger;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _logger = (context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(GetType().Name);

            string paramStr = string.Join(", ", context.ActionArguments.Select(x => $"{x.Key}={Newtonsoft.Json.JsonConvert.SerializeObject(x.Value)}"));
            string details = $"{context.Controller.GetType().Name}::{context.ActionDescriptor.DisplayName}({paramStr})";

            string id = $"{Guid.NewGuid()}";
            context.HttpContext.Items[StopWatchItemKey] = id;
            _inProgressOperations[id] = new Stopwatch();
            _inProgressOperations[id].Start();

            var request = context.HttpContext.Request;
            var url = string.Concat(request.PathBase.ToUriComponent(), request.Path.ToUriComponent(), request.QueryString.ToUriComponent());

            AuditBeginRequest(context.HttpContext?.User, id, context.Controller?.GetType().Name, context.ActionDescriptor.DisplayName, Newtonsoft.Json.JsonConvert.SerializeObject(context.ActionArguments), url);

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Items.ContainsKey(StopWatchItemKey))
            {
                String id;
                Stopwatch sw;
                if (!string.IsNullOrWhiteSpace(id = context.HttpContext.Items[StopWatchItemKey] as string) && ((sw = _inProgressOperations[id]) != null))
                {
                    sw.Stop();
                    AuditEndRequest(id, sw.ElapsedMilliseconds);
                }
            }

            base.OnActionExecuted(context);
        }

        private void AuditBeginRequest(ClaimsPrincipal user, string id, string controller, string action, string actionParameters, string url)
        {
            _logger.LogInformation($"\"Start\", \"{id}\", \"{url}\", \"{GetUser(user)}\", \"{controller}\", \"{action}\", \"{EscapeJson(actionParameters)}\"");
        }

        private void AuditEndRequest(string id, long callDurationInMilliseconds)
        {
            _logger.LogInformation($"\"End\", \"{id}\", {callDurationInMilliseconds}");
        }

        private string EscapeJson(string json)
        {
            return (json ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private string GetUser(ClaimsPrincipal user)
        {
            if (null == user.Identity || !user.Identity.IsAuthenticated) return "<<not authenticated>>";
            var nameClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToUpperInvariant() ?? "<<user is authenticated but has no name claim>>";
            return nameClaim;
        }
    }
}

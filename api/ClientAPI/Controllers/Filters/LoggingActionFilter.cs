using System.Linq;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ClientAPI.Filters
{
    public class LoggingActionFilter : IActionFilter, IExceptionFilter
    {
        private readonly ILogger<LoggingActionFilter> _logger;
        public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation(
                string.Format("Executed {0} {1}", 
                context.HttpContext.Request.RouteValues.Select(r=>r.Value).ToArray())
                );
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation(
                string.Format("Executing {0} {1}",
                context.HttpContext.Request.RouteValues.Select(r => r.Value).ToArray())
                );
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(
                context.Exception,
                string.Format("Exception encountered at {0} {1}",
                context.HttpContext.Request.RouteValues.Select(r => r.Value).ToArray())
                );
        }
    }
}

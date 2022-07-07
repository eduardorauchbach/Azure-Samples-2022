using Functions.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Functions.SampleCosmos.Domain.Functions.Helper
{
    public class ActionFilters : IFunctionInvocationFilter, IFunctionExceptionFilter
    {
        #region Properties
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        public ActionFilters(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            if (executingContext.Arguments["req"] == null)
            {
                throw new ArgumentNullException("Request");
            }

            return Task.CompletedTask;
        }

        public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _ = HttpErrorHelper.GetHttpErrorStatus(exceptionContext.Exception, out HttpStatusCode statusCode);

                string message = JsonSerializer.Serialize((exceptionContext.Exception.InnerException ?? exceptionContext.Exception).Message);

                _httpContextAccessor.HttpContext.Response.Body = new MemoryStream();
                _httpContextAccessor.HttpContext.Response.StatusCode = (int)statusCode;
                _httpContextAccessor.HttpContext.Response.Headers.Add("Message", message.RemoveDiacritics());

                _httpContextAccessor.HttpContext.Response.WriteAsync(message, cancellationToken).RunSynchronously();
            }

            return Task.CompletedTask;
        }

        Task IFunctionInvocationFilter.OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task IFunctionInvocationFilter.OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task IFunctionExceptionFilter.OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

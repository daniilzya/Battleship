using Battleship.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Battleship.Api
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationException oEx)
            {
                var response = context.Response;
                switch (oEx.ErrorCode)
                {
                    case ErrorCode.BadRequest:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = GetErrorResult(oEx.Message);
                await response.WriteAsync(result);
            }
            catch (Exception ex)
            {
                var response = context.Response;
                var result = GetErrorResult(ex.Message);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.WriteAsync(result);
            }
        }

        private string GetErrorResult(string msg) 
        {
            return JsonSerializer.Serialize(new { message = msg });
        }
    }
}

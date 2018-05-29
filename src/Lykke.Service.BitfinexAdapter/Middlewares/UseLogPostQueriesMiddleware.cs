using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Lykke.Service.BitfinexAdapter.Middlewares
{
    public static class LogPostQueriesMiddlewareExtensions
    {
        public static void UseLogPostQueriesMiddleware(this IApplicationBuilder app, ILog logger)
        {
            app.Use((c, n) => LogPostQueries(c, n, logger));
        }

        private static async Task LogPostQueries(HttpContext context, Func<Task> next, ILog logger)
        {
            if (context.Request.Method == "POST" && context.Request.ContentType == "application/json")
            {
                string requestBody;

                context.Request.EnableRewind();

                using (var sr = new StreamReader(
                    stream: context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 4096,
                    leaveOpen: true))
                {
                    requestBody = await sr.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                var responseBody = "<empty>";

                using (var ms = new MemoryStream())
                {
                    var originalResponse = context.Response.Body;
                    context.Response.Body = ms;

                    await next();

                    ms.Position = 0;
                    using (var sr = new StreamReader(stream: context.Response.Body,
                        encoding: Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: true,
                        bufferSize: 4096,
                        leaveOpen: true))
                    {
                        responseBody = await sr.ReadToEndAsync();
                    }

                    ms.Position = 0;
                    await ms.CopyToAsync(originalResponse);
                }

                if (IsSuccessful(context.Response.StatusCode))
                {
                    await logger.WriteInfoAsync("BitfinexAdapter",
                        new
                        {
                            Url = context.Request.Path,
                            Method = "POST",
                            Request = requestBody,
                            Response = responseBody
                        }.ToJson(),
                        $"POST {context.Request.Path} " +
                        $"{context.Request.ContentLength?.ToString() ?? "N/A"} bytes");
                }
                else
                {
                    await logger.WriteInfoAsync("BitfinexAdapter",
                        new
                        {
                            Url = context.Request.Path,
                            Method = "POST",
                            Request = requestBody,
                            Response = responseBody,
                            StatusCode = context.Response.StatusCode
                        }.ToJson(),
                        $"POST {context.Request.Path} " +
                        $"{context.Request.ContentLength?.ToString() ?? "N/A"} bytes");
                }
            }
            else
            {
                await next();
            }
        }

        private static bool IsSuccessful(int responseStatusCode)
        {
            return responseStatusCode >= 200 && responseStatusCode < 300;
        }
    }
}

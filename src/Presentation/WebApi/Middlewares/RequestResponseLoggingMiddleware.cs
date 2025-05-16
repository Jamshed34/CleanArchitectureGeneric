using Microsoft.AspNetCore.Http;
using Serilog;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = Log.ForContext<RequestResponseLoggingMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        string requestBody = string.Empty;
        string responseBody = string.Empty;
        var isSensitiveEndpoint = context.Request.Path.StartsWithSegments("/connect/token");

        if (!isSensitiveEndpoint)
        {
            requestBody = await ReadRequestBodyAsync(context.Request);
            _logger.Information("HTTP Request: {Method} {Path} {Body}", context.Request.Method, context.Request.Path, requestBody);
        }
        else
        {
            _logger.Information("HTTP Request: {Method} {Path} [Sensitive content omitted]", context.Request.Method, context.Request.Path);
        }

        // Save original body stream
        var originalBodyStream = context.Response.Body;

        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unhandled exception during request");
            throw;
        }

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        if (!isSensitiveEndpoint)
        {
            _logger.Information("HTTP Response: {StatusCode} {Body}", context.Response.StatusCode, responseText);
        }
        else
        {
            _logger.Information("HTTP Response: {StatusCode} [Sensitive content omitted]", context.Response.StatusCode);
        }

        await responseBodyStream.CopyToAsync(originalBodyStream);
    }


    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Seek(0, SeekOrigin.Begin);
        return body;
    }
}

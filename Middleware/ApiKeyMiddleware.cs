using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MeuProjetoIA.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-API-Key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //Endpoints publicos não necessitam de API Key
            if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/debug-env"))
            {
                await _next(context);
                return;
            }

            //Pegar chave esperada do configuration
            if(!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Chave API Invalida ou ausente.");
                return;
            }

            //Pegar a chave esperada do configuration
            var appSettingsApiKey = context.RequestServices
                .GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()
                .GetValue<string>("ApiKey");

            if(appSettingsApiKey == null || !extractedApiKey.Equals(appSettingsApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Chave API Invalida ou ausente.");
                return;
            }

            await _next(context);
        }
    }
}
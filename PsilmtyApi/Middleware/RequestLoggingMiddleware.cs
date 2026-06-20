using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Services;

namespace PsilmtyApi.Middleware;

/// <summary>
/// Registra cada petición a /api con su entrada y salida (éxito o error),
/// usuario, status HTTP, duración y excepción si la hubo.
/// </summary>
public sealed class RequestLoggingMiddleware(RequestDelegate next, IFileLogService log)
{
    private const int MaxBody = 4000;

    public async Task InvokeAsync(HttpContext ctx)
    {
        var path = ctx.Request.Path.Value ?? "";

        // Solo registramos las llamadas a la API.
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            await next(ctx);
            return;
        }

        var start = DateTime.Now;
        var sw = Stopwatch.StartNew();
        var endpoint = $"{ctx.Request.Method} {path}{ctx.Request.QueryString}";
        var entrada = await LeerEntradaAsync(ctx.Request);

        var originalBody = ctx.Response.Body;
        await using var buffer = new MemoryStream();
        ctx.Response.Body = buffer;

        string? errorMsg = null;
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            errorMsg = ex.ToString();
            if (ctx.Response.StatusCode < 400) ctx.Response.StatusCode = 500;
            throw;
        }
        finally
        {
            sw.Stop();

            string salida = "(vacío)";
            try
            {
                buffer.Position = 0;
                salida = await new StreamReader(buffer).ReadToEndAsync();
                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody);
            }
            catch { /* ignore */ }
            finally { ctx.Response.Body = originalBody; }

            errorMsg ??= ctx.Items.TryGetValue("__error", out var e) ? e?.ToString() : null;

            var http = ctx.Response.StatusCode;
            var success = errorMsg is null && http < 400;
            var usuario = ctx.User.GetUserLabel();
            var fin = DateTime.Now;

            var sb = new StringBuilder();
            sb.AppendLine($"{start:yyyy/MM/dd HH:mm:ss} | ENTRADA | metodo: {endpoint} | usuario: {usuario} | start: {start:yyyy/MM/dd HH:mm:ss} | request: {Limpiar(entrada)}");
            sb.AppendLine($"{fin:yyyy/MM/dd HH:mm:ss} | SALIDA | metodo: {endpoint} | usuario: {usuario} | status: {(success ? "SUCCESS" : "ERROR")} | statusHttp: {http} | response: {Limpiar(salida)} | end: {fin:yyyy/MM/dd HH:mm:ss} | duracionMs: {sw.ElapsedMilliseconds}");
            if (errorMsg is not null)
                sb.AppendLine($"{fin:yyyy/MM/dd HH:mm:ss} | THROW | metodo: {endpoint} | usuario: {usuario} | error: {Limpiar(errorMsg)}");
            sb.AppendLine(new string('-', 100));

            log.Write(success, sb.ToString());
        }
    }

    private static string Limpiar(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "(vacío)";
        var t = s.Length > MaxBody ? s[..MaxBody] + "...(truncado)" : s;
        return t.Replace("\r", " ").Replace("\n", " ");
    }

    private static async Task<string> LeerEntradaAsync(HttpRequest req)
    {
        try
        {
            var q = req.QueryString.HasValue ? req.QueryString.Value : "";
            var esJson = req.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false;
            if (req.ContentLength is > 0 && esJson)
            {
                req.EnableBuffering();
                using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                req.Body.Position = 0;
                return $"query={q} body={OcultarSecretos(body)}";
            }
            return $"query={q} body=(sin cuerpo JSON)";
        }
        catch
        {
            return "(no se pudo leer la entrada)";
        }
    }

    // Oculta contraseñas en los cuerpos registrados.
    private static string OcultarSecretos(string body) =>
        Regex.Replace(
            body,
            "(\"(?:password|currentPassword|newPassword|pass)\"\\s*:\\s*\")[^\"]*(\")",
            "$1***$2",
            RegexOptions.IgnoreCase);
}

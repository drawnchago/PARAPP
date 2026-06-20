using System.Text;
using PsilmtyApi.Interfaces.Services;

namespace PsilmtyApi.Services;

/// <summary>
/// Log a archivos con estructura: {raíz}/Log/yyyy/MM/dd/{success|error}/Log_yyyyMMdd.txt
/// </summary>
public sealed class FileLogService : IFileLogService
{
    private readonly string _root;
    private static readonly object _gate = new();
    private static readonly UTF8Encoding _utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public FileLogService(IHostEnvironment env)
    {
        _root = Path.Combine(env.ContentRootPath, "Log");
    }

    public void Write(bool success, string content)
    {
        try
        {
            var now = DateTime.Now;
            var dir = Path.Combine(
                _root,
                now.ToString("yyyy"),
                now.ToString("MM"),
                now.ToString("dd"),
                success ? "success" : "error");

            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"Log_{now:yyyyMMdd}.txt");

            lock (_gate)
            {
                File.AppendAllText(file, content, _utf8NoBom);
            }
        }
        catch
        {
            // El logging nunca debe romper la petición.
        }
    }
}

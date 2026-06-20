using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Models.Responses;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/oraciones")]
public sealed class OracionesController(IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var folder = Path.Combine(env.WebRootPath ?? "", "audios");
        if (!Directory.Exists(folder))
            return Ok(Array.Empty<AudioResponse>());

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var result = Directory.GetFiles(folder, "*.mp3")
            .OrderBy(f => ParseOrder(Path.GetFileName(f)))
            .Select(f =>
            {
                var filename = Path.GetFileName(f);
                return new AudioResponse(filename, ParseTitle(filename), $"{baseUrl}/audios/{Uri.EscapeDataString(filename)}");
            });

        return Ok(result);
    }

    private static int ParseOrder(string filename)
    {
        var idx = filename.IndexOf('_');
        return idx > 0 && int.TryParse(filename[..idx], out var n) ? n : 999;
    }

    private static string ParseTitle(string filename)
    {
        var name = Path.GetFileNameWithoutExtension(filename);
        var idx = name.IndexOf('_');
        if (idx >= 0) name = name[(idx + 1)..];
        return name.Replace('_', ' ');
    }
}

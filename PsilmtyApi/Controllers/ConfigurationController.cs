using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[ApiController]
[Route("api/configuracion")]
public sealed class ConfigurationController(IApplicationDataService service, IDatabaseRepository repository) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("lenguajes")]
    public async Task<IActionResult> GetLanguages() =>
        Ok(await repository.QueryAsync<object>("""
            SELECT id Id, code Codigo, name Nombre, native_name NombreNativo,
                   flag_emoji BanderaEmoji, is_default EsDefault, sort_order Orden
            FROM languages WHERE is_active=1 ORDER BY sort_order
            """));

    [AllowAnonymous]
    [HttpGet("temas")]
    public async Task<IActionResult> GetThemes() =>
        Ok(await repository.QueryAsync<object>("""
            SELECT id Id, code Clave, name Nombre, color_primary ColorPrimario,
                   color_secondary ColorSecundario, color_background ColorFondo,
                   color_surface ColorSuperficie, color_text ColorTexto,
                   color_text_soft ColorTextoSuave, color_accent ColorAcento,
                   color_error ColorError, is_dark EsOscuro, is_default EsDefault
            FROM themes WHERE is_active=1 ORDER BY sort_order
            """));

    [Authorize]
    [HttpGet("preferencias")]
    public async Task<IActionResult> GetPreferences() =>
        Ok(await service.GetPreferencesAsync(User.GetUserId()));

    [Authorize]
    [HttpPut("preferencias")]
    public async Task<IActionResult> SavePreferences(UserPreferencesRequest request) =>
        Ok(await service.SavePreferencesAsync(User.GetUserId(), request));
}

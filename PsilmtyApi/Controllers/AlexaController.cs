using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;
using PsilmtyApi.Models.Responses;

namespace PsilmtyApi.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/alexa")]
public sealed class AlexaController(IApplicationDataService service) : ControllerBase
{
    private static readonly string[] DayNames   = ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];
    private static readonly string[] MonthNames = ["enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];

    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] AlexaRequest request) =>
        request.Request.Type switch
        {
            "LaunchRequest"       => Ok(AlexaResponse.Say("Bienvenido a la parroquia San Isidro Monterrey. ¿En qué puedo ayudarte?")),
            "IntentRequest"       => Ok(await HandleIntent(request)),
            "SessionEndedRequest" => Ok(AlexaResponse.Say("Hasta luego.", endSession: true)),
            _                     => Ok(AlexaResponse.Say("No entendí tu solicitud. Intenta de nuevo."))
        };

    [HttpGet]
    public async Task<IActionResult> GetNews(
        [FromQuery] uint parroquiaId,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null) =>
        Ok(await service.GetAlexaNewsAsync(
            parroquiaId,
            from?.ToDateTime(TimeOnly.MinValue),
            to?.ToDateTime(TimeOnly.MaxValue)));

    private async Task<AlexaResponse> HandleIntent(AlexaRequest request) =>
        (request.Request.IntentName ?? "") switch
        {
            "ObtenerNoticias" => await NewsIntent(),
            "ObtenerHorarios" => await SchedulesIntent(),
            "ObtenerEventos"  => await CalendarIntent(type: null),
            "ObtenerMisas"    => await CalendarIntent(type: "mass"),
            "HabraMisaHoy"    => await MassesTodayIntent(),
            "ProximaMisa"     => await NextMassIntent(),
            "HabraMisaHora"   => await MassByHourIntent(request),
            _                 => AlexaResponse.Say("No conozco esa acción todavía.")
        };

    private async Task<AlexaResponse> NewsIntent()
    {
        var news = await service.GetAlexaNewsAsync(parishId: 1, from: null, to: null);
        if (!news.Any())
            return AlexaResponse.Say("No hay noticias disponibles en este momento.");

        var titles = news.Take(3).Select(n => (string)n.Title);
        return AlexaResponse.Say("Las últimas noticias son: " + string.Join(". ", titles) + ".");
    }

    private async Task<AlexaResponse> SchedulesIntent()
    {
        var rows = await service.GetAlexaSchedulesAsync(parishId: 1);
        if (!rows.Any())
            return AlexaResponse.Say("No encontré horarios registrados para esta parroquia.");

        var parts = rows
            .GroupBy(r => (int)r.DayOfWeek)
            .OrderBy(g => g.Key)
            .Select(day =>
            {
                var name = DayNames[day.Key];
                if ((bool)day.First().IsClosed) return $"El {name} está cerrada";
                var blocks = day
                    .Where(r => r.OpenTime != null)
                    .Select(r => $"de {FormatTime((string)r.OpenTime)} a {FormatTime((string)r.CloseTime)}");
                return $"El {name} {string.Join(" y ", blocks)}";
            });

        return AlexaResponse.Say("Los horarios de la parroquia son: " + string.Join(". ", parts) + ".");
    }

    private async Task<AlexaResponse> CalendarIntent(string? type)
    {
        var events = await service.GetAlexaCalendarAsync(parishId: 1, type: type);
        if (!events.Any())
        {
            var label = type == "mass" ? "misas" : "eventos";
            return AlexaResponse.Say($"No hay {label} próximos registrados.");
        }

        var parts = events.Select(e =>
        {
            var dt = (DateTime)e.StartDatetime;
            var dateStr = $"el {DayNames[(int)dt.DayOfWeek]} {dt.Day} de {MonthNames[dt.Month - 1]}";
            var timeStr = (bool)e.AllDay ? "" : $" a {FormatTime(dt.ToString("HH:mm"))}";
            return $"{e.Title}{timeStr} {dateStr}";
        });

        var intro = type == "mass" ? "Las próximas misas son: " : "Los próximos eventos son: ";
        return AlexaResponse.Say(intro + string.Join(". ", parts) + ".");
    }

    private async Task<AlexaResponse> MassesTodayIntent()
    {
        var masses = await service.GetAlexaMassesTodayAsync(parishId: 1);
        if (!masses.Any())
            return AlexaResponse.Say("No hay misas programadas para hoy.");

        var times = masses
            .Where(m => !(bool)m.AllDay)
            .Select(m => FormatTime(((DateTime)m.StartDatetime).ToString("HH:mm")));

        return times.Any()
            ? AlexaResponse.Say("Sí hay misa hoy. Los horarios son: " + string.Join(", ", times) + ".")
            : AlexaResponse.Say("Sí hay misa hoy.");
    }

    private async Task<AlexaResponse> NextMassIntent()
    {
        var mass = await service.GetAlexaNextMassAsync(parishId: 1);
        if (mass == null)
            return AlexaResponse.Say("No encontré misas próximas registradas.");

        var dt = (DateTime)mass.StartDatetime;
        var dayName  = DayNames[(int)dt.DayOfWeek];
        var dateStr  = $"el {dayName} {dt.Day} de {MonthNames[dt.Month - 1]}";
        var timeStr  = (bool)mass.AllDay ? "" : $" a {FormatTime(dt.ToString("HH:mm"))}";
        var location = mass.Location != null ? $" en {mass.Location}" : "";

        return AlexaResponse.Say($"La próxima misa es {dateStr}{timeStr}{location}.");
    }

    private async Task<AlexaResponse> MassByHourIntent(AlexaRequest request)
    {
        var slotValue = request.Request.Intent?.Slots?.GetValueOrDefault("hora")?.Value;
        if (slotValue == null || !int.TryParse(slotValue, out var hour))
            return AlexaResponse.Say("No entendí la hora. Por favor dime la hora de la misa.");

        var masses = await service.GetAlexaMassesTodayAsync(parishId: 1);
        var match  = masses.FirstOrDefault(m =>
            !((bool)m.AllDay) && ((DateTime)m.StartDatetime).Hour == hour);

        var hourFormatted = FormatTime($"{hour:D2}:00");
        return match != null
            ? AlexaResponse.Say($"Sí, hay misa a {FormatTime(((DateTime)match.StartDatetime).ToString("HH:mm"))} hoy.")
            : AlexaResponse.Say($"No encontré misa a {hourFormatted} hoy.");
    }

    private static string FormatTime(string hhmm)
    {
        if (!TimeOnly.TryParse(hhmm, out var t)) return hhmm;
        var period = t.Hour < 12 ? "de la mañana" : t.Hour < 20 ? "de la tarde" : "de la noche";
        var h12    = t.Hour % 12 == 0 ? 12 : t.Hour % 12;
        return t.Minute == 0
            ? $"las {h12} {period}"
            : $"las {h12} y {t.Minute} {period}";
    }
}

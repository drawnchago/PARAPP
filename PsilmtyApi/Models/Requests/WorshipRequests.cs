using System.Text.Json.Serialization;

namespace PsilmtyApi.Models.Requests;

public sealed class AdorationRequest
{
    [JsonPropertyName("titulo")]
    public string Title { get; set; } = "";

    [JsonPropertyName("fecha")]
    public DateTime Date { get; set; }

    [JsonPropertyName("duracion")]
    public int? DurationMinutes { get; set; }

    [JsonPropertyName("tipo")]
    public string Type { get; set; } = "mixta";

    [JsonPropertyName("notas")]
    public string? Notes { get; set; }
}

public sealed class AdorerRequest
{
    [JsonPropertyName("nombre")]
    public string FirstName { get; set; } = "";

    [JsonPropertyName("apellido")]
    public string LastName { get; set; } = "";

    [JsonPropertyName("correo")]
    public string? Email { get; set; }

    [JsonPropertyName("telefono")]
    public string? Phone { get; set; }

    [JsonPropertyName("instrumento")]
    public string? Instrument { get; set; }
}

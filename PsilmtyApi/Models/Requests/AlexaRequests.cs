namespace PsilmtyApi.Models.Requests;

public sealed class AlexaRequest
{
    public string Version { get; set; } = "";
    public AlexaSession? Session { get; set; }
    public AlexaRequestBody Request { get; set; } = new();
}

public sealed class AlexaSession
{
    public Dictionary<string, object>? Attributes { get; set; }
}

public sealed class AlexaRequestBody
{
    public string Type { get; set; } = "";
    public string? IntentName => Intent?.Name;
    public AlexaIntent? Intent { get; set; }
}

public sealed class AlexaIntent
{
    public string Name { get; set; } = "";
    public Dictionary<string, AlexaSlot>? Slots { get; set; }
}

public sealed class AlexaSlot
{
    public string? Value { get; set; }
}

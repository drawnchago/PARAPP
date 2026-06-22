using System.Text.Json.Serialization;

namespace PsilmtyApi.Models.Responses;

public sealed class AlexaResponse
{
    public string Version { get; set; } = "1.0";
    public AlexaResponseBody Response { get; set; } = new();

    public static AlexaResponse Say(string text, bool endSession = false) => new()
    {
        Response = new()
        {
            OutputSpeech = new() { Text = text },
            ShouldEndSession = endSession
        }
    };
}

public sealed class AlexaResponseBody
{
    public AlexaOutputSpeech OutputSpeech { get; set; } = new();
    public bool ShouldEndSession { get; set; }
}

public sealed class AlexaOutputSpeech
{
    public string Type { get; set; } = "PlainText";
    public string Text { get; set; } = "";
}

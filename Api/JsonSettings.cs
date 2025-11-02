using System.Text.Json;

namespace Api;

public static class JsonSettings
{
    public static JsonSerializerOptions CamelCaseOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true // optional: for pretty printing
    };
}
using System.Text.Json.Serialization;

namespace PanOpticon.Models;

public record JobParams([property: JsonPropertyName("pageName")] string? PageName);

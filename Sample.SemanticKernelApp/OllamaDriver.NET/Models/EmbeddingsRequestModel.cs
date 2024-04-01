namespace OllamaDriver.NET.Models;

public class EmbeddingsRequestModel
{
    public string Model { get; set; }

    public string Prompt { get; set; }

    public OptionsRequestModelPart Options { get; set; }
}

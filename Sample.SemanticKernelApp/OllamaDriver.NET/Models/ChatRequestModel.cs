namespace OllamaDriver.NET.Models;

public class ChatRequestModel
{
    public string? Model { get; set; }

    public bool? Stream { get; set; }

    public OptionsRequestModelPart? Options { get; set; }

    public IEnumerable<MessageInfo> Messages { get; set; }
}

using OllamaDriver.NET.Models;

namespace OllamaDriver.NET.ResponseModels;

public class ChatResponseModel
{
    public string? Model { get; set; }

    public MessageInfo? Message { get; set; }

    public bool Done { get; set; }
}

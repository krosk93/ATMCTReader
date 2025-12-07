using ATMCTReader.Models;

namespace ATMCTReader.Messages;

public class ReadCardResultMessage
{
    public ReadCardResultMessage(bool success, string? error, Card? card)
    {
        Success = success;
        Error = error;
        Card = card;
    }
    public string? Error { get; }
    public Card? Card { get; }
    public bool Success { get; }
}

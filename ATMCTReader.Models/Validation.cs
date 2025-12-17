using System;

namespace ATMCTReader.Models;

public class Validation
{
    public DateTime Instant { get; init; }
    public required Zone Zone { get; init; }
    public required Stop Stop { get; init; }
    public required Company Company { get; init; }
    public required Line Line { get; init; }
    public int Vehicle { get; init; }
    public bool IsTransfer { get; init; }
    public int Passengers { get; init; }
}

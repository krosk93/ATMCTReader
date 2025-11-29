using System;

namespace ATMCTReader.Models;

public class TopUp
{
    public required TicketType Type { get; init; }
    public required DateTime Date { get; init; }
}

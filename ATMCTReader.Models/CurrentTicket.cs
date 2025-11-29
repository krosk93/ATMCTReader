using System;

namespace ATMCTReader.Models;

public class CurrentTicket
{
    public required TicketType Type { get; init; }
    public int TripsLeft { get; init; }
    public required Zone FirstZone { get; init; }
    public DateTime? ExpireDate { get; init; }
}

using System;

namespace ATMCTReader.Models;

public class TicketType : BaseModel
{
    public int Trips { get; init; }
    public bool UnlimitedTrips { get; init; } = false;
    public TimeSpan? Duration { get; init; } = null;
    public bool UnlimitedTime { get; init; } = true;
    public int Zones { get; init; }
}

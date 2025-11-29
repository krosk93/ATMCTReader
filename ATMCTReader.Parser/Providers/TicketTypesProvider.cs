using System;
using ATMCTReader.Models;

namespace ATMCTReader.Parser.Providers;

public class TicketTypesProvider : BaseProvider<TicketType>
{
    public TicketTypesProvider() : base("ATMCTReader.Parser.Data.ticketTypes.json")
    {
    }

}

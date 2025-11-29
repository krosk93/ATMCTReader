using System;
using ATMCTReader.Models;

namespace ATMCTReader.Parser.Providers;

public class StopsProvider : BaseProvider<Stop>
{
    public StopsProvider() : base("ATMCTReader.Parser.Data.stops.json")
    {
    }

}

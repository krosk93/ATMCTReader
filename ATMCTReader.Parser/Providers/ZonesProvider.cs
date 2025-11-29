using System;
using ATMCTReader.Models;

namespace ATMCTReader.Parser.Providers;

public class ZonesProvider : BaseProvider<Zone>
{
    public ZonesProvider() : base("ATMCTReader.Parser.Data.zones.json")
    {
    }
}

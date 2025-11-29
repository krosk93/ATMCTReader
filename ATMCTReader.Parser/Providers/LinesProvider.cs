using System;
using ATMCTReader.Models;

namespace ATMCTReader.Parser.Providers;

public class LinesProvider : BaseProvider<Line>
{
    public LinesProvider() : base("ATMCTReader.Parser.Data.lines.json")
    {
    }

}

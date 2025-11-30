using System.Text;
using ATMCTReader.Models;
using ATMCTReader.Parser.Providers;

namespace ATMCTReader.Parser;

public static class CardParser
{
    static ArraySegment<byte> GetLine(byte[] card, int address)
    {
        return new ArraySegment<byte>(card, address, 0x10);
    }

    public static Card ParseCard(byte[] card)
    {
        var linesProvider = new LinesProvider();
        var stopsProvider = new StopsProvider();
        var zonesProvider = new ZonesProvider();
        var ticketTypesProvider = new TicketTypesProvider();
        var companiesProvider = new CompaniesProvider();
        var profilesProvider = new ProfilesProvider();

        var bytes = GetLine(card, 0x0).Reverse().ToArray();
        uint UUID = (uint)((bytes[12] << 24) + (bytes[13] << 16) + (bytes[14] << 8) + bytes[15]);

        bytes = GetLine(card, 0x10).ToArray();
        int accountId = int.Parse(
            new string(
                Convert.ToString(
                    (bytes[4] << 24) 
                    + (bytes[3] << 16) 
                    + (bytes[2] << 8) 
                    + bytes[1], 16)
                    .Reverse()
                    .ToArray()));
        
        string ownerName = Encoding.Latin1.GetString(GetLine(card, 0x80).Take(0xf).ToArray()).Trim();
        string ownerSurname1 = Encoding.Latin1.GetString(GetLine(card, 0x90).Take(0xf).ToArray()).Trim();
        string ownerSurname2 = Encoding.Latin1.GetString(GetLine(card, 0xA0).Take(0xf).ToArray()).Trim();

        bytes = GetLine(card, 0xC0).Reverse().ToArray();
        int startYear = ((bytes[12] & 15) << 4) + (bytes[13] >> 4) + 1900;
        DateTime expireDate = new DateTime(
            startYear, 
            bytes[11] & 15, 
            ((bytes[10] & 1) << 4) + (bytes[11] >> 4)
        ).AddYears(bytes[12] >> 4);

        bytes = GetLine(card, 0xD0).Reverse().ToArray();
        int p = bytes[3] >> 2;
        Profile profile = profilesProvider.Get(p);
        int ea = ((bytes[3] & 3) << 6) + (bytes[4] >> 2);
        int m = ((bytes[4] & 3) << 8) + bytes[5];

        bytes = GetLine(card, 0x100).Reverse().ToArray();
        Validation lastValidation;
        {
            int minutes = bytes[1] >> 2;
            int hours = ((bytes[1] & 0x3) << 3) + (bytes[2] >> 5);
            int day = bytes[2] & 0b11111;
            int month = bytes[3] >> 4;
            int year = startYear + (bytes[3] & 15);
            int zone = bytes[4];
            int stop = (bytes[5] << 7) + (bytes[6] >> 1);
            int op = ((bytes[6] & 1) << 7) + (bytes[7] >> 1);
            int line = ((bytes[7] & 1) << 10) + (bytes[8] << 2) + (bytes[9] >> 6);
            lastValidation = new Validation
            {
                Instant = new DateTime(year, month, day, hours, minutes, 0),
                Zone = zonesProvider.Get(zone),
                Stop = stopsProvider.Get(stop),
                Company = companiesProvider.Get(op),
                Line = linesProvider.Get(line)
            };
        }
        CurrentTicket currentTicket;
        {
            bytes = GetLine(card, 0x140).Reverse().ToArray();
            int ticketType = ((bytes[13] & 3) << 8) + bytes[14];
            int numberOfZones = bytes[5] >> 2;
            int validDaysFromFirstUse = bytes[10];
            int trips = bytes[13] >> 2;

            bytes = GetLine(card, 0x150).Reverse().ToArray();
            int tripsLeft = bytes[7] >> 1;
            int day = ((bytes[12] & 1) << 4) + (bytes[13] >> 4);
            int month = bytes[13] & 15;
            int nextYear = bytes[14] >> 7;
            int firstZone = ((bytes[2] & 15) << 4) + (bytes[3] >> 4);
            DateTime? ticketExpireDate = 
                day != 0
                    ? new DateTime(startYear + nextYear, month, day).AddDays(1).AddTicks(-1)
                    : null;
            currentTicket = new CurrentTicket
            {
                Type = ticketTypesProvider.Get(ticketType),
                FirstZone = zonesProvider.Get(firstZone),
                ExpireDate = ticketExpireDate,
                TripsLeft = tripsLeft,
                NumberOfZones = numberOfZones,
                ValidDaysFromFirstUse = validDaysFromFirstUse
            };
        }

        List<TopUp> topUps = new();
        for (int i = 0; i < 3; i++) 
        {
            bytes = GetLine(card, 0x240 + 0x10 * i).Reverse().ToArray();
            int day = ((bytes[1] & 1) << 4) + (bytes[2] >> 4);
            if(day != 0)
            {
                DateTime date = new DateTime(
                    startYear + (bytes[3] >> 4),
                    bytes[2] & 15,
                    day
                );
                int ticketType = ((bytes[6] & 3) << 8) + bytes[7];
                topUps.Add(new TopUp
                {
                Date = date,
                Type = ticketTypesProvider.Get(ticketType)
                });
            }
        }

        int curAddress = curAddress = 0x2c0;
        List<Validation> validations = new();
        for (int i = 0; i < 10; i++)
        {
            if (((curAddress + 0x10) % 0x40) == 0) curAddress += 0x10;
            bytes = GetLine(card, curAddress).Reverse().ToArray();
            int day = bytes[2] & 31;
            if(day != 0)
            {
                DateTime instant = new DateTime(
                    startYear + (bytes[3] & 15),
                    bytes[3] >> 4,
                    bytes[2] & 31,
                    ((bytes[1] & 3) << 3) + (bytes[2] >> 5),
                    bytes[1] >> 2,
                    0
                );
                int vehicle = (bytes[4] << 2) + (bytes[5] >> 6);
                int zone = ((bytes[5] & 63) << 2) + (bytes[6] >> 6);
                int line = ((bytes[9] & 127) << 4) + (bytes[10] >> 4);
                int company = ((bytes[8] & 127) << 1) + (bytes[9] >> 7);
                int stop = ((bytes[6] & 63) << 9) + (bytes[7] << 1) + (bytes[8] >> 7);
                validations.Add(new Validation
                {
                    Instant = instant,
                    Company = companiesProvider.Get(company),
                    Vehicle = vehicle,
                    Zone = zonesProvider.Get(zone),
                    Line = linesProvider.Get(line),
                    Stop = stopsProvider.Get(stop)
                });
            }
            curAddress += 0x10;
        }

        return new Card
        {
            UUID = UUID,
            AccountId = accountId,
            OwnerName = ownerName,
            OwnerSurname1 = ownerSurname1,
            OwnerSurname2 = ownerSurname2,
            StartYear = startYear,
            ExpireDate = expireDate,
            P = p,
            Profile = profile,
            EA = ea,
            M = m,
            LastValidation = lastValidation,
            CurrentTicket = currentTicket,
            TopUps = topUps,
            Validations = validations
        };
    }
}

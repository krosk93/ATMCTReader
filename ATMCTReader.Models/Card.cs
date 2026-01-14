namespace ATMCTReader.Models;

public class Card
{
    public uint UUID { get; init; }
    public string CardId
    {
        get
        {
            var temp = UUID.ToString().PadLeft(10, '0').PadRight(12, 'X');
            return $"{temp[..4]}-{temp[4..8]}-{temp[8..]}";
        }
    }
    public int AccountId { get; init; }
    public required string OwnerName { get; init; }
    public required string OwnerSurname1 { get; init; }
    public required string OwnerSurname2 { get; init; }
    public string Owner => $"{OwnerName} {OwnerSurname1} {OwnerSurname2}";
    public int StartYear { get; init; }
    public DateTime ExpireDate { get; init; }
    public int ExpireYear => ExpireDate.Year;
    public required Authority Authority { get; init; }
    public int P { get; init; }
    public required Profile Profile { get; init; }
    public int EA { get; init; }
    public int M { get; init; }
    public required LastValidation? LastValidation { get; init; }
    public required CurrentTicket CurrentTicket { get; init; }
    public required IEnumerable<TopUp> TopUps { get; init; }
    public required IEnumerable<Validation> Validations { get; init; }
    public required byte[] Raw { get; init; }
}

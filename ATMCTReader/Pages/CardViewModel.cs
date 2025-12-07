using ATMCTReader.Messages;
using ATMCTReader.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace ATMCTReader.Pages;

public partial class CardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _ownerName = string.Empty;

    [ObservableProperty]
    private string _ownerSurname1 = string.Empty;

    [ObservableProperty]
    private string ownerSurname2 = string.Empty;

    [ObservableProperty]
    private string _accountId = string.Empty;

    [ObservableProperty]
    private DateTime? _expireDate;

    [ObservableProperty]
    private string _cardId = string.Empty;

    [ObservableProperty]
    private CurrentTicket? _currentTicket = null;

    [ObservableProperty]
    private string _profile = string.Empty;

    [ObservableProperty]
    private IEnumerable<TopUp> _topUps = new List<TopUp>();

    [ObservableProperty]
    private IEnumerable<Validation> _validations = new List<Validation>();

    public double TripsProgress { 
        get 
        { 
            try 
            {
                return double.Min(((double?)CurrentTicket?.TripsLeft ?? 0d) / ((double?)CurrentTicket?.Trips ?? 0d), 1d);
            }
            catch (DivideByZeroException) {
                return 1d;
            }
        }
    }
    public string TicketName => CurrentTicket?.Type.Name ?? "";
    public bool ShowTrips => !CurrentTicket?.Type.UnlimitedTrips ?? false;
    public bool ShowExpiration => !CurrentTicket?.Type.UnlimitedTime ?? false;
    public DateTime TicketExpiration => CurrentTicket?.ExpireDate ?? DateTime.Now;
    public double TicketExpirationProgress { 
        get 
        {
            var ts = (CurrentTicket?.ExpireDate ?? DateTime.Today) - DateTime.Today;
            try
            {
                return double.Min(double.Max(((double?)ts.TotalDays ?? 0d) / ((double?)CurrentTicket?.ValidDaysFromFirstUse ?? 0d), 0d), 1d);
            }
            catch (DivideByZeroException) {
                return 1d;
            }
        }
    }
    public bool ShowCurrentTicketSeparator => ShowExpiration && ShowTrips;

    partial void OnCurrentTicketChanged(CurrentTicket? value)
    {
        OnPropertyChanged(nameof(TripsProgress));
        OnPropertyChanged(nameof(TicketName));
        OnPropertyChanged(nameof(ShowTrips));
        OnPropertyChanged(nameof(ShowExpiration));
        OnPropertyChanged(nameof(TicketExpiration));
        OnPropertyChanged(nameof(TicketExpirationProgress));
        OnPropertyChanged(nameof(ShowCurrentTicketSeparator));
    }


    public CardViewModel()
    {
        WeakReferenceMessenger.Default.Register<ReadCardResultMessage>(this, (r, m) => {
            var c = m.Card;
            if (c == null) return;
            OwnerName = c.OwnerName ?? string.Empty;
            OwnerSurname1 = c.OwnerSurname1 ?? string.Empty;
            OwnerSurname2 = c.OwnerSurname2 ?? string.Empty;
            AccountId = c.AccountId.ToString();
            ExpireDate = c.ExpireDate;
            CardId = c.CardId;
            CurrentTicket = c.CurrentTicket;
            Profile = c.Profile.Name;
            TopUps = c.TopUps;
            Validations = c.Validations.OrderByDescending(v => v.Instant);
        });
    }
}

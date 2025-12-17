using ATMCTReader.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ATMCTReader.Pages;

[QueryProperty(nameof(Card), "card")]
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
    
    [ObservableProperty]
    private Card? _card = null;

    [ObservableProperty]
    private string _authorityName = string.Empty;

    [ObservableProperty]
    private string _authorityColor = "#f28c00";

    public double CardHeight
    {
        get
        {
            double v = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            v -= 60;
            return v / 1.586;
        }
    }

    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.Navigation.PopAsync();
    }

    [RelayCommand]
    private async Task ShareCard()
    {
        string fn = $"hf-mf-atm{Card?.Authority.Id}-{Card?.CardId}-{DateTime.Now:yyyyMMddHHmmss}.bin";
        string file = Path.Combine(FileSystem.CacheDirectory, fn);

        await File.WriteAllBytesAsync(file, Card?.Raw ?? []);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir la targeta",
            File = new ShareFile(file)
        });
    }

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
    public string TicketName => CurrentTicket != null ? $"{CurrentTicket.Type.Name} ({CurrentTicket.NumberOfZones} zon{(CurrentTicket.NumberOfZones > 1 ? "es" : "a")})" : "";
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

    partial void OnCardChanged(Card? value)
    {  
        if(value != null) {
            OwnerName = value.OwnerName ?? string.Empty;
            OwnerSurname1 = value.OwnerSurname1 ?? string.Empty;
            OwnerSurname2 = value.OwnerSurname2 ?? string.Empty;
            AccountId = value.AccountId.ToString();
            ExpireDate = value.ExpireDate;
            CardId = value.CardId;
            CurrentTicket = value.CurrentTicket;
            Profile = value.Profile.Name;
            TopUps = value.TopUps;
            Validations = value.Validations.OrderByDescending(v => v.Instant);
            AuthorityName = value.Authority.Name;
            AuthorityColor = value.Authority.BaseColor;
        }
    }


    public CardViewModel() {}
}

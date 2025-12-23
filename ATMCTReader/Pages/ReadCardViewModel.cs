using System;
using ATMCTReader.Messages;
using ATMCTReader.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkiaSharp.Extended.UI.Controls;

namespace ATMCTReader.Pages;

public enum ReadingStatus
{
    WAITING_FOR_CARD,
    READING,
    SUCCESS,
    ERROR
}

public partial class ReadCardViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _error;

    [ObservableProperty]
    private Card? _card;

    [ObservableProperty]
    private ReadingStatus _readingStatus = ReadingStatus.WAITING_FOR_CARD;
    public bool IsWaiting => ReadingStatus == ReadingStatus.WAITING_FOR_CARD;
    public bool IsReading => ReadingStatus == ReadingStatus.READING;
    public bool IsSuccess => ReadingStatus == ReadingStatus.SUCCESS;
    public bool IsError => ReadingStatus == ReadingStatus.ERROR;

    [ObservableProperty]
    private bool _showDebug = false;

    public SKFileLottieImageSource AnimationName
    {
        get
        {
            var animationName = new SKFileLottieImageSource();
            animationName.File = ReadingStatus switch
            {
                ReadingStatus.WAITING_FOR_CARD => "NFC_read.json",
                ReadingStatus.READING => "NFC_processing.json",
                ReadingStatus.SUCCESS => "NFC_success.json",
                ReadingStatus.ERROR => "NFC_fail.json",
                _ => throw new NotImplementedException(),
            };
            return animationName;
        }
    } 

    public int AnimationRepeat
    {
        get
        {
            return ReadingStatus switch
            {
                ReadingStatus.WAITING_FOR_CARD => -1,
                ReadingStatus.READING => -1,
                ReadingStatus.SUCCESS => 0,
                ReadingStatus.ERROR => 0,
                _ => -1,
            };
        }
    }

    public ReadCardViewModel()
    {
        #if DEBUG
        ShowDebug = true;
        #endif

        WeakReferenceMessenger.Default.Register<ReadCardRequestMessage>(this, (r, m) => {
            if (m == null) return;
            ReadingStatus = ReadingStatus.WAITING_FOR_CARD;
        });
        WeakReferenceMessenger.Default.Register<ReadCardProgressMessage>(this, (r, m) => {
            if (m == null) return;
            ReadingStatus = ReadingStatus.READING;
        });
		WeakReferenceMessenger.Default.Register<ReadCardResultMessage>(this, async (r, m) => {
            if (m == null) return;
            if (m.Success && m.Card != null) {
                ReadingStatus = ReadingStatus.SUCCESS;
                await Task.Delay(1500);
				await Shell.Current.GoToAsync("display", new Dictionary<string, object> {{"card", m.Card}});
            }
            else 
            {
                ReadingStatus = ReadingStatus.ERROR;
                await Task.Delay(3000);
		        WeakReferenceMessenger.Default.Send(new ReadCardRequestMessage());
                ReadingStatus = ReadingStatus.WAITING_FOR_CARD;
            }
        });
    }

    [RelayCommand]
    private async Task ReadMockCardAsync()
    {
        WeakReferenceMessenger.Default.Send(new ReadCardProgressMessage());
        var zone = new Zone
        {
            Id = 140,
            Name = "Zona 1"
        };

        var stop = new Stop
        {
            Id = 15412,
            Name = "Mostra"
        };

        var company = new Company
        {
            Id = 124,
            Name = "Transports Generals"
        };

        var line = new Line
        {
            Id = 25,
            Name = "Linia Vermella"
        };

        var ticketType = new TicketType
        {
            Name = "T-Prova"
        };

        Card parsed = new Card
        {
            OwnerName = "Subjecte",
            OwnerSurname1 = "De",
            OwnerSurname2 = "Prova",
            Profile = new Profile
            {
                Name = "T-CARA"
            },
            LastValidation = new LastValidation
            {
                Zone = zone,
                Stop = stop,
                Company = company,
                Line = line
            },
            CurrentTicket = new CurrentTicket
            {
                Type = ticketType,
                FirstZone = zone
            },
            TopUps = new List<TopUp>
            {
                new TopUp
                {
                    Type = ticketType,
                    Date = DateTime.Today
                }
            },
            Validations = new List<Validation>
            {
                new Validation
                {
                    Zone = zone,
                    Stop = stop,
                    Company = company,
                    Line = line,
                    Passengers = 2,
                }
            },
            Authority = new Authority
            {
                Id = 2,
                Name = "Àrea Fake",
                BaseColor = "#005FA8"
            },
            Raw = new byte[10]
        };
        await Task.Delay(1500);
        WeakReferenceMessenger.Default.Send(new ReadCardResultMessage(true, null, parsed));
    }

    partial void OnReadingStatusChanged(ReadingStatus value)
    {
        OnPropertyChanged(nameof(IsWaiting));
        OnPropertyChanged(nameof(IsReading));
        OnPropertyChanged(nameof(IsSuccess));
        OnPropertyChanged(nameof(IsError));
        OnPropertyChanged(nameof(AnimationName));
        OnPropertyChanged(nameof(AnimationRepeat));
    }
}

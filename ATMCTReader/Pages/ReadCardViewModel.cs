using System;
using ATMCTReader.Messages;
using ATMCTReader.Models;
using CommunityToolkit.Mvvm.ComponentModel;
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

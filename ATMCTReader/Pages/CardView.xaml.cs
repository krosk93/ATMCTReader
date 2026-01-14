using ATMCTReader.Messages;
using CommunityToolkit.Mvvm.Messaging;
using MauiIcons.Core;

namespace ATMCTReader.Pages;

public partial class CardView : ContentPage
{
	public CardView()
	{
		InitializeComponent();
		// Temporary Workaround for url styled namespace in xaml
        _ = new MauiIcon();
	}

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
		WeakReferenceMessenger.Default.Send(new SizeAllocatedMessage() { Width = width, Height = height });
    }
}
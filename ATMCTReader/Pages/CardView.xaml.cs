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
}
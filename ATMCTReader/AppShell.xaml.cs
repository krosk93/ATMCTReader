using ATMCTReader.Pages;

namespace ATMCTReader;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("card/display", typeof(CardView));
	}
}

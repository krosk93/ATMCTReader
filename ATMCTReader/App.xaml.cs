namespace ATMCTReader;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
	#if MACCATALYST || WINDOWS
		window.MinimumWidth = 600;
		window.MinimumHeight = 600;
	#endif
		return window;
	}
}
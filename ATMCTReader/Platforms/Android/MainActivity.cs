using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;

namespace ATMCTReader;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(new [] {NfcAdapter.ActionTechDiscovered, NfcAdapter.ActionNdefDiscovered, NfcAdapter.ActionTagDiscovered})]
public class MainActivity : MauiAppCompatActivity
{
    private NfcAdapter? _nfcAdapter;
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
    }

    protected override void OnResume()
    {
        base.OnResume();
        if (_nfcAdapter == null)
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("NFC Not Supported");
            alert.SetMessage("This device does not support NFC.");
            alert.Show();
        }
        else
        {
            var tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
            var ndefDetected = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
            var techDetected = new IntentFilter(NfcAdapter.ActionTechDiscovered);
            var intent = new Intent(this, this.GetType()).AddFlags(ActivityFlags.SingleTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
            var nfcIntentFilter = new[] { ndefDetected, tagDetected, techDetected };
            _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, nfcIntentFilter, [[Java.Lang.Class.FromType(typeof(MifareClassic)).Name]]);
        }
    }

    protected override void OnPause()
    {
        base.OnPause();
        _nfcAdapter?.DisableForegroundDispatch(this);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        var alert = new AlertDialog.Builder(this);
        alert.SetTitle("Intent!");
        alert.SetMessage(Intent?.Action);
        alert.Show();
        if(intent?.Action == NfcAdapter.ActionTechDiscovered)
        {
            alert.SetTitle("NFC Tech found");
            alert.SetMessage("New Tech found");
            alert.Show();
        } else if (intent?.Action == NfcAdapter.ActionNdefDiscovered)
        {
            alert.SetTitle("NFC Ndef found");
            alert.SetMessage("New Ndef found");
            alert.Show();
        } else if (intent?.Action == NfcAdapter.ActionTagDiscovered)
        {
            alert.SetTitle("NFC Tag found");
            alert.SetMessage("New Tag found");
            alert.Show();
        } else if (intent?.HasExtra(NfcAdapter.ExtraTag) ?? false)
        {
            alert.SetTitle("NFC Tag found");
            alert.SetMessage("But intent action was null");
            alert.Show();
        }
        base.OnNewIntent(intent);
    }
}

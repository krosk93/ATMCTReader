using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using ATMCTReader.Parser;
using CommunityToolkit.Mvvm.Messaging;
using ATMCTReader.Messages;
using AndroidX.Activity;

namespace ATMCTReader;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Portrait)]
[IntentFilter([NfcAdapter.ActionTechDiscovered])]
public class MainActivity : MauiAppCompatActivity
{
    private bool _nfcEnabled = false;
    private NfcAdapter? _nfcAdapter;

    public MainActivity() {
        WeakReferenceMessenger.Default.Register<ReadCardRequestMessage>(this, (r, m) => {
            if (m == null) return;
            EnableNFC();
        });
        
        WeakReferenceMessenger.Default.Register<ReadCardResultMessage>(this, (r, m) => {
            if (m == null) return;
            DisableNFC();
        });
    }
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        SetTheme(Resource.Style.MainThemeEdgeToEdge);
        
        base.OnCreate(savedInstanceState);

        _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
        EdgeToEdge.Enable(this);
    }

    private void EnableNFC() 
    {
        if(!_nfcEnabled) {
            _nfcEnabled = true;
            if (_nfcAdapter == null)
            {
                RunOnUiThread(() => {
                    var alert = new AlertDialog.Builder(this);
                    alert.SetTitle(ATMCTReader.Resources.Strings.ReadCardView.NFCNotAvailableTitle);
                    alert.SetMessage(ATMCTReader.Resources.Strings.ReadCardView.NFCNotAvailableMessage);
                    alert.Show();
                });
            }
            else
            {
                var intent = new Intent(this, this.GetType()).AddFlags(ActivityFlags.SingleTop);
                PendingIntent? pendingIntent;
                if (OperatingSystem.IsAndroidVersionAtLeast(31))
                    pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Mutable);
                else
                    pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
                try 
                {
                    _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, null, [[Java.Lang.Class.FromType(typeof(NfcA)).Name]]);
                }
                catch (Exception) 
                {
                }
            }
        }
    }

    private void DisableNFC()
    {   if(_nfcEnabled) {
            _nfcAdapter?.DisableForegroundDispatch(this);
            _nfcEnabled = false;
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        EnableNFC();
    }

    protected override void OnPause()
    {
        base.OnPause();
        DisableNFC();
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        
        if(intent?.Action == NfcAdapter.ActionTechDiscovered)
        {
            ReadCard(intent);
        } 
    }

    private void ReadCard(Intent intent)
    {
        Tag? tag;
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
            tag = (Tag?)intent.GetParcelableExtra(NfcAdapter.ExtraTag, Java.Lang.Class.FromType(typeof(Tag)));
        else
            tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;

        if (tag == null)
            return;
        
        RunOnUiThread(() => {
            WeakReferenceMessenger.Default.Send(new ReadCardProgressMessage());
        });
        Task.Run(async () => {
            var mfc = MifareClassic.Get(tag);
            if (mfc == null)
            {
                RunOnUiThread(() => {
                    WeakReferenceMessenger.Default.Send(new ReadCardResultMessage(false, "Not a MFC card", null));
                });
                return;
            }

            try
            {
                mfc.Connect();
                var sb = new System.Text.StringBuilder();
                var failed = false;
                sb.AppendLine($"MIFARE Classic sectors: {mfc.SectorCount}");

                List<byte> card = new();
                await Task.Delay(200);

                for (int sector = 0; sector < mfc.SectorCount; sector++)
                {
                    var keyB = ComputeKey(sector, true);
                    bool auth = false;
                    int counter = 0;
                    do {
                        await Task.Delay(counter * 50);
                        try
                        {
                            auth = mfc.AuthenticateSectorWithKeyB(sector, keyB);
                        }
                        catch (Java.IO.IOException)
                        {
                            auth = false;
                        }
                        counter++;
                    } while(!auth && counter < 5);
                    if (!auth) failed = true;

                    sb.AppendLine($"Sector {sector}: Auth with KeyB {(auth ? "OK" : "FAIL")}");

                    if (auth)
                    {
                        int firstBlock = mfc.SectorToBlock(sector);
                        int blockCount = mfc.GetBlockCountInSector(sector);
                        for (int b = 0; b < blockCount; b++)
                        {
                            int blockIndex = firstBlock + b;
                            try
                            {
                                counter = 0;
                                byte[]? data;
                                do {
                                    await Task.Delay(counter * 50);
                                    data = mfc.ReadBlock(blockIndex);
                                    counter++;
                                } while (data?.Length < 16 && counter < 5);
                                if(data == null || data?.Length < 16) {
                                    failed = true;
                                    throw new Exception("Block with less than 16 bytes");
                                }
                                card.AddRange(data!);
                            }
                            catch (Java.IO.IOException)
                            {
                                failed = true;
                                sb.AppendLine($" Block {blockIndex}: READ ERROR");
                            }
                        }
                    }
                }

                if (failed)
                {
                    RunOnUiThread(() => {
                        WeakReferenceMessenger.Default.Send(new ReadCardResultMessage(false, "Error reading", null));
                    });
                } else {
                        // We have the full card bytes in `card` list. Parse and send to ViewModel via messenger.
                    try
                    {
                        var cardBytes = card.ToArray();
                        var parsed = CardParser.ParseCard(cardBytes);
                        RunOnUiThread(() => {
                            WeakReferenceMessenger.Default.Send(new ReadCardResultMessage(true, null, parsed));
                        });
                    }
                    catch (Exception ex)
                    {
                        failed = true;
                        sb.AppendLine($"Parser error: {ex.Message}");
                    }
                    if(failed) {
                        RunOnUiThread(() => {
                            WeakReferenceMessenger.Default.Send(new ReadCardResultMessage(false, "Error parsing", null));
                        });
                    }
                }
                
            }
            catch (Exception)
            {
                RunOnUiThread(() => {
                    WeakReferenceMessenger.Default.Send(new ReadCardResultMessage(false, "Error", null));
                });
            }
            finally
            {
                try { mfc.Close(); } catch { }
            }
        });
    }

    private byte[] ComputeKey(int sector, bool keyB)
    {
		var bytes = new byte[6];
		bytes[0] = (byte)(10 + (keyB ? 1 : 0));
        bytes[0] <<= 4;

		bytes[0] += (byte)(sector / 10);
		bytes[1] = (byte)((sector % 10) << 4);

        return bytes;
    }
}

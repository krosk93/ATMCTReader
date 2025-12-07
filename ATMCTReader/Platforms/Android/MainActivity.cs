using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using ATMCTReader.Parser;
using ATMCTReader.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ATMCTReader.Messages;

namespace ATMCTReader;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Portrait)]
[IntentFilter(new [] {NfcAdapter.ActionTechDiscovered})]
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
            if (m.Success) DisableNFC();
        });
    }
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
    }

    private void EnableNFC() 
    {
        if(!_nfcEnabled) {
            _nfcEnabled = true;
            if (_nfcAdapter == null)
            {
                var alert = new AlertDialog.Builder(this);
                alert.SetTitle("NFC Not Supported");
                alert.SetMessage("This device does not support NFC.");
                alert.Show();
            }
            else
            {
                var intent = new Intent(this, this.GetType()).AddFlags(ActivityFlags.SingleTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Mutable);
                _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, null, [[Java.Lang.Class.FromType(typeof(NfcA)).Name]]);
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
        var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
        if (tag == null)
            return;

        Task.Run(async () => {
            var mfc = MifareClassic.Get(tag);
            if (mfc == null)
            {
                RunOnUiThread(() => {
                    var alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Error");
                    alert.SetMessage("No sembla ser una targeta de l'ATM.");
                    alert.Show();
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
                    var keyB = ComputeKeyB(sector);
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
                // We have the full card bytes in `card` list. Parse and send to ViewModel via messenger.
                try
                {
                    var cardBytes = card.ToArray();
                    var parsed = CardParser.ParseCard(cardBytes);
                    RunOnUiThread(() => {
                        WeakReferenceMessenger.Default.Send(new ReadCardResultMessage(true, null, parsed));
                    });
                }
                catch (System.Exception ex)
                {
                    failed = true;
                    sb.AppendLine($"Parser error: {ex.Message}");
                }
                if(failed) {
                    RunOnUiThread(() => {
                        var alert = new AlertDialog.Builder(this);
                        alert.SetTitle("Error");
                        alert.SetMessage(sb.ToString());
                        alert.SetPositiveButton("OK", (s, e) => { });
                        alert.Show();
                    });
                }
            }
            catch (System.Exception ex)
            {
                RunOnUiThread(() => {
                    var alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Error");
                    alert.SetMessage($"Error reading tag: {ex.Message}");
                    alert.Show();
                });
            }
            finally
            {
                try { mfc.Close(); } catch { }
            }
        });
    }

    private byte[] ComputeKeyB(int sector)
    {
        // Formula: key = 0xB00000000000 + (0x1000000000 * (sectorNumber in decimal as hex))
        const ulong baseVal = 0xB00000000000UL;
        const ulong multiplier = 0x1000000000UL;

        // Interpret the decimal sector number as hexadecimal digits (e.g. sector 10 -> "10" -> 0x10)
        ulong sectorHexValue = Convert.ToUInt64(sector.ToString(), 16);
        ulong key = baseVal + (multiplier * sectorHexValue);

        // Convert to 6-byte big-endian array
        string hex = key.ToString("X12"); // 12 hex digits == 6 bytes
        var bytes = new byte[6];
        for (int i = 0; i < 6; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

        return bytes;
    }
}

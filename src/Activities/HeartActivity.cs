using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using static Android.OS.PowerManager;
using CaledosLab.Runner.Android.Specific;
using CaledosLab.Runner.Commons.Abstractions;

namespace nicold.heartrate.Activities
{
    [Activity(Label = "nicold.heartrate.heart", Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class HeartActivity : Activity
    {
        private string _deviceName;

        private IHeartRate _heartRate;

        private TextView _textHR;
        private TextView _textNow;
        private TextView _textBattery;
        private TextView _textinfo;
        private TextView _textinfo2;
        ProgressBar _progressWorking;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Heart);

            _deviceName = Intent.GetStringExtra("device") ?? "---";

            TextView device = FindViewById<TextView>(Resource.Id.textDeviceName);
            device.Text = _deviceName;

            Button button_start_hr = FindViewById<Button>(Resource.Id.button_start_hr);
            button_start_hr.Click += Button_start_hr_Click;

            Button button_stop_hr = FindViewById<Button>(Resource.Id.button_stop_hr);
            button_stop_hr.Click += Button_stop_hr_Click;

            _textHR = FindViewById<TextView>(Resource.Id.textHR);
            _textBattery = FindViewById<TextView>(Resource.Id.textBattery);
            _textinfo = FindViewById<TextView>(Resource.Id.textInfo);
            _textinfo2 = FindViewById<TextView>(Resource.Id.textInfo2);
            _textNow = FindViewById<TextView>(Resource.Id.textNow);

            _progressWorking = FindViewById<ProgressBar>(Resource.Id.progress_work);

            var powerManager = (PowerManager)ApplicationContext.GetSystemService(Context.PowerService);
            var wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "myNicolLock");
            wakeLock.Acquire();

            Task.Run(async () => await _forever());
        }
        
        protected override void OnDestroy()
        {
            _stop = true;
            _heartRate?.Stop();

            base.OnDestroy();
        }

        private bool _stop = false;
        private async Task _forever()
        {
            while (!_stop)
            {
                var data = _heartRate?.GetCurrentHeartRateValue();

                RunOnUiThread(() =>
                {
                    _textNow.Text = DateTime.Now.ToString();
                       
                    _textinfo.Text = data?.Timestamp.ToString() ?? "N/A";
                    _textHR.Text = data?.Value.ToString() ?? "---";

                    _textBattery.Text = data?.BatteryLevel?.ToString() ?? "N/A";
                    _textinfo2.Text = data?.TimestampBatteryLevel?.ToString() ?? "---";
                });

                await Task.Delay(500);
            }
        }

        private void Button_stop_hr_Click(object sender, EventArgs e)
        {
            _progressWorking.Visibility = ViewStates.Invisible;
            _heartRate.Stop();
        }
        string BLE = typeof(HeartRateEnumeratorAndroid).ToString();
        string MSBand = typeof(HeartRateEnumeratorMSBand).ToString();

        private void Button_start_hr_Click(object sender, EventArgs e)
        {
            

            string[] split = _deviceName.Split(':');

            if (split[0] == BLE)
            {
                var enumerator = new HeartRateEnumeratorAndroid();
                _heartRate = enumerator.GetHeartRate(split[1]);
                _heartRate.Start();
                _progressWorking.Visibility = ViewStates.Visible;
            }
            else if (split[0] == MSBand)
            {
                var enumerator = new HeartRateEnumeratorMSBand();
                _heartRate = enumerator.GetHeartRate(split[1]);
                _heartRate.Start();
                _progressWorking.Visibility = ViewStates.Visible;
            }

        }
    }
}
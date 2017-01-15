using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using CaledosLab.Runner.Android.Specific;
using CaledosLab.Runner.Commons.Abstractions;

namespace nicold.heartrate.Activities
{
    [Activity(Label = "nicold.heartrate.activities.main", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        private ListView _listDevices;
        ProgressBar _progressWorking;
        ProgressBar _progressWorking_msband;
        private ArrayAdapter<string> listAdapter;

        private IHeartRateEnumerator _hrEnumerator;
        private IMicrosoftBandEnumerator _msBandEnumerator;

        private string selected = "";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            listAdapter = new ArrayAdapter<string>(this, Resource.Layout.simplerow);
            _listDevices = FindViewById<ListView>(Resource.Id.list_devices);
            _listDevices.Adapter=listAdapter;
            _listDevices.ItemClick += ListDevices_ItemClick;

            Button button_start_scan = FindViewById<Button>(Resource.Id.button_start_scan_ble);
            button_start_scan.Click += Button_start_scan_Click;

            Button button_stop_scan = FindViewById<Button>(Resource.Id.button_stop_scan_ble);
            button_stop_scan.Click += Button_stop_scan_Click;

            _progressWorking = FindViewById<ProgressBar>(Resource.Id.progress_work);


            Button button_start_scan_msband = FindViewById<Button>(Resource.Id.button_start_scan_msband);
            button_start_scan_msband.Click += Button_start_scan_msband_Click;

            Button button_stop_scan_msband = FindViewById<Button>(Resource.Id.button_stop_scan_msband);
            button_stop_scan_msband.Click += Button_stop_scan_msband_Click;

            _progressWorking_msband = FindViewById<ProgressBar>(Resource.Id.progress_work_msband);
        }

        private void Button_start_scan_Click(object sender, EventArgs e)
        {
            if (_hrEnumerator == null)
            {
                _progressWorking.Visibility = ViewStates.Visible;

                _hrEnumerator = new HeartRateEnumeratorAndroid();
                _hrEnumerator.DeviceScanUpdate += _hrEnumerator_DeviceScanUpdate;
                _hrEnumerator.DeviceScanTimeout += _hrEnumerator_DeviceScanTimeout;
                _hrEnumerator.StartDeviceScan();

                listAdapter.Clear();
                listAdapter.Add($"> ble start");
            }
        }

        private void Button_stop_scan_Click(object sender, EventArgs e)
        {
            _progressWorking.Visibility = ViewStates.Invisible;
            if (_hrEnumerator != null)
            {
                _hrEnumerator.DeviceScanUpdate -= _hrEnumerator_DeviceScanUpdate;
                _hrEnumerator.DeviceScanTimeout -= _hrEnumerator_DeviceScanTimeout;
                _hrEnumerator.StopDeviceScan();
                _hrEnumerator = null;
                listAdapter.Add($"> ble stop");
            }
        }

        private void Button_start_scan_msband_Click(object sender, EventArgs e)
        {
            if (_hrEnumerator == null)
            {
                _progressWorking.Visibility = ViewStates.Visible;

                _msBandEnumerator = new MicrosoftBandEnumerator();
                _msBandEnumerator.DeviceScanUpdate += _hrEnumerator_DeviceScanUpdate;
                _msBandEnumerator.DeviceScanTimeout += _hrEnumerator_DeviceScanTimeout;
                _msBandEnumerator.StartDeviceScan();

                listAdapter.Clear();
                listAdapter.Add($"> msband start");
            }
        }

        private void Button_stop_scan_msband_Click(object sender, EventArgs e)
        {
            _progressWorking.Visibility = ViewStates.Invisible;
            if (_msBandEnumerator != null)
            {
                _msBandEnumerator.DeviceScanUpdate -= _hrEnumerator_DeviceScanUpdate;
                _msBandEnumerator.DeviceScanTimeout -= _hrEnumerator_DeviceScanTimeout;
                _msBandEnumerator.StopDeviceScan();
                _msBandEnumerator = null;
                listAdapter.Add($"> msband stop");
            }
        }

        private void _hrEnumerator_DeviceScanTimeout(object sender, EventArgs e)
        {
            listAdapter.Add($"> timeout {sender.GetType().ToString()}");
            Button_stop_scan_Click(null, null);
        }

        private void _hrEnumerator_DeviceScanUpdate(object sender, string deviceName)
        {
            listAdapter.Add($"{sender.GetType().ToString()}:{deviceName}");
        }



        private void ListDevices_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Button_stop_scan_Click(sender, e);

            selected = (string)_listDevices.GetItemAtPosition(e.Position);

            var activityHeart = new Intent(this, typeof(HeartActivity));
            activityHeart.PutExtra($"device", selected);
            StartActivity(activityHeart);
        }
    }
}


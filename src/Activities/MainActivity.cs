using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace nicold.heartrate.Activities
{
    [Activity(Label = "nicold.heartrate.main", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private ListView _listDevices;
        ProgressBar _progressWorking;
        private ArrayAdapter<string> listAdapter;

        private HeartRateEnumeratorAndroid _hrEnumerator;

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

            Button button_start_scan = FindViewById<Button>(Resource.Id.button_start_scan);
            button_start_scan.Click += Button_start_scan_Click;

            Button button_stop_scan = FindViewById<Button>(Resource.Id.button_stop_scan);
            button_stop_scan.Click += Button_stop_scan_Click;

            _progressWorking = FindViewById<ProgressBar>(Resource.Id.progress_work);
        }

        private void Button_start_scan_Click(object sender, EventArgs e)
        {
            if (_hrEnumerator == null)
            {
                _progressWorking.Visibility = ViewStates.Visible;

                _hrEnumerator = new heartrate.HeartRateEnumeratorAndroid();
                _hrEnumerator.DeviceScanUpdate += _hrEnumerator_DeviceScanUpdate;
                _hrEnumerator.DeviceScanTimeout += _hrEnumerator_DeviceScanTimeout;
                _hrEnumerator.StartDeviceScan();

                listAdapter.Clear();
                listAdapter.Add($"> start");
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
                listAdapter.Add($"> stop");
            }
        }

        private void _hrEnumerator_DeviceScanTimeout(object sender, EventArgs e)
        {
            listAdapter.Add($"> timeout");
            Button_stop_scan_Click(null, null);
        }

        private void _hrEnumerator_DeviceScanUpdate(object sender, Plugin.BLE.Abstractions.Contracts.IDevice e)
        {
            listAdapter.Add($"{e.Name}");
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


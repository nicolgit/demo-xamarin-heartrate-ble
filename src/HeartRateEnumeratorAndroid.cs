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
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace nicold.heartrate
{
    public class HeartRateEnumeratorAndroid : IHeartRateEnumerator
    {
        private Plugin.BLE.Abstractions.Contracts.IAdapter _adapter;
        private List<IDevice> _devices;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<IDevice> DeviceScanUpdate;
        public event EventHandler DeviceScanTimeout;

        public bool StartDeviceScan()
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
            _devices = new List<IDevice>();
            _cancellationTokenSource = new CancellationTokenSource();

            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
            _adapter.ScanTimeout = 10000; // millisecondi

            foreach(var dev in _adapter.ConnectedDevices)
            {
                _adapter_DeviceDiscovered(dev);
            }

            _adapter.StartScanningForDevicesAsync(serviceUuids: null, deviceFilter: null, cancellationToken: _cancellationTokenSource.Token);
            return true;
        }

        private void _adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            DeviceScanTimeout?.Invoke(this,null);
        }

        private void _adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            _adapter_DeviceDiscovered(e.Device);
        }

        private void _adapter_DeviceDiscovered(IDevice device)
        {
            _devices?.Add(device);
            DeviceScanUpdate?.Invoke(this, device);
        }

        public bool StopDeviceScan()
        {
            _cancellationTokenSource?.Cancel(true);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _adapter.DeviceDiscovered -= _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed -= _adapter_ScanTimeoutElapsed;
            return true;
        }
    }
}
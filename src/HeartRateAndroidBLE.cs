using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE;
using System.Threading;

namespace nicold.heartrate
{
    class HeartRateAndroidBLE : IHeartRate
    {
        private Guid SERVICE_HEARTRATE = Guid.ParseExact("0000180d-0000-1000-8000-00805f9b34fb", "d");
        private Guid SERVICE_BATTERY =   Guid.ParseExact("0000180f-0000-1000-8000-00805f9b34fb", "d");
        private Guid CHARACTERISTIC_HEARTRATE = Guid.ParseExact("00002a37-0000-1000-8000-00805f9b34fb", "d");
        private Guid CHARACTERISTIC_BATTERYLEVEL = Guid.ParseExact("00002a19-0000-1000-8000-00805f9b34fb", "d");

        private IBluetoothLE _ble;
        private Plugin.BLE.Abstractions.Contracts.IAdapter _adapter;
        private CancellationTokenSource _cancellationTokenSource;
        private int _connectionRetry;

        private IDevice _device;
        private IService _serviceHR;
        private IService _serviceBattery;
        private ICharacteristic _characteristicHR;
        private ICharacteristic _characteristicBatteryLevel;

        private HeartRateData _currentValue;

        private const int MAX_RETRY= 5;
        private string _deviceName = "";
        private enum Status
        {
           STATUS_STARTED = 1,
           STATUS_SCANNING_FOR_DEVICE,
           STATUS_CONNECTED,
           STATUS_DEAD
        };
        private Status _status;

        public HeartRateAndroidBLE()
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
            _ble = CrossBluetoothLE.Current;

            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
            _adapter.DeviceConnectionLost += _adapter_DeviceConnectionLost;
        }

        public HeartRateData GetCurrentHeartRateValue()
        {
            return _currentValue;
        }

        public bool Start(string deviceId)
        {
            _deviceName = deviceId;

            _device = null;
            _serviceHR = null;
            _serviceBattery = null;
            _characteristicHR = null;
            _characteristicBatteryLevel = null;

            _status = Status.STATUS_STARTED;

            while (true)
            {
                switch (_status)
                {
                    case Status.STATUS_STARTED:
                        _scanningForDevice();
                        break;
                    case Status.STATUS_SCANNING_FOR_DEVICE:
                        break;
                    case Status.STATUS_CONNECTED:
                        break;
                    case Status.STATUS_DEAD:
                        return true; //esce
                }
            }
        }

        public async void Stop()
        {
            switch (_status)
            {
                case Status.STATUS_STARTED:
                    break;
                case Status.STATUS_SCANNING_FOR_DEVICE:
                    _cancelScanningForDevice();
                    break;
                case Status.STATUS_CONNECTED:
                    await _disconnect();
                    break;
                case Status.STATUS_DEAD:
                    break;
            }

            _status = Status.STATUS_DEAD;
            return;
        }

        private async Task _disconnect()
        {
            try
            {
                _characteristicHR.ValueUpdated -= CharacteristicHR_ValueUpdated;
                _characteristicHR.StopUpdates();

                await _adapter.DisconnectDeviceAsync(_device);

                _connectionRetry = 0;
                _device = null;
                _serviceHR = null;
                _serviceBattery = null;
                _characteristicHR = null;
                _characteristicBatteryLevel = null;
                _currentValue = null;

            }
            catch (Exception e)
            {
                Android.Util.Log.Debug("_adapter_DeviceConnectionLost", e.Message);
            }
        }

        private void _scanningForDevice()
        {
            Android.Util.Log.Debug("_scanningForDevice", "");

            try
            {
                _status = Status.STATUS_SCANNING_FOR_DEVICE;

                _connectionRetry = 0;
                _cancellationTokenSource = new CancellationTokenSource();

                _adapter.ScanTimeout = 10000; //millisecondi
                _adapter.StartScanningForDevicesAsync(serviceUuids: null, deviceFilter: null, cancellationToken: _cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Android.Util.Log.Debug("_adapter_DeviceConnectionLost", e.Message);
            }   
            
            return;
        }

        private void _cancelScanningForDevice()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private void _adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            Android.Util.Log.Debug("_adapter_ScanTimeoutElapsed", $"TIMEOUT iteration {_connectionRetry}");

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (_connectionRetry<MAX_RETRY)
            {
                _connectionRetry++;
                _cancellationTokenSource = new CancellationTokenSource();
                _adapter.StartScanningForDevicesAsync(serviceUuids: null, deviceFilter: null, cancellationToken: _cancellationTokenSource.Token);
            }
            else
            {
                _status = Status.STATUS_DEAD;
            }
        }

        private async void _adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            Android.Util.Log.Debug("_adapter_DeviceDiscovered", $"Device {e.Device.Name}");

            if (e.Device.Name == _deviceName)
            {
                try
                {
                    _device = e.Device;

                    await _adapter.ConnectToDeviceAsync(_device);
                    _serviceHR = await e.Device.GetServiceAsync(SERVICE_HEARTRATE);
                    _characteristicHR = await _serviceHR?.GetCharacteristicAsync(CHARACTERISTIC_HEARTRATE);

                    _serviceBattery = await e.Device.GetServiceAsync(SERVICE_BATTERY);
                    _characteristicBatteryLevel = await _serviceBattery?.GetCharacteristicAsync(CHARACTERISTIC_BATTERYLEVEL);

                    if (_characteristicHR != null)
                    {
                        _status = Status.STATUS_CONNECTED;

                        _cancelScanningForDevice();

                        _characteristicHR.ValueUpdated += CharacteristicHR_ValueUpdated;
                        _characteristicHR.StartUpdates();
                    }
                }
                catch (Exception err)
                {
                    Android.Util.Log.Debug("_adapter_DeviceDiscovered", err.Message);
                }
            }
        }
        
        private async void _adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            Android.Util.Log.Debug("_adapter_DeviceConnectionLost", $"Device {e.Device.Name}");
            await _disconnect();
            _status = Status.STATUS_STARTED;
        }

        private async void CharacteristicHR_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs args)
        {
            // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.heart_rate_measurement.xml
            // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.battery_level.xml

            try
            {
                int heartValue;
                var bytes = args.Characteristic.Value;

                heartValue = bytes[1];

                if (_currentValue == null)
                {
                    _currentValue = new heartrate.HeartRateData();
                }
                
                _currentValue.Value = bytes[1];
                _currentValue.Timestamp = DateTime.Now;

                if (_currentValue.TimestampBatteryLevel ==null ||
                    (DateTime.Now-_currentValue.TimestampBatteryLevel).Value.Minutes>1)
                {
                    var batteryLevelByte = await _characteristicBatteryLevel?.ReadAsync();

                    _currentValue.TimestampBatteryLevel = DateTime.Now;
                    _currentValue.BatteryLevel = batteryLevelByte[0];
                }
                
                Android.Util.Log.Debug("CharacteristicHR_ValueUpdated", $"Device {_currentValue.Value}");
            }
            catch (Exception e)
            {
                Android.Util.Log.Debug("CharacteristicHR_ValueUpdated", e.Message);
            }
            
        }
    }
}
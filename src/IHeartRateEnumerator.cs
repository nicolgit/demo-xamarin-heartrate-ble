using System;
using Plugin.BLE.Abstractions.Contracts;

namespace CaledosLab.Runner.Commons.Abstractions
{
    interface IHeartRateEnumerator
    {
        bool StartDeviceScan();
        bool StopDeviceScan();
        event EventHandler<string> DeviceScanUpdate;
        event EventHandler DeviceScanTimeout;
    }
}
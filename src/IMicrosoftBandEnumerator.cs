using System;

namespace CaledosLab.Runner.Commons.Abstractions
{
    public interface IMicrosoftBandEnumerator
    {
        bool StartDeviceScan();
        bool StopDeviceScan();
        event EventHandler<string> DeviceScanUpdate;
        event EventHandler DeviceScanTimeout;

        IHeartRate GetHeartRate(string name);
    }
}
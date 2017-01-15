using System;
using System.Threading.Tasks;

namespace CaledosLab.Runner.Commons.Abstractions
{
    public interface IMicrosoftBandEnumerator
    {
        Task<string> GetMicrosoftBandName();
        bool StartDeviceScan();
        bool StopDeviceScan();
        event EventHandler<string> DeviceScanUpdate;
        event EventHandler DeviceScanTimeout;

        IHeartRate GetHeartRate(string name);
        IStepSensor GetStepSensor(string name);
    }
}
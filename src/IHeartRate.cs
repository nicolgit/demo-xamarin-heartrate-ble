using System;

namespace CaledosLab.Runner.Commons.Abstractions
{
    public class HeartRateData
    {
        public DateTime Timestamp;
        public int Value;
        public DateTime? TimestampBatteryLevel;
        public int? BatteryLevel;
    }
    
    public interface IHeartRate
    {
        bool Start(string deviceName);
        bool IsRunning { get; }
        void Stop();
        HeartRateData GetCurrentHeartRateValue();
    }
}
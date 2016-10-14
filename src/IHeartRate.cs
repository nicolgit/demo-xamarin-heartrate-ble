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

namespace nicold.heartrate
{
    public class HeartRateData
    {
        public DateTime Timestamp;
        public int Value;
        public int? BatteryLevel;
    }
    
    interface IHeartRate
    {
        bool Start(string deviceName);
        void Stop();
        HeartRateData GetCurrentHeartRateValue();
    }
}
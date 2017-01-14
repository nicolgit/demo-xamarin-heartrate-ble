using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaledosLab.Runner.Commons.Abstractions
{
    // http://motzcod.es/post/82515321689/part-1-my-stepcounter-android-step-sensors

    public interface IStepSensor
    {
        string DeviceName { get; }
        int CurrentStepCount { get; }
        DateTime LastEventDateTime { get; }
        void Start();
        void Stop();
        void Reset();
    }

    public interface IStepSensorEnumerator
    {
        IEnumerable<IStepSensor> enumerator { get; }
        void Reset();

        IStepSensor GetStepSensorByName(string name);
    }
}

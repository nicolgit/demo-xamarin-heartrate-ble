using CaledosLab.Runner.Commons.Abstractions;
using Microsoft.Band.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaledosLab.Runner.Android.Specific
{
    public class MicrosoftBand : IHeartRate, IStepSensor
    {
        string name = "";
        public string DeviceName
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        BandClient bandClient = null;

        async void SearchBand()
        {
            try
            {
                var bandClientManager = BandClientManager.Instance;
                // query the service for paired devices

                var pairedBands = await bandClientManager.GetPairedBandsAsync();
                // connect to the first device
                var bandInfo = pairedBands.FirstOrDefault();
                bandClient = await bandClientManager.ConnectAsync(bandInfo);

                if (bandClient.SensorManager.HeartRate.UserConsented == UserConsent.Unspecified ||
                    bandClient.SensorManager.HeartRate.UserConsented == UserConsent.Declined)
                    await bandClient.SensorManager.HeartRate.RequestUserConsent();

                if (bandClient.SensorManager.HeartRate.UserConsented == UserConsent.Granted)
                {
                    await bandClient.SensorManager.HeartRate.StartReadingsAsync();
                    bandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;
                }

                
                await bandClient.SensorManager.Pedometer.StartReadingsAsync();
                bandClient.SensorManager.Pedometer.ReadingChanged += Pedometer_ReadingChanged;                


            }
            catch
            {
                
            }
        }

        long totalSteps = -1;
        long firstRead = -1;
        private void Pedometer_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandPedometerReading> e)
        {
            System.Diagnostics.Debug.WriteLine("Total steps: " + e.SensorReading.TotalSteps.ToString());
            totalSteps = e.SensorReading.TotalSteps;
            if (firstRead < 0)
                firstRead = totalSteps;
        }

        HeartRateData currentData = null;
        private void HeartRate_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandHeartRateReading> e)
        {
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    point = point == "." ? "" : ".";
            //    HeartLabel.Text = "Heart Rate: " + e.SensorReading.HeartRate.ToString() + point;
             System.Diagnostics.Debug.WriteLine("Heart rate: " + e.SensorReading.HeartRate.ToString());
            //});

            currentData = new HeartRateData
            {
                Timestamp = DateTime.Now,
                Value = e.SensorReading.HeartRate
            };
        }

        public bool IsRunning
        {
            get
            {
                lock(this)
                {
                    if (bandClient != null)
                        return bandClient.IsConnected;
                    else
                        return false;
                }
            }
        }

        public int CurrentStepCount
        {
            get
            {
                return totalSteps<0 ? 0 : (int)(totalSteps - firstRead);
            }
        }

        public DateTime LastEventDateTime
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public HeartRateData GetCurrentHeartRateValue()
        {
            return currentData;
        }

        public bool Start()
        {
            Stop();
            SearchBand();
            return true;
        }

        public void Stop()
        {
            lock (this)
            {
                try
                {
                    if (bandClient != null)
                    {
                        bandClient.SensorManager.HeartRate.StopReadingsAsync();
                        bandClient.SensorManager.HeartRate.ReadingChanged -= HeartRate_ReadingChanged;

                        bandClient.SensorManager.Pedometer.StopReadingsAsync();
                        bandClient.SensorManager.Pedometer.ReadingChanged -= Pedometer_ReadingChanged;

                        bandClient = null;
                        currentData = null;
                    }
                }
                catch
                {

                }
            }
        }

        void IStepSensor.Start()
        {
            ((IHeartRate)this).Start();
        }

        public void Reset()
        {
            firstRead = totalSteps;
        }
    }
}
using CaledosLab.Runner.Commons.Abstractions;
using Microsoft.Band.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSBandTest.Droid
{
    class HeartRateMSBand : IHeartRate
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

            }
            catch
            {
                
            }
        }

        HeartRateData currentData = null;
        private void HeartRate_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandHeartRateReading> e)
        {
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    point = point == "." ? "" : ".";
            //    HeartLabel.Text = "Heart Rate: " + e.SensorReading.HeartRate.ToString() + point;
             System.Diagnostics.Debug.WriteLine(e.SensorReading.HeartRate.ToString());
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
                        bandClient = null;
                        currentData = null;
                    }
                }
                catch
                {

                }
            }
        }
    }
}
using CaledosLab.Runner.Commons.Abstractions;
using Microsoft.Band.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaledosLab.Runner.Android.Specific
{
    public class MicrosoftBandEnumerator : IMicrosoftBandEnumerator
    {
        private MicrosoftBand _microsoftBand = new MicrosoftBand();

        public event EventHandler DeviceScanTimeout;
        public event EventHandler<string> DeviceScanUpdate;

        public bool StartDeviceScan()
        {
            SearchBand();
            return true;
        }

        async void SearchBand()
        {
            try { 
                var bandClientManager = BandClientManager.Instance;
                // query the service for paired devices

                var pairedBands = await bandClientManager.GetPairedBandsAsync();
                // connect to the first device
                var bandInfo = pairedBands.FirstOrDefault();
                var bandClient = await bandClientManager.ConnectAsync(bandInfo);

                DeviceScanUpdate?.Invoke(this,bandInfo.Name);
                await Task.Delay(3000);
                DeviceScanTimeout?.Invoke(this, null);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                await Task.Delay(3000);
                DeviceScanTimeout?.Invoke(this, null);
            }
        }


        public bool StopDeviceScan()
        {
            return false;
        }

        public async Task<string> GetMicrosoftBandName()
        {
            string name;

            try
            {
                var bandClientManager = BandClientManager.Instance;
                // query the service for paired devices

                var pairedBands = await bandClientManager.GetPairedBandsAsync();

                // connect to the first device
                var bandInfo = pairedBands.FirstOrDefault();
                var bandClient = await bandClientManager.ConnectAsync(bandInfo);

                name = bandInfo.Name;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                name = null;
            }

            return name;
        }

        public IHeartRate GetHeartRate(string name)
        {
            return _microsoftBand;
        }

        public IStepSensor GetStepSensor(string name)
        {
            return _microsoftBand;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{

    class TemperatureConfig { }

    class SerialKingmarchTemperature : SerialPortDevice
    {
        private TemperatureConfig config;
        private ManualResetEvent mre;
        private double result1;
        private double result2;
        private string errMsg;
        private bool isSuccess;
        private string id;
        public SerialKingmarchTemperature(string portName, int baudrate, TemperatureConfig config, int timeout, string id) : base(portName, baudrate)
        {
            this.id = id;
            this.config = config;
            this.mre = new ManualResetEvent(false);
        }

        public override AcquisitionResult Acquisit()
        {
            AcquisitionResult ar;

            try
            {
                this.Start();
            }
            catch (Exception ex)
            {
                ar = new AcquisitionResult(false, ex.Message, 0, 0);
                return ar;
            }

            byte[] frame = GetAcquisitionFrame();
            this.SendFrame(frame);
            bool receivedEvent = this.mre.WaitOne(10 * 1000);
            if (receivedEvent)
            {
                if (isSuccess)
                {
                    ar = new AcquisitionResult(true, "", result1, result2);
                }
                else
                {
                    ar = new AcquisitionResult(false, errMsg, 0, 0);
                }
            }
            else
            {
                this.Stop();
                ar = new AcquisitionResult(false, "Timeout", 0, 0);
            }
            mre.Reset();
            return ar;
        }
        public override string GetObjectType()
        {
            return "SerialKingmarchTemperature";
        }

        private byte[] GetAcquisitionFrame()
        {
            string frame = "#" + this.id + "A!";
            byte[] arr = System.Text.Encoding.Default.GetBytes(frame);
            
            return arr;
        }

        public override void ProcessData(byte[] buffer, int length)
        {
            //frame
            // 24 37 38 EF 35 32 33 30 38 30 30 33 95 36 31 38 34 D3 33 35 37 34 D3 30 B0 21
            // $   7  8  ?  5  2  3  0  8  0  0  3  .  6  1  8  4  ?  3  5  7  4  ?  0  ?  ?

            this.isSuccess = false;

            byte head = 36;

            int indexOfHead = Array.IndexOf(buffer,head);

            int frameLength = length - indexOfHead - 1;
            byte[] frame = new byte[frameLength];

            Array.Copy(buffer, indexOfHead+1, frame, 0, frameLength);

            for (int i = 0; i < frame.Length; i++)
            {
                if (frame[i] > 0x39)
                {
                    frame[i] = (byte)'$';
                }
            }

            string received = System.Text.Encoding.Default.GetString(frame);

            string[] splited = received.Split('$');

            if(splited.Length >= 4)
            {
                this.result1 = Int32.Parse(splited[2]) / 100.0 - 40;
                this.result2 = Int32.Parse(splited[3]) / 100.0;
                this.isSuccess = true;
            }
            else
            {
                this.errMsg = "broken frame";
            }
            this.mre.Set();
        }
    }
}

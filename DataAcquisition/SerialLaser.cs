using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    class LaserConfig { }

    class SerialLaser:SerialPortDevice
    {
        private LaserConfig config;
        private ManualResetEvent mre;
        private double result;
        private string errMsg;
        private bool isSuccess;
        public SerialLaser(string portName,int baudrate,LaserConfig config,int timeout):base(portName,baudrate){
            this.config = config;
            this.mre = new ManualResetEvent(false);
        }

        public override AcquisitionResult Acquisit()
        {
            AcquisitionResult ar;

            try
            {
                this.Start();
            } catch(Exception ex)
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
                    ar = new AcquisitionResult(true, "", result, 0);
                }
                else
                {
                    ar = new AcquisitionResult(false, errMsg, result, 0);
                }
            }
            else
            {
                this.Stop();
                ar = new AcquisitionResult(false,"Timeout", 0, 0);
            }
            mre.Reset();
            return ar;
        }
        public override string GetObjectType()
        {
            return "SerialLaser";
        }

        private byte[] GetAcquisitionFrame()
        {
            byte[] arr = new byte[1];
            arr[0] = (byte)'O';
            return arr;
        }

        public override void ProcessData(byte[] buffer,int length)
        {
            this.isSuccess = false;

            if (length == 7)
            {
                if(buffer[0] == 0xFF)
                {
                    this.result = buffer[1] * 100 + buffer[2] * 10 + buffer[3] + buffer[4] * 0.1 + buffer[5] * 0.01 + buffer[6] * 0.001;
                    this.isSuccess = true;
                }
                else
                {
                    this.errMsg = "broken frame";
                }
            }
            else
            {
                this.errMsg = "broken frame";
            }
            this.mre.Set();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    //class LaserConfig { }

    class SerialKingMarSettlement : SerialPortDevice
    {
        private LaserConfig config;
        private ManualResetEvent mre;
        private double result;
        private string errMsg;
        private bool isSuccess;
        private int timeout;
        private string id;
        public SerialKingMarSettlement(string portName, int baudrate, LaserConfig config, int timeout,string id) : base(portName, baudrate)
        {
            this.config = config;
            this.id = id;
            this.timeout = timeout;
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
            bool receivedEvent = this.mre.WaitOne(this.timeout * 1000);
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
                ar = new AcquisitionResult(false, "Timeout", 0, 0);
            }
            mre.Reset();
            return ar;
        }

        public override string GetObjectType()
        {
            return "SerialKingMarSettlement";
        }

        private byte[] GetAcquisitionFrame()
        {

            //string id = "#+703057";

            byte[] frame = System.Text.Encoding.Default.GetBytes("#+"+this.id);

            return frame;
        }

        public override void ProcessData(byte[] buffer, int length)
        {
            this.isSuccess = false;

            if (length == 9)
            {
                //数据头'$'
                if (buffer[0] == 0x24)
                {
                    this.result = ((buffer[1] - 48) * 16 + (buffer[2] - 48) + (buffer[3] - 48) * 16 * 16 * 16 + (buffer[4] - 48) * 16 * 16) / 100.0;
                    
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
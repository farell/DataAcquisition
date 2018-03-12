using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    //class LaserConfig { }

    class SerialInclination : SerialPortDevice
    {
        private LaserConfig config;
        private ManualResetEvent mre;
        private double result1;
        private double result2;
        private string errMsg;
        private bool isSuccess;
        public SerialInclination(string portName, int baudrate, LaserConfig config, int timeout) : base(portName, baudrate)
        {
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
            return "SerialInclination";
        }

        private bool InclinationFrameCheck(byte[] buffer,int length)
        {
            if (length != 17)
            {
                return false;
            }

            byte sum = 0;

            for (int i = 1; i < 16; i++)
            {
                sum = (byte)(sum + buffer[i]);
            }

            if (sum == buffer[16])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private byte[] GetAcquisitionFrame()
        {
            byte[] arr = GetReadInclinationFrame(1);
            return arr;
        }

        public byte[] GetReadInclinationFrame(byte address)
        {
            byte cmd = 0x04;
            byte[] frame = new byte[5];

            frame[0] = 0x68;
            frame[1] = 0x04;
            frame[2] = address;
            frame[3] = cmd;
            frame[4] = (byte)(frame[1] + frame[2] + frame[3]);

            return frame;
        }

        public override void ProcessData(byte[] buffer, int length)
        {
            this.isSuccess = false;

            if (InclinationFrameCheck(buffer,length) == true)
            {
                try
                {
                    string deviceId = buffer[2].ToString();
                    string debugMessage = "";
                    debugMessage += "Device ID : " + deviceId + "\r\n";

                    byte[] x = new byte[4];
                    byte[] y = new byte[4];

                    Array.Copy(buffer, 4, x, 0, 4);
                    Array.Copy(buffer, 4 + 4, y, 0, 4);
                    double x_angle = packedBCD2Double(x);
                    double y_angle = packedBCD2Double(y);
                    x_angle = Math.Round(x_angle, 3);
                    y_angle = Math.Round(y_angle, 3);
                    debugMessage += "raw x_angle : " + x_angle + "\r\n";
                    debugMessage += "raw y_angle : " + y_angle + "\r\n";

                    this.isSuccess = true;
                    this.result1 = x_angle;
                    this.result2 = y_angle;
                }
                catch (Exception ex)
                {
                    this.errMsg = ex.Message;
                }
            }
            else
            {
                this.errMsg = "broken frame";
            }
            this.mre.Set();          
        }

        private double packedBCD2Double(byte[] array)
        {
            double result = 0;
            byte[] temp = new byte[8];
            for (int i = 0; i < array.Length; i++)
            {
                temp[i * 2] = (byte)(array[i] >> 4);
                temp[i * 2 + 1] = (byte)(array[i] & 0x0f);
            }
            result = temp[1] * 100 + temp[2] * 10 + temp[3] + temp[4] * 0.1 + temp[5] * 0.01 + temp[6] * 0.001 + temp[7] * 0.0001;

            if (temp[0] == 1)//符号位
            {
                result = -result;
            }

            //MessageBox.Show(result.ToString());
            return result;
        }
    }
}
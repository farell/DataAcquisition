using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;

namespace DataAcquisition
{
    class AcquisitionResult
    {
        public bool IsSuccess;
        public string ErrMsg;
        public double Result1;
        public double Result2;

        public AcquisitionResult(bool success,string msg,double r1,double r2)
        {
            this.IsSuccess = success;
            this.ErrMsg = msg;
            this.Result1 = r1;
            this.Result2 = r2;
        }
        public AcquisitionResult() { }
    }

    class SerialPortDevice
    {
        protected SerialPort comPort;
        private byte[] buffer;
        private string portName;
        private int baudrate;

        public SerialPortDevice(string portName,int baudrate)
        {
            comPort = new SerialPort(portName, baudrate, Parity.None, 8, StopBits.One);
            comPort.DataReceived += new SerialDataReceivedEventHandler(ComDataReceive);
            this.buffer = new byte[1024 * 4];
            this.portName = portName;
            this.baudrate = baudrate;
        }

        public void Start()
        {
            if (!comPort.IsOpen)
            {
                comPort.Open();
            }
        }

        public void SendFrame(byte[] buffer)
        {
            if (comPort.IsOpen)
            {
                comPort.Write(buffer, 0, buffer.Length);
            }            
        }

        public virtual AcquisitionResult Acquisit() {  return new AcquisitionResult();  }
        public virtual string GetObjectType() { return ""; }
        //public virtual byte[] GetAcquisitionFrame() { return new byte[0]; }
        public virtual void ProcessData(byte[] buffer, int length) {}
        public void Stop()
        {
            if (comPort.IsOpen)
            {
                comPort.Close();
            }
        }

        private void ComDataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            //lock (sender)
            {
                //int bytesToRead = this.comPort.BytesToRead;
                int bytesRead = 0;
                try
                {
                    bytesRead = this.comPort.Read(this.buffer, 0, this.comPort.ReadBufferSize);
                    if (bytesRead > 0)
                    {
                        this.ProcessData(this.buffer, bytesRead);
                    }
                    Stop();
                }
                catch (Exception ex)
                {
                    Stop();
                    using (StreamWriter sw = new StreamWriter(@"ErrLog\ErrLog.txt", true))
                    {
                        sw.WriteLine(this.portName + " bytesToRead :" + bytesRead + "\n");
                        sw.WriteLine(ex.ToString());
                        sw.WriteLine("---------------------------------------------------------");
                        sw.Close();
                    }
                }

            }
        }
    }
}

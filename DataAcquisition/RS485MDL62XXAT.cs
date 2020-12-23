using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    class RS485MDL62XXAT : IMonitorDevice
    {
        private static readonly string[] availablePorts = SerialPort.GetPortNames();
        protected SerialPort comPort;
        private byte[] buffer;
        private string portName;
        private int baudrate;
        private int bytesRead;
        private int bytesTotall;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ManualResetEvent mre;
        private double innage;
        private string errMsg;
        private bool isSuccess;
        private int timeout;
        private string deviceId;
        private string sensorId;
        private MDL62XXAT_Data data;
        private const int frameLength = 9;
        private SerialDataReceivedEventHandler dataRecieveHandler;
        private SerialErrorReceivedEventHandler errorRecieveHandler;

        public RS485MDL62XXAT(string portName, int baudrate, int timeout, string deviceId, string sensorId)
        {
            comPort = new SerialPort(portName, baudrate, Parity.None, 8, StopBits.One);
            dataRecieveHandler = new SerialDataReceivedEventHandler(ComDataReceive);
            comPort.DataReceived += dataRecieveHandler;
            errorRecieveHandler = new SerialErrorReceivedEventHandler(ComErrorReceive);
            comPort.ErrorReceived += errorRecieveHandler;
            this.buffer = new byte[1024 * 4];
            this.portName = portName;
            this.baudrate = baudrate;
            this.bytesRead = 0;
            this.bytesTotall = 0;

            this.sensorId = sensorId;
            this.deviceId = deviceId;
            this.timeout = timeout;
            this.mre = new ManualResetEvent(false);
            this.data = new MDL62XXAT_Data();
            this.isSuccess = false;
        }

        private void ComErrorReceive(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        public void ClosePort()
        {
            comPort.DtrEnable = false;
            comPort.RtsEnable = false;
            comPort.ErrorReceived -= errorRecieveHandler;
            comPort.DataReceived -= dataRecieveHandler;
            Thread.Sleep(500);comPort.Close();
        }

        public void OpenPort()
        {
            comPort.DtrEnable = false;
            comPort.RtsEnable = false;
            comPort.ErrorReceived += errorRecieveHandler;
            comPort.DataReceived += dataRecieveHandler;
            comPort.Open();
        }

        private void ComDataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                bytesRead = this.comPort.Read(this.buffer, bytesTotall, 100);
                bytesTotall += bytesRead;
                if (bytesTotall >= frameLength)
                {
                    this.ProcessData(this.buffer, bytesTotall);
                    this.bytesRead = 0;
                    this.bytesTotall = 0;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                this.bytesRead = 0;
                this.bytesTotall = 0;
            }
        }

        public void ProcessData(byte[] buffer, int length)
        {
            this.isSuccess = false;

            if (length == 9)
            {
                //数据头'$'
                if (buffer[0] == 0x24)
                {
                    this.innage = ((buffer[1] - 48) * 16 + (buffer[2] - 48) + (buffer[3] - 48) * 16 * 16 * 16 + (buffer[4] - 48) * 16 * 16) / 100.0;
                    this.data.SensorId = sensorId;
                    this.data.TimeStamp = "";
                    this.data.Innage = this.innage;
                    this.isSuccess = true;
                }
                else
                {
                    log.Warn(this.sensorId + " broken frame: " + ModbusUtility.BytesToHexString(buffer, length));
                    this.errMsg = "broken frame: " + ModbusUtility.BytesToHexString(buffer, length);
                }
            }
            else
            {
                log.Warn(this.sensorId + " broken frame: " + ModbusUtility.BytesToHexString(buffer, length));
                this.errMsg = "broken frame: " + ModbusUtility.BytesToHexString(buffer, length);
            }
            this.mre.Set();
        }

        public bool Acquisit()
        {
            this.isSuccess = false;
            bool ret;

            if (!availablePorts.Contains<string>(this.portName))
            {
                this.errMsg = this.portName + " not exist";
                log.Error(errMsg);
                return false;
            }
            if (comPort.IsOpen)
            {
                this.errMsg = this.portName + " is open by another process";
                log.Error(errMsg);
                return false;
            }
            try
            {
                this.bytesRead = 0;
                this.bytesTotall = 0;
                //OpenPort();

                byte[] frame = GetAcquisitionFrame();
                this.SendFrame(frame);
                bool receivedEvent = this.mre.WaitOne(this.timeout * 1000);
                if (receivedEvent)
                {
                    if (isSuccess)
                    {
                        ret = true;
                    }
                    else
                    {
                        ret = false;
                    }
                }
                else
                {
                    log.Warn(this.sensorId + "Response Timeout");
                    errMsg = "Response Timeout";
                    ret = false;
                }
                mre.Reset();
                //ClosePort();
                return ret;
            }
            catch (Exception ex)
            {
                mre.Reset();
                //ClosePort();
                log.Error(ex.Message);
                ret = false;
                return ret;
            }   
        }

        private void SendFrame(byte[] buffer)
        {
            if (comPort.IsOpen)
            {
                comPort.Write(buffer, 0, buffer.Length);
            }
        }

        private byte[] GetAcquisitionFrame()
        {

            //string id = "#+703057";

            byte[] frame = System.Text.Encoding.Default.GetBytes("#+" + this.deviceId);

            return frame;
        }

        public string GetDeviceId()
        {
            return deviceId;
        }

        public void SetDeviceId(string newId)
        {
            deviceId = newId;
        }

        public Dictionary<string, double> GetResult()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            result.Add(sensorId + "-020", innage);
            return result;
        }

        public string GetObjectType()
        {
            return "RS485MDL62XXAT";
        }

        public string GetResultString(string stamp)
        {
            if (isSuccess)
            {
                this.data.TimeStamp = stamp;
                string result = JsonConvert.SerializeObject(this.data);
                return result;
                //return GetObjectType() + " " + sensorId + " : " + innage;
            }
            else
            {
                return GetObjectType() + " " + sensorId + " : " + errMsg;
            }
        }

        public string GetSensorId()
        {
            return sensorId;
        }

        public string GetErrorMsg()
        {
            return errMsg;
        }

        public string GetPortName()
        {
            return comPort.PortName;
        }
    }
}

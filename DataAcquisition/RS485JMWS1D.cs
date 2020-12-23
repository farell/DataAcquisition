using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    class RS485JMWS1D:IMonitorDevice
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
        private double temperature;
        private double humidity;
        private string errMsg;
        private bool isSuccess;
        private int timeout;
        private string deviceId;
        private string sensorId;
        private JMWS1D_Data data;
        private const int frameLength = 15;

        private SerialDataReceivedEventHandler dataRecieveHandler;
        private SerialErrorReceivedEventHandler errorRecieveHandler;

        public RS485JMWS1D(string portName, int baudrate, int timeout, string deviceId, string sensorId)
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
            this.data = new JMWS1D_Data();
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
        private void SendFrame(byte[] buffer)
        {
            if (comPort.IsOpen)
            {
                comPort.Write(buffer, 0, buffer.Length);
            }
        }

        private byte[] GetAcquisitionFrame()
        {
            string frame = "#" + this.deviceId + "A!";
            byte[] arr = System.Text.Encoding.Default.GetBytes(frame);

            return arr;
        }

        public void ProcessData(byte[] buffer, int length)
        {
            //frame
            // 24 37 38 EF 35 32 33 30 38 30 30 33 95 36 31 38 34 D3 33 35 37 34 D3 30 B0 21
            // $   7  8  ?  5  2  3  0  8  0  0  3  .  6  1  8  4  ?  3  5  7  4  ?  0  ?  !

            this.isSuccess = false;

            byte head = 36;

            int indexOfHead = Array.IndexOf(buffer, head);

            int frameLength = length - indexOfHead - 1;
            byte[] frame = new byte[frameLength];

            Array.Copy(buffer, indexOfHead + 1, frame, 0, frameLength);

            for (int i = 0; i < frame.Length; i++)
            {
                if (frame[i] > 0x39)
                {
                    frame[i] = (byte)'$';
                }
            }

            string received = System.Text.Encoding.Default.GetString(frame);

            string[] splited = received.Split('$');

            if (splited.Length >= 4)
            {
                try
                {
                    this.temperature = Int32.Parse(splited[2]) / 100.0 - 40;
                    this.humidity = Int32.Parse(splited[3]) / 100.0;

                    this.data.SensorId = sensorId;
                    this.data.Temperature = this.temperature;
                    this.data.Humidity = this.humidity;

                    this.isSuccess = true;
                }
                catch (Exception ex)
                {
                    log.Error(ex + received);
                    this.errMsg = ex.Message;
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
                log.Error(ex);
                mre.Reset();
                //ClosePort();
                errMsg = ex.Message;
                ret = false;
                return ret;
            }

            
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
            result.Add(sensorId + "-005", temperature);
            result.Add(sensorId + "-006", humidity);
            return result;
        }

        public string GetObjectType()
        {
            return "RS485JMWS1D";
        }

        public string GetResultString(string stamp)
        {
            if (isSuccess)
            {
                this.data.TimeStamp = stamp;
                string result = JsonConvert.SerializeObject(this.data);
                return result;
                //return GetObjectType() + " " + sensorId + " : temperature: " + temperature + " ,humidity: " + humidity + "\r\n";
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

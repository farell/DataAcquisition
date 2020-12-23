using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    class RS485AS109:IMonitorDevice
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string[] availablePorts = SerialPort.GetPortNames();
        private SerialPort comPort;
        private byte[] buffer;
        private string portName;
        private int baudrate;
        private int bytesRead;
        private int bytesTotall;

        private ManualResetEvent mre;
        private double temperature;
        private double humidity;
        private string errMsg;
        private bool isSuccess;
        private int timeout;
        private string deviceId;
        private string sensorId;
        private const int frameLength = 9;
        private AS109_Data data;
        private SerialDataReceivedEventHandler dataRecieveHandler;
        private SerialErrorReceivedEventHandler errorRecieveHandler;
        public RS485AS109(string portName, int baudrate, int timeout, string deviceId, string sensorId)
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

            this.humidity = 0;
            this.temperature = 0;
            this.deviceId = deviceId;
            this.sensorId = sensorId;
            this.timeout = timeout;
            this.mre = new ManualResetEvent(false);
            this.isSuccess = false;
            this.data = new AS109_Data();
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
                errMsg = ex.StackTrace;
                log.Error(ex.StackTrace);
                this.bytesRead = 0;
                this.bytesTotall = 0;
            }
        }

        private void ProcessData(byte[] buffer, int length)
        {
            this.isSuccess = false;

            bool frameCheckPassed = ModbusUtility.RtuFrameCheck(buffer, frameLength);

            if (frameCheckPassed)
            {
                string deviceId = buffer[0].ToString();
                byte frameType = buffer[1];

                //测试ID 是否匹配
                if (deviceId == this.deviceId)
                {
                    if (frameType == 0x03)
                    {
                        short humidity_tmp = buffer[3];
                        humidity_tmp = (short)(humidity_tmp << 8);
                        humidity_tmp = (short)(humidity_tmp + buffer[4]);
                        short tempreture_tmp = buffer[5];
                        tempreture_tmp = (short)(tempreture_tmp << 8);
                        tempreture_tmp = (short)(tempreture_tmp + buffer[6]);
                        this.humidity = humidity_tmp / 10.0;
                        this.temperature = tempreture_tmp / 10.0;
                        this.isSuccess = true;
                        this.data.SensorId = sensorId;
                        this.data.Temperature = temperature;
                        this.data.Humidity = humidity;
                    }
                    else
                    {
                        log.Warn(this.sensorId + " wrong frame type " + frameType);
                        this.errMsg = "frame type error: " + frameType;
                    }
                }
                else
                {
                    log.Warn(this.sensorId + "wrong Device Id: " + deviceId + " expected: " + this.deviceId);
                    this.errMsg = "Wrong device id " + deviceId;
                }
            }
            else
            {
                log.Warn(this.sensorId + " broken frame: " + ModbusUtility.BytesToHexString(buffer, length));
                this.errMsg = "broken frame: " + ModbusUtility.BytesToHexString(buffer, length);
            }
            this.mre.Set();
        }

        private bool RtuFrameCheck(byte[] buffer, int length)
        {
            byte[] crc = ModbusUtility.CalculateCrc(buffer, length - 2);
            if (crc[0] == buffer[length - 2] && crc[1] == buffer[length - 1])
            {
                return true;
            }
            else
            {
                errMsg = this.sensorId + " frame check error";
                return false;
            }
        }

        private byte[] GetAcquisitionFrame()
        {
            byte[] frame = ModbusUtility.GetReadHoldingRegisterFrame(byte.Parse(this.deviceId), 0, 2);

            return frame;
        }

        private void SendFrame(byte[] buffer)
        {
            if (comPort.IsOpen)
            {
                comPort.Write(buffer, 0, buffer.Length);
            }
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
                    log.Warn(this.sensorId + "  Response Timeout");
                    errMsg = "Response Timeout";
                    ret = false;
                }
                mre.Reset();
                //ClosePort();
            }
            catch (Exception ex)
            {
                mre.Reset();
                //ClosePort();
                errMsg = ex.StackTrace;
                log.Error(ex.StackTrace);
                return false;
            }
            return ret;
        }

        public string GetDeviceId()
        {
            return deviceId;
        }

        public string GetErrorMsg()
        {
            return errMsg;
        }

        public string GetObjectType()
        {
            return "RS485AS109";
        }

        public Dictionary<string, double> GetResult()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            result.Add(sensorId + "-005", temperature);
            result.Add(sensorId + "-006", humidity);
            return result;
        }

        public string GetResultString(string stamp)
        {
            if (isSuccess)
            {
                this.data.TimeStamp = stamp;
                string result = JsonConvert.SerializeObject(this.data);
                return result;
                //return GetObjectType() + " " + sensorId + " temperature: " + temperature + " humidity: " + humidity;
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

        public void SetDeviceId(string newId)
        {
            deviceId = newId;
        }

        public string GetPortName()
        {
            return comPort.PortName;
        }
    }
}

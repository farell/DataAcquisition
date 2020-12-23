using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    class RS485BGK3475DM:IMonitorDevice
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
        private const int frameLength = 13;
        private BGK3475DM_Data data;
        private SerialDataReceivedEventHandler dataRecieveHandler;
        private SerialErrorReceivedEventHandler errorRecieveHandler;
        public RS485BGK3475DM(string portName, int baudrate, int timeout, string deviceId, string sensorId)
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
            this.isSuccess = false;
            this.data = new BGK3475DM_Data();
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

        public void ProcessData(byte[] raw_buffer, int length)
        {
            this.isSuccess = false;

            byte[] bufferFiltered = RemoveInterference(raw_buffer, length);

            byte[] buffer = new byte[frameLength];
            Buffer.BlockCopy(bufferFiltered, 0, buffer, 0, frameLength);

            bool frameCheckPassed = ModbusUtility.RtuFrameCheck(buffer, buffer.Length);

            if (frameCheckPassed)
            {
                string deviceId = buffer[0].ToString();
                byte frameType = buffer[1];

                //测试ID 是否匹配
                if (deviceId == this.deviceId)
                {
                    if (frameType == 0x03)
                    {
                        int startIndex = 3;
                        byte[] arr = new byte[4];
                        arr[0] = buffer[startIndex + 0];
                        arr[1] = buffer[startIndex + 1];
                        arr[2] = buffer[startIndex + 2];
                        arr[3] = buffer[startIndex + 3];

                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(arr);

                        innage = (double)BitConverter.ToSingle(arr, 0);
                        innage = Math.Round(innage, 3);
                        this.data.SensorId = sensorId;
                        this.data.Innage = innage;
                        this.isSuccess = true;
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
                    log.Warn(this.sensorId + " Response Timeout");
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
                //ClosePort();
                errMsg = ex.Message;
                ret = false;
                return ret;
            }

            
        }


        private bool RtuFrameCheck(byte[] buffer, int length)
        {
            //if (buffer.Length != 13)
            //{
            //    return false;
            //}
            byte[] crc = ModbusUtility.CalculateCrc(buffer, length - 2);
            if (crc[0] == buffer[length - 2] && crc[1] == buffer[length - 1])
            {
                return true;
            }
            else
            {
                //log.Warn(this.sensorId + " frame check error");
                errMsg = this.sensorId + " frame check error";
                return false;
            }
        }

        private byte[] RemoveInterference(byte[] buffer, int length)
        {
            int i = 0;
            while (i < length)
            {
                if (buffer[i] == 0xff)
                {
                    i++;
                }
                else
                {
                    break;
                }
            }
            byte[] frame = new byte[length - i];
            Buffer.BlockCopy(buffer, i, frame, 0, length - i);
            return frame;
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

            byte deviceId = Byte.Parse(this.deviceId);

            byte startAddress = 0;
            ushort numOfPoints = 4;
            byte[] frame = ModbusUtility.GetReadHoldingRegisterFrame(deviceId, startAddress, numOfPoints);

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
            return "RS485BGK3475DM";
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

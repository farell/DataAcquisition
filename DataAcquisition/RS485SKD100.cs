using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    class RS485SKD100 : IMonitorDevice
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string[] availablePorts = SerialPort.GetPortNames();
        protected SerialPort comPort;
        private byte[] buffer;
        private string portName;
        private int baudrate;
        private int bytesRead;
        private int bytesTotall;

        private ManualResetEvent mre;
        private double distance;
        private string errMsg;
        private bool isSuccess;
        private int timeout;
        private string deviceId;
        private string sensorId;
        private const int frameLength = 8;
        private SKD100_Data data;
        private SerialDataReceivedEventHandler dataRecieveHandler;
        private SerialErrorReceivedEventHandler errorRecieveHandler;
        public RS485SKD100(string portName, int baudrate, int timeout, string deviceId, string sensorId)
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

            this.deviceId = deviceId;
            this.sensorId = sensorId;
            this.timeout = timeout;
            this.mre = new ManualResetEvent(false);
            this.isSuccess = false;
            this.data = new SKD100_Data();
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

        private void ProcessData(byte[] buffer, int length)
        {
            this.isSuccess = false;

            bool frameCheckPassed = RtuFrameCheck(buffer, length);
            if (frameCheckPassed)
            {
                string deviceId = buffer[0].ToString();
                byte frameType = buffer[1];

                //测试ID 是否匹配
                if (deviceId == this.deviceId)
                {
                    if (frameType == 0x06)
                    {
                        //测量帧回应
                    }
                    else if (frameType == 0x03)
                    {
                        //结果帧回应，包含测量结果
                        byte[] tmp = new byte[4];
                        Array.Copy(buffer, 3, tmp, 0, 4);

                        if (tmp[0] != 0xff || tmp[1] != 0xff || tmp[2] != 0xff || tmp[3] != 0xff)
                        {
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(tmp);
                            distance = Math.Round((float)((BitConverter.ToUInt32(tmp, 0)) / 1000.0), 3);
                            this.data.SensorId = sensorId;
                            this.data.Distance = distance;
                            this.isSuccess = true;
                        }
                        else
                        {
                            log.Warn(this.sensorId + " Measure distance failed!");
                        }
                    }
                    else
                    {
                        log.Warn(this.sensorId + " wrong frame type " + frameType);
                    }
                }
                else
                {
                    log.Warn(this.sensorId + "wrong Device Id: " + deviceId + " expected: " + this.deviceId);
                    this.errMsg = "Device Id is not the same";
                }
            }
            else
            {
                log.Warn(this.sensorId + " broken frame: " + ModbusUtility.BytesToHexString(buffer, length));
                this.errMsg = "broken frame: " + ModbusUtility.BytesToHexString(buffer, length);
            }
            this.mre.Set();
        }

        private byte[] GetMeasureFrame(string deviceId)
        {
            byte id = Byte.Parse(deviceId);
            byte startAddress = 0;
            ushort measureCmd = 0x004F;
            byte[] measureFrame = ModbusUtility.GetWriteSingleRegisterFrame(id, startAddress, measureCmd);
            return measureFrame;
        }

        private byte[] GetMeasureResultFrame(string deviceId)
        {
            byte id = Byte.Parse(deviceId);

            byte startAddress = 0;
            byte[] measureFrame = ModbusUtility.GetReadHoldingRegisterFrame(id, startAddress, 2);
            return measureFrame;
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
                return false;
            }
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
                log.Error(this.errMsg);
                return false;
            }
            if (comPort.IsOpen)
            {
                this.errMsg = this.portName + " is open by another process";
                log.Error(this.errMsg);
                return false;
            }
            try
            {
                this.bytesRead = 0;
                this.bytesTotall = 0;
                //OpenPort();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }

            byte[] frame = GetMeasureFrame(this.deviceId);
            this.SendFrame(frame);
            bool receivedEvent = this.mre.WaitOne(this.timeout * 1000);
            if (receivedEvent)
            {
                mre.Reset();

                frame = GetMeasureResultFrame(this.deviceId);
                this.SendFrame(frame);
                receivedEvent = this.mre.WaitOne(this.timeout * 1000);
                if (receivedEvent)
                {
                    if (isSuccess)
                    {
                        ret = true;
                    }
                    else
                    {
                        errMsg = "Measure distance failed";
                        log.Error(errMsg);
                        ret = false;
                    }
                }
                else
                {
                    ret = false;
                    errMsg = "Receiving Get Measure Reslut Response Timeout";
                    log.Error(errMsg);
                }
                mre.Reset();
            }
            else
            {
                mre.Reset();
                errMsg = "Receiving Measure Response Timeout";
                log.Error(errMsg);
                ret = false;
            }
            //ClosePort();
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
            return "RS485SKD100";
        }

        public Dictionary<string, double> GetResult()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            result.Add(sensorId + "-021", distance);
            return result;
        }

        public string GetResultString(string stamp)
        {
            if (isSuccess)
            {
                this.data.TimeStamp = stamp;
                string result = JsonConvert.SerializeObject(this.data);
                return result;
                //return GetObjectType() + " " + sensorId + " : " + distance;
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    class RS485ACA826T:IMonitorDevice
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
        private double x;
        private double y;
        private double temperature;
        private string errMsg;
        private bool isSuccess;
        private int timeout;
        private string deviceId;
        private string sensorId;
        private ACA826T_Data data;
        private const int frameLength = 17;
        private SerialDataReceivedEventHandler dataRecieveHandler;
        private SerialErrorReceivedEventHandler errorRecieveHandler;
        public RS485ACA826T(string portName, int baudrate, int timeout, string deviceId, string sensorId)
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

            this.x = 0;
            this.y = 0;
            this.temperature = 0;
            this.deviceId = deviceId;
            this.sensorId = sensorId;
            this.timeout = timeout;
            this.mre = new ManualResetEvent(false);
            this.data = new ACA826T_Data();
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

        private void ProcessData(byte[] raw_buffer, int length)
        {
            this.isSuccess = false;

            byte[] bufferFiltered = RemoveInterference(raw_buffer, length);

            byte[] buffer = new byte[frameLength];
            Buffer.BlockCopy(bufferFiltered, 0, buffer, 0, frameLength);

            if (InclinationFrameCheck(buffer, buffer.Length) == true)
            {
                try
                {
                    string deviceId = buffer[2].ToString();
                    string debugMessage = "";
                    debugMessage += "Device ID : " + deviceId + "\r\n";

                    byte[] x = new byte[4];
                    byte[] y = new byte[4];
                    byte[] t = new byte[4];

                    Array.Copy(buffer, 4, x, 0, 4);
                    Array.Copy(buffer, 4 + 4, y, 0, 4);
                    Array.Copy(buffer, 4 + 4+4, t, 0, 4);
                    double x_angle = packedBCD2Double(x);
                    double y_angle = packedBCD2Double(y);
                    double temperature = packedBCD2Double(t);
                    x_angle = Math.Round(x_angle, 3);// - initX;
                    y_angle = Math.Round(y_angle, 3);// - initY;
                    temperature = Math.Round(temperature, 2);// - initY;
                    debugMessage += "raw x_angle : " + x_angle + "\r\n";
                    debugMessage += "raw y_angle : " + y_angle + "\r\n";
                    debugMessage += "temperature : " + temperature + "\r\n";

                    this.isSuccess = true;
                    this.x = x_angle;
                    this.y = y_angle;
                    this.temperature = temperature;
                    this.data.SensorId = sensorId;
                    this.data.X = x_angle;
                    this.data.Y = y_angle;
                    this.data.Temperature = temperature;
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
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
            return result;
        }

        private byte[] GetAcquisitionFrame()
        {
            byte[] arr = GetReadInclinationFrame(Byte.Parse(deviceId));
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

        private bool InclinationFrameCheck(byte[] buffer, int length)
        {
            if (length != 17)
            {
                log.Warn(this.sensorId + " frame check error:length != 17");
                errMsg = this.sensorId + " frame check error:length != 17";
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
                log.Warn(this.sensorId + " frame check error: Check SUM failed");
                errMsg = this.sensorId + " frame check error: Check SUM failed";
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

        public bool Acquisit()
        {
            this.isSuccess = false;
            bool ret = false;

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
                }
                else
                {
                    log.Info(this.sensorId + "  Response Timeout");
                    errMsg = "Response Timeout";
                }
                mre.Reset();
                //ClosePort();
                return ret;
            }
            catch (Exception ex)
            {

                mre.Reset();
                //ClosePort();
                errMsg = ex.Message;
                log.Error(ex.Message);
                return false;
            }
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
            return "RS485ACA826T";
        }

        public Dictionary<string, double> GetResult()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            //result.Add("x", x);
            //result.Add("y", y);
            result.Add(sensorId + "-001", x);
            result.Add(sensorId + "-002", y);
            return result;
        }

        public string GetResultString(string stamp)
        {
            if (isSuccess)
            {
                this.data.TimeStamp = stamp;
                string result = JsonConvert.SerializeObject(this.data);
                return result;
                //return GetObjectType() + " " + sensorId + " : x: " + x + " ,y: " + y + "\r\n";
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

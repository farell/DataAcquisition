using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    class RS485Micro_40A : IMonitorDevice
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
        private double frequency;
        private string errMsg;
        private bool isSuccess;
        private int timeout;
        private string deviceId;
        private string sensorId;
        private string channel;
        private const int frameLength = 25;
        private Bgk_Micro_40A_Data data;
        private SerialDataReceivedEventHandler dataRecieveHandler;
        private SerialErrorReceivedEventHandler errorRecieveHandler;
        public RS485Micro_40A(string portName, int baudrate, int timeout, string _deviceId, string sensorId,int channelNo)
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

            this.frequency = 0;
            this.temperature = 0;
            this.sensorId = sensorId;
            this.channel = channelNo.ToString();
            this.timeout = timeout;
            this.mre = new ManualResetEvent(false);
            this.isSuccess = false;
            this.data = new Bgk_Micro_40A_Data();
            this.deviceId = _deviceId;
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
                log.Error(ex.StackTrace);
                errMsg = ex.StackTrace;
                this.bytesRead = 0;
                this.bytesTotall = 0;
            }
        }

        private double ResistanceToTemperature(double resistance)
        {
            double a = 1.4051e-3;
            double b = 2.369e-4;
            double c = 1.019e-7;
            double temp  = 1 / (a + b * Math.Log(resistance) + c * Math.Log(resistance) * Math.Log(resistance) * Math.Log(resistance)) - 273.2;
            return temp;
        }

        private void ProcessData(byte[] buffer, int length)
        {
            this.isSuccess = false;

            bool frameCheckPassed = AsciiFrameCheck(buffer, frameLength);

            if (frameCheckPassed)
            {
                string frame = System.Text.Encoding.Default.GetString(buffer, 0, frameLength);
                try
                {
                    if (frame.StartsWith(":"))
                    {
                        string id = frame.Substring(1, 2).Trim();
                        string frame_type = frame.Substring(3, 2);
                        string val1 = frame.Substring(7, 8);
                        string val2 = frame.Substring(15, 8).Trim();

                        //Convert.ToByte("f1", 16)
                        Convert.ToByte(id, 16);

                        if (Convert.ToByte(id, 16) == Byte.Parse(this.deviceId))
                        {
                            if (frame_type == "21")
                            {
                                if (val1.StartsWith("E") || val2.StartsWith("E"))
                                {
                                    errMsg = "sensor fault: " + val1 + " " + val2;
                                    log.Error(errMsg);
                                }
                                else
                                {
                                    frequency = double.Parse(val1);
                                    temperature = Math.Round(ResistanceToTemperature(double.Parse(val2)), 1);
                                    this.data.SensorId = sensorId;
                                    this.data.Value1 = frequency;
                                    this.data.Value2 = double.Parse(val2);
                                    this.isSuccess = true;
                                }
                            }
                            else
                            {
                                errMsg = "frame type not correct: " + frame_type;
                                log.Error(errMsg);
                            }
                        }
                        else
                        {
                            errMsg = "address not correct: " + id;
                            log.Error(errMsg);
                        }
                    }
                    else
                    {
                        errMsg = "broken frame: " + frame;
                        log.Error(errMsg);
                    }
                }
                catch(Exception ex)
                {
                    errMsg = ex.StackTrace + frame;
                    log.Error(ex.StackTrace + frame);
                }
                
            }
            else
            {
                this.errMsg = "frame check failed : " + BitConverter.ToString(buffer,0,frameLength);
                log.Error(errMsg);
            }
            this.mre.Set();
        }

        private bool AsciiFrameCheck(byte[] buffer, int length)
        {
            byte[] crc = ModbusUtility.CalculateCrc(buffer, length - 2);
            if (crc[0] == buffer[length - 2] && crc[1] == buffer[length - 1])
            {
                return true;
            }
            else
            {
                errMsg = this.sensorId + " frame check error";
                return true;
            }
        }

        public byte[] GetAcquisitionFrame()
        {
            //byte[] frame = ModbusUtility.GetReadHoldingRegisterFrame(byte.Parse(this.deviceId), 0, 2);
            //string channel = "";
            List<byte> frame = new List<byte>();
            frame.Add(0x3A);//head

            byte addr = byte.Parse(deviceId);

            byte[] byteArr = new byte[1];
            byteArr[0] = addr;

            string addrString = BitConverter.ToString(byteArr);

            frame.Add((byte)addrString[0]);
            frame.Add((byte)addrString[1]);

            frame.Add((byte)'2');
            frame.Add((byte)'1');

            //ch
            if (channel.Length < 2)
            {
                frame.Add((byte)'0');
                frame.Add((byte)channel[0]);
            }
            else
            {
                frame.Add((byte)channel[0]);
                frame.Add((byte)channel[1]);
            }

            //byte[] f = new byte[frame.Count - 1];
            //Array.Copy(frame.ToArray(), 1, f, 0, frame.Count - 1);

            //byte b_lrc = ModbusUtility.CalculateLrc(f);

            //byte[] a_lrc = new byte[1];
            //a_lrc[0] = b_lrc;

            //string s_lrc = BitConverter.ToString(a_lrc);

            //frame.Add((byte)s_lrc[0]);
            //frame.Add((byte)s_lrc[1]);

            frame.Add((byte)'F');
            frame.Add((byte)'F');

            frame.Add(0x0D);
            frame.Add(0x0A);
            return frame.ToArray();
        }

        private void SendFrame(byte[] buffer)
        {
            if (comPort.IsOpen)
            {
                byte[] head = new byte[1];
                head[0] = 0x3A;
                comPort.Write(head,0,1);//wake up the Micro-4a
                Thread.Sleep(1000);
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
                if (comPort.IsOpen)
                {
                    //ClosePort();
                }
                errMsg = ex.StackTrace;
                log.Error(errMsg);
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
            return "RS485Micro_40A";
        }

        public Dictionary<string, double> GetResult()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            result.Add(sensorId + "-005", temperature);
            result.Add(sensorId + "-012", frequency);
            return result;
        }

        public string GetResultString(string stamp)
        {
            if (isSuccess)
            {
                this.data.TimeStamp = stamp;
                string result = JsonConvert.SerializeObject(this.data);
                return result;
                //return GetObjectType() + " " + sensorId + " temperature: " + temperature + " frequency: " + frequency;
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

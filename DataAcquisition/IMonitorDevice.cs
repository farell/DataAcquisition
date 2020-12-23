using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAcquisition
{
    public interface IMonitorDevice
    {
        void OpenPort();
        void ClosePort();
        bool Acquisit();
        string GetDeviceId();
        void SetDeviceId(string newId);
        Dictionary<string, double> GetResult();
        string GetObjectType();
        string GetResultString(string stamp);
        string GetSensorId();
        string GetErrorMsg();
        string GetPortName();
    }
}

using System.IO.Ports;

namespace INAV_SIM_OSD
{
    internal class SerialConnection : IMSPConnection
    {
        private readonly SerialPort _serialPort;

        public SerialConnection(string portName, int baudrate)
        {
            _serialPort = new SerialPort(portName, baudrate);
        }

        public void Close()
        {
            _serialPort.Close();
        }

        public void Dispose()
        {
            _serialPort.Dispose();
        }

        public bool IsOpen()
        {
            return _serialPort.IsOpen;
        }

        public void Open()
        {
            _serialPort.Open();
        }

        public int ReadByte()
        {
            return _serialPort.ReadByte();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }
    }
}

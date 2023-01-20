using System.Net;
using System.Net.Sockets;

namespace INAV_SIM_OSD
{
    internal class TCPConnection : IMSPConnection
    {
        private readonly TcpClient _tcpClient;
        private readonly IPAddress? _address;
        private readonly int _port;

        public TCPConnection(IPAddress? iPAddress, int port)
        {
            _tcpClient = new TcpClient();
            _address = iPAddress;
            _port = port;
        }

        public bool IsOpen()
        {
            return _tcpClient.Connected;
        }

        public void Open()
        {
            if (_address is null)
                throw new InvalidOperationException("Invalid IP address");

            _tcpClient.Connect(_address, _port);
            _tcpClient.SendTimeout = 1000;
        }

        public int ReadByte()
        {
            if (!IsOpen())
                throw new InvalidOperationException("TCP connection is closed.");

            return _tcpClient.GetStream().ReadByte();

        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (!IsOpen())
                throw new InvalidOperationException("TCP connection is closed.");


            _tcpClient.GetStream().Write(buffer, offset, count);
        }

        ~TCPConnection()
        {
            Dispose(false);
        }

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tcpClient.Close();
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}

using System.Net;

namespace INAV_SIM_OSD
{
    internal static class Helper
    {
        public static string[] GetAvaiableComPorts()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }

        public static string[] GetFontNames()
        {
            return Directory.GetFileSystemEntries(OSD.FONT_FOLDER, "*", SearchOption.TopDirectoryOnly).Select(e => Path.GetFileName(e)).ToArray();
        }

        public static bool TryParseIpString(string ipString, out IPAddress? iPAddress, out int port)
        {
            iPAddress = IPAddress.None;
            port = 0;

            if (string.IsNullOrWhiteSpace(ipString))
                return false;

            string[] ip = ipString.Split(':');
            return (ip is not null && ip.Length == 2 &&
                IPAddress.TryParse(ip[0], out iPAddress) &&
                int.TryParse(ip[1], out port) &&
                port > 0 && port <= 65535);
        }

        public static byte GetLowerByte(UInt16 value)
        {
            return (byte)(value & 0xFF);
        }

        public static byte GetUpperByte(UInt16 value)
        {
            return (byte)((value & 0xFF00) >> 8);
        }

        public static unsafe T BufferToStruct<T>(byte[] buffer) where T : unmanaged
        {
            fixed (byte* bufferPtr = buffer)
            {
                return *(T*)bufferPtr;
            }
        }
    }
}

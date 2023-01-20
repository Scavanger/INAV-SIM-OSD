using System.Net;
using System.Runtime.InteropServices;

namespace INAV_SIM_OSD
{
    public class MSP : IDisposable
    {
        private const int MSP_TIMEOUT = 1000; // ms

        public const UInt16 MSP_FC_VARIANT = 2;
        public const UInt16 MSP_DISPLAYPORT = 182;

        public enum State
        {
            IDLE,
            HEADER_START,
            HEADER_M,
            HEADER_X,
            HEADER_V1,
            PAYLOAD_V1,
            CHECKSUM_V1,
            HEADER_V2_OVER_V1,
            PAYLOAD_V2_OVER_V1,
            CHECKSUM_V2_OVER_V1,
            HEADER_V2_NATIVE,
            PAYLOAD_V2_NATIVE,
            CHECKSUM_V2_NATIVE,
            COMMAND_RECEIVED
        }

        public enum Version
        {
            MSP_V1,
            MSP_V2,
            MSP_V2_OVER_V1,
            MSP_V2_NATIVE
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderV1
        {
            public byte Size;
            public byte Command;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderV2
        {
            public byte Flags;
            public UInt16 Command;
            public UInt16 Size;
        }

        public class Frame
        {
            public Version Version { get; set; }
            public byte CRCV1 { get; set; }
            public byte CRCV2 { get; set; }
            public UInt16 Command { get; set; }
            public byte Flags { get; set; }
            public byte[] Buffer { get; set; }
            public int Offset { get; set; }
            public int DataSize { get; set; }
            public State State { get; set; }

            public Frame()
            {
                Offset = 0;
                Buffer = new byte[1024];
                Version = Version.MSP_V1;
                CRCV1 = 0;
                CRCV2 = 0;
                Command = 0;
                Flags = 0;
                DataSize = 0;
                State = State.IDLE;
            }
        }


        private const byte MSP_START = (byte)'$';
        private const byte MSP_V1 = (byte)'M';
        private const byte MSP_V2 = (byte)'X';
        private const byte MSP_FROM_FC = (byte)'>';
        private const byte MSP_TO_FC = (byte)'<';
        private const byte MSP_V2_FRAMEID = 0xFF;

        private IMSPConnection? _connection;

        public event EventHandler? FrameReceived;

        public Frame ReceivedFrame { get; private set; }

        protected virtual void OnFrameReceived()
        {
            FrameReceived?.Invoke(this, EventArgs.Empty);
        }

        public MSP()
        {
            ReceivedFrame = new();
        }

        ~MSP()
        {
            Dispose(false);
        }

        public void SetConnection(string connectionString)
        {
            _connection?.Dispose();

            if (connectionString.Contains("COM"))
                _connection = new SerialConnection(connectionString, 115200);
            else if (Helper.TryParseIpString(connectionString, out IPAddress? iPAddress, out int port))
                _connection = new TCPConnection(iPAddress, port);
        }

        public void Open()
        {
            if (_connection is null)
                throw new InvalidOperationException(nameof(_connection));

            if (!_connection.IsOpen())
                _connection.Open();
        }

        public void ClosePort()
        {
            if (_connection is not null && _connection.IsOpen())
                _connection.Close();
        }

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        private void SendMSP(UInt16 cmd, byte[]? payload)
        {
            if (_connection is null)
                throw new InvalidOperationException(nameof(_connection));

            if (!_connection.IsOpen())
                throw new IOException("Serial Port closed");

            // Only MSP v2
            int bufferLength = 9;
            int payloadLength = 0;
            if (payload is not null || payload?.Length == 0)
            {
                bufferLength += payload.Length;
                payloadLength = payload.Length;
            }

            byte[] buffer = new byte[bufferLength];
            buffer[0] = MSP_START;
            buffer[1] = MSP_V2;
            buffer[2] = MSP_TO_FC;
            buffer[3] = 0;
            buffer[4] = Helper.GetLowerByte((UInt16)cmd);
            buffer[5] = Helper.GetUpperByte((UInt16)cmd);
            buffer[6] = Helper.GetLowerByte((UInt16)payloadLength);
            buffer[7] = Helper.GetUpperByte((UInt16)payloadLength);

            if (payload is not null)
                Buffer.BlockCopy(payload, 0, buffer, 8, payload.Length);

            int crc = 0;
            for (int i = 3; i < bufferLength - 1; i++)
                crc = Crc8_Dvb_S2(crc, buffer[i]);

            buffer[bufferLength - 1] = (byte)crc;
            _connection.Write(buffer, 0, buffer.Length);
        }

        public void SendReceive(CancellationToken ct, byte[]? payload = null)
        {
            if (_connection is null)
                throw new InvalidOperationException(nameof(_connection));

            DateTime start = DateTime.Now;
            ReceivedFrame.State = State.IDLE;
            SendMSP(MSP_FC_VARIANT, payload);

            while (ReceivedFrame.State != State.COMMAND_RECEIVED && !ct.IsCancellationRequested)
            {
                int b = _connection.ReadByte();
                if (b == -1)
                    return;

                if (ProcessFrame((byte)b) && ReceivedFrame.State == State.COMMAND_RECEIVED)
                {
                    TimeSpan duration = DateTime.Now - start;
                    if (duration > TimeSpan.FromMilliseconds(MSP_TIMEOUT))
                        break;

                    OnFrameReceived();
                }
            }

        }

        private bool ProcessFrame(byte b)
        {
            switch (ReceivedFrame.State)
            {
                case State.IDLE:
                    if (b == MSP_START)
                    {
                        ReceivedFrame = new();
                        ReceivedFrame.Version = Version.MSP_V1;
                        ReceivedFrame.State = State.HEADER_START;
                    }
                    else
                        return false;
                    break;
                case State.HEADER_START:
                    ReceivedFrame.State = b switch
                    {
                        MSP_V1 => State.HEADER_M,
                        MSP_V2 => State.HEADER_X,
                        _ => State.IDLE,
                    };
                    break;
                case State.HEADER_M:
                    if (b == MSP_FROM_FC)
                    {
                        ReceivedFrame.Offset = 0;
                        ReceivedFrame.CRCV1 = 0;
                        ReceivedFrame.CRCV2 = 0;
                        ReceivedFrame.State = State.HEADER_V1;
                    }
                    else
                        ReceivedFrame.State = State.IDLE;
                    break;
                case State.HEADER_X:
                    if (b == MSP_FROM_FC)
                    {
                        ReceivedFrame.Offset = 0;
                        ReceivedFrame.CRCV2 = 0;
                        ReceivedFrame.Version = Version.MSP_V2_NATIVE; ;
                        ReceivedFrame.State = State.HEADER_V2_NATIVE;
                    }
                    break;
                case State.HEADER_V1:
                    ReceivedFrame.Buffer[ReceivedFrame.Offset++] = b;
                    ReceivedFrame.CRCV1 ^= b;
                    if (ReceivedFrame.Offset == Marshal.SizeOf(typeof(HeaderV1)))
                    {
                        HeaderV1 header = Helper.BufferToStruct<HeaderV1>(ReceivedFrame.Buffer[..ReceivedFrame.Offset]);
                        if (header.Size > ReceivedFrame.Buffer.Length)
                            ReceivedFrame.State = State.IDLE;
                        else if (header.Command == MSP_V2_FRAMEID)
                        {
                            if (header.Size >= Marshal.SizeOf(typeof(HeaderV2)) + 1)
                            {
                                ReceivedFrame.State = State.HEADER_V2_OVER_V1;
                            }
                            else
                                ReceivedFrame.State = State.IDLE;
                        }
                        else
                        {
                            ReceivedFrame.DataSize = header.Size;
                            ReceivedFrame.Command = header.Command;
                            ReceivedFrame.Offset = 0;
                            ReceivedFrame.State = ReceivedFrame.DataSize > 0 ? State.PAYLOAD_V1 : State.CHECKSUM_V1;
                        }

                    }
                    break;

                case State.PAYLOAD_V1:
                    ReceivedFrame.Buffer[ReceivedFrame.Offset++] = b;
                    ReceivedFrame.CRCV1 ^= b;
                    if (ReceivedFrame.Offset == ReceivedFrame.DataSize)
                        ReceivedFrame.State = State.CHECKSUM_V1;
                    break;

                case State.CHECKSUM_V1:
                    if (ReceivedFrame.CRCV1 == b)
                        ReceivedFrame.State = State.COMMAND_RECEIVED;
                    else
                        ReceivedFrame.State = State.IDLE;
                    break;

                case State.HEADER_V2_OVER_V1:
                    ReceivedFrame.Buffer[ReceivedFrame.Offset++] = b;
                    ReceivedFrame.CRCV1 ^= b;
                    ReceivedFrame.CRCV2 = (byte)Crc8_Dvb_S2(ReceivedFrame.CRCV2, b);
                    if (ReceivedFrame.Offset == Marshal.SizeOf(typeof(HeaderV1)) + Marshal.SizeOf(typeof(HeaderV2)))
                    {
                        HeaderV2 header = Helper.BufferToStruct<HeaderV2>(ReceivedFrame.Buffer[..ReceivedFrame.Offset]);
                        if (header.Size > ReceivedFrame.Buffer.Length)
                            ReceivedFrame.State = State.IDLE;
                        else
                        {
                            ReceivedFrame.Command = header.Command;
                            ReceivedFrame.Flags = header.Flags;
                            ReceivedFrame.Offset = 0;
                            ReceivedFrame.State = ReceivedFrame.DataSize > 0 ? State.PAYLOAD_V2_OVER_V1 : State.CHECKSUM_V2_OVER_V1;
                        }
                    }
                    break;

                case State.PAYLOAD_V2_OVER_V1:
                    ReceivedFrame.CRCV2 = (byte)Crc8_Dvb_S2(ReceivedFrame.CRCV2, b);
                    ReceivedFrame.CRCV1 ^= b;
                    ReceivedFrame.Buffer[ReceivedFrame.Offset++] = b;

                    if (ReceivedFrame.Offset == ReceivedFrame.DataSize)
                        ReceivedFrame.State = State.CHECKSUM_V2_OVER_V1;

                    break;

                case State.CHECKSUM_V2_OVER_V1:
                    ReceivedFrame.CRCV1 ^= b;

                    if (ReceivedFrame.Offset == ReceivedFrame.DataSize)
                        ReceivedFrame.State = State.CHECKSUM_V1;
                    else
                        ReceivedFrame.State = State.IDLE;

                    break;

                case State.HEADER_V2_NATIVE:
                    ReceivedFrame.Buffer[ReceivedFrame.Offset++] = b;
                    ReceivedFrame.CRCV2 = (byte)Crc8_Dvb_S2(ReceivedFrame.CRCV2, b);
                    if (ReceivedFrame.Offset == Marshal.SizeOf(typeof(HeaderV2)))
                    {
                        HeaderV2 header = Helper.BufferToStruct<HeaderV2>(ReceivedFrame.Buffer[..ReceivedFrame.Offset]);
                        if (header.Size >= ReceivedFrame.Buffer.Length)
                            ReceivedFrame.State = State.IDLE;
                        else
                        {
                            ReceivedFrame.DataSize = header.Size;
                            ReceivedFrame.Command = header.Command;
                            ReceivedFrame.Flags = header.Flags;
                            ReceivedFrame.Offset = 0;
                            ReceivedFrame.State = ReceivedFrame.DataSize > 0 ? State.PAYLOAD_V2_NATIVE : State.CHECKSUM_V2_NATIVE;
                        }
                    }
                    break;

                case State.PAYLOAD_V2_NATIVE:
                    ReceivedFrame.CRCV2 = (byte)Crc8_Dvb_S2(ReceivedFrame.CRCV2, b);
                    ReceivedFrame.Buffer[ReceivedFrame.Offset++] = b;

                    if (ReceivedFrame.Offset == ReceivedFrame.DataSize)
                        ReceivedFrame.State = State.CHECKSUM_V2_NATIVE;

                    break;

                case State.CHECKSUM_V2_NATIVE:
                    if (ReceivedFrame.CRCV2 == b)
                        ReceivedFrame.State = State.COMMAND_RECEIVED;
                    else
                        ReceivedFrame.State = State.IDLE;

                    break;

            }
            return true;
        }


        public static int Crc8_Dvb_S2(int crc, int ch)
        {
            crc ^= ch;
            for (int i = 0; i < 8; ++i)
            {
                if ((crc & 0x80) != 0)
                {
                    crc = ((crc << 1) & 0xFF) ^ 0xD5;
                }
                else
                {
                    crc = (crc << 1) & 0xFF;
                }
            }
            return crc;
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                _connection?.Close();
                _connection?.Dispose();
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

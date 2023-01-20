namespace INAV_SIM_OSD
{
    internal interface IMSPConnection : IDisposable
    {
        public void Close();

        public void Open();

        public bool IsOpen();

        public int ReadByte();

        public void Write(byte[] buffer, int offset, int count);

    }
}

using System.Diagnostics;

namespace INAV_SIM_OSD
{
    internal class OSDInterface
    {
        private readonly Dictionary<string, string> PROCESS_NAMES = new Dictionary<string, string>()
            {
                {"RealFlight64", "RealFlight" },
                {"RealFlight", "RealFlight" },
                {"X-Plane", "X-Plane" }
            };

        private SynchronizationContext? _synchronizationContext;
        private readonly TaskScheduler _scheduler;
        private CancellationTokenSource _cts;
        private readonly MSP _msp;
        private OSD? _osd;

        public event EventHandler? Disconnect;

        public OSDInterface()
        {
            _scheduler = TaskScheduler.Default;
            _synchronizationContext = SynchronizationContext.Current;
            _cts = new();
            _msp = new();
            _msp.FrameReceived += Msp_FrameReceived;
        }

        protected virtual void OnDisconnect()
        {
            Disconnect?.Invoke(this, EventArgs.Empty);
        }

        private void Msp_FrameReceived(object? sender, EventArgs e)
        {
            if (_msp.ReceivedFrame.State == MSP.State.COMMAND_RECEIVED && _msp.ReceivedFrame.Command == MSP.MSP_DISPLAYPORT)
                _osd?.Decode(_msp.ReceivedFrame.Buffer, _msp.ReceivedFrame.DataSize);
        }

        public async Task StartAsync(string? connection, string? font)
        {
            if (string.IsNullOrEmpty(connection))
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(font))
                throw new ArgumentNullException(nameof(font));

            _cts = new CancellationTokenSource();

            Process? procces;
            try
            {
                procces = Process.GetProcesses().Where(p => PROCESS_NAMES.ContainsKey(p.ProcessName)).First();
            }
            catch
            {
                throw new Exception("Unable to detect simulator.");
            }

            _osd = new(procces.MainWindowHandle)
            {
                FontName = font
            };

            _ = Task.Factory.StartNew(() =>
            {
                _osd.Run(_scheduler);
            }, _cts.Token, TaskCreationOptions.LongRunning, _scheduler);

            _msp.SetConnection(connection);
            _msp.Open();

            await Task.Factory.StartNew(() =>
            {
                DateTime start = DateTime.Now;
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        if (DateTime.Now - start > TimeSpan.FromMilliseconds(125))
                        {
                            _msp.SendReceive(_cts.Token);
                            _osd.Show = true;
                        }
                    }
                    catch
                    {
                        _osd.Show = false;
                        Stop();

                        _synchronizationContext?.Post(new SendOrPostCallback(_ => OnDisconnect()), null);
                        
                        break;
                    }
                }
            }, _cts.Token, TaskCreationOptions.LongRunning, _scheduler);
        }

        public void Stop()
        {
            _cts.Cancel();
            _osd?.Close();
            _msp.ClosePort();
        }
    }
}

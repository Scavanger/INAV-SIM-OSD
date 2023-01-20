namespace INAV_SIM_OSD
{
    public partial class MainForm : Form
    {
        private bool _isRunning = false;
        private readonly string[] _fontNames;
        private readonly OSDInterface _OSDInterface;


        public MainForm()
        {
            InitializeComponent();

            _OSDInterface = new();
            _OSDInterface.Disconnect += _OSDInterface_Disconnect;
            _fontNames = Helper.GetFontNames();
            _textBoxIP.Enabled = false;

        }

        private void _OSDInterface_Disconnect(object? sender, EventArgs e)
        {
            Stop();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _comboBoxPort.Items.Add("TCP");
            _comboBoxPort.Items.AddRange(Helper.GetAvaiableComPorts());
            _comboBoxFont.Items.AddRange(_fontNames);

            _comboBoxPort.SelectedItem = Properties.Settings.Default.LastComPort;
            _textBoxIP.Text = Properties.Settings.Default.LastIP;
            _comboBoxFont.SelectedItem = Properties.Settings.Default.LastFont;

            if (_comboBoxPort.SelectedItem is null)
                _comboBoxPort.SelectedItem = _comboBoxPort.Items[0];
            
            if (_comboBoxFont.SelectedItem is null && _comboBoxFont.Items.Count > 0)
                _comboBoxFont.SelectedItem = _comboBoxFont.Items[0];

            if (string.IsNullOrEmpty(_textBoxIP.Text))
                _textBoxIP.Text = "127.0.0.1:5763";

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.LastComPort = _comboBoxPort.SelectedItem as string;
            Properties.Settings.Default.LastIP = _textBoxIP.Text;
            Properties.Settings.Default.LastFont = _comboBoxFont.SelectedItem as string;

            Properties.Settings.Default.Save();
        }


        private void ComboBoxPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((_comboBoxPort.SelectedItem as String) == "TCP")
                _textBoxIP.Enabled = true;
            else
                _textBoxIP.Enabled = false;
        }

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            string? lastSelectedItem = _comboBoxPort.SelectedItem as string;
            _comboBoxPort.Items.Clear();
            _comboBoxPort.Items.AddRange(Helper.GetAvaiableComPorts());
            _comboBoxPort.Items.Add("TCP");

            _comboBoxPort.SelectedItem = lastSelectedItem;

            if (_comboBoxPort.Items.Count == 0)
                _buttonStartStop.Enabled = false;
        }

        private void Stop()
        {
            _toolStripStatusLabel.Text = "Stopped";
            _OSDInterface.Stop();
            _buttonStartStop.Text = "Start";
            _isRunning = false;
            _comboBoxFont.Enabled = true;
            _comboBoxPort.Enabled = true;
            _textBoxIP.Enabled = true;
            _buttonRefresh.Enabled = true;
        }

        private async void ButtonStartStop_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                Stop();
            }
            else
            {
                try
                {
                    string? connection = string.Empty;
                    if (string.IsNullOrEmpty(_comboBoxPort.SelectedItem as string))
                        throw new Exception("No MSP Port selected.");
                    else
                        connection = _comboBoxPort.SelectedItem as string;

                    if (Helper.TryParseIpString(_textBoxIP.Text, out _, out _))
                        connection = _textBoxIP.Text;
                    else
                        throw new Exception("Invalid IP address and/or port");

                    _toolStripStatusLabel.Text = string.Format("Simulator detected, injecting OSD.");
                    _buttonStartStop.Text = "Stop";
                    _comboBoxFont.Enabled = false;
                    _comboBoxPort.Enabled = false;
                    _textBoxIP.Enabled = false;
                    _buttonRefresh.Enabled = false;
                    _isRunning = true;

                    await _OSDInterface.StartAsync(connection, _comboBoxFont.SelectedItem as string);
                }
                catch (Exception ex)
                {
                    _toolStripStatusLabel.Text = "Error";
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    _OSDInterface.Stop();
                    _buttonStartStop.Text = "Start";
                    _isRunning = false;
                    _comboBoxFont.Enabled = true;
                    _comboBoxPort.Enabled = true;
                    _textBoxIP.Enabled = true;
                    _buttonRefresh.Enabled = true;
                }
            }
        }
    }
}
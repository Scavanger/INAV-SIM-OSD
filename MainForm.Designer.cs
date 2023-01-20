namespace INAV_SIM_OSD
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._buttonRefresh = new System.Windows.Forms.Button();
            this._comboBoxPort = new System.Windows.Forms.ComboBox();
            this._comboBoxFont = new System.Windows.Forms.ComboBox();
            this._labelPort = new System.Windows.Forms.Label();
            this._labelIP = new System.Windows.Forms.Label();
            this._labelFont = new System.Windows.Forms.Label();
            this._buttonStartStop = new System.Windows.Forms.Button();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._textBoxIP = new System.Windows.Forms.TextBox();
            this._statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _buttonRefresh
            // 
            this._buttonRefresh.Location = new System.Drawing.Point(145, 28);
            this._buttonRefresh.Name = "_buttonRefresh";
            this._buttonRefresh.Size = new System.Drawing.Size(59, 23);
            this._buttonRefresh.TabIndex = 0;
            this._buttonRefresh.Text = "Refresh";
            this._buttonRefresh.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this._buttonRefresh.UseVisualStyleBackColor = true;
            this._buttonRefresh.Click += new System.EventHandler(this.ButtonRefresh_Click);
            // 
            // _comboBoxPort
            // 
            this._comboBoxPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxPort.FormattingEnabled = true;
            this._comboBoxPort.Location = new System.Drawing.Point(12, 27);
            this._comboBoxPort.Name = "_comboBoxPort";
            this._comboBoxPort.Size = new System.Drawing.Size(127, 23);
            this._comboBoxPort.TabIndex = 1;
            this._comboBoxPort.SelectedIndexChanged += new System.EventHandler(this.ComboBoxPort_SelectedIndexChanged);
            // 
            // _comboBoxFont
            // 
            this._comboBoxFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxFont.FormattingEnabled = true;
            this._comboBoxFont.Location = new System.Drawing.Point(12, 72);
            this._comboBoxFont.Name = "_comboBoxFont";
            this._comboBoxFont.Size = new System.Drawing.Size(333, 23);
            this._comboBoxFont.TabIndex = 3;
            // 
            // _labelPort
            // 
            this._labelPort.AutoSize = true;
            this._labelPort.Location = new System.Drawing.Point(12, 9);
            this._labelPort.Name = "_labelPort";
            this._labelPort.Size = new System.Drawing.Size(59, 15);
            this._labelPort.TabIndex = 4;
            this._labelPort.Text = "MSP Port:";
            // 
            // _labelIP
            // 
            this._labelIP.AutoSize = true;
            this._labelIP.Location = new System.Drawing.Point(227, 11);
            this._labelIP.Name = "_labelIP";
            this._labelIP.Size = new System.Drawing.Size(42, 15);
            this._labelIP.TabIndex = 5;
            this._labelIP.Text = "IP:Port";
            // 
            // _labelFont
            // 
            this._labelFont.AutoSize = true;
            this._labelFont.Location = new System.Drawing.Point(12, 54);
            this._labelFont.Name = "_labelFont";
            this._labelFont.Size = new System.Drawing.Size(31, 15);
            this._labelFont.TabIndex = 6;
            this._labelFont.Text = "Font";
            // 
            // _buttonStartStop
            // 
            this._buttonStartStop.Location = new System.Drawing.Point(390, 42);
            this._buttonStartStop.Name = "_buttonStartStop";
            this._buttonStartStop.Size = new System.Drawing.Size(87, 39);
            this._buttonStartStop.TabIndex = 7;
            this._buttonStartStop.Text = "Start";
            this._buttonStartStop.UseVisualStyleBackColor = true;
            this._buttonStartStop.Click += new System.EventHandler(this.ButtonStartStop_Click);
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripStatusLabel});
            this._statusStrip.Location = new System.Drawing.Point(0, 105);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(509, 22);
            this._statusStrip.SizingGrip = false;
            this._statusStrip.TabIndex = 8;
            this._statusStrip.Text = "statusStrip1";
            // 
            // _toolStripStatusLabel
            // 
            this._toolStripStatusLabel.Name = "_toolStripStatusLabel";
            this._toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // _textBoxIP
            // 
            this._textBoxIP.Location = new System.Drawing.Point(227, 29);
            this._textBoxIP.Name = "_textBoxIP";
            this._textBoxIP.Size = new System.Drawing.Size(118, 23);
            this._textBoxIP.TabIndex = 9;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 127);
            this.Controls.Add(this._textBoxIP);
            this.Controls.Add(this._statusStrip);
            this.Controls.Add(this._buttonStartStop);
            this.Controls.Add(this._labelFont);
            this.Controls.Add(this._labelIP);
            this.Controls.Add(this._labelPort);
            this.Controls.Add(this._comboBoxFont);
            this.Controls.Add(this._comboBoxPort);
            this.Controls.Add(this._buttonRefresh);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "INAV Sim OSD";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button _buttonRefresh;
        private ComboBox _comboBoxPort;
        private ComboBox _comboBoxFont;
        private Label _labelPort;
        private Label _labelIP;
        private Label _labelFont;
        private Button _buttonStartStop;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _toolStripStatusLabel;
        private TextBox _textBoxIP;
    }
}
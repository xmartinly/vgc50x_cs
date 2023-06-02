using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using VGC50x.Plot;
using VGC50x.Utils;
using System.Timers;
using System.Security.Cryptography;

namespace VGC50x
{
    public delegate void SendDataDelegate(string s_data, int data_field);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// SerialPortUtils instance
        /// </summary>
        private readonly SerialPortUtils _ser_conn = new();

        public MainWindow()
        {
            InitializeComponent();
            ReadingsModel.TestReadings(this.readings_plot);
            FindComPorts();
            btn_ctrl.IsEnabled = false;
            _ser_conn.SendData = ReciveMsg;
        }

        #region
        private static System.Timers.Timer? _read_timer;
        private static System.Timers.Timer? _state_timer;

        private readonly string _ayt = "AYT\r";
        private readonly string _tid = "TID\r";
        private readonly string _prx = "PRX\r";
        private readonly string _res = "RES\r";

        private void SetReadTimer(int read_intvl = 200)
        {
            if (read_intvl < 200 && _read_timer != null)
            {
                _read_timer.Stop();
                return;
            }
            if (_read_timer == null)
            {
                _read_timer = new System.Timers.Timer(read_intvl);
            }
            // Hook up the Elapsed event for the timer.
            _read_timer.Elapsed += OnReadTimer;
            _read_timer.AutoReset = true;
            _read_timer.Enabled = true;
        }

        private void OnReadTimer(Object source, ElapsedEventArgs e)
        {
            if (_ser_conn == null || !_ser_conn.GetPortState())
            {
                return;
            }

            _ser_conn.AddCmd(_prx);
        }

        private void SetStateTimer(int state_intvl = 1000)
        {
            if (state_intvl < 1000 && _state_timer != null)
            {
                _state_timer.Stop();
                return;
            }
            if (_state_timer == null)
            {
                _state_timer = new System.Timers.Timer(state_intvl);
            }
            // Hook up the Elapsed event for the timer.
            _state_timer.Elapsed += OnStateTimer;
            _state_timer.AutoReset = true;
            _state_timer.Enabled = true;
        }

        private void OnStateTimer(Object source, ElapsedEventArgs e)
        {
            if (_ser_conn == null || !_ser_conn.GetPortState())
            {
                return;
            }
            _ser_conn.AddCmd(_tid);
        }

        #endregion

        /// <summary>
        /// find avilable serial port
        /// </summary>
        public void FindComPorts()
        {
            string[] ports = _ser_conn.GetPortNames();
            if (ports != null)
            {
                foreach (string port in ports)
                {
                    cb_port.Items.Add(port);
                }
                cb_port.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// start/stop acquire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_ctrl_Click(object sender, RoutedEventArgs e)
        {
            if (_read_timer != null && _read_timer.Enabled)
            {
                SetReadTimer(0);
                return;
            }
            SetReadTimer(200);
        }

        private void Tb_path_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Trace.WriteLine(sender.GetType());
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            tb_path.Text = m_Dir;
        }

        /// <summary>
        /// open serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_conn_Click(object sender, RoutedEventArgs e)
        {
            if (_ser_conn.GetPortState())
            {
                _ser_conn.OpenClosePort();
                if (_read_timer != null)
                {
                    _read_timer.Stop();
                }
                if (_state_timer != null)
                {
                    _state_timer.Stop();
                }
                return;
            }
            string s_port = cb_port.Text;
            string s_bdrate = cb_bdrt.Text;
            int i_bdrate = 115200;
            if (s_bdrate.Length > 0) { i_bdrate = int.Parse(s_bdrate); }
            bool port_opened = _ser_conn.OpenClosePort(s_port, i_bdrate);
            btn_ctrl.IsEnabled = port_opened;
            if (port_opened)
            {
                _ser_conn.WriteCommand(_ayt);

                if (_state_timer != null)
                {
                    SetStateTimer(1000);
                }
            }
        }

        /// <summary>
        /// change report unit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cb_uni_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int i_uni = cb_uni.SelectedIndex;
            if (i_uni < 0 || !_ser_conn.GetPortState()) { return; }
            string str_uni = String.Format("UNI,{0}\r", i_uni);
            byte[] ba_uni = System.Text.Encoding.UTF8.GetBytes(str_uni);
            _ser_conn.WriteCommand(ba_uni);
        }

        private void ReciveMsg(string msg, int field)
        {
            Trace.WriteLine("MainWindow：" + msg);
        }
    }
}
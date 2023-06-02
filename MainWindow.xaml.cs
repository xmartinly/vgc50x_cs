using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using VGC50x.Plot;
using VGC50x.Utils;

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
        private readonly SerialPortUtils m_ser_conn = new();

        public MainWindow()
        {
            InitializeComponent();
            ReadingsModel.TestReadings(this.readings_plot);
            FindComPorts();
            btn_ctrl.IsEnabled = false;
            m_ser_conn.SendData = ReciveMsg;
        }

        /// <summary>
        /// find avilable serial port
        /// </summary>
        public void FindComPorts()
        {
            string[] ports = SerialPortUtils.GetPortNames();
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
            if (m_ser_conn.GetPortState())
            {
                m_ser_conn.OpenClosePort();
            }
            string s_port = cb_port.Text;
            string s_bdrate = cb_bdrt.Text;
            int i_bdrate = 115200;
            if (s_bdrate.Length > 0) { i_bdrate = int.Parse(s_bdrate); }

            Trace.WriteLine(cb_bdrt.Text);
            bool port_opened = m_ser_conn.OpenClosePort(s_port, i_bdrate);
            btn_ctrl.IsEnabled = port_opened;
            if (port_opened)
            {
                string tid = "AYT\r";
                byte[] ba_tid = System.Text.Encoding.UTF8.GetBytes(tid);
                SerialPortUtils.WriteCommand(ba_tid);
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
            if (i_uni < 0 || !m_ser_conn.GetPortState()) { return; }
            string str_uni = String.Format("UNI,{0}\r", i_uni);
            byte[] ba_uni = System.Text.Encoding.UTF8.GetBytes(str_uni);
            SerialPortUtils.WriteCommand(ba_uni);
        }

        private void ReciveMsg(string msg, int field)
        {
            Trace.WriteLine("MainWindow：" + msg);
        }
    }
}
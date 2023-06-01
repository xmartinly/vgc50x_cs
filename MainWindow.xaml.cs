using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using VGC50x.Utils;

namespace VGC50x
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FindComPorts();
            btn_ctrl.IsEnabled = false;
        }

        public void FindComPorts()
        {
            string[] ports = Utils.SerialPortUtils.GetPortNames();
            if (ports != null)
            {
                foreach (string port in ports)
                {
                    cb_port.Items.Add(port);
                }
                cb_port.SelectedIndex = 0;
            }
        }

        private void Btn_ctrl_Click(object sender, RoutedEventArgs e)
        {
            //int open_port = Utils.SerialPortUtils.getPortState();
            //if (open_port == 0)
            //{
            //    string s_port = cb_port.Text;
            //    int i_bdrate = (int)cb_bdrt.SelectedValue;
            //    Utils.SerialPortUtils.OpenClosePort(s_port, i_bdrate);
            //}
            string tid = "UNI,3\r";
            byte[] ba_tid = System.Text.Encoding.UTF8.GetBytes(tid);
            Utils.SerialPortUtils.SendData(ba_tid);
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

        private void Btn_conn_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.SerialPortUtils.getPortState())
            {
                Utils.SerialPortUtils.OpenClosePort();
            }
            string s_port = cb_port.Text;
            string s_bdrate = cb_bdrt.Text;
            int i_bdrate = 115200;
            if (s_bdrate.Length > 0) { i_bdrate = int.Parse(s_bdrate); }

            Trace.WriteLine(cb_bdrt.Text);
            bool port_opened = Utils.SerialPortUtils.OpenClosePort(s_port, i_bdrate);
            btn_ctrl.IsEnabled = port_opened;
            if (port_opened)
            {
                //string tid = "UNI,3\r";
                string tid = "AYT\r";
                byte[] ba_tid = System.Text.Encoding.UTF8.GetBytes(tid);
                Utils.SerialPortUtils.SendData(ba_tid);
            }
        }

        private void Cb_uni_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var cc = cb_uni.SelectedIndex;
            //var aaa = cb_uni.Items.IndexOf(cc);
            Trace.WriteLine(cc);
        }
    }
}
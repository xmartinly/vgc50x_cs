using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Timers;

namespace VGC50x.Utils

{
    public class SerialPortUtils
    {
        private static SerialPort? _serial_port = null;
        private List<byte> _read_buffer = new(4096);
        private readonly byte[] _enq = { 0x05 };
        private readonly List<byte> _msg_end = new() { 0x0d, 0x0a };
        private static Timer? _cmd_timer;
        private Queue<string> _cmd_queue = new();
        private bool _read_process = false;

        public SendDataDelegate? SendData = null;

        public void SetTimer(int intvl = 20)
        {
            // Create a timer with a two second interval.
            if (intvl < 20 && _cmd_timer != null)
            {
                _cmd_timer.Stop();
                return;
            }
            if (_cmd_timer == null)
            {
                _cmd_timer = new Timer(intvl);
            }
            // Hook up the Elapsed event for the timer.
            _cmd_timer.Elapsed += OnTimedEvent;
            _cmd_timer.AutoReset = true;
            _cmd_timer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (_cmd_queue.Count == 0 || _read_process) { return; }
            WriteCommand(_cmd_queue.Dequeue());
        }

        public void ClearCmdQueue()
        {
            _cmd_queue.Clear();
        }

        public void AddCmd(string cmd)
        {
            _cmd_queue.Enqueue(cmd);
        }

        /// <summary>
        /// check byte[] equal
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        private bool ByteEquals(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length) { return false; }
            if (b1 == null || b2 == null) { return false; }
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) { return false; }
            }
            return true;
        }

        /// <summary>
        /// get port names
        /// </summary>
        /// <returns></returns>
        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// get serial port open state
        /// </summary>
        /// <returns></returns>
        public bool GetPortState()
        {
            if (_serial_port == null) { return false; }
            return _serial_port.IsOpen;
        }

        /// <summary>
        /// open or close serial port
        /// </summary>
        /// <param name="comName"></param>
        /// <param name="baud"></param>
        /// <returns></returns>
        public bool OpenClosePort(string comName = "COM1", int baud = 115200)
        {
            //串口未打开
            if (_serial_port == null || !_serial_port.IsOpen)
            {
                _serial_port = new SerialPort
                {
                    //串口名称
                    PortName = comName,
                    //波特率
                    BaudRate = baud,
                    //数据位
                    DataBits = 8,
                    //停止位
                    StopBits = StopBits.One,
                    //校验位
                    Parity = Parity.None
                };
                //打开串口
                _serial_port.Open();
                //串口数据接收事件实现
                _serial_port.DataReceived += new SerialDataReceivedEventHandler(ReceiveData);
                SetTimer();
            }
            //串口已经打开
            else
            {
                _serial_port.Close();
                SetTimer(-1);
            }
            return _serial_port.IsOpen;
        }

        /// <summary>
        /// serial port data receive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReceiveData(object sender, SerialDataReceivedEventArgs e)
        {
            _read_process = true;
            SerialPort _SerialPort = (SerialPort)sender;
            int read_attempts = 0;
            bool read_finished = false;
            bool crlr_chk = false;
            while (!read_finished && read_attempts < 100)
            {
                int read_count = _SerialPort.BytesToRead;
                byte[] temp_bytes = new byte[read_count];
                _SerialPort.Read(temp_bytes, 0, read_count);
                _read_buffer.AddRange(temp_bytes);
                int buff_count = _read_buffer.Count;
                if (buff_count >= 2)
                {
                    List<byte> last_two = _read_buffer.GetRange(buff_count - 2, 2);
                    crlr_chk = Enumerable.SequenceEqual(last_two, _msg_end);
                }
                if (crlr_chk)
                {
                    if (_read_buffer[0] == 0x06)
                    {
                        WriteCommand(_enq);
                        _read_buffer.Clear();
                        return;
                    }
                    if (SendData is not null)
                    {
                        SendData(System.Text.Encoding.Default.GetString(_read_buffer.ToArray()), 1);
                    }
                    _read_buffer.Clear();
                    read_finished = true;
                }
                ++read_attempts;
            }
            _read_process = false;
        }

        /// <summary>
        /// send command
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void WriteCommand(byte[] cmd)
        {
            if (_serial_port != null && _serial_port.IsOpen)
            {
                _serial_port.Write(cmd, 0, cmd.Length);
            }
        }

        public void WriteCommand(string cmd)
        {
            byte[] ba_cmd = System.Text.Encoding.UTF8.GetBytes(cmd);
            if (_serial_port != null && _serial_port.IsOpen)
            {
                _serial_port.Write(ba_cmd, 0, ba_cmd.Length);
            }
        }
    }
}
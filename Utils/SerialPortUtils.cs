using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;

namespace VGC50x.Utils

{
    public class SerialPortUtils
    {
        private static SerialPort? _serial_port = null;
        private List<byte> _read_buffer = new(4096);
        private readonly byte[] _enq = { 0x05 };
        private readonly List<byte> _msg_end = new() { 0x0d, 0x0a };

        private bool _read_process = false;

        private int _read_count = 0;

        private Queue<byte[]> _cmd_queue = new();

        public void StartTimer(int intvl = 100)
        {
            //定义一个对象
            System.Threading.Timer timer = new(
              new System.Threading.TimerCallback(WriteSingleCmd), null,
              0, intvl);//1S定时器
        }

        public void ClearCmdQueue()
        {
            _cmd_queue.Clear();
        }

        public void AddCmd(byte[] cmd)
        {
            _cmd_queue.Enqueue(cmd);
        }

        public void WriteSingleCmd(object? a)
        {
            _read_count++;
            Trace.WriteLine($"ReadCount: {_read_count}");
            if (_cmd_queue.Count == 0 || _read_process) { return; }
            WriteCommand(_cmd_queue.Dequeue());
        }

        public SendDataDelegate? SendData = null;

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
            }
            //串口已经打开
            else
            {
                _serial_port.Close();
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
        public void WriteCommand(byte[] data)
        {
            if (_serial_port != null && _serial_port.IsOpen)
            {
                _serial_port.Write(data, 0, data.Length);
            }
        }
    }
}
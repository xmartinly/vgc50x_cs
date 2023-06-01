using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;

namespace VGC50x.Utils

{
    public class SerialPortUtils
    {
        private SerialPort? serial_port = null;
        private List<byte> buffer = new(4096);
        private readonly byte[] enq = { 0x05 };
        private readonly List<byte> msg_end = new() { 0x0d, 0x0a };

        /// <summary>
        /// check byte[] equal
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        private static bool ByteEquals(byte[] b1, byte[] b2)
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
        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }
        /// <summary>
        /// get serial port open state
        /// </summary>
        /// <returns></returns>
        public bool GetPortState()
        {
            if (serial_port == null) { return false; }
            return serial_port.IsOpen;
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
            if (serial_port == null || !serial_port.IsOpen)
            {
                serial_port = new SerialPort
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
                serial_port.Open();
                //串口数据接收事件实现
                serial_port.DataReceived += new SerialDataReceivedEventHandler(ReceiveData);
            }
            //串口已经打开
            else
            {
                serial_port.Close();
            }
            return serial_port.IsOpen;
        }

        /// <summary>
        /// serial port data receive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReceiveData(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort _SerialPort = (SerialPort)sender;
            int read_attempts = 0;
            bool read_finished = false;
            bool crlr_chk = false;
            while (!read_finished && read_attempts < 100)
            {
                int read_count = _SerialPort.BytesToRead;
                byte[] temp_bytes = new byte[read_count];
                _SerialPort.Read(temp_bytes, 0, read_count);
                buffer.AddRange(temp_bytes);
                int buff_count = buffer.Count;
                if (buff_count >= 2)
                {
                    List<byte> last_two = buffer.GetRange(buff_count - 2, 2);
                    crlr_chk = Enumerable.SequenceEqual(last_two, msg_end);
                }
                if (crlr_chk)
                {
                    if (buffer[0] == 0x06)
                    {
                        SendData(enq);
                    }
                    string str = System.Text.Encoding.Default.GetString(buffer.ToArray());
                    Trace.WriteLine("收到数据：" + str);
                    buffer.Clear();
                    read_finished = true;
                }
                ++read_attempts;
            }
        }

        /// <summary>
        /// send command
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void SendData(byte[] data)
        {
            if (serial_port != null && serial_port.IsOpen)
            {
                serial_port.Write(data, 0, data.Length);
            }
        }
    }
}
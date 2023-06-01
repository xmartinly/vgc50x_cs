using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;

namespace VGC50x.Utils

{
    public class SerialPortUtils
    {
        private readonly byte[] enq = { 0x05 };

        //private readonly byte[] msg_end = { 0x0d, 0x0a };
        private List<byte> buffer = new List<byte>(4096);

        private List<byte> msg_end = new List<byte> { 0x0d, 0x0a };
        private List<byte> ack_trans = new List<byte> { 0x06, 0x0d, 0x0a };

        private bool byteEquals(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length) return false;
            if (b1 == null || b2 == null) return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }

        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        private SerialPort? serial_port = null;

        public bool getPortState()
        {
            if (serial_port == null) { return false; }
            return serial_port.IsOpen;
        }

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

        public void ReceiveData(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort _SerialPort = (SerialPort)sender;
            bool read_finished = false;
            bool crlr_chk = false;
            while (!read_finished)
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
            }

            //int _bytesToRead = _SerialPort.BytesToRead;
            //byte[] recvData = new byte[_bytesToRead];
            //buffer.AddRange(recvData);
            //while (!read_finished)
            //{
            //    _SerialPort.Read(recvData, 0, _bytesToRead);

            //    if (buffer.Length > 2)
            //    {
            //    }
            //}

            //if (byteEquals(recvData, acl_trans))
            //{
            //    SendData(enq);
            //    return;
            //}

            //string str = System.Text.Encoding.Default.GetString(recvData);
            ////向控制台打印数据
            //Trace.WriteLine("收到数据：" + str);
        }

        public bool SendData(byte[] data)
        {
            if (serial_port != null && serial_port.IsOpen)
            {
                serial_port.Write(data, 0, data.Length);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
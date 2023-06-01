using System;
using System.Collections.Generic;
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
            try
            {
                byte[]? readBuffer = null;
                int n = _SerialPort.BytesToRead;
                byte[] buf = new byte[n];
                _SerialPort.Read(buf, 0, n);
                //1.缓存数据
                buffer.AddRange(buf);
                //2.完整性判断
                while (buffer.Count >= 2)
                {
                    if (buffer.Count == 3 && buffer[0] == 0x06)   //ACK 数据比较
                    {
                        if (Enumerable.SequenceEqual(buffer, ack_trans))
                        {
                            SendData(enq);
                            return;
                        }
                        break;
                    }
                    //2.1 查找数据标记头
                    if (buffer[0] == 0x06) //ACK反馈
                    {
                        int len = buffer[1];
                        if (buffer.Count < len + 2)
                        {
                            //数据未接收完整跳出循环
                            break;
                        }
                        readBuffer = new byte[len + 2];
                        //得到完整的数据，复制到readBuffer中
                        buffer.CopyTo(0, readBuffer, 0, len + 2);
                        //从缓冲区中清除
                        buffer.RemoveRange(0, len + 2);

                        //触发外部处理接收消息事件
                    }
                    else //开始标记或版本号不正确时清除
                    {
                        buffer.RemoveAt(0);
                    }
                }
            }
            catch (Exception ex)
            {
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
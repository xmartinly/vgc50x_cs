using System.Collections.Generic;

namespace VGC50x.Utils
{
    internal class ByteArray
    {
        private List<byte> list;

        //初始化
        public ByteArray()
        {
            list = new List<byte>();
        }

        //添加单个字节
        public void Add(byte item)
        {
            list.Add(item);
        }

        //添加数组
        public void Add(byte[] item)
        {
            list.AddRange(item);
        }

        //清除
        public void Clear()
        {
            list = new List<byte>();
        }

        //获取数组
        public byte[] array
        {
            get { return list.ToArray(); }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gungnir
{
    public class StreamManage
    {
        private MemoryStream ms = new MemoryStream();
        private string filepath = null;
        /// <summary>
        /// 内存流
        /// </summary>
        public MemoryStream MS
        {
            get
            {
                return ms;
            }
        }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath
        {
            get
            {
                return filepath;
            }
        }
        /// <summary>
        /// 新建文件
        /// </summary>
        public void NewFile()
        {
            byte[] Buffer = new byte[1];
            Buffer[0] = 0;
            ms.Write(Buffer, 0, 1);
        }
        /// <summary>
        /// 从文件读取数据到内存流
        /// </summary>
        /// <param name="FilePath">文件完整路径</param>
        public bool LoadFile(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                FileStream StreamFrom = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                if (StreamFrom.Length > 0xFFFFFFFF)
                {
                    MessageBox.Show("文件大于4G！", "警告");
                    StreamFrom.Close();
                    StreamFrom.Dispose();
                    return false;
                }
                else
                {
                    try
                    {
                        byte[] Buffer = new byte[1];
                        while ((StreamFrom.Read(Buffer, 0, Buffer.Length)) > 0)
                            ms.Write(Buffer, 0, 1);
                        filepath = FilePath;
                        return true;
                    }
                    catch (OutOfMemoryException)
                    {
                        MessageBox.Show("没有足够的内存！", "警告");
                        return false;
                    }
                }
            }
            else
            {
                MessageBox.Show("文件不存在！", "错误");
                return false;
            }
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="FilePath">文件完整路径</param>
        /// <param name="StreamFrom">内存流</param>
        public void Save(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                FileStream StreamTo = new FileStream(FilePath, FileMode.Open, FileAccess.Write);
                byte[] Buffer = new byte[1];
                ms.Position = 0;
                while (ms.Read(Buffer, 0, 1) > 0)
                    StreamTo.Write(Buffer, 0, 1);
                StreamTo.Close();
                StreamTo.Dispose();
                MessageBox.Show("保存完成！\n总大小：" + ms.Length.ToString(), "提示");
            }
            else
                MessageBox.Show("文件不存在，请选择另存为！", "警告");
        }
        /// <summary>
        /// 另存为文件
        /// </summary>
        /// <param name="FilePath">文件完整路径</param>
        public void SaveToFile(string FilePath)
        {
            FileStream StreamTo = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
            byte[] Buffer = new byte[1];
            ms.Position = 0;
            while (ms.Read(Buffer, 0, 1) > 0)
                StreamTo.Write(Buffer, 0, 1);
            StreamTo.Close();
            StreamTo.Dispose();
            MessageBox.Show("保存完成！\n总大小：" + ms.Length.ToString(), "提示");
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="Position">插入的位置</param>
        /// <param name="Buffer">插入的数据</param>
        /// <returns></returns>
        public bool Instert(int Position, byte Buffer)
        {
            if (ms == null || Position < 0 || Position > ms.Length)
                return false;
            else
            {
                if (1 + ms.Length > 0xFFFFFFFF)
                {
                    MessageBox.Show("无法插入，插入数据后数据大于4G！", "警告");
                    return false;
                }
                else
                {
                    byte[] data = new byte[1];
                    ms.SetLength(ms.Length + 1);
                    for (int i = (int)ms.Length - 2; i >= Position; i--)
                    {
                        ms.Position = i;
                        ms.Read(data, 0, 1);
                        ms.Position = i + 1;
                        ms.Write(data, 0, 1);
                    }
                    ms.Position = Position;
                    ms.WriteByte(Buffer);
                    return true;
                }
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <param name="Position">删除的位置</param>
        /// <param name="buffer">删除的长度</param>
        /// <returns></returns>
        public bool Delete(int Position)
        {
            if (ms == null || Position < 0 || Position > ms.Length)
                return false;
            else
            {
                byte[] data = new byte[1];
                int i = Position;
                int l = 0;
                do
                {
                    ms.Position = i + 1;
                    l = ms.Read(data, 0, data.Length);
                    ms.Position = i;
                    ms.Write(data, 0, l);
                    i += l;
                }
                while (l >= data.Length);
                ms.SetLength(ms.Length - 1);
                return true;
            }
        }
    }
}

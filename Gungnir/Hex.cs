using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.ComponentModel;

namespace Gungnir
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:Gungnir"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:Gungnir;assembly=Gungnir"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误: 
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:Hex/>
    ///
    /// </summary>
    public class Hex : Control
    {
        static Hex()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Hex), new FrameworkPropertyMetadata(typeof(Hex)));
        }
        #region 字段
        private long linecount = 1;
        private int cursoroffset = 0;
        private bool ishex = true;
        private bool hexhigh = false;
        public IntPtr hwnd;
        public ScrollViewer sv = new ScrollViewer();
        private int ViewLine = 28;
        private int baseaddress = 0;
        private int viewaddress = 0;
        private bool resize = false;
        public TextBlock stattext;
        #endregion

        #region 属性
        /// <summary>
        /// HEX流
        /// </summary>
        public StreamManage SM = new StreamManage();
        /// <summary>
        /// 光标是否在高位
        /// </summary>
        public bool IsHigh
        {
            get
            {
                return hexhigh;
            }
            set
            {
                hexhigh = value;
            }
        }
        /// <summary>
        /// 光标是否在hex中
        /// </summary>
        public bool IsHex
        {
            get
            {
                return ishex;
            }
            set
            {
                ishex = value;
            }
        }
        /// <summary>
        /// 光标对应的数据位置
        /// </summary>
        public int HCursor
        {
            get
            {
                return cursoroffset;
            }
            set
            {
                cursoroffset = value;
            }
        }
        /// <summary>
        /// 行数
        /// </summary>
        public long LineCount
        {
            get
            {
                return linecount;
            }
            set
            {
                linecount = value;
            }
        }
        /// <summary>
        /// 界面显示的数据的起始地址
        /// </summary>
        public int BaseAddress
        {
            get
            {
                return baseaddress;
            }
            set
            {
                baseaddress = value;
            }
        }
        /// <summary>
        /// 是否可以更改大小，是否是插入模式
        /// </summary>
        public bool IsReSize
        {
            get
            {
                return resize;
            }
            set
            {
                resize = value;
            }
        }
        #endregion

        #region 元素信息
        /// <summary>
        /// 元素类型
        /// </summary>
        public enum EleType
        {
            /// <summary>
            /// 空
            /// </summary>
            None,
            /// <summary>
            /// 地址区
            /// </summary>
            Address,
            /// <summary>
            /// 十六进制区
            /// </summary>
            Hex,
            /// <summary>
            /// 字符区
            /// </summary>
            Char
        }
        /// <summary>
        /// 元素信息
        /// </summary>
        public class EleInfo
        {
            internal EleType type;
            internal int col = 0;
            internal int row = 0;
            internal int x = 0;
            internal int y = 0;
            public int X
            {
                get
                {
                    return x;
                }
            }
            public int Y
            {
                get
                {
                    return y;
                }
            }
            public int ColumnIndex
            {
                get
                {
                    return col;
                }
            }
            public int RowIndex
            {
                get
                {
                    return row;
                }
            }
        }
        /// <summary>
        /// 获取元素信息
        /// </summary>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        /// <returns>返回元素信息</returns>
        public EleInfo GetEleInfo(int x, int y)
        {
            EleInfo info = new EleInfo();
            info.y = y;
            info.x = x;
            info.row = info.y;
            if (info.x >= 0 && info.x <= 64)
            {
                info.type = EleType.Address;
            }
            else if (info.x > 80 && info.x <= 464)
            {
                info.type = EleType.Hex;
                info.col = (info.x - 80) / 24;
                if (info.x <= info.col * 24 + 80 + 8 && info.x >= info.col * 24 + 80)
                    IsHigh = true;
                else if (info.x > info.col * 24 + 80 + 8 && info.x <= info.col * 24 + 80 + 24)
                    IsHigh = false;
            }
            else if (info.x > 480 && info.x <= 608)
            {
                info.type = EleType.Char;
                info.col = (info.x - 480) / 8;
                IsHigh = false;
            }
            info.row = info.y / 16;
            return info;
        }
        /*ASCII编码时，黑体，字号16，单字节字符高16宽8，以此计算字符坐标，
        地址区起0至64，hex区起80至464（最后有个空格），char区起480至608*/
        #endregion

        #region 绘制函数
        /// <summary>
        /// 画地址区，蓝色
        /// </summary>
        /// <param name="dc">画布</param>
        /// <param name="StartOffset">起始地址</param>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        protected virtual void DrawAddress(DrawingContext dc, int StartOffset, int x, int y)
        {
            dc.DrawText(new FormattedText(StartOffset.ToString("X8"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("黑体"), 16, Brushes.Blue), new Point(x, y));
        }
        /// <summary>
        /// 画hex区，黑色
        /// </summary>
        /// <param name="dc">画布</param>
        /// <param name="buf">缓存区</param>
        /// <param name="line">第几行</param>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        protected virtual void DrawHex(DrawingContext dc, byte[] buf, int line, int x, int y)
        {
            StringBuilder vBuffer = new StringBuilder(128);
            int j = 16;
            if (SM.MS.Length % 16 != 0)
            {
                if (line == LineCount - 1)
                    j = (int)SM.MS.Length % 16;
            }
            if (line >= linecount)
                j = 0;
            for (int i = 0; i < j; i++)
            {
                vBuffer.Append(buf[i].ToString("X2"));
                vBuffer.Append(" ");
            }
                dc.DrawText(new FormattedText(vBuffer.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("黑体"), 16, Brushes.Black), new Point(x, y));
        }
        /// <summary>
        /// 话字符区，绿色
        /// </summary>
        /// <param name="dc">画布</param>
        /// <param name="buf">缓存区</param>
        /// <param name="line">第几行</param>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        protected virtual void DrawChar(DrawingContext dc, byte[] buf, int line, int x, int y)
        {
            StringBuilder vBuffer = new StringBuilder(128);
            int j = 16;
            if (SM.MS.Length % 16 != 0)
            {
                if (line == LineCount - 1)
                    j = (int)SM.MS.Length % 16;
            }
            if (line >= linecount)
                j = 0;
            for (int i = 0; i < j; i++)
                if (buf[i] > 32 && buf[i] < 126)
                    vBuffer.Append(((char)buf[i]).ToString());
                else
                    vBuffer.Append(".");
            dc.DrawText(new FormattedText(vBuffer.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("黑体"), 16, Brushes.Green), new Point(x, y));
        }
        #endregion
        public Hex()
        {
            Height = LineCount * 16;
            //Width = 608;
            sv.Content = this;
            sv.VerticalAlignment = VerticalAlignment.Top;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.ScrollChanged += sv_ScrollChanged;
            Loaded += window_loaded;
            sv.KeyDown += sv_OnKeyDown;
            Cursor = Cursors.IBeam;
        }
        private void window_loaded(object sender, RoutedEventArgs e)
        {
            hwnd = ((HwndSource)PresentationSource.FromVisual(this)).Handle;
            //ScrollBar sb = sv.Template.FindName("PART_VerticalScrollBar", sv) as ScrollBar;
        }
        private void sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            BaseAddress = (int)sv.VerticalOffset / 16 * 16;
            if (sv.VerticalOffset % 16 > 8)
                BaseAddress += 16;
            InvalidateVisual();
        }
        private void sv_OnKeyDown(object sender, KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            byte[] data = new byte[1];
            byte[] old = new byte[1];
            SM.MS.Position = HCursor;
            SM.MS.Read(old, 0, 1);
            SM.MS.Position = HCursor;
            switch (e.Key)
            {
                #region 键盘与字符转换及写入流
                case Key.A:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(0xA + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0xA0 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.B:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(0xB + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0xB0 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.C:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(0xC + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0xC0 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.D:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(0xD + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0xD0 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.E:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(0xE + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0xE0 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.F:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(0xF + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0xF0 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad0:
                case Key.D0:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(old[0] & 0xF0);
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(old[0] & 0xF);
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad1:
                case Key.D1:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(1 + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0x10 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad2:
                case Key.D2:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(2 + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0x20 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad3:
                case Key.D3:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(3 + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0x30 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad4:
                case Key.D4:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(4 + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0x40 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad5:
                case Key.D5:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(5 + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0x50 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad6:
                case Key.D6:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(6 + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0x60 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad7:
                case Key.D7:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(7 + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0x70 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad8:
                case Key.D8:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(8 + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0x80 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.NumPad9:
                case Key.D9:
                    if (IsHex == true)
                    {
                        if (IsHigh == false)
                        {
                            data[0] = (byte)(9 + (old[0] & 0xF0));
                            if (HCursor != SM.MS.Length - 1)
                            {
                                HCursor += 1;
                                IsHigh = true;
                            }
                        }
                        else
                        {
                            data[0] = (byte)(0x90 + (old[0] & 0xF));
                            IsHigh = false;
                        }
                        SM.MS.Write(data, 0, 1);
                        BaseAddress = viewaddress;
                        sv.ScrollToVerticalOffset(BaseAddress);
                        InvalidateVisual();
                    }
                    else
                        MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
                    break;
                case Key.Delete:
                case Key.Back:
                    if (IsHex == true)
                    {
                        if (IsReSize == false)
                        {
                            if (IsHigh == true)
                            {
                                data[0] = (byte)(old[0] & 0xF);
                                if (HCursor != 0)
                                {
                                    HCursor -= 1;
                                    IsHigh = false;
                                }
                            }
                            else
                            {
                                data[0] = (byte)(old[0] & 0xF0);
                                IsHigh = true;
                            }
                            SM.MS.Write(data, 0, 1);
                            BaseAddress = viewaddress;
                            sv.ScrollToVerticalOffset(BaseAddress);
                        }
                        else
                        {
                            SM.Delete(HCursor);
                            if (HCursor == (int)SM.MS.Length)
                                HCursor -= 1;
                        }
                    }
                    else
                    {
                        if (IsReSize == false)
                        {
                            data[0] = 0;
                            SM.MS.Write(data, 0, 1);
                            HCursor -= 1;
                            BaseAddress = viewaddress;
                            sv.ScrollToVerticalOffset(BaseAddress);
                        }
                        else
                        {
                            SM.Delete(HCursor);
                            if (HCursor == (int)SM.MS.Length)
                                HCursor -= 1;
                        }
                    }
                    InvalidateVisual();
                    break;
                    #endregion
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point mouseXY = Mouse.GetPosition(this);
            if (mouseXY.X < 80)
                Cursor = Cursors.Arrow;
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ButtonState == e.LeftButton)
            {
                Point mouseXY = Mouse.GetPosition(this);
                EleInfo ele = GetEleInfo((int)mouseXY.X, (int)mouseXY.Y);
                HCursor = ele.RowIndex * 16 + ele.ColumnIndex;
                if (ele.type == EleType.Hex)
                    IsHex = true;
                else if (ele.type == EleType.Char)
                    IsHex = false;
                viewaddress = BaseAddress;
                InvalidateVisual();
            }
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            LineCount = SM.MS.Length / 16;
            if (SM.MS.Length % 16 != 0)
                LineCount += 1;
            int startoffset = 0;
            Rect hselect;
            Brush hbrush, cbrush;
            if (IsHex == true)
            {
                hbrush = Brushes.Yellow;
                cbrush = Brushes.Red;
            }
            else
            {
                hbrush = Brushes.Red;
                cbrush = Brushes.Yellow;
            }
            SM.MS.Position = BaseAddress;
            if (IsHigh == false)
                hselect = new Rect(new Point(HCursor % 16 * 24 + 80 + 8, HCursor / 16 * 16), new Point(HCursor % 16 * 24 + 16 + 80, HCursor / 16 * 16 + 16));
            else
                hselect = new Rect(new Point(HCursor % 16 * 24 + 80, HCursor / 16 * 16), new Point(HCursor % 16 * 24 + 8 + 80, HCursor / 16 * 16 + 16));
            dc.DrawRectangle(hbrush, null, hselect);
            Rect cselect = new Rect(new Point(HCursor % 16 * 8 + 480, HCursor / 16 * 16), new Point(HCursor % 16 * 8 + 8 + 480, HCursor / 16 * 16 + 16));
            dc.DrawRectangle(cbrush, null, cselect);
            for (int i = 0, y = 0; i < ViewLine; i++, y += 16)
            {
                byte[] vBuffer = new byte[16];
                SM.MS.Read(vBuffer, 0, vBuffer.Length);
                if (BaseAddress + startoffset < SM.MS.Length)
                    DrawAddress(dc, BaseAddress + startoffset, 0, y + (int)sv.VerticalOffset);
                DrawHex(dc, vBuffer, BaseAddress/16 + i, 80, y + (int)sv.VerticalOffset);
                DrawChar(dc, vBuffer, BaseAddress/16 + i, 480, y + (int)sv.VerticalOffset);
                startoffset += 16;
                SM.MS.Position = BaseAddress + startoffset;
            }
            Height = LineCount * 16;
            stattext.Text = "行：" + (HCursor / 16 + 1).ToString() + "  列：" + (HCursor % 16 + 1).ToString() + " 总大小：0x" + SM.MS.Length.ToString("X");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gungnir
{
    /// <summary>
    /// Change.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeWindow : Window
    {
        public ChangeWindow()
        {
            InitializeComponent();
        }
        public Hex hex;
        public class Member
        {
            public string 偏移地址 { get; set; }
            public string 原文件 { get; set; }
            public string 对比文件 { get; set; }
        }
        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog FileDialog = new Microsoft.Win32.OpenFileDialog();
            FileDialog.Filter = "所有文件|*.*";
            FileDialog.Title = "打开文件";
            if (FileDialog.ShowDialog() == true)
            {
                PathText.Text = FileDialog.FileName;
            }
        }

        private void StartFind_Click(object sender, RoutedEventArgs e)
        {
            StreamManage newsm = new StreamManage();
            if(newsm.LoadFile(PathText.Text))
            {
                int length;
                byte[] oldb = new byte[1];
                byte[] newb = new byte[1];
                ObservableCollection<Member> memberData = new ObservableCollection<Member>();
                if (newsm.MS.Length >= hex.SM.MS.Length)
                    length = (int)hex.SM.MS.Length;
                else
                    length = (int)newsm.MS.Length;
                for (int i=0;i<length;i++)
                {
                    newsm.MS.Position = i;
                    hex.SM.MS.Position = i;
                    newsm.MS.Read(newb, 0, 1);
                    hex.SM.MS.Read(oldb, 0, 1);
                    if (oldb[0] != newb[0])
                    {
                        memberData.Add(new Member()
                        {
                            偏移地址 = i.ToString("X8"),
                            原文件 = oldb[0].ToString("X2"),
                            对比文件 = newb[0].ToString("X2")
                        });
                    }
                }
                dataGrid.ItemsSource = memberData;
                ViewSize.Text = "原文件大小：0x" + hex.SM.MS.Length.ToString("X") + " " + "对比文件大小：0x" + newsm.MS.Length.ToString("X");
            }
        }
    }
}

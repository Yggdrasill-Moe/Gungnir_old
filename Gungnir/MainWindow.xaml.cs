using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Windows.Interop;

namespace Gungnir
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        Hex newhex;
        ChangeWindow window;

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog FileDialog = new Microsoft.Win32.OpenFileDialog();
            FileDialog.Filter = "所有文件|*.*";
            FileDialog.Title = "打开文件";
            if (FileDialog.ShowDialog() == true)
            {
                newhex = new Hex();
                TabItem MyTab = new TabItem();
                HexTab.Items.Clear();
                HexTab.Items.Add(MyTab);
                HexTab.SelectedIndex = HexTab.Items.Count - 1;
                newhex.SM.LoadFile(FileDialog.FileName);
                MyTab.Content = newhex.sv;
                MyTab.Header = FileDialog.SafeFileName;
                Save.IsEnabled = true;
                SaveAS.IsEnabled = true;
                OpenFile.IsEnabled = true;
                NewFile.IsEnabled = true;
                Change.IsEnabled = true;
                TSave.IsEnabled = true;
                TSaveAs.IsEnabled = true;
                TOpenFile.IsEnabled = true;
                TNewFile.IsEnabled = true;
                TChange.IsEnabled = true;
                RNoSize.IsEnabled = true;
                RReSize.IsEnabled = true;
                RNoSize.IsChecked = true;
                newhex.IsReSize = false;
                newhex.stattext = StatText;
            }
        }

        private void SaveAS_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog FileDialog = new Microsoft.Win32.SaveFileDialog();
            FileDialog.Title = "另存为文件";
            FileDialog.Filter = "所有文件|*.*";
            if (FileDialog.ShowDialog() == true)
                newhex.SM.SaveToFile(FileDialog.FileName);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            newhex = new Hex();
            TabItem MyTab = new TabItem();
            HexTab.Items.Clear();
            HexTab.Items.Add(MyTab);
            HexTab.SelectedIndex = HexTab.Items.Count - 1;
            newhex.SM.NewFile();
            MyTab.Content = newhex.sv;
            MyTab.Header = "新建文件";
            SaveAS.IsEnabled = true;
            OpenFile.IsEnabled = true;
            NewFile.IsEnabled = true;
            Change.IsEnabled = true;
            TSave.IsEnabled = false;
            TSaveAs.IsEnabled = true;
            TOpenFile.IsEnabled = true;
            TNewFile.IsEnabled = true;
            TChange.IsEnabled = true;
            RNoSize.IsEnabled = true;
            RReSize.IsEnabled = true;
            RNoSize.IsChecked = true;
            newhex.IsReSize = false;
            newhex.stattext = StatText;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            newhex.SM.Save(newhex.SM.FilePath);
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            window = new ChangeWindow();
            //window.Owner = Application.Current.MainWindow;
            window.Show();
            window.hex = newhex;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("作者：Destinyの火狐\n2016-03-28", "作者信息");
        }

        private void RNoSize_Checked(object sender, RoutedEventArgs e)
        {
            newhex.IsReSize = false;
            InEnter.IsEnabled = false;
            InTextBlock.IsEnabled = false;
            InText.IsEnabled = false;
            InText.Text = null;
        }

        private void RReSize_Checked(object sender, RoutedEventArgs e)
        {
            newhex.IsReSize = true;
            InEnter.IsEnabled = true;
            InTextBlock.IsEnabled = true;
            InText.IsEnabled = true;
        }

        private void InEnter_Click(object sender, RoutedEventArgs e)
        {
            if (InText.Text.Length > 0)
            {
                if (newhex.IsHex == true)
                {
                    byte data = Convert.ToByte(InText.Text, 16);
                    newhex.SM.Instert(newhex.HCursor, data);
                    newhex.InvalidateVisual();
                }
                else
                    MessageBox.Show("无法通过编辑ASCII来编辑HEX", "错误");
            }
        }

        private void InText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.D1 &&
                e.Key != Key.NumPad1 &&
                e.Key != Key.D2 &&
                e.Key != Key.NumPad2 &&
                e.Key != Key.D3 &&
                e.Key != Key.NumPad3 &&
                e.Key != Key.D4 &&
                e.Key != Key.NumPad4 &&
                e.Key != Key.D5 &&
                e.Key != Key.NumPad5 &&
                e.Key != Key.D6 &&
                e.Key != Key.NumPad6 &&
                e.Key != Key.D7 &&
                e.Key != Key.NumPad7 &&
                e.Key != Key.D8 &&
                e.Key != Key.NumPad8 &&
                e.Key != Key.D9 &&
                e.Key != Key.NumPad9 &&
                e.Key != Key.D0 &&
                e.Key != Key.NumPad0 &&
                e.Key != Key.A &&
                e.Key != Key.B &&
                e.Key != Key.C &&
                e.Key != Key.D &&
                e.Key != Key.E &&
                e.Key != Key.F &&
                e.Key != Key.Back &&
                e.Key != Key.Delete &&
                e.Key != Key.Left &&
                e.Key != Key.Right)
            {
                e.Handled = true;
            }
        }
    }
}

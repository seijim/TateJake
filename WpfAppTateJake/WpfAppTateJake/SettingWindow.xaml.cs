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
using System.Windows.Shapes;

namespace WpfAppTateJake
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        MainWindow mainWindow;

        public SettingWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            ComboBoxFontType.Text = Properties.Settings.Default["FontType"].ToString();
            ComboBoxFontWeight.Text = Properties.Settings.Default["FontWeight"].ToString();
            TextBoxFontSize.Text = Properties.Settings.Default["FontSize"].ToString();
            TextBoxAdjustedSize.Text = Properties.Settings.Default["AdjustedSize"].ToString();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["FontType"] = ComboBoxFontType.Text.Trim();
            Properties.Settings.Default["FontWeight"] = ComboBoxFontWeight.Text.Trim();
            double fontSize;
            if (double.TryParse(TextBoxFontSize.Text.Trim(), out fontSize))
            {
                Properties.Settings.Default["FontSize"] = fontSize;
            }
            double adjustedSize;
            if (double.TryParse(TextBoxAdjustedSize.Text.Trim(), out adjustedSize))
            {
                Properties.Settings.Default["AdjustedSize"] = adjustedSize;
            }

            Properties.Settings.Default.Save();
            mainWindow.UpdateDefaultSetting();
            this.Close();
        }
    }
}

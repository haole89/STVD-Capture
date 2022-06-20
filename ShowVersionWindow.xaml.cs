using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace AVerCapSDKDemo
{
    public struct SYSTEM_INFO
    {
        public ushort wProcessorArchitecture;
        public ushort wReserved;
        public uint dwPageSize;
        public uint lpMinimumApplicationAddress;
        public uint lpMaximumApplicationAddress;
        public uint dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort wProcessorLevel;
        public ushort wProcessorRevision;
    }

    /// <summary>
    /// ShowVersionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShowVersionWindow : Window
    {
        const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;

        public ShowVersionWindow()
        {
            InitializeComponent();
            ShowVersion();
        }

        [DllImport("kernel32.dll")]
        public static extern void GetNativeSystemInfo(ref SYSTEM_INFO SystemInfo);

        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileString(string szSectionName, string szKeyName, string szDefault,
                                                          StringBuilder szRetValue, uint uSize, string szFileName);

        private void ShowVersion()
        {
            string szFileName =  System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            szFileName = szFileName.Substring(0,szFileName.LastIndexOf('\\'));
            szFileName = szFileName + "\\version.ini";
            StringBuilder szValue = new StringBuilder("", 20);
            GetPrivateProfileString("Product", "Version", "", szValue, 20, szFileName);
            this.label6.Content = szValue.ToString();
            GetPrivateProfileString("DemoAP", "CaptureSDK Version", "", szValue, 10, szFileName);
            this.label7.Content = szValue.ToString();
            GetPrivateProfileString("API Dll", "CaptureSDK Version", "", szValue, 10, szFileName);
            this.label8.Content = szValue.ToString();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

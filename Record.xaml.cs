using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

namespace AVerCapSDKDemo
{
    /// <summary>
    /// Record.xaml 的交互逻辑
    /// </summary>
    public partial class Record : Window
    {
        private IntPtr m_hCaptureDevice = new IntPtr(0);
        private IntPtr m_hRecordobject = new IntPtr(0);
        private IntPtr m_hRecordobject_2 = new IntPtr(0);
        private DEMOSTATE m_DemoState = DEMOSTATE.DEMO_STATE_STOP;
        private MainWindow m_obMainWindow;
        public bool m_bHide = true;

        private uint m_DeviceType;
        //
        private string m_szFileName = null;
        private string m_szFileName_Audio = null;
        string start_time = "06";
        int start_time_hour = 06;
        int start_time_min = 00;
        string end_time = "02";
        int end_time_hour = 02;
        int end_time_min = 00;
        //Thread t;
        DateTime current_time;
        private DispatcherTimer timer;

        int check; // 0: reset lại giá trị khi đến end_time; 1: đang trong thời gian record
                       // check để kiểm soát việc nếu khi chạy chương trình tại thời điểm không phải là start thì vẫn ghi file
        int check2 ; // 0: Trong ngày chưa stop; 1: đến thời điểm stop



        //
        [DllImport("kernel32.dll")]
        public static extern void GetNativeSystemInfo(ref SYSTEM_INFO SystemInfo);
        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileString(string szSectionName, string szKeyName, string szDefault,
                                                          StringBuilder szRetValue, uint uSize, string szFileName);
        [DllImport("kernel32.dll")]
        public static extern uint WritePrivateProfileString(string szSectionName, string szKeyName, string szValueName, string szFileName);
        public Record(uint dwDeviceType, MainWindow Window, DEMOSTATE DemoState, IntPtr hCaptureDevice)
        {
            m_hCaptureDevice = hCaptureDevice;
            m_DemoState = DemoState;
            m_obMainWindow = Window;
            m_DeviceType = dwDeviceType;
            InitializeComponent();
            InitWindow();

        }

        private void InitWindow()
        {
            InitCommon();
        }

        private void InitCommon()
        {
            button_Start.IsEnabled = true;
            button_Stop.IsEnabled = false;
            button_Start_Video.IsEnabled = true;
            button_Stop_Video.IsEnabled = false;
            if (m_DemoState == DEMOSTATE.DEMO_STATE_STOP ||
                (m_DemoState & DEMOSTATE.DEMO_STATE_CAP_IMAGE) == DEMOSTATE.DEMO_STATE_CAP_IMAGE ||
                (m_DemoState & DEMOSTATE.DEMO_STATE_CALLBACK_VIDEO) == DEMOSTATE.DEMO_STATE_CALLBACK_VIDEO
                )
            {
                button_Start.IsEnabled = false;
                button_Stop.IsEnabled = false;
                button_Start_Video.IsEnabled = false;
                button_Stop_Video.IsEnabled = false;
            }
            check = 0; check2 = 0;
            

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }



        private void button_StartCommon_Click(object sender, RoutedEventArgs e)
        {
            button_Start.IsEnabled = false;
            button_Stop.IsEnabled = true;
            m_szFileName = textBox_ConfigPath.Text;
            m_szFileName_Audio = textBox_ConfigPath_Audio.Text;
            start_time = txt_start.Text;
            end_time = txt_end.Text;
            button_Start_Video.IsEnabled = false;
            button_Stop_Video.IsEnabled = false;

            try
            {
                start_time_hour = int.Parse(start_time);
                end_time_hour = int.Parse(end_time);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception converting datetime", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


            timer = new DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
            // new thread
            //t = new Thread(doing_work);
            //t.Start();



        }

        //async
        private void TimerTick(object sender, EventArgs e)
        {
                       
            current_time = DateTime.Now;
            int current_hour = current_time.Hour;
            
            // current time equals end time                
            if (current_hour == end_time_hour && check2 == 1)
            {
                AVerCapAPI.AVerStopRecordFile(m_hRecordobject);
                AVerCapAPI.AVerStopRecordFile(m_hRecordobject_2);
                check = 0; check2 = 0; 
            }
            else
            {
                // long capture
                if (current_hour >= start_time_hour && check == 0)
                {
                    doing_capture();
                    check = 1; check2 = 1;

                }
            }
            

        }
        private void doing_capture()
        {
            // make new record
            StringBuilder szValue = new StringBuilder("", 10);
            StringBuilder szValue_audio = new StringBuilder("", 10);
            GetPrivateProfileString("CHANEL", "CHANEL_NUMBER", "", szValue, 10, m_szFileName);
            GetPrivateProfileString("CHANEL", "CHANEL_NUMBER", "", szValue_audio, 10, m_szFileName_Audio);

            string pre_name = "c" + szValue.ToString() + "_" + current_time.ToString("yyyyMMdd") + "_video.mp4";
            string video_name = "D:\\Recording\\Data\\" + pre_name;
            string pre_audio = "c" + szValue.ToString() + "_" + current_time.ToString("yyyyMMdd") + "_audio.mp4";
            string audio_name = "D:\\Recording\\Data\\" + pre_audio;

            WritePrivateProfileString("OUTPUT_PROPERTY", "AVM_REC_OUTPUTINFO_FILENAME", video_name, m_szFileName);
            WritePrivateProfileString("OUTPUT_PROPERTY", "AVM_REC_OUTPUTINFO_FILENAME", audio_name, m_szFileName_Audio);

            int iRetVal = AVerCapAPI.AVerStartRecordFile(m_hCaptureDevice, ref m_hRecordobject, m_szFileName);
            int iRetVal_2 = AVerCapAPI.AVerStartRecordFile(m_hCaptureDevice, ref m_hRecordobject_2, m_szFileName_Audio);
            if (iRetVal != 0 || iRetVal_2 != 0)
            {
                if (iRetVal == (int)ERRORCODE.CAP_EC_HDCP_PROTECTED_CONTENT)
                {
                    AVerCapAPI.AVerStopRecordFile(m_hRecordobject);
                    MessageBox.Show("The program content is protected!");
                    return;
                }
                if (iRetVal_2 == (int)ERRORCODE.CAP_EC_HDCP_PROTECTED_CONTENT)
                {
                    AVerCapAPI.AVerStopRecordFile(m_hRecordobject_2);
                    MessageBox.Show("The program content is protected!");
                    return;
                }
                AVerCapAPI.AVerStopRecordFile(m_hRecordobject);
                AVerCapAPI.AVerStopRecordFile(m_hRecordobject_2);
                return;
            }
        }
        private void doing_capture_video()
        {
            // make new record
            StringBuilder szValue = new StringBuilder("", 10);
       
            GetPrivateProfileString("CHANEL", "CHANEL_NUMBER", "", szValue, 10, m_szFileName);    

            string pre_name = "c" + szValue.ToString() + "_" + current_time.ToString("yyyyMMdd") + ".mp4";
            string video_name = "D:\\Recording\\Data\\" + pre_name;            

            WritePrivateProfileString("OUTPUT_PROPERTY", "AVM_REC_OUTPUTINFO_FILENAME", video_name, m_szFileName);
           
            int iRetVal = AVerCapAPI.AVerStartRecordFile(m_hCaptureDevice, ref m_hRecordobject, m_szFileName);
           
            if (iRetVal != 0)
            {
                if (iRetVal == (int)ERRORCODE.CAP_EC_HDCP_PROTECTED_CONTENT)
                {
                    AVerCapAPI.AVerStopRecordFile(m_hRecordobject);
                    MessageBox.Show("The program content is protected!");
                    return;
                }                
                AVerCapAPI.AVerStopRecordFile(m_hRecordobject);
                return;
            }
        }


        private void button_StopCommon_Click(object sender, RoutedEventArgs e)
        {
            AVerCapAPI.AVerStopRecordFile(m_hRecordobject);
            AVerCapAPI.AVerStopRecordFile(m_hRecordobject_2);
            button_Start.IsEnabled = true;
            button_Stop.IsEnabled = false;
            //t.Abort();
            button_Start_Video.IsEnabled = true;
            button_Stop_Video.IsEnabled = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            AVerCapAPI.AVerStopRecordFile(m_hRecordobject);
            AVerCapAPI.AVerStopRecordFile(m_hRecordobject_2);
            //t.Abort();
        }

        public void UpdateDemoWindow(DEMOSTATE DemoState)
        {
            m_DemoState = DemoState;
            InitWindow();
        }

        private void button_Browse_Click(object sender, RoutedEventArgs e)
        {
            Stream myStream;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "config File (*.ini)|*.ini";
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = "D:\\Recording\\Configure\\Video";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                if ((myStream = openFileDialog.OpenFile()) != null)
                {
                    textBox_ConfigPath.Text = openFileDialog.FileName;
                    myStream.Close();
                }
            }
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime current_time;
                current_time = DateTime.Now;
                string xml_path = "https://xmltv.ch/xmltv/xmltv-tnt.xml";
                String pre_name = current_time.ToString("yyyy-MM-dd_HHmmss") + ".xml";
                String xml_name = "D:\\Recording\\xml\\" + pre_name;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                System.Net.WebClient wc = new System.Net.WebClient();

                //string webData = wc.DownloadString(xml_path);
                var htmlData = wc.DownloadData(xml_path);
                var webData = Encoding.UTF8.GetString(htmlData);
                System.IO.File.WriteAllText(xml_name, webData, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception saving xml", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void button_Browse_Audio_Click(object sender, RoutedEventArgs e)
        {
            Stream myStream;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "config File (*.ini)|*.ini";
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = "D:\\Recording\\Configure\\Audio";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                if ((myStream = openFileDialog.OpenFile()) != null)
                {
                    textBox_ConfigPath_Audio.Text = openFileDialog.FileName;
                    myStream.Close();
                }
            }
        }

        //async
        private void TimerTick_Video(object sender, EventArgs e)
        {

            current_time = DateTime.Now;
            int current_hour = current_time.Hour;

            // current time equals end time                
            if (current_hour == end_time_hour && check2 == 1)
            {
                AVerCapAPI.AVerStopRecordFile(m_hRecordobject);               
                check = 0; check2 = 0;
            }
            else
            {
                // long capture
                if (current_hour >= start_time_hour && check == 0)
                {
                    doing_capture_video();
                    check = 1; check2 = 1;

                }
            }
        }

        private void button_Start_Video_Click(object sender, RoutedEventArgs e)
        {
            button_Start_Video.IsEnabled = false;
            button_Stop_Video.IsEnabled = true;
            m_szFileName = textBox_ConfigPath.Text;           
            start_time = txt_start.Text;
            end_time = txt_end.Text;
            button_Stop.IsEnabled = false;
            button_Start.IsEnabled = false;

            try
            {
                start_time_hour = int.Parse(start_time);
                end_time_hour = int.Parse(end_time);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception converting datetime", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


            timer = new DispatcherTimer();
            timer.Tick += TimerTick_Video;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }



        private void button_Stop_Video_Click(object sender, RoutedEventArgs e)
        {
            AVerCapAPI.AVerStopRecordFile(m_hRecordobject);
            button_Start_Video.IsEnabled = true;
            button_Stop_Video.IsEnabled = false;
            button_Stop.IsEnabled = true;
            button_Start.IsEnabled = true;
        }
    }
}

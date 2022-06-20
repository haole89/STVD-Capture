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
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace AVerCapSDKDemo
{
    /// <summary>
    /// CaptureImage.xaml 的交互逻辑
    /// </summary>
    public partial class CaptureImage : Window
    {
        private IntPtr m_hCaptureDevice = new IntPtr(0);
        private DEMOSTATE m_DemoState = DEMOSTATE.DEMO_STATE_STOP;
        private MainWindow m_obMainWindow;
        private CAPTURE_IMAGE_INFO m_CaptureImageInfo = new CAPTURE_IMAGE_INFO();
        public bool m_bHide = true;
        private bool m_bStart = false;
        private string m_strSavePath;
        private int m_iImageType=0;

        public CaptureImage(MainWindow Window, DEMOSTATE DemoState, IntPtr hCaptureDevice)
        {
            InitializeComponent();
            m_obMainWindow = Window;
            m_DemoState = DemoState;
            m_hCaptureDevice = hCaptureDevice;
            string str = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (str != "")
            {
                m_strSavePath = str + "\\AVerCapSDK.bmp";
            }
            else
            {
                m_strSavePath = "D:\\AVerCapSDK.bmp";
            }
            m_CaptureImageInfo.dwVersion = 1;
            m_CaptureImageInfo.dwImageType = 1;
            m_CaptureImageInfo.bOverlayMix = 1;
            m_CaptureImageInfo.dwDurationMode = (uint)DURATIONMODE.DURATION_COUNT;
            m_CaptureImageInfo.dwCapNumPerSec = 0;
            //m_CaptureImageInfo.lpFileName=m_strSavePath;
            m_CaptureImageInfo.dwDuration = 10;
            InitWindow();
        }

        private void InitWindow()
        {
            comboBox_ImageFormat.IsEnabled = true;
            radioButton_OverlayYes.IsEnabled = true;
            radioButton_OverlayNo.IsEnabled = true;
            radioButton_Count.IsEnabled = true;
            radioButton_Time.IsEnabled = true;
            textBox_Bottom.IsEnabled = true;
            textBox_CapNumPerSec.IsEnabled = false;
            textBox_Duration.IsEnabled = true;
            textBox_Left.IsEnabled = true;
            textBox_Right.IsEnabled = true;
            textBox_SavePath.IsEnabled = true;
            textBox_Top.IsEnabled = true;
            button_Browse.IsEnabled = true;
            button_StartStopCap.IsEnabled = true;
            button_CaptureSingleImage.IsEnabled = true;
            m_bStart = false;
            button_StartStopCap.Content = "Start Capture";

            textBox_Left.Text = m_CaptureImageInfo.rcCapRect.Left.ToString();
            textBox_Right.Text = m_CaptureImageInfo.rcCapRect.Right.ToString();
            textBox_Top.Text = m_CaptureImageInfo.rcCapRect.Top.ToString();
            textBox_Bottom.Text = m_CaptureImageInfo.rcCapRect.Bottom.ToString();
            textBox_CapNumPerSec.Text = m_CaptureImageInfo.dwCapNumPerSec.ToString();
            textBox_SavePath.Text = m_strSavePath;
            comboBox_ImageFormat.SelectedIndex = m_iImageType;
            uint uVideoSource = 0;
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref uVideoSource);
            if (m_DemoState == DEMOSTATE.DEMO_STATE_STOP || uVideoSource == (uint)VIDEOSOURCE.VIDEOSOURCE_ASI)
            {
                comboBox_ImageFormat.IsEnabled = false;
                radioButton_OverlayYes.IsEnabled = false;
                radioButton_OverlayNo.IsEnabled = false;
                radioButton_Count.IsEnabled = false;
                radioButton_Time.IsEnabled = false;
                textBox_Bottom.IsEnabled = false;
                textBox_CapNumPerSec.IsEnabled = false;
                textBox_Duration.IsEnabled = false;
                textBox_Left.IsEnabled = false;
                textBox_Right.IsEnabled = false;
                textBox_SavePath.IsEnabled = false;
                textBox_Top.IsEnabled = false;
                button_Browse.IsEnabled = false;
                button_StartStopCap.IsEnabled = false;
                button_CaptureSingleImage.IsEnabled = false;
            }
			m_CaptureImageInfo.dwVersion = 0;
            if (AVerCapAPI.AVerCaptureImageStart(m_hCaptureDevice, ref m_CaptureImageInfo) == (int)ERRORCODE.CAP_EC_NOT_SUPPORTED) 
            {
               comboBox_ImageFormat.IsEnabled = false;
                radioButton_OverlayYes.IsEnabled = false;
                radioButton_OverlayNo.IsEnabled = false;
                radioButton_Count.IsEnabled = false;
                radioButton_Time.IsEnabled = false;
                textBox_Bottom.IsEnabled = false;
                textBox_CapNumPerSec.IsEnabled = false;
                textBox_Duration.IsEnabled = false;
                textBox_Left.IsEnabled = false;
                textBox_Right.IsEnabled = false;
                textBox_SavePath.IsEnabled = false;
                textBox_Top.IsEnabled = false;
                button_Browse.IsEnabled = false;
                button_StartStopCap.IsEnabled = false;
                button_CaptureSingleImage.IsEnabled = false;
            }
            m_CaptureImageInfo.dwVersion = 1;
        }

        private void button_Browse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.AddExtension = true;
            saveFileDialog.OverwritePrompt = false;
            switch (comboBox_ImageFormat.SelectedIndex)
            {
                case 0:
                    saveFileDialog.Filter = "Bitmap File (*.bmp)|*.bmp";
                    saveFileDialog.FileName = "AVerCapSDK.bmp";
                    saveFileDialog.DefaultExt = "bmp";
                    break;
                case 1:
                    saveFileDialog.Filter = "JPG File (*.jpg)|*.jpg";
                    saveFileDialog.FileName = "AVerCapSDK.jpg";
                    saveFileDialog.DefaultExt = "jpg";
                    break;
                case 2:
                    saveFileDialog.Filter = "PNG File (*.png)|*.png";
                    saveFileDialog.FileName = "AVerCapSDK.png";
                    saveFileDialog.DefaultExt = "png";
                    break;
                case 3:
                    saveFileDialog.Filter = "TIFF File (*.tif)|*.tif";
                    saveFileDialog.FileName = "AVerCapSDK.tif";
                    saveFileDialog.DefaultExt = "tif";
                    break;
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                textBox_SavePath.Text = saveFileDialog.FileName;
                m_strSavePath = saveFileDialog.FileName;
            }
        }

        private void button_StartStopCap_Click(object sender, RoutedEventArgs e)
        {
            m_bStart = !m_bStart;
            if (m_bStart)
            {
                if (textBox_Duration.Text == "")
                {
                    MessageBox.Show("The duration must not be NULL.", "AVer Capture SDK");
                    m_bStart = !m_bStart;
                    return;
                }
                if (textBox_SavePath.Text == "")
                {
                    MessageBox.Show("The path must not be NULL.", "AVer Capture SDK");
                    m_bStart = !m_bStart;
                    return;
                }
                if (!textBox_SavePath.Text.Contains("\\"))
                {
                    MessageBox.Show("Invalid Path.", "AVer Capture SDK");
                    m_bStart = !m_bStart;
                    return;
                }
                try
                {
                    System.IO.FileStream Tempfile = System.IO.File.Create(textBox_SavePath.Text + "AVerTemp");
                    Tempfile.Close();
                    System.IO.File.Delete(textBox_SavePath.Text + "AVerTemp");
                }
                catch (System.Exception)
                {
                    MessageBox.Show("Invalid Path.", "AVer Capture SDK");
                    m_bStart = !m_bStart;
                    return;
                }

                m_CaptureImageInfo.lpFileName = textBox_SavePath.Text;

                switch (comboBox_ImageFormat.SelectedIndex)
                {
                    case 0:
                        m_CaptureImageInfo.dwImageType = (uint)IMAGETYPE.IMAGETYPE_BMP;
                        break;
                    case 1:
                        m_CaptureImageInfo.dwImageType = (uint)IMAGETYPE.IMAGETYPE_JPG;
                        break;
                    case 2:
                        m_CaptureImageInfo.dwImageType = (uint)IMAGETYPE.IMAGETYPE_PNG;
                        break;
                    case 3:
                        m_CaptureImageInfo.dwImageType = (uint)IMAGETYPE.IMAGETYPE_TIFF;
                        break;
                }
                m_CaptureImageInfo.dwCaptureType = (uint)CT_SEQUENCE.CT_SEQUENCE_FRAME;

                if (radioButton_OverlayYes.IsChecked == true)
                {
                    m_CaptureImageInfo.bOverlayMix = 1;
                }
                else
                {
                    m_CaptureImageInfo.bOverlayMix = 0;
                }

                if (radioButton_Count.IsChecked == true)
                {
                    m_CaptureImageInfo.dwDurationMode = (uint)DURATIONMODE.DURATION_COUNT;
                }
                else
                {
                    m_CaptureImageInfo.dwDurationMode = (uint)DURATIONMODE.DURATION_TIME;
                }

                m_CaptureImageInfo.dwDuration = uint.Parse(textBox_Duration.Text);
                if (m_CaptureImageInfo.dwDuration == 0)
                {
                    m_CaptureImageInfo.dwDuration = uint.MaxValue;
                }
                else if (m_CaptureImageInfo.dwDuration == 1 && radioButton_Count.IsChecked == true)
                {
                    MessageBox.Show("The duration must not 1.");
                    m_bStart = !m_bStart;
                    return;
                }

                if (textBox_Left.Text == "" || textBox_Right.Text == "" || textBox_Top.Text == "" || textBox_Bottom.Text == "")
                {
                    MessageBox.Show("The four parameters of capture area must not be NULL.");
                    m_bStart = !m_bStart;
                    return;
                }
                m_CaptureImageInfo.rcCapRect.Left = int.Parse(textBox_Left.Text);
                m_CaptureImageInfo.rcCapRect.Right = int.Parse(textBox_Right.Text);
                m_CaptureImageInfo.rcCapRect.Top = int.Parse(textBox_Top.Text);
                m_CaptureImageInfo.rcCapRect.Bottom = int.Parse(textBox_Bottom.Text);

                if (textBox_CapNumPerSec.Text == "")
                {
                    MessageBox.Show("The capture numbers per sec. must not be NULL.");
                    m_bStart = !m_bStart;
                    return;
                }
                uint dwFrameRate = 0;
                AVerCapAPI.AVerGetVideoOutputFrameRate(m_hCaptureDevice, ref dwFrameRate);
                uint dwCapNumPerSec = uint.Parse(textBox_CapNumPerSec.Text);
                if (dwCapNumPerSec < 0 || dwCapNumPerSec > (dwFrameRate / 100))
                {
                    MessageBox.Show("The capture numbers per sec. must between 0 and frame rate.");
                    m_bStart = !m_bStart;
                    return;
                }
                m_CaptureImageInfo.dwCapNumPerSec = dwCapNumPerSec;

                m_CaptureImageInfo.dwVersion = 1;

                int iRetVal = AVerCapAPI.AVerCaptureImageStart(m_hCaptureDevice, ref m_CaptureImageInfo);
                if (iRetVal != 0)
                {
                    if (iRetVal == (int)ERRORCODE.CAP_EC_HDCP_PROTECTED_CONTENT)
                    {
                        MessageBox.Show("The program content is protected!");
                        m_bStart = !m_bStart;
                        return;
                    }
                    MessageBox.Show("The width and height of capture area are not a multiple of 2 or out of range.");
                    m_bStart = !m_bStart;
                    return;
                }
                m_obMainWindow.UpdateDemoState(DEMOSTATE.DEMO_STATE_CAP_IMAGE, true);
                button_StartStopCap.Content = "End Capture";
            }
            else
            {
                AVerCapAPI.AVerCaptureImageStop(m_hCaptureDevice, 0);
                m_obMainWindow.UpdateDemoState(DEMOSTATE.DEMO_STATE_CAP_IMAGE, false);
                button_StartStopCap.Content = "Start Capture";
            }

        }

        private void radioButton_Count_Checked(object sender, RoutedEventArgs e)
        {

          
            try
            {
                textBox_Duration.Text = "10";
                label_Unit.Content = "pcs";
                textBox_CapNumPerSec.IsEnabled = false;
            }
            catch (System.Exception ex)
            {   
            }
        }

        private void radioButton_Time_Checked(object sender, RoutedEventArgs e)
        {
            textBox_CapNumPerSec.IsEnabled = true;
            textBox_Duration.Text = "1000";
            label_Unit.Content = "ms";
        }

        public void ModifyName(ref CAPTUREIMAGE_NOTIFY_INFO pCapImageNotifyInfo)
        {
            if(m_bStart)
            {
                if (pCapImageNotifyInfo.dwVersion == 1)
                {
                    if (pCapImageNotifyInfo.bFinished == 1)
                    {
                       
                        this.Dispatcher.Invoke
                            (new Action(() =>
                            {
                                new Thread(() =>
                                {
                                    MessageBox.Show("Finished!");
                                }).Start();
                                m_obMainWindow.UpdateDemoState(DEMOSTATE.DEMO_STATE_CAP_IMAGE, false);
                                button_StartStopCap.Content = "Start Capture";
                            }));
                        return;
                    }
                    string szType = pCapImageNotifyInfo.lpFileName.Substring(pCapImageNotifyInfo.lpFileName.Length - 4);
                    string szPath = pCapImageNotifyInfo.lpFileName.Substring(0, pCapImageNotifyInfo.lpFileName.LastIndexOf('_') - 14);
                    string szNewName = szPath + '_' + pCapImageNotifyInfo.dwImageIndex.ToString() + szType;
                    FileInfo FInfo = new FileInfo(szNewName);
                    FInfo.Delete();
                    DirectoryInfo DInfo = new DirectoryInfo(pCapImageNotifyInfo.lpFileName);
                    DInfo.MoveTo(szNewName);
                }
            }
        }

        private void textBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (!(e.Key >= Key.D0 && e.Key <= Key.D9) && !(e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) && e.Key != Key.Back)
            {
                e.Handled = true;
            }
        }

        private void comboBox_ImageFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StringBuilder sbTemp;
            switch (comboBox_ImageFormat.SelectedIndex)
            {
                case 0:
                    sbTemp = new StringBuilder(textBox_SavePath.Text);
                    sbTemp.Remove(sbTemp.Length - 3, 3);
                    sbTemp.Append("bmp");
                    textBox_SavePath.Text = sbTemp.ToString();
                    break;
                case 1:
                    sbTemp = new StringBuilder(textBox_SavePath.Text);
                    sbTemp.Remove(sbTemp.Length - 3, 3);
                    sbTemp.Append("jpg");
                    textBox_SavePath.Text = sbTemp.ToString();
                    break;
                case 2:
                    sbTemp = new StringBuilder(textBox_SavePath.Text);
                    sbTemp.Remove(sbTemp.Length - 3, 3);
                    sbTemp.Append("png");
                    textBox_SavePath.Text = sbTemp.ToString();
                    break;
                case 3:
                    sbTemp = new StringBuilder(textBox_SavePath.Text);
                    sbTemp.Remove(sbTemp.Length - 3, 3);
                    sbTemp.Append("tif");
                    textBox_SavePath.Text = sbTemp.ToString();
                    break;
            }
            m_iImageType = comboBox_ImageFormat.SelectedIndex;
            m_strSavePath = textBox_SavePath.Text;
        }

        private void button_CaptureSingleImage_Click(object sender, RoutedEventArgs e)
        {
             if (textBox_SavePath.Text == "")
            {
                MessageBox.Show("The path must not be NULL.", "AVer Capture SDK");
                return;
            }
             if (!textBox_SavePath.Text.Contains("\\"))
            {
                MessageBox.Show("Invalid Path.", "AVer Capture SDK");
                return;
            }
            try
            {
                System.IO.FileStream Tempfile = System.IO.File.Create(textBox_SavePath.Text + "AVerTemp");
                Tempfile.Close();
                System.IO.File.Delete(textBox_SavePath.Text + "AVerTemp");
            }
            catch (System.Exception)
            {
                MessageBox.Show("Invalid Path.", "AVer Capture SDK");
                return;

            }
            m_CaptureImageInfo.lpFileName = textBox_SavePath.Text;
            switch (comboBox_ImageFormat.SelectedIndex)
            {
                case 0:
                    m_CaptureImageInfo.dwImageType = (uint)IMAGETYPE.IMAGETYPE_BMP;
                    break;
                case 1:
                    m_CaptureImageInfo.dwImageType = (uint)IMAGETYPE.IMAGETYPE_JPG;
                    break;
                case 2:
                    m_CaptureImageInfo.dwImageType = (uint)IMAGETYPE.IMAGETYPE_PNG;
                    break;
                case 3:
                    m_CaptureImageInfo.dwImageType = (uint)IMAGETYPE.IMAGETYPE_TIFF;
                    break;
            }
            m_CaptureImageInfo.dwCaptureType = (uint)CT_SEQUENCE.CT_SEQUENCE_FRAME;
            if (radioButton_OverlayYes.IsChecked == true)
            {
                m_CaptureImageInfo.bOverlayMix = 1;
            }
            else
            {
                m_CaptureImageInfo.bOverlayMix = 0;
            }

            if (textBox_Left.Text == "" || textBox_Right.Text == "" || textBox_Top.Text == "" || textBox_Bottom.Text == "")
            {
                MessageBox.Show("The four parameters of capture area must not be NULL.");
                return;
            }
            m_CaptureImageInfo.rcCapRect.Left = int.Parse(textBox_Left.Text);
            m_CaptureImageInfo.rcCapRect.Right = int.Parse(textBox_Right.Text);
            m_CaptureImageInfo.rcCapRect.Top = int.Parse(textBox_Top.Text);
            m_CaptureImageInfo.rcCapRect.Bottom = int.Parse(textBox_Bottom.Text);

            m_CaptureImageInfo.dwDurationMode = (uint)DURATIONMODE.DURATION_COUNT;
            m_CaptureImageInfo.dwDuration = 1;
            m_CaptureImageInfo.dwVersion = 1;
            int iRetVal = AVerCapAPI.AVerCaptureImageStart(m_hCaptureDevice, ref m_CaptureImageInfo);
            if (iRetVal != 0)
            {
                if (iRetVal == (int)ERRORCODE.CAP_EC_HDCP_PROTECTED_CONTENT)
                {
                    MessageBox.Show("The program content is protected!");
                    return;
                }
                MessageBox.Show("The width and height of capture area are not a multiple of 2 or out of range.");
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_bHide)
            {
                e.Cancel = true;
                this.Hide();
            }
            else 
            {
                AVerCapAPI.AVerCaptureImageStop(m_hCaptureDevice, 0);
                m_obMainWindow.UpdateDemoState(DEMOSTATE.DEMO_STATE_CAP_IMAGE, false);
                button_StartStopCap.Content = "Start Capture";
            }
        }

        public void UpdateDemoWindow(DEMOSTATE DemoState)
        {
            m_DemoState = DemoState;
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW || m_DemoState == DEMOSTATE.DEMO_STATE_STOP || m_DemoState == (DEMOSTATE.DEMO_STATE_CALLBACK_VIDEO | DEMOSTATE.DEMO_STATE_PREVIEW))
            {
                InitWindow();
            }
        }
    }
}

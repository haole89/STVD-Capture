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

namespace AVerCapSDKDemo
{
    /// <summary>
    /// PreviewSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PreviewSetting : Window
    {
        private IntPtr m_hCaptureDevice = new IntPtr(0);
        private DEMOSTATE m_DemoState = DEMOSTATE.DEMO_STATE_STOP;
        private MainWindow m_obMainWindow;
        private int m_iIsVideoEnable = 3;
        private int m_iIsAudioEnable = 3;
        private int m_iIsMaintainEnable = 3;
        public bool m_bHide = true;
        public PreviewSetting(MainWindow Window, DEMOSTATE DemoState, IntPtr hCaptureDevice)
        {
            InitializeComponent();
            m_obMainWindow = Window;
            m_DemoState = DemoState;
            m_hCaptureDevice = hCaptureDevice;
            InitWindow();
        }
        private void InitWindow()
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_STOP || m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                radioButton_AudioEnable.IsEnabled = true;
                radioButton_AudioDisable.IsEnabled = true;
                radioButton_VideoEnable.IsEnabled = true;
                radioButton_VideoDisable.IsEnabled = true;
                radioButton_MaintainEnable.IsEnabled = true;
                radioButton_MaintainDisable.IsEnabled = true;
                comboBox_VideoRender.IsEnabled = true;
            }
            else
            {
                radioButton_AudioEnable.IsEnabled = false;
                radioButton_AudioDisable.IsEnabled = false;
                radioButton_VideoEnable.IsEnabled = false;
                radioButton_VideoDisable.IsEnabled = false;
                radioButton_MaintainEnable.IsEnabled = false;
                radioButton_MaintainDisable.IsEnabled = false;
                comboBox_VideoRender.IsEnabled = false;
            }
            AVerCapAPI.AVerGetVideoPreviewEnabled(m_hCaptureDevice, ref m_iIsVideoEnable);
            if (m_iIsVideoEnable == 1)
            {
                radioButton_VideoEnable.IsChecked = true;
            }
            else if (m_iIsVideoEnable == 0)
            {
                radioButton_VideoDisable.IsChecked = true;
            }
            else
            {
                MessageBox.Show("Failed to get the current settings(Video)", "AVer Capture SDK");
            }
            AVerCapAPI.AVerGetAudioPreviewEnabled(m_hCaptureDevice, ref m_iIsAudioEnable);
            if (m_iIsAudioEnable == 1)
            {
                radioButton_AudioEnable.IsChecked = true;
            }
            else if (m_iIsAudioEnable == 0)
            {
                radioButton_AudioDisable.IsChecked = true;
            }
            else
            {
                MessageBox.Show("Failed to get the current settings(Audio)", "AVer Capture SDK");
            }
            
            uint uVideoRenderer = 0;
            AVerCapAPI.AVerGetVideoRenderer(m_hCaptureDevice, ref uVideoRenderer);
            comboBox_VideoRender.SelectedIndex = (int)uVideoRenderer;
            AVerCapAPI.AVerGetMaintainAspectRatioEnabled(m_hCaptureDevice, ref m_iIsMaintainEnable);
            if (m_iIsMaintainEnable == 1)
            {
                radioButton_MaintainEnable.IsChecked = true;
            }
            else
            {
                radioButton_MaintainDisable.IsChecked = true;
            }
            uint uWidth, uHeight;
            // string strAspectRatio;
            uWidth = uHeight = 0;
            AVerCapAPI.AVerGetAspectRatio(m_hCaptureDevice, ref uWidth, ref uHeight);
            Label_AspectRatio.Content = string.Format("({0,2:d}:{1,2:d})", uWidth, uHeight);
        }

        private void radioButton_VideoEnable_Click(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            m_iIsVideoEnable = 1;
            AVerCapAPI.AVerSetVideoPreviewEnabled(m_hCaptureDevice, m_iIsVideoEnable);
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        private void radioButton_VideoDisable_Click(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            m_iIsVideoEnable = 0;
            AVerCapAPI.AVerSetVideoPreviewEnabled(m_hCaptureDevice, m_iIsVideoEnable);
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        private void radioButton_AudioEnable_Click(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            m_iIsAudioEnable = 1;
            AVerCapAPI.AVerSetAudioPreviewEnabled(m_hCaptureDevice, m_iIsAudioEnable);
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        private void radioButton_AudioDisable_Click(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            m_iIsAudioEnable = 0;
            AVerCapAPI.AVerSetAudioPreviewEnabled(m_hCaptureDevice, m_iIsAudioEnable);
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }


        private void radioButton_MaintainEnable_Click(object sender, RoutedEventArgs e)
        {
            m_iIsMaintainEnable = 1;
            AVerCapAPI.AVerSetMaintainAspectRatioEnabled(m_hCaptureDevice, m_iIsMaintainEnable);
            InitWindow();
        }

        private void radioButton_MaintainDisable_Click(object sender, RoutedEventArgs e)
        {
            m_iIsMaintainEnable = 0;
            AVerCapAPI.AVerSetMaintainAspectRatioEnabled(m_hCaptureDevice, m_iIsMaintainEnable);
            InitWindow();
        }
        public void UpdateDemoWindow(DEMOSTATE DemoState)
        {
            m_DemoState = DemoState;
            InitWindow();
        }

        private void PreviewSettingDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_bHide)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void comboBox_VideoRender_DropDownClosed(object sender, EventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            uint uVideoRenderer = (uint)comboBox_VideoRender.SelectedIndex;
            int iRetVal = AVerCapAPI.AVerSetVideoRenderer(m_hCaptureDevice, uVideoRenderer);
            if (iRetVal != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("The current system does not support this video renderer.");
            }
            m_obMainWindow.m_bHadSetVideoRenderer = true;
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
            InitWindow();
        }
    }
}

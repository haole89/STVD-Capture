using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace AVerCapSDKDemo
{
    public struct OVERLAY_CONTENT
    {
        public OVERLAY_CONTENT_INFO OverContentInfo;
        public uint uOverStartTime;
    }

    public struct LISTVIEW_OBJECT
    {
        public string ID { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
    }
    /// <summary>
    /// VideoProcess.xaml 的交互逻辑
    /// </summary>
    public partial class VideoProcess : Window
    {
        private IntPtr m_hCaptureDevice = new IntPtr(0);
        private DEMOSTATE m_DemoState = DEMOSTATE.DEMO_STATE_STOP;
        private MainWindow m_obMainWindow;
        public bool m_bHide = true;
        private uint m_uCurrCardType = 0;
        private uint m_uVideoDownScaleMode = 0;
        private uint m_uVideoDownScaleWidth = 0;
        private uint m_uVideoDownScaleHeight = 0;
        public VideoProcess(MainWindow Window, DEMOSTATE DemoState, IntPtr hCaptureDevice)
        {
            m_obMainWindow = Window;
            m_DemoState = DemoState;
            m_hCaptureDevice = hCaptureDevice;
            InitializeComponent();
            InitWindow(); 
        }
        private void InitWindow() 
        {
            InitCommonPage();
            InitClipPage();
            InitDownscale();
            InitRotate();
        }
        private void InitRotate() 
        {
            uint uVideoSource = 0;
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref uVideoSource);
            if ((m_DemoState == DEMOSTATE.DEMO_STATE_STOP || m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW) && (uVideoSource != (uint)VIDEOSOURCE.VIDEOSOURCE_ASI))
            {
                comboBox_RotateType.IsEnabled = true;
            }
            else
            {
                comboBox_RotateType.IsEnabled = false;
            }
            uint uRotateMode = 0;
            if (AVerCapAPI.AVerGetVideoRotateMode(m_hCaptureDevice, ref uRotateMode)== (int)ERRORCODE.CAP_EC_NOT_SUPPORTED)
            {
                comboBox_RotateType.IsEnabled = false;
            };
            switch (uRotateMode)
            {
                case (uint)VIDEOROTATE.VIDEOROTATE_NONE:
                    comboBox_RotateType.SelectedIndex = 0;
                    break;
                case (uint)VIDEOROTATE.VIDEOROTATE_CW90:
                    comboBox_RotateType.SelectedIndex = 1;
                    break;
                case (uint)VIDEOROTATE.VIDEOROTATE_CCW90:
                    comboBox_RotateType.SelectedIndex = 2;
                    break;
            }
        }

        private void InitDownscale() 
        {
            uint uVideoSource = 0;
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref uVideoSource);
            if ((m_DemoState == DEMOSTATE.DEMO_STATE_STOP || m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW) && (uVideoSource != (uint)VIDEOSOURCE.VIDEOSOURCE_ASI))
            {
                radioButton_None.IsEnabled=true;
                radioButton_HalfHeight.IsEnabled=true;
                radioButton_HalfWidth.IsEnabled=true;
                radioButton_HalfBoth.IsEnabled=true;
                radioButton_Custom.IsEnabled = true;
                button_SetDownscale.IsEnabled = true;          
            }
            else
            {
                radioButton_None.IsEnabled = false;
                radioButton_HalfHeight.IsEnabled = false;
                radioButton_HalfWidth.IsEnabled = false;
                radioButton_HalfBoth.IsEnabled = false;
                radioButton_Custom.IsEnabled = false;
                button_SetDownscale.IsEnabled = false;
                textBox_Width.IsEnabled = false;
                textBox_Height.IsEnabled = false;
            }	
            if (AVerCapAPI.AVerGetVideoDownscaleMode(m_hCaptureDevice, ref m_uVideoDownScaleMode, ref m_uVideoDownScaleWidth, ref m_uVideoDownScaleHeight)
                == (int)ERRORCODE.CAP_EC_NOT_SUPPORTED)
            {
                radioButton_None.IsEnabled = false;
                radioButton_HalfHeight.IsEnabled = false;
                radioButton_HalfWidth.IsEnabled = false;
                radioButton_HalfBoth.IsEnabled = false;
                radioButton_Custom.IsEnabled = false;
                button_SetDownscale.IsEnabled = false;
                textBox_Width.IsEnabled = false;
                textBox_Height.IsEnabled = false;
            };
            switch (m_uVideoDownScaleMode)
            {
                case (uint)DOWNSCALEMODE.DSMODE_NONE:
                    radioButton_None.IsChecked = true;
                    break;
                case (uint)DOWNSCALEMODE.DSMODE_HALFHEIGHT:
                    radioButton_HalfHeight.IsChecked = true;
                    break;
                case (uint)DOWNSCALEMODE.DSMODE_HALFWIDTH:
                    radioButton_HalfWidth.IsChecked = true;
                    break;
                case (uint)DOWNSCALEMODE.DSMODE_HALFBOTH:
                    radioButton_HalfBoth.IsChecked = true;
                    break;
                case (uint)DOWNSCALEMODE.DSMODE_CUSTOM:
                    radioButton_Custom.IsChecked = true;
                    textBox_Width.IsEnabled = true;
                    textBox_Height.IsEnabled = true;
                    textBox_Width.Text = m_uVideoDownScaleWidth.ToString();
                    textBox_Height.Text = m_uVideoDownScaleHeight.ToString();
                    break;
            }
        }

        private void GetActualVideoRect(ref uint uWidth, ref uint uHeight)
        {
            VIDEO_RESOLUTION VideoResolution =new VIDEO_RESOLUTION();
            VideoResolution.dwVersion = 1;
            AVerCapAPI.AVerGetVideoResolutionEx(m_hCaptureDevice, ref VideoResolution);
            uWidth = VideoResolution.dwWidth;
            uHeight = VideoResolution.dwHeight;
        }

        private void InitClipPage() 
        {
            uint uVideoSource = 0;
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref uVideoSource);
            if ((m_DemoState == DEMOSTATE.DEMO_STATE_STOP || m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)&&( uVideoSource != (uint)VIDEOSOURCE.VIDEOSOURCE_ASI))
            {
                radioButtonYES.IsEnabled=true;
                radioButtonNO.IsEnabled = true;
                textBoxLeft.IsEnabled = true;
                textBoxTop.IsEnabled = true;
                textBoxRight.IsEnabled = true;
                textBoxBottom.IsEnabled = true;
                button_SetClip.IsEnabled = true;
            }
            else
            {
                radioButtonYES.IsEnabled = false;
                radioButtonNO.IsEnabled = false;
                textBoxLeft.IsEnabled = false;
                textBoxTop.IsEnabled = false;
                textBoxRight.IsEnabled = false;
                textBoxBottom.IsEnabled = false;
                button_SetClip.IsEnabled = false;
            }	

            RECT rcClipVideoRect = new RECT();
            uint uWidth = 0;
            uint uHeight = 0;
            int bSignalPresence = 0;
            AVerCapAPI.AVerGetSignalPresence(m_hCaptureDevice, ref bSignalPresence);
            if (AVerCapAPI.AVerGetVideoClippingRect(m_hCaptureDevice, ref rcClipVideoRect) == (int)ERRORCODE.CAP_EC_NOT_SUPPORTED)
            {
                radioButtonYES.IsEnabled = false;
                radioButtonNO.IsEnabled = false;
                textBoxLeft.IsEnabled = false;
                textBoxTop.IsEnabled = false;
                textBoxRight.IsEnabled = false;
                textBoxBottom.IsEnabled = false;
                button_SetClip.IsEnabled = false;
            };
            GetActualVideoRect(ref uWidth, ref uHeight);

            if ((rcClipVideoRect.Left == 0 && rcClipVideoRect.Top == 0 &&
                rcClipVideoRect.Right == (uWidth - 1) && rcClipVideoRect.Bottom == (uHeight - 1)) || (bSignalPresence == 0))
            {
                radioButtonNO.IsChecked = true;
                textBoxLeft.IsEnabled = false;
                textBoxTop.IsEnabled = false;
                textBoxRight.IsEnabled = false;
                textBoxBottom.IsEnabled = false;
            }
            else
            {
                radioButtonYES.IsChecked = true;
            }
            textBoxLeft.Text = rcClipVideoRect.Left.ToString();
            textBoxTop.Text = rcClipVideoRect.Top.ToString();
            textBoxRight.Text = rcClipVideoRect.Right.ToString();
            textBoxBottom.Text = rcClipVideoRect.Bottom.ToString();
        }

        private void textBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (!(e.Key >= Key.D0 && e.Key <= Key.D9) && !(e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) && e.Key != Key.Back)
            {
                e.Handled = true;
            }
        }
        private void InitCommonPage()
        {
            radioButton_NoiseEnable.IsEnabled = true;
            radioButton_NoiseDisable.IsEnabled = true;
            comboBox_Enhancement.IsEnabled = true;
            comboBox_DeInterlance.IsEnabled = true;
        

            uint dwDeinterlaceMode = 0;
            if(AVerCapAPI.AVerGetDeinterlaceMode(m_hCaptureDevice, ref dwDeinterlaceMode) == (int)ERRORCODE.CAP_EC_NOT_SUPPORTED) 
            {
                comboBox_DeInterlance.IsEnabled = false;
            };
            switch (dwDeinterlaceMode)
            {
                case (uint)DEINTERLACEMODE.DEINTERLACE_NONE:
                    comboBox_DeInterlance.SelectedIndex = 0;
                    break;
                case (uint)DEINTERLACEMODE.DEINTERLACE_WEAVE:
                    comboBox_DeInterlance.SelectedIndex = 1;
                    break;
                case (uint)DEINTERLACEMODE.DEINTERLACE_BOB:
                    comboBox_DeInterlance.SelectedIndex = 2;
                    break;
                case (uint)DEINTERLACEMODE.DEINTERLACE_BLEND:
                    comboBox_DeInterlance.SelectedIndex = 3;
                    break;
            }
            uint dwEnhanceMode = 0;
            if (AVerCapAPI.AVerGetVideoEnhanceMode(m_hCaptureDevice, ref dwEnhanceMode) == (int)ERRORCODE.CAP_EC_NOT_SUPPORTED)
            {
                comboBox_Enhancement.IsEnabled = false;
            };
            switch (dwEnhanceMode)
            {
                case (uint)VIDEOENHANCE.VIDEOENHANCE_NONE:
                    comboBox_Enhancement.SelectedIndex = 0;
                    break;
                case (uint)VIDEOENHANCE.VIDEOENHANCE_NORMAL:
                    comboBox_Enhancement.SelectedIndex = 1;
                    break;
                case (uint)VIDEOENHANCE.VIDEOENHANCE_SPLIT:
                    comboBox_Enhancement.SelectedIndex = 2;
                    break;
                case (uint)VIDEOENHANCE.VIDEOENHANCE_COMPARE:
                    comboBox_Enhancement.SelectedIndex = 3;
                    break;
            }
            int uIsNoiseReductionEnable = 0;
            if (AVerCapAPI.AVerGetNoiseReductionEnabled(m_hCaptureDevice, ref uIsNoiseReductionEnable) == (int)ERRORCODE.CAP_EC_NOT_SUPPORTED)
            {
                radioButton_NoiseEnable.IsEnabled = false;
                radioButton_NoiseDisable.IsEnabled = false;
            };
            if (uIsNoiseReductionEnable == 1)
            {
                radioButton_NoiseEnable.IsChecked = true;
            }
            else
            {
                radioButton_NoiseDisable.IsChecked = true;
            }
          
     
            uint uVideoSource = 0;
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref uVideoSource);
            if (m_DemoState == DEMOSTATE.DEMO_STATE_STOP || uVideoSource == (uint)VIDEOSOURCE.VIDEOSOURCE_ASI) 
            {
                radioButton_NoiseEnable.IsEnabled = false;
                radioButton_NoiseDisable.IsEnabled = false;
                comboBox_Enhancement.IsEnabled = false;
                comboBox_DeInterlance.IsEnabled = false;
            }
        }

        public void UpdateDemoWindow(DEMOSTATE DemoState)
        {
            if ((m_DemoState == DEMOSTATE.DEMO_STATE_STOP || m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW) &&
                (DemoState == DEMOSTATE.DEMO_STATE_STOP || DemoState == DEMOSTATE.DEMO_STATE_PREVIEW))
            {
                m_DemoState = DemoState;
            }
            m_DemoState = DemoState;
            InitCommonPage();
            InitClipPage();
            InitDownscale();
            InitRotate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_bHide)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void comboBox_DeInterlance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboBox_DeInterlance.SelectedIndex)
            {
                case 0:
                    AVerCapAPI.AVerSetDeinterlaceMode(m_hCaptureDevice,(uint)DEINTERLACEMODE.DEINTERLACE_NONE);
                    break;
                case 1:
                    AVerCapAPI.AVerSetDeinterlaceMode(m_hCaptureDevice,(uint)DEINTERLACEMODE.DEINTERLACE_WEAVE);
                    break;
                case 2:
                    AVerCapAPI.AVerSetDeinterlaceMode(m_hCaptureDevice,(uint)DEINTERLACEMODE.DEINTERLACE_BOB);
                    break;
                case 3:
                    AVerCapAPI.AVerSetDeinterlaceMode(m_hCaptureDevice,(uint)DEINTERLACEMODE.DEINTERLACE_BLEND);
                    break;
            }
        }

        private void comboBox_Enhancement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboBox_Enhancement.SelectedIndex)
            {
                case 0:
                    AVerCapAPI.AVerSetVideoEnhanceMode(m_hCaptureDevice,(uint)VIDEOENHANCE.VIDEOENHANCE_NONE);
                    break;
                case 1:
                    AVerCapAPI.AVerSetVideoEnhanceMode(m_hCaptureDevice, (uint)VIDEOENHANCE.VIDEOENHANCE_NORMAL);
                    break;
                case 2:
                    AVerCapAPI.AVerSetVideoEnhanceMode(m_hCaptureDevice, (uint)VIDEOENHANCE.VIDEOENHANCE_SPLIT);
                    break;
                case 3:
                    AVerCapAPI.AVerSetVideoEnhanceMode(m_hCaptureDevice, (uint)VIDEOENHANCE.VIDEOENHANCE_COMPARE);
                    break;
            }
        }

        private void radioButton_NoiseEnable_Checked(object sender, RoutedEventArgs e)
        {
            AVerCapAPI.AVerSetNoiseReductionEnabled(m_hCaptureDevice, 1);
        }

        private void radioButton_NoiseDisable_Checked(object sender, RoutedEventArgs e)
        {
            AVerCapAPI.AVerSetNoiseReductionEnabled(m_hCaptureDevice, 0);
        }

        private void radioButtonNO_Checked(object sender, RoutedEventArgs e)
        {
            textBoxLeft.IsEnabled = false;
            textBoxTop.IsEnabled = false;
            textBoxRight.IsEnabled = false;
            textBoxBottom.IsEnabled = false;
        }

        private void radioButtonYES_Checked(object sender, RoutedEventArgs e)
        {
            textBoxLeft.IsEnabled = true;
            textBoxTop.IsEnabled = true;
            textBoxRight.IsEnabled = true;
            textBoxBottom.IsEnabled = true;
        }

        private void button_SetClip_Click(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }           
            int iRetVal = 0;
            RECT rcClipVideoRect = new RECT();
            if (radioButtonYES.IsChecked == true)
            {
                if (textBoxLeft.Text == "" || textBoxRight.Text == "" || textBoxTop.Text == "" || textBoxBottom.Text == "")
                {
                    MessageBox.Show("The four parameters of clipping area must not be NULL.");
                    if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
                    {
                        m_obMainWindow.startStreaming();
                    }
                    return;
                }
                rcClipVideoRect.Left = int.Parse(textBoxLeft.Text);
                rcClipVideoRect.Top = int.Parse(textBoxTop.Text);
                rcClipVideoRect.Right = int.Parse(textBoxRight.Text);
                rcClipVideoRect.Bottom = int.Parse(textBoxBottom.Text);
            }
            else
            {
                rcClipVideoRect.Left = 0;
                rcClipVideoRect.Top = 0;
                rcClipVideoRect.Right = 0;
                rcClipVideoRect.Bottom = 0;
            }
            iRetVal = AVerCapAPI.AVerSetVideoClippingRect(m_hCaptureDevice, rcClipVideoRect);
            if (iRetVal != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("The width and height of clipping area are not a multiple of 16 or out of range.");
                if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
                {
                    m_obMainWindow.startStreaming();
                }
                return;
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        private void radioButton_None_Checked(object sender, RoutedEventArgs e)
        {
            m_uVideoDownScaleMode = (uint)DOWNSCALEMODE.DSMODE_NONE;
            textBox_Width.IsEnabled = false;
            textBox_Height.IsEnabled = false;
        }

        private void radioButton_HalfHeight_Checked(object sender, RoutedEventArgs e)
        {
            m_uVideoDownScaleMode = (uint)DOWNSCALEMODE.DSMODE_HALFHEIGHT;
            textBox_Width.IsEnabled = false;
            textBox_Height.IsEnabled = false;
        }

        private void radioButton_HalfWidth_Checked(object sender, RoutedEventArgs e)
        {
            m_uVideoDownScaleMode = (uint)DOWNSCALEMODE.DSMODE_HALFWIDTH;
            textBox_Width.IsEnabled = false;
            textBox_Height.IsEnabled = false;
        }

        private void radioButton_HalfBoth_Checked(object sender, RoutedEventArgs e)
        {
            m_uVideoDownScaleMode = (uint)DOWNSCALEMODE.DSMODE_HALFBOTH;
            textBox_Width.IsEnabled = false;
            textBox_Height.IsEnabled = false;
        }

        private void radioButton_Custom_Checked(object sender, RoutedEventArgs e)
        {
            m_uVideoDownScaleMode = (uint)DOWNSCALEMODE.DSMODE_CUSTOM;
            textBox_Width.IsEnabled = true;
            textBox_Height.IsEnabled = true;
        }

        private void button_SetDownscale_Click(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            } 
            int nRetVal = 0;
            if (m_uVideoDownScaleMode == (uint)DOWNSCALEMODE.DSMODE_CUSTOM)
            {
                if (textBox_Height.Text == "" || textBox_Width.Text == "")
                {
                    MessageBox.Show("The height and width must not be NULL.", "AVer Capture SDK");
                    if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
                    {
                        m_obMainWindow.startStreaming();
                    }
                    return;
                }
                try
                {
                    m_uVideoDownScaleWidth = uint.Parse(textBox_Width.Text);
                    m_uVideoDownScaleHeight = uint.Parse(textBox_Height.Text);
                    if (m_uVideoDownScaleWidth % 16 != 0 || m_uVideoDownScaleHeight % 16 != 0 ||
                        m_uVideoDownScaleWidth < 128 || m_uVideoDownScaleWidth > 2000 || m_uVideoDownScaleHeight < 128 || m_uVideoDownScaleHeight > 2000)
                    {
                        MessageBox.Show("The height and width must be multiple of 16 and between 128 and 2000.", "AVer Capture SDK");
                        if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
                        {
                            m_obMainWindow.startStreaming();
                        }
                        return;
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Height and width must be digits", "AVer Capture SDK");
                    if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
                    {
                        m_obMainWindow.startStreaming();
                    }
                    return;
                }
                catch (OverflowException)
                {
                    MessageBox.Show("Out of range", "AVer Capture SDK");
                    if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
                    {
                        m_obMainWindow.startStreaming();
                    }
                    return;
                }
            }
            else
            {
                m_uVideoDownScaleWidth = 0;
                m_uVideoDownScaleHeight = 0;
            }
            nRetVal = AVerCapAPI.AVerSetVideoDownscaleMode(m_hCaptureDevice, m_uVideoDownScaleMode, m_uVideoDownScaleWidth, m_uVideoDownScaleHeight);
            if (nRetVal != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                if (m_uVideoDownScaleMode == (uint)DOWNSCALEMODE.DSMODE_CUSTOM)
                {
                    MessageBox.Show("The height and width must not be larger than the original size or the clipped size.");
                }
                else if (m_uVideoDownScaleMode == (uint)DOWNSCALEMODE.DSMODE_HALFHEIGHT)
                {
                    MessageBox.Show("The height must not be smaller than 128.");
                }
                else if (m_uVideoDownScaleMode == (uint)DOWNSCALEMODE.DSMODE_HALFWIDTH)
                {
                    MessageBox.Show("The width must not be smaller than 128.");
                }
                else if (m_uVideoDownScaleMode == (uint)DOWNSCALEMODE.DSMODE_HALFBOTH)
                {
                    MessageBox.Show("The height and width must not be smaller than 128.");
                }
                if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
                {
                    m_obMainWindow.startStreaming();
                }
                return;
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }


        private void comboBox_RotateType_DropDownClosed(object sender, EventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            uint uRotateMode = 0;
            switch (comboBox_RotateType.SelectedIndex)
            {
                case 0:
                    uRotateMode = (uint)VIDEOROTATE.VIDEOROTATE_NONE;
                    break;
                case 1:
                    uRotateMode = (uint)VIDEOROTATE.VIDEOROTATE_CW90;
                    break;
                case 2:
                    uRotateMode = (uint)VIDEOROTATE.VIDEOROTATE_CCW90;
                    break;
            }
            AVerCapAPI.AVerSetVideoRotateMode(m_hCaptureDevice, uRotateMode);
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }
    }
}

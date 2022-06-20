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
using System.Windows.Threading;
using System.Collections;
using System.Runtime.InteropServices;

namespace AVerCapSDKDemo
{
    public enum MODE
    {
        MODE_RESOLUTIONTOSELINDEX = 0,
        MODE_SELINDEXTORESOLUTION = 1
    }
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceSetting:Window
    {
        private List<uint> m_VideoSourceList = new List<uint>();
        private List<uint> m_AudioSourceList = new List<uint>();
        private List<uint> m_VideoResolutionList = new List<uint>();
        private ArrayList m_AudioSampleRateList = new ArrayList();
        
        private AUDIOCAPTURESOURCE_INFO[] m_pAudioCapSourceInfo;
        private AUDIOCAPTURESOURCE_INPUTTYPE_INFO[] m_pAudioCapSourceInputTypeInfo;
        private AUDIOCAPTURESOURCE_FORMAT_INFO[] m_pAudioCapSourceFormatInfo;
        private IntPtr m_hCaptureDevice = new IntPtr(0);
        private DEMOSTATE m_DemoState = DEMOSTATE.DEMO_STATE_STOP;
        private uint m_uCaptureType;
        private MainWindow m_obMainWindow;
        private DispatcherTimer m_Timer = new DispatcherTimer();
        public bool m_bHide=true;
        private uint m_uCurrCardType = 0;
        private uint m_uVideoSource=0;
        private uint m_uVideoFormat=0;
        private bool m_bIsRangeFramerate = false;
        private bool m_bIsRangeResolution = false;
        //private System.Windows.Forms.DateTimePicker dateTimePicker;

        public DeviceSetting(MainWindow Window, DEMOSTATE DemoState, IntPtr hCaptureDevice, uint uCapType, uint uCurrentCardType)
        {
            InitializeComponent();
            m_obMainWindow = Window;
            m_DemoState = DemoState;
            m_hCaptureDevice = hCaptureDevice;
            m_uCaptureType = uCapType;
            m_uCurrCardType = uCurrentCardType;
            //range support bool

            uint dwVideoSource = 3;//hdmi
            uint dwFormat = 0;
            RESOLUTION_RANGE_INFO ResolutionRangeInfo = new RESOLUTION_RANGE_INFO();
            ResolutionRangeInfo.dwVersion = 1;
            int hr = AVerCapAPI.AVerGetVideoResolutionRangeSupported(m_hCaptureDevice, dwVideoSource, dwFormat, ref  ResolutionRangeInfo);
            if (hr == (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                if (ResolutionRangeInfo.bRange == 0)
                {

                    m_bIsRangeResolution = false;
                }
                else
                {

                    m_bIsRangeResolution = true;

                }
            }
            FRAMERATE_RANGE_INFO FrameRateRangeInfo = new FRAMERATE_RANGE_INFO();
            FrameRateRangeInfo.dwVersion = 1;
            hr = AVerCapAPI.AVerGetVideoInputFrameRateRangeSupported(m_hCaptureDevice, dwVideoSource, dwFormat, 0, 0, ref  FrameRateRangeInfo);

            // AVerCapAPI.AVerGetVideoInput (m_hCaptureDevice, dwVideoSource, dwFormat, ref  ResolutionRangeInfo);

            if (hr == (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                if (FrameRateRangeInfo.bRange == 0)
                {
                    m_bIsRangeFramerate = false;

                }
                else
                {
                    m_bIsRangeFramerate = true;


                }
            }
            //add range
            InitWindow();
        }

        private void InitWindow()
        {
            InitVideoDevice();
            InitAudioDevice();
            InitMiscellaneous();
        }

        private void InitEmbedded()
        {
            uint uChannels = 0;
            AVerCapAPI.AVerGetEmbeddedAudioChannel(m_hCaptureDevice, ref uChannels);
            int nIndex = 0;
            switch (uChannels)
            {
                case 0x0003:
                    nIndex = 0;
                    break;
                case 0x000C:
                    nIndex = 1;
                    break;
                case 0x0030:
                    nIndex = 2;
                    break;
                case 0x00C0:
                    nIndex = 3;
                    break;
                case 0x0300:
                    nIndex = 4;
                    break;
                case 0x0C00:
                    nIndex = 5;
                    break;
                case 0x3000:
                    nIndex = 6;
                    break;
                case 0xC000:
                    nIndex = 7;
                    break;
            }
            comboBox_Channels.SelectedIndex = nIndex;
        }

        private void comboBoxChannel_SelectionChangeCommitted(object sender, SelectionChangedEventArgs e)
        {
            uint uChannels = 0;
            switch (comboBox_Channels.SelectedIndex)
            {
                case 0:
                    uChannels = 0x0003;
                    break;
                case 1:
                    uChannels = 0x000C;
                    break;
                case 2:
                    uChannels = 0x0030;
                    break;
                case 3:
                    uChannels = 0x00C0;
                    break;
                case 4:
                    uChannels = 0x0300;
                    break;
                case 5:
                    uChannels = 0x0C00;
                    break;
                case 6:
                    uChannels = 0x3000;
                    break;
                case 7:
                    uChannels = 0xC000;
                    break;
            }
            int iRetVal = AVerCapAPI.AVerSetEmbeddedAudioChannel(m_hCaptureDevice, uChannels);
            if (iRetVal != 0)
            {
                MessageBox.Show("Set embedded audio channel failed!");
            }
        }

       

        private void InitMiscellaneous()
        {
            uint dwVideoSource = 0;
            uint uChannels = 0;
            //hr suported: not supported
            int hr = AVerCapAPI.AVerGetEmbeddedAudioChannel(m_hCaptureDevice, ref uChannels);

            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref dwVideoSource);
            if (hr == (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                InitEmbedded();
            }
            else
            {
                comboBox_Channels.IsEnabled = false;
            }
        }

        private void InitAudioDevice()
        {

            m_AudioSourceList.Clear();
            comboBox_AudioSource.Items.Clear();

            //Init
            uint uVideoSource = 0;
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref uVideoSource);

            uint dwSupported = 0;
            AVerCapAPI.AVerGetAudioSourceSupportedEx(m_hCaptureDevice, uVideoSource, ref dwSupported);

            int index = 0;

            if (Convert.ToBoolean((uint)AUDIOSOURCE.AUDIOSOURCE_NONE & dwSupported))
            {
                comboBox_AudioSource.IsEnabled = false;

            }
            if (Convert.ToBoolean((uint)AUDIOSOURCE.AUDIOSOURCE_COMPOSITE & dwSupported))
            {
                comboBox_AudioSource.Items.Add("composite");
                m_AudioSourceList.Add((uint)AUDIOSOURCE.AUDIOSOURCE_COMPOSITE);
                index = index + 1;

            }
            if (Convert.ToBoolean((uint)AUDIOSOURCE.AUDIOSOURCE_SVIDEO & dwSupported))
            {
                comboBox_AudioSource.Items.Add("S-Video");
                m_AudioSourceList.Add((uint)AUDIOSOURCE.AUDIOSOURCE_SVIDEO);
                index = index + 1;

            }
            if (Convert.ToBoolean((uint)AUDIOSOURCE.AUDIOSOURCE_COMPONENT & dwSupported))
            {
                comboBox_AudioSource.Items.Add("component");
                m_AudioSourceList.Add((uint)AUDIOSOURCE.AUDIOSOURCE_COMPONENT);
                index = index + 1;

            }
            if (Convert.ToBoolean((uint)AUDIOSOURCE.AUDIOSOURCE_HDMI & dwSupported))
            {
                comboBox_AudioSource.Items.Add("HDMI");
                m_AudioSourceList.Add((uint)AUDIOSOURCE.AUDIOSOURCE_HDMI);
                index = index + 1;

            }
            if (Convert.ToBoolean((uint)AUDIOSOURCE.AUDIOSOURCE_VGA & dwSupported))
            {
                comboBox_AudioSource.Items.Add("Line In");
                m_AudioSourceList.Add((uint)AUDIOSOURCE.AUDIOSOURCE_VGA);
                index = index + 1;

            }
            if (Convert.ToBoolean((uint)AUDIOSOURCE.AUDIOSOURCE_SDI & dwSupported))
            {
                comboBox_AudioSource.Items.Add("SDI");
                m_AudioSourceList.Add((uint)AUDIOSOURCE.AUDIOSOURCE_SDI);
                index = index + 1;
            }
            if (Convert.ToBoolean((uint)AUDIOSOURCE.AUDIOSOURCE_ASI & dwSupported))
            {

                comboBox_AudioSource.Items.Add("ASI");
                m_AudioSourceList.Add((uint)AUDIOSOURCE.AUDIOSOURCE_ASI);
                index = index + 1;
            }
            if (Convert.ToBoolean((uint)AUDIOSOURCE.AUDIOSOURCE_DVI & dwSupported))
            {

                comboBox_AudioSource.Items.Add("DVI");
                m_AudioSourceList.Add((uint)AUDIOSOURCE.AUDIOSOURCE_DVI);
                index = index + 1;
            }


            uint dwSource = 0;
            AVerCapAPI.AVerGetAudioSource(m_hCaptureDevice, ref dwSource);
            int nSelIndex = 0;

            for (int i = 0; i < index; i++)
            {
                if (dwSource == m_AudioSourceList[i])
                {

                    nSelIndex = i;
                    break;

                }

            }

            comboBox_AudioSource.SelectedIndex = nSelIndex;
            if (m_DemoState == DEMOSTATE.DEMO_STATE_STOP || m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                comboBox_AudioSource.IsEnabled = true;
                comboBox_AudioCaptureSource.IsEnabled = true;
                comboBox_InputType.IsEnabled = true;
                comboBox_AudioFormat.IsEnabled = true;
                button_SetThirdPart.IsEnabled = true;
                checkBox_ThirdParty.IsEnabled = true;
            }
            else
            {
                comboBox_AudioSource.IsEnabled = false;
                comboBox_AudioCaptureSource.IsEnabled = false;
                comboBox_InputType.IsEnabled = false;
                comboBox_AudioFormat.IsEnabled = false;
                button_SetThirdPart.IsEnabled = false;
                checkBox_ThirdParty.IsEnabled = false;
            }
           
            InitThirdAudioCap();
            //检测是否第三方使用中
            if (checkBox_ThirdParty.IsChecked != true)
            {
                comboBox_AudioCaptureSource.IsEnabled = false;
                comboBox_InputType.IsEnabled = false;
                comboBox_AudioFormat.IsEnabled = false;
                button_SetThirdPart.IsEnabled = false;
            }
        }

        private void InitThirdAudioCap()
        {
            comboBox_AudioCaptureSource.IsEnabled = true;
            comboBox_InputType.IsEnabled = true;
            comboBox_AudioFormat.IsEnabled = true;
            comboBox_AudioCaptureSource.Items.Clear();
            comboBox_InputType.Items.Clear();
            comboBox_AudioFormat.Items.Clear();

            comboBox_AudioCaptureSource.Items.Add("None");
            comboBox_AudioCaptureSource.SelectedIndex = 0;
            comboBox_InputType.IsEnabled = false;
            comboBox_AudioFormat.IsEnabled = false;
            AUDIOCAPTURESOURCE_SETTING AudioCapSourceSetting = new AUDIOCAPTURESOURCE_SETTING();
            AudioCapSourceSetting.dwVersion = 1;
            AVerCapAPI.AVerGetThirdPartyAudioCapSource(m_hCaptureDevice, ref AudioCapSourceSetting);
            //Enum ThirdParty Audio Cap
            uint dwNum, i;
            dwNum = i = 0;
            int ret = -1;
            ret = AVerCapAPI.AVerEnumThirdPartyAudioCapSource(m_hCaptureDevice, IntPtr.Zero, ref dwNum);
            if (ret != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("Can't enumerate third party audio capture source.");
                return;
            }
            if (dwNum != 0)
            {
                m_pAudioCapSourceInfo = new AUDIOCAPTURESOURCE_INFO[dwNum];
                for (int k = 0 ; k < dwNum ; ++k)
                {
                    m_pAudioCapSourceInfo[k].szName = new char[256];
                }
                int StructSize = Marshal.SizeOf(typeof(AUDIOCAPTURESOURCE_INFO));
                IntPtr pt = Marshal.AllocHGlobal((IntPtr)(StructSize * dwNum));
                Marshal.WriteInt32(pt, 1);
                AVerCapAPI.AVerEnumThirdPartyAudioCapSource(m_hCaptureDevice, pt, ref dwNum);
                string szCurrAudioCapSourceName;
                for (i = 0 ; i < dwNum ; ++i)
                {
                    m_pAudioCapSourceInfo[i] = (AUDIOCAPTURESOURCE_INFO)Marshal.PtrToStructure(
                        (IntPtr)((uint)pt + i * StructSize), typeof(AUDIOCAPTURESOURCE_INFO));
                    szCurrAudioCapSourceName = new string(m_pAudioCapSourceInfo[i].szName);
                    for (int j = 0 ; j < szCurrAudioCapSourceName.Length ; ++j)
                    {
                        if (szCurrAudioCapSourceName[j] == '\0')
                        {
                            szCurrAudioCapSourceName = szCurrAudioCapSourceName.Substring(0, j);
                            break;
                        }
                    }
                    comboBox_AudioCaptureSource.Items.Add(szCurrAudioCapSourceName);
                    if (m_pAudioCapSourceInfo[i].dwIndex == AudioCapSourceSetting.dwCapSourceIndex)
                    {
                        comboBox_AudioCaptureSource.SelectedIndex = (int)(i + 1);
                    }
                }
                Marshal.FreeHGlobal(pt);
            }
            if (AudioCapSourceSetting.dwCapSourceIndex == uint.MaxValue)
            {
                return;
            }
            checkBox_ThirdParty.IsChecked = true;
            //Enum Input Type
            ret = AVerCapAPI.AVerEnumThirdPartyAudioCapSourceInputType(m_hCaptureDevice, AudioCapSourceSetting.dwCapSourceIndex, IntPtr.Zero, ref dwNum);
            if (ret != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("Can't enumerate input type.");
            }
            else
            {
                if (dwNum != 0)
                {
                    m_pAudioCapSourceInputTypeInfo = new AUDIOCAPTURESOURCE_INPUTTYPE_INFO[dwNum];
                    for (int k = 0 ; k < dwNum ; ++k)
                    {
                        m_pAudioCapSourceInputTypeInfo[k].szName = new char[256];
                    }
                    m_pAudioCapSourceInputTypeInfo[0].dwVersion = 1;
                    int StructSize = Marshal.SizeOf(typeof(AUDIOCAPTURESOURCE_INPUTTYPE_INFO));
                    IntPtr pt = Marshal.AllocHGlobal((IntPtr)(StructSize * dwNum));
                    Marshal.WriteInt32(pt, 1);
                    AVerCapAPI.AVerEnumThirdPartyAudioCapSourceInputType(m_hCaptureDevice, AudioCapSourceSetting.dwCapSourceIndex, pt, ref dwNum);
                    comboBox_InputType.IsEnabled = true;
                    string szAudioCapSourceInputTypeName;
                    for (i = 0 ; i < dwNum ; ++i)
                    {
                        m_pAudioCapSourceInputTypeInfo[i] = (AUDIOCAPTURESOURCE_INPUTTYPE_INFO)Marshal.PtrToStructure(
                        (IntPtr)((uint)pt + i * StructSize), typeof(AUDIOCAPTURESOURCE_INPUTTYPE_INFO));
                        szAudioCapSourceInputTypeName = new string(m_pAudioCapSourceInputTypeInfo[i].szName);
                        for (int j = 0 ; j < szAudioCapSourceInputTypeName.Length ; ++j)
                        {
                            if (szAudioCapSourceInputTypeName[j] == '\0')
                            {
                                szAudioCapSourceInputTypeName = szAudioCapSourceInputTypeName.Substring(0, j);
                                break;
                            }
                        }
                        comboBox_InputType.Items.Add(szAudioCapSourceInputTypeName);
                        if (m_pAudioCapSourceInputTypeInfo[i].dwIndex == AudioCapSourceSetting.dwInputTypeIndex)
                        {
                            comboBox_InputType.SelectedIndex = (int)i;
                        }
                    }
                    Marshal.FreeHGlobal(pt);
                }
            }
            //Enum Sample format
            ret = AVerCapAPI.AVerEnumThirdPartyAudioCapSourceSampleFormat(m_hCaptureDevice, AudioCapSourceSetting.dwCapSourceIndex, IntPtr.Zero, ref dwNum);
            if (ret != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("Can't enumerate sample format.");
            }
            else
            {
                if (dwNum != 0)
                {
                    m_pAudioCapSourceFormatInfo = new AUDIOCAPTURESOURCE_FORMAT_INFO[dwNum];
                    for (int k = 0 ; k < dwNum ; ++k)
                    {
                        m_pAudioCapSourceFormatInfo[k].szName = new char[256];
                    }
                    m_pAudioCapSourceFormatInfo[0].dwVersion = 1;
                    int StructSize = Marshal.SizeOf(typeof(AUDIOCAPTURESOURCE_FORMAT_INFO));
                    IntPtr pt = Marshal.AllocHGlobal((IntPtr)(StructSize * dwNum));
                    Marshal.WriteInt32(pt, 1);
                    AVerCapAPI.AVerEnumThirdPartyAudioCapSourceSampleFormat(m_hCaptureDevice, AudioCapSourceSetting.dwCapSourceIndex, pt, ref dwNum);
                    comboBox_AudioFormat.IsEnabled = true;
                    string szAudioCapSourceFormatName;
                    for (i = 0 ; i < dwNum ; ++i)
                    {
                        m_pAudioCapSourceFormatInfo[i] = (AUDIOCAPTURESOURCE_FORMAT_INFO)Marshal.PtrToStructure(
                        (IntPtr)((uint)pt + i * StructSize), typeof(AUDIOCAPTURESOURCE_FORMAT_INFO));
                        szAudioCapSourceFormatName = new string(m_pAudioCapSourceFormatInfo[i].szName);
                        for (int j = 0 ; j < szAudioCapSourceFormatName.Length ; ++j)
                        {
                            if (szAudioCapSourceFormatName[j] == '\0')
                            {
                                szAudioCapSourceFormatName = szAudioCapSourceFormatName.Substring(0, j);
                                break;
                            }
                        }
                        comboBox_AudioFormat.Items.Add(szAudioCapSourceFormatName);
                        if (m_pAudioCapSourceFormatInfo[i].dwIndex == AudioCapSourceSetting.dwFormatIndex)
                        {
                            comboBox_AudioFormat.SelectedIndex = (int)i;
                        }
                    }
                    Marshal.FreeHGlobal(pt);
                }
            }
        }

       

        private void InitVideoDevice()
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_STOP || m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                textBox_InputVideoInfo.IsEnabled = true;
                comboBox_VideoSource.IsEnabled = true;
                radioButton_NTSC.IsEnabled = true;
                radioButton_PAL.IsEnabled = true;
                comboBox_Resolution.IsEnabled = true;
                checkBox_Custom.IsEnabled = true;
                textBox_Width.IsEnabled = true;
                textBox_Height.IsEnabled = true;
                button_SetResolution.IsEnabled = true;
                comboBox_FrameRate.IsEnabled = true;
                textBox_FrameRate.IsEnabled = true;
                button_SetFrameRate.IsEnabled = true;
            }
            else
            {
                textBox_InputVideoInfo.IsEnabled = false;
                comboBox_VideoSource.IsEnabled = false;
                radioButton_NTSC.IsEnabled = false;
                radioButton_PAL.IsEnabled = false;
                comboBox_Resolution.IsEnabled = false;
                checkBox_Custom.IsEnabled = false;
                textBox_Width.IsEnabled = false;
                textBox_Height.IsEnabled = false;
                button_SetResolution.IsEnabled = false;
                comboBox_FrameRate.IsEnabled = false;
                textBox_FrameRate.IsEnabled = false;
                button_SetFrameRate.IsEnabled = false;
            }
            InitVideoFormat();
            checkBox_Custom.IsEnabled = false;
            textBox_Width.IsEnabled = false;
            textBox_Height.IsEnabled = false;
            button_SetResolution.IsEnabled = false;
            InitVideoSource();
            InitInputVideoInfo();
            InitVideoResolution();
            InitFrameRate();
         
            if (m_Timer.IsEnabled == true)
            {
                m_Timer.Stop();
            }
            m_Timer.Interval = TimeSpan.FromMilliseconds(1000);
            m_Timer.Tick += new EventHandler(m_Timer_Tick);
            m_Timer.Start();
        }

        private void m_Timer_Tick(object sender, EventArgs e)
        {
            InitInputVideoInfo();
        }

        private void InitInputVideoInfo()
        {
            INPUT_VIDEO_INFO InputVideoInfo = new INPUT_VIDEO_INFO();
            InputVideoInfo.dwVersion = 2;
            int bSignalPresence = 0;
            String strInputInfo, strVideoInfo, strSignalPresence, strHDCPProtected, strAudioSamplingRate;
            if ((int)ERRORCODE.CAP_EC_SUCCESS != AVerCapAPI.AVerGetSignalPresence(m_hCaptureDevice, ref bSignalPresence))
            {
                textBox_InputVideoInfo.Text = "Can't get signal presence.";
                return;
            }
            if (bSignalPresence != 0)
            {
                strSignalPresence = "     Signal Presence:    TRUE";
            }
            else
            {
                strSignalPresence = "     Signal Presence:    FALSE";
            }
            if ((int)ERRORCODE.CAP_EC_SUCCESS != AVerCapAPI.AVerGetVideoInfo(m_hCaptureDevice, ref InputVideoInfo))
            {
                textBox_InputVideoInfo.Text = "Get input video info failed!";
                return;
            }
            double dFrameRate = InputVideoInfo.dwFramerate / 100.0;
            uint uWidth = InputVideoInfo.dwWidth;
            uint uHeight = InputVideoInfo.dwHeight;
            if (InputVideoInfo.bProgressive==1)
            {
                strVideoInfo = string.Format("     Video:                    {0,-4:d}*{1,-4:d}@{2,4:f2}p", uWidth, uHeight, dFrameRate);
            }
            else
            {
                strVideoInfo = string.Format("     Video:                    {0,-4:d}*{1,-4:d}@{2,4:f2}i", uWidth, uHeight, dFrameRate*2);
            }
            uint dwMode=0;
            AVerCapAPI.AVerGetMacroVisionMode(m_hCaptureDevice, ref dwMode);
            if (dwMode > 0)
            {
                strHDCPProtected = "     HDCP Protected:   TRUE";
            }
            else
            {
                strHDCPProtected = "     HDCP Protected:   FALSE";
            }
            INPUT_AUDIO_INFO InputAudioInfo=new INPUT_AUDIO_INFO();
            InputAudioInfo.dwVersion = 1;
            if ((int)ERRORCODE.CAP_EC_SUCCESS != AVerCapAPI.AVerGetAudioInfo(m_hCaptureDevice, ref InputAudioInfo))
            {
                strAudioSamplingRate = "     Audio:                    Analog Not Support";
            }
            else 
            {
                strAudioSamplingRate = string.Format("     Audio:                    {0,-6:d}Hz", InputAudioInfo.dwSamplingRate);
            }
            strInputInfo = "Input Source Status:\r\n\r\n" + strSignalPresence + "\r\n\r\n" + strHDCPProtected + "\r\n\r\n" + strVideoInfo + "\r\n\r\n" + strAudioSamplingRate;
            textBox_InputVideoInfo.Text = strInputInfo;
        }

        private void InitVideoSource()
        {
            //Clear
            //Clear
            m_VideoSourceList.Clear();
            comboBox_VideoSource.Items.Clear();

            //Init


            uint SourceNum = 0;
            AVerCapAPI.AVerGetVideoSourceSupported(m_hCaptureDevice, null, ref SourceNum);
            uint[] pdwSupported = new uint[SourceNum];
            AVerCapAPI.AVerGetVideoSourceSupported(m_hCaptureDevice, pdwSupported, ref SourceNum);

            int index = 0;
            foreach (int i in pdwSupported)
            {
                switch (i)
                {
                    case 0:
                        comboBox_VideoSource.Items.Add("composite");
                        m_VideoSourceList.Add(0);
                        index = index + 1;
                        break;
                    case 1:
                        comboBox_VideoSource.Items.Add("S-Video");
                        m_VideoSourceList.Add(1);
                        index = index + 1;
                        break;
                    case 2:
                        comboBox_VideoSource.Items.Add("component");
                        m_VideoSourceList.Add(2);
                        index = index + 1;
                        break;
                    case 3:
                        comboBox_VideoSource.Items.Add("HDMI");
                        m_VideoSourceList.Add(3);
                        index = index + 1;
                        break;
                    case 4:
                        comboBox_VideoSource.Items.Add("VGA");
                        m_VideoSourceList.Add(4);
                        index = index + 1;
                        break;
                    case 5:
                        comboBox_VideoSource.Items.Add("SDI");
                        m_VideoSourceList.Add(5);
                        index = index + 1;
                        break;
                    case 6:
                        comboBox_VideoSource.Items.Add("ASI");
                        m_VideoSourceList.Add(6);
                        index = index + 1;
                        break;
                    case 7:
                        comboBox_VideoSource.Items.Add("DVI");
                        m_VideoSourceList.Add(7);
                        index = index + 1;
                        break;

                }
            }

            uint uVideoSource = 0;
            int nSelIndex = 0;
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref uVideoSource);
            for (int i = 0; i < index; i++)
            {
                if (uVideoSource == m_VideoSourceList[i])
                {

                    nSelIndex = i;
                    break;

                }

            }

            comboBox_VideoSource.SelectedIndex = nSelIndex;
        }

        private void InitVideoFormat()
        {
            uint uVideoFormat=0;
            int iReturn=AVerCapAPI.AVerGetVideoFormat(m_hCaptureDevice, ref uVideoFormat);
            if (iReturn == (int)ERRORCODE.CAP_EC_NOT_SUPPORTED) 
            {
                radioButton_NTSC.IsEnabled = false;
                radioButton_PAL.IsEnabled = false;
                return;
            }
            switch (uVideoFormat)
            {
                case (uint)VIDEOFORMAT.VIDEOFORMAT_NTSC:
                    radioButton_NTSC.IsChecked = true;
                    break;
                case (uint)VIDEOFORMAT.VIDEOFORMAT_PAL:
                    radioButton_PAL.IsChecked = true;
                    break;
            }
        }

        private void InitVideoResolution()
        {
            m_VideoResolutionList.Clear();
            comboBox_Resolution.Items.Clear();
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref m_uVideoSource);
            AVerCapAPI.AVerGetVideoFormat(m_hCaptureDevice, ref m_uVideoFormat);
            uint SolutionNum = 0;
            AVerCapAPI.AVerGetVideoResolutionSupported(m_hCaptureDevice, m_uVideoSource, m_uVideoFormat, null, ref SolutionNum);
            uint[] pdwSupported = new uint[SolutionNum];
            AVerCapAPI.AVerGetVideoResolutionSupported(m_hCaptureDevice, m_uVideoSource, m_uVideoFormat, pdwSupported, ref SolutionNum);

            uint m_uVideoResolution = 0;
            VIDEO_RESOLUTION VideoResolution =new VIDEO_RESOLUTION();
            VideoResolution.dwVersion = 1;
            AVerCapAPI.AVerGetVideoResolutionEx(m_hCaptureDevice, ref VideoResolution);
            m_uVideoResolution = VideoResolution.dwVideoResolution;
            int index = 0;
            foreach (uint i in pdwSupported)
            {
                m_VideoResolutionList.Add(i);
                if (m_uVideoResolution == i)
                {

                    comboBox_Resolution.SelectedIndex = index;

                }
                switch (i)
                {
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_640X480:
                        comboBox_Resolution.Items.Add("640X480");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_704X576:
                        comboBox_Resolution.Items.Add("704X576");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_720X480:
                        comboBox_Resolution.Items.Add("720X480");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_720X576:
                        comboBox_Resolution.Items.Add("720X576");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1920X1080:
                        comboBox_Resolution.Items.Add("1920X1080");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_160X120:
                        comboBox_Resolution.Items.Add("160X120");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_176X144:
                        comboBox_Resolution.Items.Add("176X144");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_240X176:
                        comboBox_Resolution.Items.Add("240X176");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_240X180:
                        comboBox_Resolution.Items.Add("240X180");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_320X240:
                        comboBox_Resolution.Items.Add("320X240");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_352X240:
                        comboBox_Resolution.Items.Add("352X240");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_352X288:
                        comboBox_Resolution.Items.Add("352X288");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_640X240:
                        comboBox_Resolution.Items.Add("640X240");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_640X288:
                        comboBox_Resolution.Items.Add("640X288");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_720X240:
                        comboBox_Resolution.Items.Add("720X240");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_720X288:
                        comboBox_Resolution.Items.Add("720X288");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_80X60:
                        comboBox_Resolution.Items.Add("80X60  ");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_88X72:
                        comboBox_Resolution.Items.Add("88X72  ");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_128X96:
                        comboBox_Resolution.Items.Add("128X96 ");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_640X576:
                        comboBox_Resolution.Items.Add("640X576");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_180X120:
                        comboBox_Resolution.Items.Add("180X120");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_180X144:
                        comboBox_Resolution.Items.Add("180X144");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_360X240:
                        comboBox_Resolution.Items.Add("360X240");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_360X288:
                        comboBox_Resolution.Items.Add("360X288");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_768X576:
                        comboBox_Resolution.Items.Add("768X576");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_384x288:
                        comboBox_Resolution.Items.Add("384x288");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_192x144:
                        comboBox_Resolution.Items.Add("192x144 ");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X720:
                        comboBox_Resolution.Items.Add("1280X720");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1024X768:
                        comboBox_Resolution.Items.Add("1024X768");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X800:
                        comboBox_Resolution.Items.Add("1280X800");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X1024:
                        comboBox_Resolution.Items.Add("1280X1024");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1440X900:
                        comboBox_Resolution.Items.Add("1440X900");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1600X1200:
                        comboBox_Resolution.Items.Add("1600X1200");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1680X1050:
                        comboBox_Resolution.Items.Add("1680X1050");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_800X600:
                        comboBox_Resolution.Items.Add("800X600");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X768:
                        comboBox_Resolution.Items.Add("1280X768");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1360X768:
                        comboBox_Resolution.Items.Add("1360X768");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1152X864:
                        comboBox_Resolution.Items.Add("1152X864");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X960:
                        comboBox_Resolution.Items.Add("1280X960");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_702X576:
                        comboBox_Resolution.Items.Add("702X576");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_720X400:
                        comboBox_Resolution.Items.Add("720X400");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1152X900:
                        comboBox_Resolution.Items.Add("1152X900");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1360X1024:
                        comboBox_Resolution.Items.Add("1360X1024");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1366X768:
                        comboBox_Resolution.Items.Add("1366X768");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1400X1050:
                        comboBox_Resolution.Items.Add("1400X1050");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1440X480:
                        comboBox_Resolution.Items.Add("1440X480");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1440X576:
                        comboBox_Resolution.Items.Add("1440X576");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1600X900:
                        comboBox_Resolution.Items.Add("1600X900");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1920X1200:
                        comboBox_Resolution.Items.Add("1920X1200");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1440X1080:
                        comboBox_Resolution.Items.Add("1440X1080");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1600X1024:
                        comboBox_Resolution.Items.Add("1600X1024");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_3840X2160:
                        comboBox_Resolution.Items.Add("3840X2160");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1152X768:
                        comboBox_Resolution.Items.Add("1152X768");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_176X120:
                        comboBox_Resolution.Items.Add("176X120");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_704X480:
                        comboBox_Resolution.Items.Add("704X480");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1792X1344:
                        comboBox_Resolution.Items.Add("1792X1344");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1856X1392:
                        comboBox_Resolution.Items.Add("1856X1392");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_1920X1440:
                        comboBox_Resolution.Items.Add("1920X1440");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_2048X1152:
                        comboBox_Resolution.Items.Add("2048X1152");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_2560X1080:
                        comboBox_Resolution.Items.Add("2560X1080");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_2560X1440:
                        comboBox_Resolution.Items.Add("2560X1440");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_2560X1600:
                        comboBox_Resolution.Items.Add("2560X1600");
                        index = index + 1;
                        break;
                    case (uint)VIDEORESOLUTION.VIDEORESOLUTION_4096X2160:
                        comboBox_Resolution.Items.Add("4096X2160");
                        index = index + 1;
                        break;

                }


            }

            if (m_bIsRangeResolution == true)
            {
                checkBox_Custom.IsEnabled = true;
            }
            if (VideoResolution.bCustom == 1)
            {
                checkBox_Custom.IsChecked = true;
                textBox_Width.Text = VideoResolution.dwWidth.ToString();
                textBox_Height.Text = VideoResolution.dwHeight.ToString();
                textBox_Width.IsEnabled = true;
                textBox_Height.IsEnabled = true;
                button_SetResolution.IsEnabled = true;
                comboBox_Resolution.IsEnabled = false;
            }
            else
            {
                checkBox_Custom.IsChecked = false;
                textBox_Width.Text = VideoResolution.dwWidth.ToString();
                textBox_Height.Text = VideoResolution.dwHeight.ToString();
                textBox_Width.IsEnabled = false;
                textBox_Height.IsEnabled = false;
                button_SetResolution.IsEnabled = false;
                comboBox_Resolution.IsEnabled = true;
            }
        }

        public bool InitFrameRate()
        {
            if (m_bIsRangeFramerate)
            {
                comboBox_FrameRate.IsEnabled = false;
                textBox_FrameRate.IsEnabled = true;
                button_SetFrameRate.IsEnabled = true;
                uint uFrameRate = 0;
                int ir = AVerCapAPI.AVerGetVideoInputFrameRate(m_hCaptureDevice, ref uFrameRate);
                if (ir != (int)ERRORCODE.CAP_EC_SUCCESS)
                {
                    return false;
                }
                textBox_FrameRate.Text = uFrameRate.ToString();
            }
            else
            {
                comboBox_FrameRate.IsEnabled = true;
                textBox_FrameRate.IsEnabled = false;
                button_SetFrameRate.IsEnabled = false;
                comboBox_FrameRate.Items.Clear();
                uint uNum = 0;
                int lr = 0;
            

                VIDEO_RESOLUTION VideoResolution = new VIDEO_RESOLUTION();
                VideoResolution.dwVersion = 1;
                AVerCapAPI.AVerGetVideoResolutionEx(m_hCaptureDevice, ref VideoResolution);
                lr = AVerCapAPI.AVerGetVideoInputFrameRateSupportedEx(m_hCaptureDevice, m_uVideoSource, m_uVideoFormat, VideoResolution.dwVideoResolution, null, ref uNum);
                uint[] pdwSupported = new uint[uNum];
                if (lr != (int)ERRORCODE.CAP_EC_SUCCESS)
                {
                    if (lr == (int)ERRORCODE.CAP_EC_NOT_SUPPORTED)
                    {
                        comboBox_FrameRate.IsEnabled = false;
                        return true;
                    }
                    return false;
                }
                uint dwFrameRate = 0;
                lr = AVerCapAPI.AVerGetVideoInputFrameRateSupportedEx(m_hCaptureDevice, m_uVideoSource, m_uVideoFormat, VideoResolution.dwVideoResolution, pdwSupported, ref uNum);
                if (lr != (int)ERRORCODE.CAP_EC_SUCCESS)
                {
                    return false;
                }
                bool bSelected = false;
                for (int i = 0; i < uNum; i++)
                {
                    comboBox_FrameRate.Items.Add(pdwSupported[i].ToString());
                    if (pdwSupported[i] == dwFrameRate)
                    {
                        comboBox_FrameRate.SelectedIndex = i;
                        bSelected = true;
                    }
                }
                if (!bSelected)
                {
                    comboBox_FrameRate.SelectedIndex = 0;
                }
            }
            return true;
        }

        private void DeviceSettingDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_bHide)
            {
                e.Cancel = true;
                this.Hide();
            }
        }  

        private void DeviceSettingDlg_Closed(object sender, EventArgs e)
        {
            if (m_Timer.IsEnabled == true)
            {
                m_Timer.Stop();
            }
        }


        private void button_SetResolution_Click(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            VIDEO_RESOLUTION VideoResolution = new VIDEO_RESOLUTION();
            VideoResolution.dwVersion = 1;
            VideoResolution.bCustom = 1;
            VideoResolution.dwWidth = uint.Parse(textBox_Width.Text);
            VideoResolution.dwHeight = uint.Parse(textBox_Height.Text);
            int lr = AVerCapAPI.AVerSetVideoResolutionEx(m_hCaptureDevice, ref VideoResolution);
            if (lr != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                if (lr == (int)ERRORCODE.CAP_EC_NOT_SUPPORTED)
                {
                    MessageBox.Show("Not Supported!");
                }
                MessageBox.Show("Set the custom resolution failed!");
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        private void comboBox_Resolution_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBox_Resolution.SelectedIndex == -1)
            {
                return;
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }

            uint nVideoResolution = 0;
            int nSelIndex = comboBox_Resolution.SelectedIndex;
            nVideoResolution = (uint)m_VideoResolutionList[nSelIndex];

            VIDEO_RESOLUTION VideoResolution =new VIDEO_RESOLUTION();
            VideoResolution.dwVersion = 1;
            VideoResolution.bCustom = 0;
            VideoResolution.dwVideoResolution = nVideoResolution;
            AVerCapAPI.AVerSetVideoResolutionEx(m_hCaptureDevice, ref VideoResolution);
            if (!InitFrameRate())
                MessageBox.Show("Init FrameRate failed!");
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        public bool SetFrameRate()
        {
            if (m_bIsRangeFramerate)
            {
                if (!textBox_FrameRate.IsEnabled)
                {
                    return true;
                }
                uint dwFrameRate = uint.Parse(textBox_FrameRate.Text);
                int lr = AVerCapAPI.AVerSetVideoInputFrameRate(m_hCaptureDevice, dwFrameRate);
                if (lr != (int)ERRORCODE.CAP_EC_SUCCESS)
                {
                    if (lr == (int)ERRORCODE.CAP_EC_INVALID_PARAM)
                    {
                        MessageBox.Show("Invalid framerate!");
                    }
                    return false;
                }
            }
            else
            {
                if (!comboBox_FrameRate.IsEnabled)
                {
                    return true;
                }
                uint dwFrameRate =uint.Parse(comboBox_FrameRate.Text);
                int lr = AVerCapAPI.AVerSetVideoInputFrameRate(m_hCaptureDevice, dwFrameRate);
                if (lr != (int)ERRORCODE.CAP_EC_SUCCESS)
                {
                    return false;
                }
            }
            return true;
        }

        private void button_SetFrameRate_Click(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            if (!SetFrameRate())
            {
                MessageBox.Show("Set FrameRate is failed!");
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        private void comboBox_VideoSource_DropDownClosed(object sender, EventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            uint VideoSource = m_VideoSourceList[(comboBox_VideoSource.SelectedIndex)];
            int lr = AVerCapAPI.AVerSetVideoSource(m_hCaptureDevice, VideoSource);
            InitAudioDevice();
            if (lr != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("Set Video Source failed!");
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
            else
            {
                InitVideoDevice();
            }
        }

        private void radioButton_NTSC_Checked(object sender, RoutedEventArgs e)
        {
            //add not supported
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            int lr = AVerCapAPI.AVerSetVideoFormat(m_hCaptureDevice, (uint)VIDEOFORMAT.VIDEOFORMAT_NTSC);
            if (lr != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                radioButton_NTSC.IsChecked = false;
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
            else
            {
                InitVideoDevice();
            }
        }

        private void radioButton_PAL_Checked(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            AVerCapAPI.AVerSetVideoFormat(m_hCaptureDevice, (uint)VIDEOFORMAT.VIDEOFORMAT_PAL);
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
            else
            {
                InitVideoDevice();
            }
        }

        private void comboBox_FrameRate_DropDownClosed(object sender, EventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            if (!SetFrameRate())
            {
                MessageBox.Show("Set FrameRate is failed!");
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        private void textBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (!(e.Key >= Key.D0 && e.Key <= Key.D9) && !(e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) && e.Key != Key.Back && e.Key != Key.Enter)
            {
                e.Handled = true;
            }
        }

 public void UpdateDemoWindow(DEMOSTATE DemoState)
        {
            m_DemoState = DemoState;
            InitWindow();
        }


       

        private void checkBox_Custom_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_Custom.IsChecked == true)
            {
                textBox_Width.IsEnabled = true;
                textBox_Height.IsEnabled = true;
                button_SetResolution.IsEnabled = true;
                comboBox_Resolution.IsEnabled = false;
            }
            else
            {
                textBox_Width.IsEnabled = false;
                textBox_Height.IsEnabled = false;
                button_SetResolution.IsEnabled = false;
                comboBox_Resolution.IsEnabled = true;
            }
        }
        private void comboBox_AudioSource_DropDownClosed(object sender, EventArgs e)
        {

            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            uint AudioSource = m_AudioSourceList[(comboBox_AudioSource.SelectedIndex)];
            int lr = AVerCapAPI.AVerSetAudioSource(m_hCaptureDevice, AudioSource);
            if (lr != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("Set Audio Source failed!");
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
            else
            {
                InitAudioDevice();
            }

        }

        private void checkBox_ThirdParty_Checked(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            if (checkBox_ThirdParty.IsChecked == true)
            {
                InitThirdAudioCap();
                button_SetThirdPart.IsEnabled = true;
            }
            else
            {
                AUDIOCAPTURESOURCE_SETTING AudioCapSourceSetting=new AUDIOCAPTURESOURCE_SETTING();
                AudioCapSourceSetting.dwVersion = 1;
                AudioCapSourceSetting.dwCapSourceIndex = 0xffffffff;
                int hr =AVerCapAPI.AVerSetThirdPartyAudioCapSource(m_hCaptureDevice, ref AudioCapSourceSetting);
                InitThirdAudioCap();
                comboBox_AudioCaptureSource.IsEnabled = false;
                comboBox_InputType.IsEnabled = false;
                comboBox_AudioFormat.IsEnabled = false;
                button_SetThirdPart.IsEnabled = false;
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        private void button_SetThirdPart_Click(object sender, RoutedEventArgs e)
        {
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.stopStreaming();
            }
            AUDIOCAPTURESOURCE_SETTING AudioCapSourceSetting = new AUDIOCAPTURESOURCE_SETTING();
            AudioCapSourceSetting.dwVersion = 1;
            if (checkBox_ThirdParty.IsChecked == true)
            {
                AudioCapSourceSetting.dwCapSourceIndex = Convert.ToUInt32(comboBox_AudioCaptureSource.SelectedIndex);
                if (AudioCapSourceSetting.dwCapSourceIndex == 0)
                {
                    AudioCapSourceSetting.dwCapSourceIndex = uint.MaxValue;
                    AVerCapAPI.AVerSetThirdPartyAudioCapSource(m_hCaptureDevice, ref AudioCapSourceSetting);
                    MessageBox.Show("Please Select a third-party audio capture device");
                    if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
                    {
                        m_obMainWindow.startStreaming();
                    }
                    return;
                }
                else
                {
                    if (!comboBox_InputType.IsEnabled || !comboBox_AudioFormat.IsEnabled)
                    {
                        MessageBox.Show("Selected third-party audio capture device abnormalities.");
                        if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
                        {
                            m_obMainWindow.startStreaming();
                        }
                        return;
                    }
                    AudioCapSourceSetting.dwCapSourceIndex = AudioCapSourceSetting.dwCapSourceIndex - 1;
                    AudioCapSourceSetting.dwInputTypeIndex = Convert.ToUInt32(comboBox_InputType.SelectedIndex);
                    AudioCapSourceSetting.dwFormatIndex = Convert.ToUInt32(comboBox_AudioFormat.SelectedIndex);
                    AVerCapAPI.AVerSetThirdPartyAudioCapSource(m_hCaptureDevice, ref AudioCapSourceSetting);
                }
            }
            if (m_DemoState == DEMOSTATE.DEMO_STATE_PREVIEW)
            {
                m_obMainWindow.startStreaming();
            }
        }

        private void comboBox_AudioCaptureSource_DropDownClosed(object sender, EventArgs e)
        {
            comboBox_InputType.Items.Clear();
            comboBox_AudioFormat.Items.Clear();
            comboBox_InputType.IsEnabled = false;
            comboBox_AudioFormat.IsEnabled = false;
            int iCapSourceIndex = comboBox_AudioCaptureSource.SelectedIndex - 1;
            if (iCapSourceIndex == -1)
            {
                return;
            }
            uint dwNum, i;
            dwNum = i = 0;
            int ret = -1;
            ret = AVerCapAPI.AVerEnumThirdPartyAudioCapSourceInputType(m_hCaptureDevice, (uint)iCapSourceIndex, IntPtr.Zero, ref dwNum);
            if (ret != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("Can't enumerate input type.");
            }
            else
            {
                if (dwNum != 0)
                {
                    m_pAudioCapSourceInputTypeInfo = new AUDIOCAPTURESOURCE_INPUTTYPE_INFO[dwNum];
                    for (int k = 0 ; k < dwNum ; ++k)
                    {
                        m_pAudioCapSourceInputTypeInfo[k].szName = new char[256];
                    }
                    m_pAudioCapSourceInputTypeInfo[0].dwVersion = 1;
                    int StructSize = Marshal.SizeOf(typeof(AUDIOCAPTURESOURCE_INPUTTYPE_INFO));
                    IntPtr pt = Marshal.AllocHGlobal((IntPtr)(StructSize * dwNum));
                    Marshal.WriteInt32(pt, 1);
                    AVerCapAPI.AVerEnumThirdPartyAudioCapSourceInputType(m_hCaptureDevice, (uint)iCapSourceIndex, pt, ref dwNum);
                    comboBox_InputType.IsEnabled = true;
                    string szCapSourceInputTypeName;
                    for (i = 0 ; i < dwNum ; ++i)
                    {
                        m_pAudioCapSourceInputTypeInfo[i] = (AUDIOCAPTURESOURCE_INPUTTYPE_INFO)Marshal.PtrToStructure(
                        (IntPtr)((uint)pt + i * StructSize), typeof(AUDIOCAPTURESOURCE_INPUTTYPE_INFO));
                        szCapSourceInputTypeName = new string(m_pAudioCapSourceInputTypeInfo[i].szName);
                        for (int j = 0 ; j < szCapSourceInputTypeName.Length ; ++j)
                        {
                            if (szCapSourceInputTypeName[j] == '\0')
                            {
                                szCapSourceInputTypeName = szCapSourceInputTypeName.Substring(0, j);
                                break;
                            }
                        }
                        comboBox_InputType.Items.Add(szCapSourceInputTypeName);
                    }
                    Marshal.FreeHGlobal(pt);
                    comboBox_InputType.SelectedIndex = 0;
                }
            }
            //Enum Sample format
            ret = AVerCapAPI.AVerEnumThirdPartyAudioCapSourceSampleFormat(m_hCaptureDevice, (uint)iCapSourceIndex, IntPtr.Zero, ref dwNum);
            if (ret != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("Can't enumerate sample format.");
            }
            else
            {
                m_pAudioCapSourceFormatInfo = new AUDIOCAPTURESOURCE_FORMAT_INFO[dwNum];
                for (int k = 0 ; k < dwNum ; ++k)
                {
                    m_pAudioCapSourceFormatInfo[k].szName = new char[256];
                }
                m_pAudioCapSourceFormatInfo[0].dwVersion = 1;
                int StructSize = Marshal.SizeOf(typeof(AUDIOCAPTURESOURCE_FORMAT_INFO));
                IntPtr pt = Marshal.AllocHGlobal((IntPtr)(StructSize * dwNum));
                Marshal.WriteInt32(pt, 1);
                AVerCapAPI.AVerEnumThirdPartyAudioCapSourceSampleFormat(m_hCaptureDevice, (uint)iCapSourceIndex, pt, ref dwNum);
                comboBox_AudioFormat.IsEnabled = true;
                string szAudioCapSourceFormatName;
                for (i = 0 ; i < dwNum ; ++i)
                {
                    m_pAudioCapSourceFormatInfo[i] = (AUDIOCAPTURESOURCE_FORMAT_INFO)Marshal.PtrToStructure(
                        (IntPtr)((uint)pt + i * StructSize), typeof(AUDIOCAPTURESOURCE_FORMAT_INFO));
                    szAudioCapSourceFormatName = new string(m_pAudioCapSourceFormatInfo[i].szName);
                    for (int j = 0 ; j < szAudioCapSourceFormatName.Length ; ++j)
                    {
                        if (szAudioCapSourceFormatName[j] == '\0')
                        {
                            szAudioCapSourceFormatName = szAudioCapSourceFormatName.Substring(0, j);
                            break;
                        }
                    }
                    comboBox_AudioFormat.Items.Add(szAudioCapSourceFormatName);
                }
                Marshal.FreeHGlobal(pt);
                comboBox_AudioFormat.SelectedIndex = 0;
            }
        }

        //private void radioButtonEnable_Checked(object sender, RoutedEventArgs e)
        //{
        //    dateTimePicker.Enabled = true;
        //}

        //private void radioButtonDisable_Checked(object sender, RoutedEventArgs e)
        //{
        //    dateTimePicker.Enabled = false;
        //}

       

       
    }
}

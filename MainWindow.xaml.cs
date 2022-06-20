using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Threading;

namespace AVerCapSDKDemo
{
#if _X64_MACHINE
    using LONGPTR = System.Int64;
#else
    using LONGPTR = System.Int32;
#endif
    public partial class MainWindow : Window
    {
        private DEMOSTATE m_DemoState=DEMOSTATE.DEMO_STATE_STOP;
        private uint m_uDeviceNum = 0;
        private int m_iCurrentDeviceIndex = -1;
        private bool m_bIsStartStreaming = false;
        private IntPtr m_hCaptureDevice = new IntPtr();
        public uint m_uCaptureType = 0;
        private uint m_uCurrentCardType = (uint)CARDTYPE.CARDTYPE_NULL;

        //Dynamically generated menu
        private MenuItem[] m_Device;
        private MenuItem[] m_SDDevice;
        private MenuItem[] m_HDDevice;
        //...Dynamically generated menu

        //for MixVideo
        private int m_bMixVideoPreviewIsEnabled = 0;
        private int m_bMixAudioPreviewIsEnabled = 0;
        private int m_nLeftRatio = -1;
        private int m_nTopRatio = -1;
        private int m_nHeightRatio = -1;
        private int m_nWidthRatio = -1;
        private bool m_bMixingVideo = false;
        private static VIDEOCAPTURECALLBACK m_VideoCaptureCallBack;

        private IntPtr m_hMixCaptureDevice = new IntPtr();
        private int m_nMixDeviceIndex = -1;
        private int m_nMixVideoSource = -1;
        private uint[,] m_uVideoResolutionArray = new uint[20, 4] {
        {720, 480, 0, 2}, {720, 480, 1, 3}, {720, 576, 0, 4}, {720, 576, 1, 5}, {1280, 720, 1, 6}, {1920, 1080, 0, 7}, 
        {1024, 768, 1, 8}, {1280, 800, 1, 9}, {1280, 1024, 1, 10}, {1440, 900, 1, 11}, {1600, 1200, 1, 12}, {1680, 1050, 1, 13},
        {1920, 1080, 1, 14}, {1920, 1080, 1, 15}, {640, 480, 1, 16}, {800, 600, 1, 17}, {1280, 768, 1, 18}, {1360, 768, 1, 19},
        {1152, 864, 1, 35}, {1280, 960, 1, 36}
        };
        //...for MixVideo

        private NOTIFYEVENTCALLBACK m_NotifyEventCallback = new NOTIFYEVENTCALLBACK(NotifyEventCallback);

        public bool m_bHadSetVideoRenderer = false;

        //private System.Windows.Forms.PictureBox pictureBoxShowMain = null;

        private DeviceSetting     m_ShowDeviceSetting = null;
        private PreviewSetting    m_ShowPreviewSetting = null;
        private VideoProcess      m_ShowVideoProcess = null;

        private CaptureImage      m_ShowCaptureImage = null;
        private Record            m_ShowRecord = null;




        const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        private bool m_bRestore=false;
        private string m_szFileName = null;
        [DllImport("kernel32.dll")]
        public static extern void GetNativeSystemInfo(ref SYSTEM_INFO SystemInfo);
        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileString(string szSectionName, string szKeyName, string szDefault,
                                                          StringBuilder szRetValue, uint uSize, string szFileName);
        [DllImport("kernel32.dll")]
        public static extern uint WritePrivateProfileString(string szSectionName, string szKeyName, string szValueName, string szFileName);


        public MainWindow()
        {
            InitializeComponent();
            SYSTEM_INFO SystemInfo = new SYSTEM_INFO();
            GetNativeSystemInfo(ref SystemInfo);
            StringBuilder szValue = new StringBuilder("", 20);
            string str = System.Environment.CurrentDirectory;
            if (SystemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64 ||
                SystemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_IA64)
            {
                //64 bit os
                m_szFileName = str + "\\AVerCapSDKDemoAP_WPF.ini";
            }
            else
            {
                //32 bit os
                m_szFileName = str + "\\AVerCapSDKDemoAP_WPF.ini";
            }
            GetPrivateProfileString("DemoSetting", "IsRestore", "NO",szValue, 10, m_szFileName);
            string szTemp = szValue.ToString();
            if (szTemp.Equals("NO"))
            {
                m_bRestore = false;
                IsRestore.IsChecked = false;
            }
            else
            {
                m_bRestore = true;
                IsRestore.IsChecked = true;
            }
            InitMainWindow();
        }

        protected override void OnActivated(EventArgs e)
        {
          //  pictureBoxShowMain.Size = new System.Drawing.Size(640, 520);
            base.OnActivated(e);
        }

        private int InitMainWindow()
        {
            pictureBoxShowMain.Location = new System.Drawing.Point(0, 0);
            pictureBoxShowMain.TabIndex = 1;
            pictureBoxShowMain.TabStop = false;
            pictureBoxShowMain.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxShowMain_Paint);
#if false
            Window w = new Window();
            w.WindowStyle = WindowStyle.None;
            w.Width = 800;
            w.Height = 600;
            w.AllowsTransparency = true;
            w.Opacity = 0.5;
            w.Show();
#endif

            StringBuilder szDeviceName = new StringBuilder("", 260);
            if (AVerCapAPI.AVerGetDeviceNum(ref m_uDeviceNum) != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show("Can't get the number of devices.", "AVer Capture SDK");
                return -1;
            }
            if (m_uDeviceNum == 0)
            {
                this.deviceMenuItem.IsEnabled = false;
                this.functionMenuItem.IsEnabled = false;
                return 0;
            }
            m_Device = new MenuItem[m_uDeviceNum];
            m_SDDevice = new MenuItem[m_uDeviceNum];
            m_HDDevice = new MenuItem[m_uDeviceNum];
            for (int i = 0; i < m_uDeviceNum; ++i)
            {
                AVerCapAPI.AVerGetDeviceName((uint)i, szDeviceName);
                if (AVerCapAPI.AVerGetDeviceName((uint)i, szDeviceName) != (int)ERRORCODE.CAP_EC_SUCCESS)
                {
                    MessageBox.Show("Can't get the name of device.", "AVer Capture SDK");
                    return -1;
                }
                m_uCurrentCardType = GetCurrentCardType(i);
                m_Device[i] = new MenuItem();
                m_Device[i].Header = szDeviceName.ToString();
                m_Device[i].Name = "device" + i.ToString() + "MenuItem";
                m_Device[i].TabIndex = i;
                deviceMenuItem.Items.Add(m_Device[i]);
                if (m_uCurrentCardType == (uint)CARDTYPE.CARDTYPE_C727 || m_uCurrentCardType == (uint)CARDTYPE.CARDTYPE_C729 ||
                    m_uCurrentCardType == (uint)CARDTYPE.CARDTYPE_C129 )
                {
                    m_SDDevice[i] = new MenuItem();
                    m_SDDevice[i].Name = "SDdevice" + i.ToString() + "MenuItem";
                    m_SDDevice[i].Click += SelectCaptureDevice_Click;
                    m_SDDevice[i].Header = "SD Device";
                    m_HDDevice[i] = new MenuItem();
                    m_HDDevice[i].Name = "HDdevice" + i.ToString() + "MenuItem";
                    m_HDDevice[i].Click += SelectCaptureDevice_Click;
                    m_HDDevice[i].Header = "HD Device";
                    m_SDDevice[i].TabIndex = i;
                    m_HDDevice[i].TabIndex = i;
                    m_Device[i].Items.Add(m_SDDevice[i]);
                    m_Device[i].Items.Add(m_HDDevice[i]);
                }
                else
                {

                    m_Device[i].Click += SelectCaptureDevice_Click;


                }
            }
            if (m_bRestore)
            {
                StringBuilder szValue = new StringBuilder("", 50);
                GetPrivateProfileString("DeviceName", "DeviceName", "", szValue, 50, m_szFileName);

                string strLastDeciveName = szValue.ToString();
                strLastDeciveName = strLastDeciveName.Substring(strLastDeciveName.IndexOf(':') + 1);


                for (int i = 0; i < m_uDeviceNum; i++)
                {
                    StringBuilder wszDeviceName = new StringBuilder("", 50);
                    AVerCapAPI.AVerGetDeviceName((uint)i, wszDeviceName);
                    string szCurrName = wszDeviceName.ToString();
                    szCurrName = szCurrName.Substring(szCurrName.IndexOf(':') + 1);
                    if (szCurrName == strLastDeciveName)
                    {
                        GetPrivateProfileString("DemoSetting", "DeviceType", "", szValue, 10, m_szFileName);
                        uint dwCaptureType = uint.Parse(szValue.ToString());
                        uint dwCardType = 0;
                        AVerCapAPI.AVerGetDeviceType((uint)i, ref dwCardType);
                        if (dwCardType == (uint)CARDTYPE.CARDTYPE_C727 || dwCardType == (uint)CARDTYPE.CARDTYPE_C729 ||
                        dwCardType == (uint)CARDTYPE.CARDTYPE_C129)
                        {
                            if (dwCaptureType == (uint)CAPTURETYPE.CAPTURETYPE_SD)
                            {
                                SelectCaptureDevice_Click(m_SDDevice[i], null);
                            }
                            else if (dwCaptureType == (uint)CAPTURETYPE.CAPTURETYPE_HD)
                            {
                                SelectCaptureDevice_Click(m_HDDevice[i], null);
                            }
                        }
                        else
                        {

                            SelectCaptureDevice_Click(m_Device[i], null);

                        }

                        GetPrivateProfileString("VideoSource", "VideoSource", "", szValue, 10, m_szFileName);
                        uint dwVideoSource = uint.Parse(szValue.ToString());
                        AVerCapAPI.AVerSetVideoSource(m_hCaptureDevice, dwVideoSource);

                        GetPrivateProfileString("AudioSource", "AudioSource", "", szValue, 10, m_szFileName);
                        uint dwAudioSource = uint.Parse(szValue.ToString());
                        AVerCapAPI.AVerSetAudioSource(m_hCaptureDevice, dwAudioSource);

                        GetPrivateProfileString("VideoFormat", "VideoFormat", "", szValue, 10, m_szFileName);
                        uint dwVideoFormat = uint.Parse(szValue.ToString());
                        AVerCapAPI.AVerSetVideoFormat(m_hCaptureDevice, dwVideoFormat);

                        GetPrivateProfileString("VideoFrameRate", "VideoFrameRate", "", szValue, 10, m_szFileName);
                        uint dwVideoFrameRate = uint.Parse(szValue.ToString());
                        AVerCapAPI.AVerSetVideoInputFrameRate(m_hCaptureDevice, dwVideoFrameRate);

                        VIDEO_RESOLUTION VideoResolution = new VIDEO_RESOLUTION();
                        VideoResolution.dwVersion = 1;
                        GetPrivateProfileString("VideoResolution", "Resolution", "", szValue, 10, m_szFileName);
                        VideoResolution.dwVideoResolution = uint.Parse(szValue.ToString());
                        GetPrivateProfileString("VideoResolution", "ResolutionWidth", "", szValue, 10, m_szFileName);
                        VideoResolution.dwWidth = uint.Parse(szValue.ToString());
                        GetPrivateProfileString("VideoResolution", "ResolutionHeight", "", szValue, 10, m_szFileName);
                        VideoResolution.dwHeight = uint.Parse(szValue.ToString());
                        GetPrivateProfileString("VideoResolution", "IsCustom", "", szValue, 10, m_szFileName);
                        string szTemp = szValue.ToString();
                        if (szTemp.Equals("NO"))
                        {
                            VideoResolution.bCustom = 0;
                        }
                        else
                        {
                            VideoResolution.bCustom = 1;
                        }
                        AVerCapAPI.AVerSetVideoResolutionEx(m_hCaptureDevice, ref VideoResolution);
                    }
                }
                functionMenuItem.IsEnabled = true;
            }
            return 0;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (m_iCurrentDeviceIndex != -1)
            {
                SaveSettingAsIni();
            }
            if (m_hCaptureDevice != (IntPtr)0)
            {
                DeleteCaptureDevice();
            }
            base.OnClosed(e);
        }

        private void DeleteCaptureDevice()
        {
            if (m_bIsStartStreaming)
            {
                AVerCapAPI.AVerStopStreaming(m_hCaptureDevice);
                m_bIsStartStreaming = false;
            }
            AVerCapAPI.AVerDeleteCaptureObject(m_hCaptureDevice);
            m_hCaptureDevice = (IntPtr)0;
        }

        //for MixVideo
        public uint GetVideoResolutionIndex(uint dwWidth, uint dwHeight)
        {
            uint dwResoluton = 0;

            if (dwWidth == 640 && dwHeight == 480)                
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_640X480;
            else if (dwWidth == 704 && dwHeight == 576)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_704X576;
            else if (dwWidth == 720 && dwHeight == 480)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_720X480;
            else if (dwWidth == 720 && dwHeight == 576)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_720X576;
            else if (dwWidth == 1920 && dwHeight == 1080)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1920X1080;
            else if (dwWidth == 1024 && dwHeight == 768)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1024X768;
            else if (dwWidth == 1280 && dwHeight == 720)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X720;
            else if (dwWidth == 1280 && dwHeight == 800)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X800;
            else if (dwWidth == 1280 && dwHeight == 1024)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X1024;
            else if (dwWidth == 1440 && dwHeight == 900)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1440X900;
            else if (dwWidth == 1600 && dwHeight == 1200)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1600X1200;
            else if (dwWidth == 1680 && dwHeight == 1050)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1680X1050;
            else if (dwWidth == 800 && dwHeight == 600)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_800X600;
            else if (dwWidth == 1280 && dwHeight == 768)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X768;
            else if (dwWidth == 1152 && dwHeight == 864)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1152X864;
            else if (dwWidth == 1280 && dwHeight == 960)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1280X960;
            else if (dwWidth == 1360 && dwHeight == 768)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_1360X768;
            else if (dwWidth == 160 && dwHeight == 120)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_160X120;
            else if (dwWidth == 176 && dwHeight == 144)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_176X144;
            else if (dwWidth == 240 && dwHeight == 176)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_240X176;
            else if (dwWidth == 240 && dwHeight == 180)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_240X180;
            else if (dwWidth == 320 && dwHeight == 240)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_320X240;
            else if (dwWidth == 352 && dwHeight == 240)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_352X240;
            else if (dwWidth == 352 && dwHeight == 288)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_352X288;
            else if (dwWidth == 640 && dwHeight == 240)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_640X240;
            else if (dwWidth == 640 && dwHeight == 288)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_640X288;
            else if (dwWidth == 720 && dwHeight == 240)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_720X240;
            else if (dwWidth == 720 && dwHeight == 288)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_720X288;
            else if (dwWidth == 80 && dwHeight == 60)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_80X60;
            else if (dwWidth == 88 && dwHeight == 72)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_88X72;
            else if (dwWidth == 128 && dwHeight == 96)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_128X96;
            else if (dwWidth == 640 && dwHeight == 576)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_640X576;
            else if (dwWidth == 180 && dwHeight == 120)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_180X120;
            else if (dwWidth == 180 && dwHeight == 144)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_180X144;
            else if (dwWidth == 360 && dwHeight == 240)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_360X240;
            else if (dwWidth == 360 && dwHeight == 288)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_360X288;
            else if (dwWidth == 768 && dwHeight == 576)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_768X576;
            else if (dwWidth == 384 && dwHeight == 288)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_384x288;
            else if (dwWidth == 192 && dwHeight == 144)
                dwResoluton = (uint)VIDEORESOLUTION.VIDEORESOLUTION_192x144;

            return dwResoluton;
        }
 
  

        private void GetCapturesize(IntPtr hCaptureDevice, ref uint puHeight, ref uint puWidth)
        {
            RECT rcUpscaleBlackRect = new RECT();
            AVerCapAPI.AVerGetVideoUpscaleBlackRect(hCaptureDevice, ref rcUpscaleBlackRect);
            puWidth = (uint)(rcUpscaleBlackRect.Right - rcUpscaleBlackRect.Left + 1);
            puHeight = (uint)(rcUpscaleBlackRect.Bottom - rcUpscaleBlackRect.Top + 1);
        }
        //...for MixVideo

        public uint GetCurrentCardType(int iCurrentDeviceIndex)
        {
            if (iCurrentDeviceIndex < 0)
                return (uint)CARDTYPE.CARDTYPE_NULL;
            uint DeviceType = 0;

            AVerCapAPI.AVerGetDeviceType((uint)iCurrentDeviceIndex, ref DeviceType);
            return DeviceType;
        }

        private void SelectCaptureDevice_Click(object sender, RoutedEventArgs e)
        {
            m_DemoState = DEMOSTATE.DEMO_STATE_STOP;
            MenuItem DeviceMenuObject = sender as MenuItem;

            int iTempIndex = DeviceMenuObject.TabIndex;
            int iTempdeviceType = -1;
            if (DeviceMenuObject.Name == "SDdevice" + iTempIndex.ToString() + "MenuItem")
            {
                iTempdeviceType = (int)CAPTURETYPE.CAPTURETYPE_SD;
            }
            else
            {
                iTempdeviceType = (int)CAPTURETYPE.CAPTURETYPE_HD;
            }

            if (DeviceMenuObject.IsChecked == true)
            {
                DeleteCaptureDevice();
                SaveSettingAsIni();
                m_uCaptureType = 0;
                m_iCurrentDeviceIndex = -1;
                DeviceMenuObject.IsChecked = false;
                if (m_iCurrentDeviceIndex == -1)
                {
                    m_uCurrentCardType = GetCurrentCardType(m_iCurrentDeviceIndex);
                }
                DeleteFunctionWindows();
                return;
            }

            if (m_hCaptureDevice != (IntPtr)0)
            {
                DeleteCaptureDevice();
                DeleteFunctionWindows();
            }

            for (int i = 0; i < m_uDeviceNum; ++i)
            {
                if (m_Device[i] != null)
                {
                    m_Device[i].IsChecked = false;
                }
                if (m_SDDevice[i] != null)
                {
                    m_SDDevice[i].IsChecked = false;
                }
                if (m_HDDevice[i] != null)
                {
                    m_HDDevice[i].IsChecked = false;
                }
            }
            m_uCaptureType = 0;
            m_iCurrentDeviceIndex = -1;

            int iRetVal = 0;
            if (iTempdeviceType == (int)CAPTURETYPE.CAPTURETYPE_SD)
            {
                iRetVal = AVerCapAPI.AVerCreateCaptureObjectEx((uint)iTempIndex, (uint)CAPTURETYPE.CAPTURETYPE_SD, pictureBoxShowMain.Handle, ref m_hCaptureDevice);
            }
            else
            {               
                iRetVal = AVerCapAPI.AVerCreateCaptureObjectEx((uint)iTempIndex, (uint)CAPTURETYPE.CAPTURETYPE_HD, pictureBoxShowMain.Handle, ref m_hCaptureDevice);
            }
            switch (iRetVal)
            {
                case (int)ERRORCODE.CAP_EC_SUCCESS:
                    break;
                case (int)ERRORCODE.CAP_EC_DEVICE_IN_USE:
                    MessageBox.Show("The capture device has already been used.", "AVer Capture SDK");
                    return;
                default:
                    MessageBox.Show("Can't initialize the capture device.", "AVer Capture SDK");
                    return;
            }
            m_iCurrentDeviceIndex = iTempIndex;
            DeviceMenuObject.IsChecked = true;
            this.Title = "TV Workstation - Channel " + iTempIndex.ToString();

            if (m_uCaptureType == 0)
            {
                if (iTempdeviceType == (int)CAPTURETYPE.CAPTURETYPE_SD)
                {
                    m_uCaptureType = (uint)CAPTURETYPE.CAPTURETYPE_SD;
                }
                else
                {
                    m_uCaptureType = (uint)CAPTURETYPE.CAPTURETYPE_HD;
                }
                m_uCurrentCardType = GetCurrentCardType(m_iCurrentDeviceIndex);
            }
        }

        public void startStreaming() 
        {
            if (!m_bHadSetVideoRenderer)
                AVerCapAPI.AVerSetVideoRenderer(m_hCaptureDevice, (uint)VIDEORENDERER.VIDEORENDERER_EVR);

            GCHandle gchThis = GCHandle.Alloc(this);
            AVerCapAPI.AVerSetEventCallback(m_hCaptureDevice, m_NotifyEventCallback, 0, GCHandle.ToIntPtr(gchThis));
            if (AVerCapAPI.AVerStartStreaming(m_hCaptureDevice) != (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                 MessageBox.Show("Can't start streaming", "AVer Capture SDK");
                 return;
            }
            RECT RectClient = new RECT();
            RectClient.Left = 0;
            RectClient.Top = 0;
            RectClient.Right = pictureBoxShowMain.Width;
            RectClient.Bottom = pictureBoxShowMain.Height;
            AVerCapAPI.AVerSetVideoWindowPosition(m_hCaptureDevice, RectClient);
            m_bIsStartStreaming = true;
            UpdateDemoState(DEMOSTATE.DEMO_STATE_PREVIEW, true);
            

        }

        private void startStreamingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            startStreaming();

        }

        private void MainWindow_Resize(object sender, SizeChangedEventArgs e)
        {
            RECT RectClient = new RECT();
            RectClient.Left = 0;
            RectClient.Top = 0;
            RectClient.Right = pictureBoxShowMain.Width;
            RectClient.Bottom = pictureBoxShowMain.Height;
            AVerCapAPI.AVerSetVideoWindowPosition(m_hCaptureDevice, RectClient);
        }

        private void functionMenuItem_DropDownOpening(object sender, RoutedEventArgs e)
        {
            MenuItem FunctionMenuObject = sender as MenuItem;
            for (int i = 0; i < FunctionMenuObject.Items.Count; ++i)
            {
                MenuItem it = FunctionMenuObject.Items.GetItemAt(i) as MenuItem;
                if (it != null && it.TabIndex != -1)
                {
                    it.IsEnabled = false;
                }
            }
            uint uVideoSource = 0;
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref uVideoSource);
            if (m_iCurrentDeviceIndex >= 0)
            {
                if (m_bIsStartStreaming)
                {
                    startStreamingMenuItem.IsEnabled = false;
                    stopStreamingMenuItem.IsEnabled = true;
                    SerialNumberMenuItem.IsEnabled = true;
                }
                else 
                {
                    startStreamingMenuItem.IsEnabled = true;
                    stopStreamingMenuItem.IsEnabled = false;
                }
                DeviceSettingMenuItem.IsEnabled = true;
                PreviewSettingMenuItem.IsEnabled = true;
                VideoProcessMenuItem.IsEnabled = true;
                SerialNumberMenuItem.IsEnabled = true;
          
                CaptureImageMenuItem.IsEnabled = true;
                RecordMenuItem.IsEnabled = true;
     
            }
        }

        public void stopStreaming()
        {
            if (m_bIsStartStreaming)
            {
                AVerCapAPI.AVerStopStreaming(m_hCaptureDevice);
                m_bIsStartStreaming = false;
            }
            AVerCapAPI.AVerSetEventCallback(m_hCaptureDevice, null, 0, IntPtr.Zero);
            pictureBoxShowMain.Refresh();
        }

        private void stopStreamingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            stopStreaming();
            UpdateDemoState(DEMOSTATE.DEMO_STATE_STOP, true);
        }

        private void SerialNumberMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder szDeviceSerial = new StringBuilder("", 12);
            int hr = 0;
            hr = AVerCapAPI.AVerGetDeviceSerialNum((uint)m_iCurrentDeviceIndex, szDeviceSerial);
            if (hr == (int)ERRORCODE.CAP_EC_SUCCESS)
            {
                MessageBox.Show(szDeviceSerial.ToString().Substring(0, 12));
            }
            else
            {
                MessageBox.Show("not support");

            }
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowVersionWindow ShowVerWindow = new ShowVersionWindow();
            ShowVerWindow.Owner = this;
            ShowVerWindow.ShowDialog();
        }
   
        private void pictureBoxShowMain_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (m_hCaptureDevice != IntPtr.Zero)
            {
                AVerCapAPI.AVerRepaintVideo(m_hCaptureDevice);
            }
        }

        public static int NotifyEventCallback(uint dwEventCode, IntPtr lpEventData, IntPtr lpUserData)
        {
            switch (dwEventCode)
            {
                case (uint)CAPTUREEVENT.EVENT_CAPTUREIMAGE:
                    {
                        if (lpUserData == null || lpEventData == null)
                            return 0;

                        GCHandle gchThis = GCHandle.FromIntPtr(lpUserData);
                        CAPTUREIMAGE_NOTIFY_INFO CaptureImageNotifyInfo = new CAPTUREIMAGE_NOTIFY_INFO();
                        CaptureImageNotifyInfo = (CAPTUREIMAGE_NOTIFY_INFO)Marshal.PtrToStructure(lpEventData, typeof(CAPTUREIMAGE_NOTIFY_INFO));
                        if (((MainWindow)gchThis.Target).m_ShowCaptureImage != null)
                            ((MainWindow)gchThis.Target).m_ShowCaptureImage.ModifyName(ref CaptureImageNotifyInfo);
                    }
                    break;
                case (uint)CAPTUREEVENT.EVENT_CHECKCOPP:
                    {
                        uint plErrorID = (uint)Marshal.ReadInt32(lpEventData);
                        string strErrorID = "";
                        switch (plErrorID)
                        {
                            case (uint)COPPERRCODE.COPP_ERR_UNKNOWN:
                                strErrorID = "COPP_ERR_UNKNOWN";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_NO_COPP_HW:
                                strErrorID = "COPP_ERR_NO_COPP_HW";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_NO_MONITORS_CORRESPOND_TO_DISPLAY_DEVICE:
                                strErrorID = "COPP_ERR_NO_MONITORS_CORRESPOND_TO_DISPLAY_DEVICE";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_CERTIFICATE_CHAIN_FAILED:
                                strErrorID = "COPP_ERR_CERTIFICATE_CHAIN_FAILED";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_STATUS_LINK_LOST:
                                strErrorID = "COPP_ERR_STATUS_LINK_LOST";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_NO_HDCP_PROTECTION_TYPE:
                                strErrorID = "COPP_ERR_NO_HDCP_PROTECTION_TYPE";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_HDCP_REPEATER:
                                strErrorID = "COPP_ERR_HDCP_REPEATER";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_HDCP_PROTECTED_CONTENT:
                                strErrorID = "COPP_ERR_HDCP_PROTECTED_CONTENT";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_GET_CRL_FAILED:
                                strErrorID = "COPP_ERR_GET_CRL_FAILED";
                                break;
                        }
                        MessageBox.Show(strErrorID);
                    }
                    break;
                default:
                    return 0;
            }
            return 1;
        }

        public void GetMainWindowPictureBoxWidthAndHeight(ref int uWidth, ref int uHeight)
        {
            pictureBoxShowMain.PointToClient(pictureBoxShowMain.Location);
            uWidth = pictureBoxShowMain.Width;
            uHeight = pictureBoxShowMain.Height;
        }

        public void GetMainWindowPictureBoxRect(ref RECT rect)
        {
            rect.Left = pictureBoxShowMain.Left;
            rect.Right = pictureBoxShowMain.Right;
            rect.Top = pictureBoxShowMain.Top;
            rect.Bottom = pictureBoxShowMain.Bottom;
        }

        public IntPtr GetMainWindowPictureBoxHandle()
        {
            return pictureBoxShowMain.Handle;
        }



      
        private void DeviceSettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (m_ShowDeviceSetting == null)
            {
                m_ShowDeviceSetting = new DeviceSetting(this, m_DemoState, m_hCaptureDevice,m_uCaptureType,m_uCurrentCardType);
                m_ShowDeviceSetting.Owner = this;
            }
            m_ShowDeviceSetting.Show();
            m_ShowDeviceSetting.Focus();
        }

        private void PreviewSettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (m_ShowPreviewSetting == null)
            {
                m_ShowPreviewSetting = new PreviewSetting(this, m_DemoState, m_hCaptureDevice);
                m_ShowPreviewSetting.Owner = this;
            }
            m_ShowPreviewSetting.Show();
            m_ShowPreviewSetting.Focus();
        }

        private void VideoProcessMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (m_ShowVideoProcess == null)
            {
                m_ShowVideoProcess = new VideoProcess(this, m_DemoState, m_hCaptureDevice);
                m_ShowVideoProcess.Owner = this;
            }
            m_ShowVideoProcess.Show();
            m_ShowVideoProcess.Focus();
        }

       

        private void CaptureImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (m_ShowCaptureImage==null)
            {
                m_ShowCaptureImage = new CaptureImage(this, m_DemoState, m_hCaptureDevice);
                m_ShowCaptureImage.Owner = this;
            }
            m_ShowCaptureImage.Show();
            m_ShowCaptureImage.Focus();
        }

        private void RecordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (m_ShowRecord == null)
            {
                m_ShowRecord = new Record(m_uCurrentCardType, this, m_DemoState, m_hCaptureDevice);
                m_ShowRecord.Owner = this;
            }
            m_ShowRecord.Show();
            m_ShowRecord.Focus();
        }

        public void UpdateDemoState(DEMOSTATE DemoState, bool bSwitch) 
        {
            if (DemoState == DEMOSTATE.DEMO_STATE_STOP)
	        {
                m_DemoState = DEMOSTATE.DEMO_STATE_STOP;
	        }
            if (bSwitch)
            {
                m_DemoState = m_DemoState | DemoState;
                if (m_ShowDeviceSetting!=null)
                {
                    m_ShowDeviceSetting.UpdateDemoWindow(m_DemoState);
                }
                if (m_ShowPreviewSetting!=null)
                {
                    m_ShowPreviewSetting.UpdateDemoWindow(m_DemoState);
                }
                if (m_ShowCaptureImage != null)
                {
                    m_ShowCaptureImage.UpdateDemoWindow(m_DemoState);
                }
                if (m_ShowVideoProcess!=null)
                {
                    m_ShowVideoProcess.UpdateDemoWindow(m_DemoState);
                }
                if (m_ShowRecord!=null)
                {
                    m_ShowRecord.UpdateDemoWindow(m_DemoState);
                }
            }
            else 
            {
                m_DemoState = m_DemoState & (~DemoState);
                if (m_ShowDeviceSetting != null)
                {
                    m_ShowDeviceSetting.UpdateDemoWindow(m_DemoState);
                }
                if (m_ShowPreviewSetting != null)
                {
                    m_ShowPreviewSetting.UpdateDemoWindow(m_DemoState);
                }
                if (m_ShowCaptureImage != null)
                {
                    m_ShowCaptureImage.UpdateDemoWindow(m_DemoState);
                }
                if (m_ShowVideoProcess != null)
                {
                    m_ShowVideoProcess.UpdateDemoWindow(m_DemoState);
                }
             
                if (m_ShowRecord != null)
                {
                    m_ShowRecord.UpdateDemoWindow(m_DemoState);
                }
            }
        }



        private void DeleteFunctionWindows()
        {
            if (m_ShowDeviceSetting != null)
            {
                m_ShowDeviceSetting.m_bHide = false;
                m_ShowDeviceSetting.Close();
                m_ShowDeviceSetting = null;
            }
            if (m_ShowPreviewSetting != null)
            {
                m_ShowPreviewSetting.m_bHide = false;
                m_ShowPreviewSetting.Close();
                m_ShowPreviewSetting = null;
            }
            if (m_ShowVideoProcess != null)
            {
                m_ShowVideoProcess.m_bHide = false;
                m_ShowVideoProcess.Close();
                m_ShowVideoProcess = null;
            }
          
            if (m_ShowCaptureImage != null)
            {
                m_ShowCaptureImage.m_bHide = false;
                m_ShowCaptureImage.Close();
                m_ShowCaptureImage = null;
            }
            if (m_ShowRecord != null)
            {
                m_ShowRecord.m_bHide = false;
                m_ShowRecord.Close();
                m_ShowRecord = null;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            m_bRestore = !m_bRestore;
            if (m_bRestore)
            {
                uint a =WritePrivateProfileString("DemoSetting", "IsRestore", "YES", m_szFileName);
                IsRestore.IsChecked = true;
            }
            else
            {
                uint a=WritePrivateProfileString("DemoSetting", "IsRestore", "NO", m_szFileName);
                IsRestore.IsChecked = false;
            }
        }

        private void SaveSettingAsIni()
        {
            StringBuilder szValue = new StringBuilder("", 50);
            AVerCapAPI.AVerGetDeviceName((uint)m_iCurrentDeviceIndex, szValue);
            string szCurrName = szValue.ToString();
            szCurrName = szCurrName.Substring(szCurrName.IndexOf(':') + 1);


            string strDeviceType = m_uCaptureType.ToString();

            WritePrivateProfileString("DeviceName", "DeviceName", szCurrName, m_szFileName);
            WritePrivateProfileString("DemoSetting", "DeviceType", strDeviceType, m_szFileName);

            //get device setting
            uint dwVideoSource = 0;
            AVerCapAPI.AVerGetVideoSource(m_hCaptureDevice, ref dwVideoSource);
            string strVideoSource = dwVideoSource.ToString();
            WritePrivateProfileString("VideoSource", "VideoSource", strVideoSource, m_szFileName);

            uint dwAudioSource = 0;
            AVerCapAPI.AVerGetAudioSource(m_hCaptureDevice, ref dwAudioSource);
            string strAudioSource = dwAudioSource.ToString();
            WritePrivateProfileString("AudioSource", "AudioSource", strAudioSource, m_szFileName);

            uint dwVideoFormat = 0;
            AVerCapAPI.AVerGetVideoFormat(m_hCaptureDevice, ref dwVideoFormat);
            string strVideoFormat = dwVideoFormat.ToString();
            WritePrivateProfileString("VideoFormat", "VideoFormat", strVideoFormat, m_szFileName);

            uint dwFrameRate = 0;
            AVerCapAPI.AVerGetVideoInputFrameRate(m_hCaptureDevice, ref dwFrameRate);
            string strFrameRate = dwFrameRate.ToString();
            WritePrivateProfileString("VideoFrameRate", "VideoFrameRate", strFrameRate, m_szFileName);

            VIDEO_RESOLUTION VideoResolution = new VIDEO_RESOLUTION();
            VideoResolution.dwVersion = 1;
            AVerCapAPI.AVerGetVideoResolutionEx(m_hCaptureDevice, ref VideoResolution);
            string strResolution = VideoResolution.dwVideoResolution.ToString();
            WritePrivateProfileString("VideoResolution", "Resolution", strResolution, m_szFileName);

            if (VideoResolution.bCustom != 0)
            {
                WritePrivateProfileString("VideoResolution", "IsCustom", "YES", m_szFileName);
            }
            else
            {
                WritePrivateProfileString("VideoResolution", "IsCustom", "NO", m_szFileName);
            }
            string strResolutionWidth = VideoResolution.dwWidth.ToString();
            WritePrivateProfileString("VideoResolution", "ResolutionWidth", strResolutionWidth, m_szFileName);
            string strResolutionHeight = VideoResolution.dwHeight.ToString();
            WritePrivateProfileString("VideoResolution", "ResolutionHeight", strResolutionHeight, m_szFileName);
        }

        private void functionMenuItem_Click(object sender, RoutedEventArgs e)
        {

        } 
    }
}

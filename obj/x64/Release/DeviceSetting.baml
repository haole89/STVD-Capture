   M S B A M L   `   `   `  ���� [  WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms   AVerCapSDKDemo_WPF"   AVerCapSDKDemo.DeviceSetting   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Omy@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     -   $��DeviceSettingDlg��  � Title$  Device Setting��$	��502��$	��410�� � 
ResizeMode$ NoResize=�.��� .+��� -   $��tabControl1��$	��463��$
��Left=�$	��Top=�$	��384��.@��� -   $��VideoDevice��$��Video Device�� � DataContext �� .��� .+��� -   $��textBox_InputVideoInfo��$	��156��$
��Left=�$��	10,13,0,0q�$	��Top=�$	��354�� �� 
IsReadOnly . $ �� HorizontalScrollBarVisibility$ Hidden=�" �� VerticalScrollBarVisibility$ Visible=��� -   $��label1��$��Video Source:��$��28��$
��Left=�$��
19,176,0,0q�$	��Top=�$��90��$��Right=��� -   $��comboBox_VideoSource��$��23��$
��Left=�$��127,180,0,0q�$	��Top=�$	��213���� -   $��label2��$��Video Format:��$��28��$
��Left=�$��
19,210,0,0q�$	��Top=�$��90��$��Normal"�$��Right=�	� -   $��radioButton_NTSC��$
��NTSC��$��16��$
��Left=�$��127,215,0,0q�$	��Top=�	� -	   $��radioButton_PAL��$	��PAL��$��16��$
��Left=�$��224,215,0,0q�$	��Top=��� -
   $��label3��$��Resolution:��$��28��$
��Left=�$��
19,238,0,0q�$	��Top=�$��90��$��Right=��� -   $��comboBox_Resolution��$��23��$
��Left=�$��127,238,0,0q�$	��Top=�$	��213���� -   $��checkBox_Custom��$��Custom��$��16��$
��Left=�$��
56,278,0,0q�$	��Top=��� -   $��label4��$��Width:��$��28��$
��Left=�$��Right=�$��106,269,0,0q�$	��Top=�$��69���� -   $��label5��$��Height:��$��28��$
��Left=�$��Right=�$��106,298,0,0q�$	��Top=�$��69���� -   $��textBox_Width��$��23��$
��Left=�$��176,272,0,0q�$	��Top=�$��72���� -   $��textBox_Height��$��23��$
��Left=�$��176,301,0,0q�$	��Top=�$��72���� -   $��button_SetResolution��$	��Set��$��23��$
��Left=�$��279,274,0,0q�$	��Top=�$��61���� -   $��label6��$��Frame Rate:��$��28��$
��Left=�$��Right=�$��
19,332,0,0q�$	��Top=�$��90���� -   $��comboBox_FrameRate��$��23��$
��Left=�$��127,332,0,0q�$	��Top=�$	��213���� -   $��label7��$��Frame Rate:��$��28��$
��Left=�$��Right=�$��
19,366,0,0q�$	��Top=�$��90���� -   $��textBox_FrameRate��$��23��$
��Left=�$��127,371,0,0q�$	��Top=�$	��121���� -   $��button_SetFrameRate��$	��Set��$��23��$
��Left=�$��279,370,0,0q�$	��Top=�$��61���� -   $��AudioDevice��$��Audio Device��.���  � ColumnDefinitions �� $	��50* ��� $
��324* �.+��� -   $��label9��$��Audio Format:��$��28��$
��Left=�$��
32,198,0,0q�$	��Top=�$��Right=�$��1���� -   $��label10��$��Audio Source:��$��28��$
��Left=�$��	28,41,0,0q�$	��Top=�$��Right=�$��1���� -   $��label11��$��Audio Capture Source:��$��28��$
��Left=�$��
19,130,0,0q�$	��Top=�$	��155��$��Right=�$��2���� -   $��label12��$��Input Type:��$��28��$
��Left=�$��
49,164,0,0q�$	��Top=�$��Right=�$��1���� -   $��comboBox_AudioSource��$��23��$
��Left=�$��
141,41,0,0q�$	��Top=�$	��120��$��1���� -   $!��comboBox_AudioCaptureSource��$��23��$
��Left=�$��141,130,0,0q�$	��Top=�$	��120��$��1���� -   $��comboBox_AudioFormat��$��23��$
��Left=�$��141,198,0,0q�$	��Top=�$	��120��$��1���� -   $��comboBox_InputType��$��23��$
��Left=�$��141,164,0,0q�$	��Top=�$	��120��$��1���� -    $��checkBox_ThirdParty��$ ��Third-Party(Sound Card...)��$��16��$
��Left=�$��	9,108,0,0q�$	��Top=�$��2���� -!   $��button_SetThirdPart��$	��Set��$��23��$
��Left=�$��105,239,0,0q�$	��Top=�$��75��$��1���� -"   $��Miscellaneous��$��Miscellaneous��.��� .+��� -#   $��	groupBox1��$ ��Channels Of Embedded Audio��$	��112��$
��Left=�$��	22,16,0,0q�$	��Top=�$	��329��.��� .+��� -$   $��label13��$��	Channels:��$��28��$
��Left=�$��	21,29,0,0q�$	��Top=�$��68��$��Right=��� -%   $��comboBox_Channels��$��23��$
��Left=�$��
109,31,0,0q�$	��Top=�$	��158��.@��� .��1, 2�� .��3, 4�� .��5, 6�� .��7, 8�� .��9, 10�� .��11, 12�� .��13, 14�� .��15, 16
   M S B A M L   `   `   `  ����  AVerCapSDKDemo_WPF!    AVerCapSDKDemo.VideoProcess   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  VideoProcess��5   	   $	��640��6   $	��407��6+    � 
ResizeMode$ NoResize=�67   .��5      � .+�5   
   �� -   5        $��tabControl1��5   =   $	��596��6   $
��Left=�6"   $	��Top=�6P   $	��384��6h   .@�5      �� -   5        $��Common��5   &   $��Common��6   .��5      � .+�5	      �� -   5        $��
groupBox16��5	   q   $��De-Interlance:��6   $��72��67   $
��Left=�6C   $��	17,15,0,0q�6^   $	��Top=�6�   $	��345��6�   .��5
      � .+�5      �� -   5        $��label9��5   w   $��De-Interlance:��6$   $��28��6=   $
��Left=�6I   $��	62,11,0,0q�6d   $	��Top=�6�   $��Right=�6�   �� 5      -   5        $��comboBox_DeInterlance��5   b   $��23��6'   $
��Left=�63   $��
159,13,0,0q�6N   $	��Top=�6   $	��143��6�   .@�5   "   �� .��6/   None65   �� 5   "   .��6/   Weave66   �� 5   "   .��6/   Bob64   �� 5   "   .��6/   Blend66   5      5      5      �� 5      -   5        $��
groupBox17��5   t   $��Video Enhancement��6   $��74��6:   $
��Left=�6F   $��	17,93,0,0q�6a   $	��Top=�6�   $	��345��6�   .��5      � .+�5      �� -   5        $��label8��5   {   $��Video Enhancement:��6$   $��28��6A   $
��Left=�6M   $��	25,11,0,0q�6h   $	��Top=�6�   $��Right=�6�   �� 5      -	   5        $��comboBox_Enhancement��5   b   $��23��6'   $��Right=�63   $��	0,13,31,0q�6O   $	��Top=�6~   $	��143��6�   .@�5   "   �� .��6/   None65   �� 5   "   .��6/   Normal67   �� 5   "   .��6/   Split66   �� 5   "   .��6/   	Compare68   5      5      5      �� 5      -
   5        $��
groupBox18��5   s   $��Noise Reduction��6   $��78��68   $
��Left=�6D   $��
17,172,0,0q�6_   $	��Top=�6�   $	��345��6�   .��5       � .+�5!      �� -   5        $��label10��5!   y   $��Noise Reduction:��6$   $��28��6?   $
��Left=�6K   $��	45,15,0,0q�6f   $	��Top=�6�   $��Right=�6�   	� 5"      -   5        $��radioButton_NoiseEnable��5"   v   $��Enable��6*   $��16��6;   $
��Left=�6G   $��
159,21,0,0q�6b   $	��Top=�6�   6�   	� 5#      -   5        $��radioButton_NoiseDisable��5#   ]   $��Disable��6*   $��16��6<   $��244,21,14,0q�6H   $	��Top=�6}   6�   5$      5%      5&      5'      �� 5(      -   5        $
��Clip��5(   $   $
��Clip��6   .��5)      � .+�5*      �� -   5        $��	groupBox8��5*   m   $��
Clip Video��6   $��57��63   $
��Left=�6?   $��	53,38,0,0q�6Z   $	��Top=�6~   $	��270��6�   .��5+      � .+�5,      �� -   5        $��label11��5,   r   $��Clip Video:��6$   $��28��6:   $
��Left=�6F   $��9,6,0,0q�6a   $	��Top=�6�   	� 5-      -   5        $��radioButtonNO��5-   �   $��NO��6+   $��16��68   $
��Left=�6D    d� 	IsChecked$ FalseY�6_   $��	92,11,0,0q�6q   $	��Top=�6�   6�   	� 5.      -   5        $��radioButtonYES��5.   t   $	��YES��6+   $��16��69   $��Right=�6E   $��	0,11,69,0q�6a   $	��Top=�6�   6�   5/      50      �� 51      -   5        $��	groupBox9��51   p   $��Capture Area��6   $��98��65   $
��Left=�6A   $��
53,101,0,0q�6\   $	��Top=�6�   $	��270��6�   .��52      � .+�53      �� -   5        $��label12��53   l   $��Left:��6$   $��28��64   $
��Left=�6@   $��6,6,0,0q�6[   $	��Top=�6{   �� 54      -   5        $��textBoxLeft��54   `   $��23��6&   $
��Left=�62   $��48,8,0,0q�6N   $	��Top=�6s   $��74��6�   6�   �� 55      -   5        $��label13��55   m   $
��Top:��6$   $��28��63   $
��Left=�6?   $��	126,8,0,0q�6Z   $	��Top=�6|   �� 56      -   5        $��
textBoxTop��56   a   $��23��6&   $
��Left=�62   $��	178,8,0,0q�6N   $	��Top=�6s   $��74��6�   6�   �� 57      -   5        $��label14��57   n   $��Right:��6$   $��28��65   $
��Left=�6A   $��6,35,0,0q�6\   $	��Top=�6}   �� 58      -   5        $��textBoxRight��58   a   $��23��6&   $
��Left=�62   $��	48,37,0,0q�6N   $	��Top=�6u   $��74��6�   6�   �� 59      -   5        $��label15��59   q   $��Bottom:��6$   $��28��66   $
��Left=�6B   $��
126,35,0,0q�6]   $	��Top=�6�   �� 5:      -   5        $��textBoxBottom��5:   b   $��23��6&   $
��Left=�62   $��
178,35,0,0q�6N   $	��Top=�6w   $��74��6�   6�   5;      5<      �� 5=      -   5        $��button_SetClip��5=   g   $	��Set��6   $��23��6+   $
��Left=�67   $��145,224,0,0q�6R   $	��Top=�6}   $��75��6�   6�   5>      5?      �� 5@      -   5        $��	Downscale��5@   )   $��	Downscale��6   .��5A      � .+�5B      �� -   5        $��
groupBox10��5B   y   $��Down Scale Video Size��6   $	��292��6>   $
��Left=�6K   $��	62,48,0,0q�6f   $	��Top=�6�   $	��266��6�   .��5C      � .+�5D      	� -   5        $��radioButton_None��5D   s   $
��None��6*   $��16��69   $
��Left=�6E   $��	74,35,0,0q�6`   $	��Top=�6�   6�   	� 5E      -    5        $��radioButton_Custom��5E   v   $��Custom��6*   $��16��6;   $
��Left=�6G   $��
74,123,0,0q�6b   $	��Top=�6�   6�   	� 5F      -!   5        $��radioButton_HalfBoth��5F   y   $��	Half Both��6*   $��16��6>   $
��Left=�6J   $��
74,101,0,0q�6e   $	��Top=�6�   6�   	� 5G      -"   5        $��radioButton_HalfWidth��5G   y   $��
Half Width��6*   $��16��6?   $
��Left=�6K   $��	74,79,0,0q�6f   $	��Top=�6�   6�   	� 5H      -#   5        $��radioButton_HalfHeight��5H   z   $��Half Height��6*   $��16��6@   $
��Left=�6L   $��	74,57,0,0q�6g   $	��Top=�6�   6�   �� 5I      -$   5        $��label16��5I   q   $��Width:��6$   $��28��65   $
��Left=�6A   $��122,142,0,0q�6\   $	��Top=�6�   �� 5J      -%   5        $��label17��5J   r   $��Height:��6$   $��28��66   $
��Left=�6B   $��118,166,0,0q�6]   $	��Top=�6�   �� 5K      -&   5        $��textBox_Width��5K   b   $��23��6&   $
��Left=�62   $��173,144,0,0q�6M   $	��Top=�6w   $��50��6�   {�.  6�   �� 5L      -'   5        $��textBox_Height��5L   b   $��23��6&   $
��Left=�62   $��173,168,0,0q�6M   $	��Top=�6x   $��50��6�   {�.  6�   �� 5M      -(   5        $��button_SetDownscale��5M   n   $	��Set��6%   $��23��63   $
��Left=�6?   $��
74,213,0,0q�6Z   $	��Top=�6�   $��75��6�   6�   5N      5O      5P      5Q      �� 5R      -)   5        $��Rotate��5R   &   $��Rotate��6   .��5S      � .+�5T      �� -*   5        $��
groupBox11��5T   p   $��Rotate Video��6   $	��120��65   $
��Left=�6B   $��	39,90,0,0q�6]   $	��Top=�6�   $	��305��6�   .��5U      � .+�5V      �� -+   5        $��label18��5V   u   $��Rotate Type:��6$   $��28��6;   $
��Left=�6G   $��	18,37,0,0q�6b   $	��Top=�6�   �� 5W      -,   5        $��comboBox_RotateType��5W   b   $��23��6'   $
��Left=�63   $��
105,37,0,0q�6N   $	��Top=�6}   $	��181��6�   .@�5X   "   �� .��6/   NONE65   �� 5Y   "   .��6/   Clockwise 906=   �� 5Z   "   .��6/   Counter Clockwise 906E   5[      5\      5]      5^      5_      5`      5a      5b      
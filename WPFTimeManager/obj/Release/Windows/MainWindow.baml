   M S B A M L   `   `   `  ���� I  EWPFTimeManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:WPFTimeManagerWPFTimeManager  l hSystem.Windows.Controls.DataVisualization.Toolkit, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null�{clr-namespace:System.Windows.Controls.DataVisualization.Charting;assembly=System.Windows.Controls.DataVisualization.Toolkit2System.Windows.Controls.DataVisualization.Charting �rclr-namespace:System.Windows.Controls.DataVisualization;assembly=System.Windows.Controls.DataVisualization.Toolkit)System.Windows.Controls.DataVisualization     WPFTimeManager.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     .localclr-namespace:WPFTimeManager     Ddxc4http://schemas.devexpress.com/winfx/2008/xaml/charts     �chartingToolkit{clr-namespace:System.Windows.Controls.DataVisualization.Charting;assembly=System.Windows.Controls.DataVisualization.Toolkit     �datavisrclr-namespace:System.Windows.Controls.DataVisualization;assembly=System.Windows.Controls.DataVisualization.Toolkit       � Title$  
MainWindow��$	��721��$	��818�� � 
ResizeMode$ CanMinimize=� � WindowStyle$ 
ToolWindow=�
 � Top$ 0�� � Left$ 0�� � 	Resources "   WPFTimeManager.TimeConverter   TimeConverter��%   &	          .��� $
��Left=�$	��570��$	��Top=�$	��790�� � RowDefinitions �� $	��52* ��� $
��258* ��� $
��121* ��� $
��130* ��� $��9* �.+��� -    ��Name$ 
labelToday��$��За сегодня!��$
��Left=�$��10,0,0,0q�$	��Top=�$��26��$��80��&   System.Windows.Controls.DataGrid  -     Name$ dataGrid��$
��Left=�$��1��$	��Top=�$	��378��$	��800��	   SelectionMode$	 Single=�
   SelectionUnit$
 FullRow=�   AutoGenerateColumns .     CanUserAddRows .     CanUserDeleteRows .     	RowHeight$ 18Q�   
IsReadOnly . $��	0,0,-10,0q�   CanUserReorderColumns .     CanUserResizeColumns .  $��2������    Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Имя процесса��   MinWidth$	 612Q�   Width5  /System.Windows.Controls.DataGridLengthConverter$ SizeToHeader    MaxWidth$	 612Q�1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *	process+   	CellStyle ��  �� 
TargetType*  $System.Windows.Controls.DataGridCell"  .���  �� Property �@�ToolTip �� Value �� *	process+  $ Суммарно��$ 60Q�$ SizeToHeader $ 60Q� �� *sum+ �� StringFormat$ {0:00:00:00}��   SortMemberPath �� *sum+  $ Безделье��$ 60Q�$ SizeToHeader $ 60Q� �� *idle+$ {0:00:00:00}�� �� *idle+  $ В работе��$ 60Q�$ SizeToHeader $ 60Q� �� *work+$ {0:00:00:00}�� �� *work+   RowDetailsTemplate �� .R�  $ dataGridSon��$
��Left=�$��1��$	��Top=�$	��200��$	��740�� .   .   .  $y�	Collapsed=�$ 18Q� .    $ Имя процесса��$	 550Q�$ SizeToHeader $	 660Q� �� *
processSon+ �� "  .���  �@�ToolTip �� *
processSon+  $ Суммарно��$ 60Q�$ SizeToHeader $ 60Q� �� *sumSon+$ {0:00:00:00}�� �� *sumSon+  $ Безделье��$ 60Q�$ SizeToHeader $ 60Q� �� *	idleSon+$ {0:00:00:00}�� �� *	idleSon+  $ В работе��$ 60Q�$ SizeToHeader $ 60Q� �� *	workSon+$ {0:00:00:00}�� �� *	workSon+>  8System.Windows.Controls.DataVisualization.Charting.Chart  -     Name$ columnChart��$	��257��$
��Left=�$��
0,0,0,-123q�    Title$4  .Графики                                ��$��Bottom=�$	��610��$��3��$��2��   PieDataPointControlTemplate! �� 
TargetTypeE	  ?System.Windows.Controls.DataVisualization.Charting.PieDataPoint" F�Name# 	  Geometry$ ^� ToolTip% �� 	Converter& 	  FormattedRatio��%�   &	       7   �� "! 	 .R�� .+�F� $" Slice��#��z# #��z��#��z��$ �� .+��� ���� *Key+�� $2��,Время в формате ДД:ЧЧ:ММ:���� ���� *Value+8%   �� #��z& '   Series' B
  <System.Windows.Controls.DataVisualization.Charting.PieSeries
  -   ( 
 Name$( columnSeries��H  BSystem.Windows.Controls.DataVisualization.Charting.DataPointSeries)   DependentValuePath$) Value��*   IndependentValuePath$	* Key��+   IsSelectionEnabled+ . &   System.Windows.Media.TextOptions,   TextFormattingMode$, Display=�-   ItemsSource- �� . 
  Palette. L  FSystem.Windows.Controls.DataVisualization.ResourceDictionaryCollection    DataPointStyle�� %E   &	       7  �� " 	 .���  � ��
 �  ����  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 � � ���  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 ��  ���  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 �   ���  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 � �����  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 ��� ���  � ��8   �� %K   &	       7  �� " 	 .���  � �� �	#FFBB6A09��  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 �i����  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 �k�����  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 ��i����  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 �� K���  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 ��p����  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 ������  � ��8   �� %K   &	       7  �� " 	 .���  � �� �	#FF64702C��  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 � E����  � ��8   �� %K   &	       7  �� " 	 .���  � �� �	#FF591550��  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 ������  � ��8   �� %E   &	       7  �� " 	 .���  � ��
 ��2����  � ��8   �� -   / ��Name$/ 
buttonIdle��$��Безделья��$
��Left=�$��615,22,-10,0q�$��3��$	��Top=�$	��185��$��26���� -   $/ buttonActive��$��Активности��$
��Left=�$��615,47,-10,0q�$��3��$	��Top=�$	��185��$��26���� -   $/ 	buttonSum��$��Суммарный��$
��Left=�$��615,73,-10,0q�$��3��$	��Top=�$	��185��$��26���� -   $ label1��$$��Показать график:��$
��Left=�$��	649,0,0,0q�$��3��$	��Top=�$	��141��$��26���� -	   $ labelY��$��За вчера��$
��Left=�$��	119,0,0,0q�$	��Top=�$��26��$��72��
����  ��� -
   0 ��Name$0 comboBoxOneDay��$
��Left=�$��
615,14,0,0q�$	��Top=�$	��129��1 �� 
IsReadOnly1 . $��22��$y�Hidden=��� -   $ labelDay��$#��Конкретный день��$
��Left=�$��	503,8,0,0q�$	��Top=�$��32��$	��107��$y�Hidden=��� -   $ label��$��От:��$
��Left=�$��
224,10,0,0q�$	��Top=�$��32��$��26���� -   $ label2��$
��Left=�$��
360,10,0,0q�$	��Top=�$��32��$��27��$��До:��(  "System.Windows.Controls.DatePicker  -   2  Name$2 firstDatePicker��$
��Left=�$��
255,12,0,0q�$	��Top=�$��24��$	��100��  -   $2 secondDatePicker��$
��Left=�$��
392,12,0,0q�$	��Top=�$��24��$	��100���� -   $ 	labelWeek��$��За неделю��$
��Left=�$��	10,26,0,0q�$	��Top=�$��70��
����  �$��26���� -   $ 
labelMonth��$��За месяц��$
��Left=�$��
119,26,0,0q�$	��Top=�$��72��
����  �$��26���� -   $/ 	graphNext��$��>��$
��Left=�$��749,29,0,-51q�$��4��$	��Top=�$��30��$��31��$y�Hidden=��� -   $/ 	graphPrev��$��<��$
��Left=�$��614,29,0,-51q�$��4��$	��Top=�$��30��$��31��{�.  $y�Hidden=��� -   $ labelValueList��$��От * до *��$
��Left=�$��649,29,0,-51q�$��4��$	��Top=�$��31��$��95��$y�Hidden=��� -   $ 	labelHelp��$'��!LCTRL+RSHIFT+ALT+F11-скрыть��$
��Left=�$��615,91,-10,-123q�$	��Top=�$��4��$	��185��$��41���� -   $/ buttonRefresh��$��Center=�$��
758,12,6,0q�$	��Top=�$��26��$��22��$	��↺���� -   $/ buttonWorker��$<��6Просмотреть отчет сотрудника��$
��Left=�$��26��$��615,112,-10,0q�$��3��$	��Top=�$	��185��$��2���� -   $/ buttonClear��$��Очистить логи��$
��Left=�$��26��$��615,8,-10,-25q�$��4��$	��Top=�$	��185���� -   $/ buttonChangePassword��$!��Сменить пароль��$
��Left=�$��26��$��615,34,-10,-51q�$��4��$	��Top=�$	��185���� -   $/ buttonIdleTime��$0��*Изменить время простоя��$
��Left=�$��26��$��615,60,-10,-77q�$��4��$	��Top=�$	��185��
   M S B A M L   `   `   `  ����H  DSmartHouseWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:SmartHouseWPFSmartHouseWPF  5   	       SmartHouseWPF.LoginWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:SmartHouseWPF     5   	     � Title$   Вход в систему��5   	   $	��215��6    $��273.941��6-    � WindowStartupLocation$ CenterScreen=�6=    � 
ResizeMode$ NoResize=�6b   .��5	      � $��20,10q�6    � RowDefinitions 5
   
   �� 5      $
��auto �6   6,   �� 5      $
��auto �6   6,    � ColumnDefinitions 5   
   �� 5      $��1* �6   6,   �� 5      $��3* �6   6,   .+�5   
   � -   5         �Name$ grdFieldDataUser��5   0   $��2��6   $��0��6#    5      �� 5      $
��auto �6    60   �� 5      $
��auto �6    60   �� 5      $
��auto �6    60   �� 5      $
��auto �6    60   �� 5      $
��auto �6    60    5      �� 5      $��1* �6#   60   �� 5      $��3* �6#   60   .+�5      �� $��0��6   $��0��6!   $��Center=�61   .��6L   	ФИО:6R   �� 5       -   5         ��Name$ tbLogin��5    @   $��0��6   $��1��6#   $	��0,5q�63   6S   �� 5$      $��1��6   $��0��6!   $��Center=�61   .��6M   Пароль:6V   G� 5&      -   5         G�Name$ 
tbPassword��5&   E   $��1��6   $��1��6'   $	��0,5q�67   6[   �� 5(      $��2��6   $��0��6!   $��Center=�61   .��6L   Группа:6U   �� 5*      -   5         ��Name$ cbxUserGroup��5*      $��2��6-   $��1��6:   $	��0,5q�6J   6Z   �� 5,      -   5        	 ��Name$	 btnRegistration��5,   V   $��3��6   $��2��6"   $	��0,5q�66   
�������.��6�   &$Зарегистрироваться6�   5-      �� 5/   
   -   5        $	 btnLogin��5/   R   $��1��6   $��2��6   $	��0,5q�62   
�������.��6|   
Войти6�   5A      5B      
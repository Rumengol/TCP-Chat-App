   M S B A M L   `   `   `  ����H  DChatAppServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ChatAppServerChatAppServer  5   	   &     ChatAppServer.RoomCreationDialog   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:ChatAppServer     5   	   -   5          � Title$  Créer un salon��5   	    � SizeToContent$ WidthAndHeight=�6     � WindowStartupLocation$ Manual=�6?   .��5	      � $��15q�6    � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      .+�5   
   �� -   5         ��Name$ lblRoom��5      $��0��6!   $��0��61   .��6>   Entrez le nom du salon :6X   �� 5   
   -   5         ��Name$ txtRoom��5      $��1��6#   $��0��60   $	��175��6@   6Q   � 5   
   $��2��6   $��2��6!   $��Right=�65   $��0,15,0,0q�6Q   .+�5      �� -   5        $��btnDialogOk��5   &    �� 	IsDefault$
 True��6   $��30��6S   $��0,0,10,0q�6a   .��6s   _Ok6x   �� 5       �� IsCancel$
 True��6   $��50��6%   .��63   
_Annuler6=   5      5      5      
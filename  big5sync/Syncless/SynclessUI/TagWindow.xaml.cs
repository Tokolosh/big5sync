﻿using System;
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

namespace Syncless
{
    /// <summary>
    /// Interaction logic for CreateTagWindow.xaml
    /// </summary>
    public partial class TagWindow : Window
    {		
		private MainWindow _main;
        private string _selectedtype;
        
		public TagWindow(MainWindow main)
        {
            InitializeComponent();
			
			_main = main;
			_selectedtype = "";
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			string tagName = TxtBoxTagName.Text.Trim();
			
            if(tagName != "") {
                if(_selectedtype != "") {
					bool tagexists = false;
					
					if (_selectedtype == "File")
					{
						tagexists = _main.CreateFileTag(tagName);
					}
					else if (_selectedtype == "Folder")
					{
						tagexists = _main.CreateFolderTag(tagName);
					}
					
					if(tagexists) {
						string messageBoxText = "Please specify another tagname.";
						string caption = "Tag Already Exist";
						MessageBoxButton button = MessageBoxButton.OK;
						MessageBoxImage icon = MessageBoxImage.Error;
		
						MessageBox.Show(messageBoxText, caption, button, icon);
					} else {
						this.Close();
					}
				} else {
					string messageBoxText = "Please select a type.";
					string caption = "Tag Type Not Selected";
					MessageBoxButton button = MessageBoxButton.OK;
					MessageBoxImage icon = MessageBoxImage.Error;
	
					MessageBox.Show(messageBoxText, caption, button, icon);
				}
			} else {
                string messageBoxText = "Please specify a tagname.";
                string caption = "Tagname Empty";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
			}            
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	this.Close();
        }

		private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Keyboard.Focus(TxtBoxTagName);
		}

		private void BtnFile_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_selectedtype = "File";
		}

		private void BtnFolder_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_selectedtype = "Folder";
		}
    }
}

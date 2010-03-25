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
using System.IO;
using Syncless.Core;
using Microsoft.Windows.Controls;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Visitor;
using System.Data;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for PreviewSyncWindow.xaml
    /// </summary>
    public partial class PreviewSyncWindow : Window
    {		
		private MainWindow _main;
        
		public PreviewSyncWindow(MainWindow main, string _selectedtag)
        {
            InitializeComponent();
			
			_main = main;

            RootCompareObject rco = _main.gui.PreviewSync(_selectedtag);

            if (rco != null)
            {

            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			this.Close();
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
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
using SynclessUI.Visitor;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for PreviewSyncWindow.xaml
    /// </summary>
    public partial class PreviewSyncWindow : Window
    {		
		private MainWindow _main;
        private DataTable _previewSyncData;
        
		public PreviewSyncWindow(MainWindow main, string _selectedtag)
        {
			this.InitializeDataGrid();
            InitializeComponent();
			
			_main = main;

            RootCompareObject rco = _main.gui.PreviewSync(_selectedtag);

            if (rco != null)
            {
                // SyncUIHelper.TraverseFolderHelper(rco, new SyncerVisitor(request.Config, _previewSyncData));
            }
        }
		
		private void InitializeDataGrid() {
            _previewSyncData = new DataTable();
            _previewSyncData.Columns.Add(new DataColumn("Path1", typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn("Operation", typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn("Path2", typeof(string)));

            var row = _previewSyncData.NewRow();
            _previewSyncData.Rows.Add(row);
			row["Path1"] = "World Of Warcraft";
			row["Operation"] = "Blizzard";
			row["Path2"] = "Blizzard";

            row = _previewSyncData.NewRow();
            _previewSyncData.Rows.Add(row);
			row["Path1"] = "Halo 3";
			row["Operation"] = "Bungie";
			row["Path2"] = "Microsoft";

            row = _previewSyncData.NewRow();
            _previewSyncData.Rows.Add(row);
			row["Path1"] = "Gears Of War";
			row["Operation"] = "Epic";
			row["Path2"] = "Microsoft";
		
			InitializeComponent();
		}

		public DataTable PreviewSyncData
        { get { return _previewSyncData; } }
		
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
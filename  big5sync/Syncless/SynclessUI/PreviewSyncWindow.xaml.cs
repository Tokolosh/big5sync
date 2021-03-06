﻿/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Core.Exceptions;
using SynclessUI.Helper;
using SynclessUI.Visitor;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for PreviewSyncWindow.xaml
    /// </summary>
    public partial class PreviewSyncWindow : Window
    {
        private readonly MainWindow _main;
        private DataTable _previewSyncData;
        private readonly string _selectedTag;
        private BackgroundWorker _previewWorker;
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete

        /// <summary>
        /// The Preview Sync DataTable which will be used to populate with data from the PreviewVisitor
        /// </summary>
        public DataTable PreviewSyncData
        {
            get { return _previewSyncData; }
        }

        /// <summary>
        /// Initializes the PreviewSyncWindow
        /// </summary>
        /// <param name="main">Reference to the Main Window</param>
        /// <param name="selectedTag">Tagname to preview</param>
        public PreviewSyncWindow(MainWindow main, string selectedTag)
        {
            _selectedTag = selectedTag;

            InitializePreviewSyncDataTable();

            _main = main;
            Owner = _main;
            ShowInTaskbar = false;

            InitializePreviewWorker(selectedTag);

            InitializeComponent();
        }

        private void InitializePreviewWorker(string selectedTag)
        {
            _previewWorker = new BackgroundWorker();
            _previewWorker.DoWork += _previewWorker_DoWork;
            _previewWorker.RunWorkerCompleted += _previewWorker_RunWorkerCompleted;
            _previewWorker.WorkerSupportsCancellation = true;
            _previewWorker.RunWorkerAsync(selectedTag);
        }

        private void InitializePreviewSyncDataTable()
        {
            _previewSyncData = new DataTable();
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Source, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Operation, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Dest, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Tooltip, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.SourceIcon, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.DestIcon, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.SourceLastModifiedDate, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.SourceLastModifiedTime, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.SourceSize, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.DestLastModifiedDate, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.DestLastModifiedTime, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.DestSize, typeof(string)));
        }

        private void _previewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                RootCompareObject rco = e.Result as RootCompareObject;
                if (rco != null)
                {
                    
                    ProgressBarAnalyzing.Foreground = (Brush) ProgressBarAnalyzing.Resources["GreenColor"];
                    ProgressBarAnalyzing.Value = 100;
                    ProgressBarAnalyzing.IsIndeterminate = false;
                    LblProgress.Content = "Analyzing Completed!";
					LblChanges.Visibility = Visibility.Visible;
                    LblCancel.Content = "Close";
                }
            }
        }

        private void _previewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string selectedTag = e.Argument as string;
                RootCompareObject rco = _main.LogicLayer.PreviewSync(selectedTag);
                if (_previewWorker.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Result = rco;
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => Populate(rco)));
                }
            } catch(UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        private void Populate(RootCompareObject rco)
        {
            try
            {
                _previewSyncData.Rows.Clear();

                PreviewVisitor visitor = new PreviewVisitor(_previewSyncData);

                if (rco != null)
                {
                    SyncUIHelper.TraverseFolderHelper(rco, visitor);
                }

                _previewSyncData.AcceptChanges();
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _main.LogicLayer.CancelPreview(_selectedTag);
            _previewWorker.CancelAsync();
            BtnClose.IsEnabled = false;
            Close();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            _closingAnimationNotCompleted = false;
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_closingAnimationNotCompleted)
            {
                BtnClose.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock pathBox = (TextBlock) sender;
            string path = pathBox.Text;

            if (CheckPathExist(path))
            {
                try
                {
                    ProcessStartInfo openWithInfo = new ProcessStartInfo(path);
                    openWithInfo.ErrorDialog = true;
                    Process.Start(openWithInfo);
                }
                catch (Win32Exception)
                {}
            }
            else
                DialogHelper.ShowError(this, "Error Opening File/Folder", "The file/folder may not exist.");

            e.Handled = true;
        }

        private bool CheckPathExist(string path)
        {
            bool exists = false;
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);

                if (di.Exists)
                    exists = true;
            } catch {} // Do nothing since it will return false, and the desired behavior is to return false

            try
            {
                FileInfo fi = new FileInfo(path);

                if (fi.Exists)
                    exists = true;
            } catch { } // Do nothing since it will return false, and the desired behavior is to return false

            return exists;
        }
    }
}
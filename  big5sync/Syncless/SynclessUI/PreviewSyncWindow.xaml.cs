﻿using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
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
        private readonly DataTable _previewSyncData;
        private readonly string _selectedTag;
        private BackgroundWorker _previewWorker;
        private RootCompareObject _rco;
        private bool _closingAnimationNotCompleted = true;

        public PreviewSyncWindow(MainWindow main, string selectedTag)
        {
            _selectedTag = selectedTag;

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
            _main = main;
            Owner = _main;
            ShowInTaskbar = false;

            
            //Populate(_main.Gui.PreviewSync(selectedTag));

            _previewWorker = new BackgroundWorker();
            _previewWorker.DoWork += _previewWorker_DoWork;
            _previewWorker.RunWorkerCompleted += _previewWorker_RunWorkerCompleted;
            _previewWorker.WorkerSupportsCancellation = true;
            _previewWorker.RunWorkerAsync(selectedTag);

            InitializeComponent();
        }

        void _previewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                RootCompareObject rco = e.Result as RootCompareObject;
                if (rco != null)
                {
                    Populate(rco);
                    ProgressBarAnalyzing.Foreground = (Brush) ProgressBarAnalyzing.Resources["GreenColor"];
                    ProgressBarAnalyzing.Value = 100;
                    ProgressBarAnalyzing.IsIndeterminate = false;
                    LblProgress.Content = "Analyzing Completed!";
                    LblCancel.Content = "Close";
                }
            }
        }

        void _previewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string selectedTag = e.Argument as string;
            RootCompareObject rco = _main.Gui.PreviewSync(selectedTag);
            if (_previewWorker.CancellationPending)
            {
                e.Cancel = true;
                rco = null;
            }
            e.Result = rco;
        }

        public DataTable PreviewSyncData
        {
            get { return _previewSyncData; }
        }

        private void Populate(RootCompareObject rco)
        {
            try
            {
                _previewSyncData.Rows.Clear();

                var visitor = new PreviewVisitor(_previewSyncData);

                if (rco != null)
                {
                    SyncUIHelper.TraverseFolderHelper(rco, visitor);
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        public void Test(IAsyncResult result)
        {
            Console.WriteLine("Call back Hit");
            Console.WriteLine(result.CompletedSynchronously);
            Console.WriteLine(result.IsCompleted);
            Console.WriteLine(result.AsyncWaitHandle);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _main.Gui.CancelPreview(_selectedTag);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closingAnimationNotCompleted)
            {
                BtnClose.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }

        private void Path_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			TextBlock pathBox = (TextBlock) sender;

            string path = pathBox.Text;

            bool exists = false;

            try
            {
                DirectoryInfo di = new DirectoryInfo(path);

                if (di.Exists)
                    exists = true;
            }
            catch
            {

            }

            try
            {
                FileInfo fi = new FileInfo(path);

                if (fi.Exists)
                    exists = true;
            }
            catch
            {

            }

            if (exists)
                Process.Start(new ProcessStartInfo(path));
            else
                DialogHelper.ShowError(this, "Error Opening File/Folder", "The file/folder may not exist.");

            e.Handled = true;
        }

        #region Nested type: PreviewSyncDelegate

        private delegate RootCompareObject PreviewSyncDelegate(string tagname);

        #endregion

        #region Nested type: UpdateDelegate

        private delegate void UpdateDelegate();

        #endregion
    }
}
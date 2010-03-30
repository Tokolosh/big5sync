﻿using System.Windows;

namespace SynclessUI.Helper
{
    public static class DialogHelper
    {
        public static void ShowError(string caption, string message)
        {
            var dw = new DialogWindow(caption, message, DialogType.Error);
            dw.ShowDialog();
        }

        public static void ShowInformation(string caption, string message)
        {
            var dw = new DialogWindow(caption, message, DialogType.Information);
            dw.ShowDialog();
        }

        public static bool ShowWarning(string caption, string message)
        {
            var dw = new DialogWindow(caption, message, DialogType.Warning);
            dw.ShowDialog();

            return (bool) Application.Current.Properties["DialogWindowChoice"];
        }

        public static DialogWindow ShowIndeterminate(string caption, string message)
        {
            var dw = new DialogWindow(caption, message, DialogType.Indeterminate);
            return dw;
        }

        public static void DisplayUnhandledExceptionMessage()
        {
            ShowError("Unexpected Error",
                      "An unexpected error has occured. \n\nPlease help us by - \n 1. Submitting the debug.log in your Syncless Application Folder\\log to big5.syncless@gmail.com \n 2. Raise it as an issue on our GCPH @ http://code.google.com/p/big5sync/issues/list\n\n Please restart Syncless.");
        }
    }
}
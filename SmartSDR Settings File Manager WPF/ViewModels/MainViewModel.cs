/* ======================================================================================
    Developed by T. Ellison
    Copyright 2026. BDNI Consulting.  All Rights Reserved.
 ====================================================================================== */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace SmartSDR_Settings_File_Manager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private sealed record AppConfig(
        string CfgFile,
        string BackupFile,
        string ProcessName,
        string RestoreDialogTitle);

    // ── Per-app descriptors ──────────────────────────────────────────────────────────────

    private static readonly AppConfig Ssdr = new(
        "SSDR.settings", "SSDR.settings.backup", "SmartSDR",
        "Select a Backed Up SmartSDR for Windows settings file to Restore");

    private static readonly AppConfig Cat = new(
        "CAT.settings", "CAT.settings.backup", "Cat",
        "Select a Backed Up SmartSDR CAT settings file to Restore");

    private static readonly AppConfig Dax = new(
        "DAX.settings", "DAX.settings.backup", "DAX",
        "Select a Backed Up SmartSDR DAX settings file to Restore");

    // ── Folder paths ─────────────────────────────────────────────────────────────────────

    private static readonly string FlexRadioFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FlexRadio Systems");

    private static readonly string BackupFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SmartSDR Settings Backup");

    // ── Cached statics ───────────────────────────────────────────────────────────────────

    private static readonly string AppVersion =
        $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "2.0"}";

    private static readonly Regex PrefixRegex = new(@"^[0-9A-Z\-_]*$", RegexOptions.IgnoreCase);

    // ── Observable properties ────────────────────────────────────────────────────────────

    [ObservableProperty]
    private string _statusMessage = " Ready ";

    [ObservableProperty]
    private Brush _statusColor = Brushes.Black;

    [ObservableProperty]
    private string _backupFilePrefix = string.Empty;

    partial void OnBackupFilePrefixChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            SetStatus(" Ready ");
        else if (PrefixRegex.IsMatch(value))
            SetStatus(" Prefix name accepted ");
        else
            SetStatus(" ERROR: Only numbers, letters, underscores and dashes are permitted ", 2);
    }

    // =======================================================================================
    // SSDR Commands
    // =======================================================================================

    [RelayCommand] private void SaveSsdr()    => ExecuteSave(Ssdr);
    [RelayCommand] private void RestoreSsdr() => ExecuteRestore(Ssdr);
    [RelayCommand] private void ResetSsdr()   => ExecuteReset(Ssdr);

    // =======================================================================================
    // CAT Commands
    // =======================================================================================

    [RelayCommand] private void SaveCat()    => ExecuteSave(Cat);
    [RelayCommand] private void RestoreCat() => ExecuteRestore(Cat);
    [RelayCommand] private void ResetCat()   => ExecuteReset(Cat);

    // =======================================================================================
    // DAX Commands
    // =======================================================================================

    [RelayCommand] private void SaveDax()    => ExecuteSave(Dax);
    [RelayCommand] private void RestoreDax() => ExecuteRestore(Dax);
    [RelayCommand] private void ResetDax()   => ExecuteReset(Dax);

    // =======================================================================================
    // Misc Commands
    // =======================================================================================

    [RelayCommand]
    private void CleanUp() => RemoveBackupFiles();

    [RelayCommand]
    private void ViewBackupFiles()
    {
        if (!Directory.Exists(BackupFolder))
        {
            SetStatus(" Warning: Backup Folder does not exist — save a settings file first ", 1);
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = BackupFolder,
            UseShellExecute = true
        });

        SetStatus($" Opened backup folder: {BackupFolder} ");
    }

    [RelayCommand]
    private void Export()
    {
        SetStatus(" Export Backup Folder to a different location or drive");

        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Select a destination folder or drive to export settings files to.",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog(Application.Current.MainWindow) == true)
            ExportBackupFiles(dialog.SelectedPath);
        else
            SetStatus(" Backup Folder Export cancelled", 1);
    }

    [RelayCommand]
    private void Import()
    {
        SetStatus(" Import Backup Folder from a different location or drive");

        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Select the folder containing previously exported settings files.",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
            SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog(Application.Current.MainWindow) == true)
            ImportBackupFiles(dialog.SelectedPath);
        else
            SetStatus(" Backup Folder Import cancelled", 1);
    }

    // =======================================================================================
    // Menu info commands
    // =======================================================================================

    [RelayCommand]
    private void ShowAbout()
    {
        SetStatus(" Showing What is this? information");
        string caption = $"What is this? — SmartSDR Settings File Manager {AppVersion}";
        string message =
            "SmartSDR Settings File Manager backs up, restores, and resets the settings files used by SmartSDR, CAT, and DAX.\n\n" +
            "Saved files are stored in the SmartSDR Settings Backup folder inside your My Documents folder. " +
            "Settings can be restored from that folder if a configuration becomes corrupted or needs to be rolled back. " +
            "Resetting a settings file deletes it so the application recreates it with factory defaults on the next launch.\n\n" +
            "Check the status bar at the bottom of the window for feedback after each operation.\n\n" +
            "SmartSDR is a registered trademark of FlexRadio, Inc.";
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        SetStatus(" Ready ");
    }

    [RelayCommand]
    private void ShowChangeLog()
    {
        SetStatus(" Showing Change Log information");
        string caption = $"Change Log: SmartSDR Settings File Manager - Version: {AppVersion}";
        string message =
            "v3.0.0 (18-May-2026): Programmer: T.Ellison\n" +
            "Converted program to WPF from WinForms to modernize the UI. Now requires .NET v8. General clean up and maintenance.\n\n" +
            "v2.0.0 (3-Jul-2024): Programmer: T.Ellison\n" +
            "Added Backup Folder Import/Export feature.\n" +
            "Settings File Backup folder moved to My Documents.\n\n" +
            "v1.8.0 (14-Jun-2024): Programmer: T.Ellison\n" +
            "Final version of v1 release. Core functionality complete.";
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        SetStatus(" Ready ");
    }

    [RelayCommand]
    private void ShowWarranty()
    {
        SetStatus(" Showing Warranty information");
        string caption = $"Warranty: SmartSDR Settings File Manager - Version: {AppVersion}";
        string message =
            "This software is provided as-is, without any express or implied warranty, and without support of any kind.\n\n" +
            "BDNI Consulting, FlexRadio, Inc., and any contributing programmers are not liable for any damages or consequences " +
            "arising from the use of this application.\n\n" +
            "Copyright 2026. BDNI Consulting. All Rights Reserved.";
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        SetStatus(" Ready ");
    }

    [RelayCommand]
    private void ShowHelp()
    {
        SetStatus(" Showing Help information");
        string caption = $"Help: SmartSDR Settings File Manager - Version: {AppVersion}";
        string message =
            "SAVE\n" +
            "Copies the current settings file for SmartSDR, DAX, or CAT to your SmartSDR Settings Backup folder in My Documents. " +
            "Use the File Prefix field to tag the filename — useful when moving settings to another PC.\n\n" +
            "RESTORE\n" +
            "Opens a file picker so you can select a previously saved settings file and restore it to the FlexRadio %APPDATA% folder. " +
            "Use this to recover from a corrupted or misconfigured settings file. The existing settings file will be overwritten.\n\n" +
            "RESET\n" +
            "Deletes the active settings file for the selected application (not the backup). " +
            "The next time the application starts, it will create a fresh default settings file automatically.\n\n" +
            "CLEAN UP\n" +
            "Removes all files from the SmartSDR Settings Backup folder. This action cannot be undone.\n\n" +
            "FILE PREFIX\n" +
            "An optional label prepended to the filename when saving — for example, entering HomeShack saves the file as HomeShack.SSDR.settings. " +
            "Leave blank for standard filenames. Allowed characters: letters, numbers, hyphens, and underscores.\n\n" +
            "FILES > EXPORT\n" +
            "Copies all files from the Backup folder to a location you choose — such as a USB drive — for off-machine storage.\n\n" +
            "FILES > IMPORT\n" +
            "Copies settings files from a previously exported folder back into the SmartSDR Settings Backup folder.";
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        SetStatus(" Ready ");
    }

    // =======================================================================================
    // Operation dispatchers — apply process guard then delegate to the core method
    // =======================================================================================

    private void ExecuteSave(AppConfig app)
    {
        SetStatus(string.Empty);
        if (IsAppRunning(app, "SAVING")) return;

        string prefixName = string.IsNullOrEmpty(BackupFilePrefix)
            ? app.CfgFile
            : $"{BackupFilePrefix}.{app.CfgFile}";

        SaveFile(app.CfgFile, prefixName);
    }

    private void ExecuteRestore(AppConfig app)
    {
        SetStatus(string.Empty);
        if (IsAppRunning(app, "RESTORING")) return;

        var ofd = new OpenFileDialog
        {
            Title = app.RestoreDialogTitle,
            InitialDirectory = BackupFolder,
            DefaultExt = "settings",
            CheckFileExists = true,
            CheckPathExists = true,
            Filter = $"settings files (*{app.CfgFile})|*{app.CfgFile}",
            FilterIndex = 1,
            RestoreDirectory = true
        };

        if (ofd.ShowDialog() != true) return;

        RestoreFile(app.CfgFile, ofd.FileName, app.BackupFile);
    }

    private void ExecuteReset(AppConfig app)
    {
        SetStatus(string.Empty);
        if (IsAppRunning(app, "RESETTING")) return;

        ResetFile(app.CfgFile, app.BackupFile);
    }

    // =======================================================================================
    // Core file operation methods
    // =======================================================================================

    private bool SaveFile(string cfgFile, string prefixName)
    {
        try { Directory.CreateDirectory(BackupFolder); }
        catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }

        string sourcePath = Path.Combine(FlexRadioFolder, cfgFile);
        string destPath = Path.Combine(BackupFolder, prefixName);

        if (!File.Exists(sourcePath))
        {
            SetStatus($" ERROR: Source {cfgFile} file does not exist! Restore or Reset the {cfgFile} file ", 2);
            return false;
        }

        if (File.Exists(destPath))
        {
            string caption = "Backup Settings File Overwrite Confirmation";
            string message = $" The {prefixName} backup file will be overwritten!\n\nAre you certain you want to overwrite the backup settings file?";
            SetStatus($" {caption} ");

            if (MessageBox.Show(message, caption, MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                SetStatus(" File SAVE operation terminated by the user ", 1);
                return false;
            }
        }

        try
        {
            File.Copy(sourcePath, destPath, true);
            SetStatus($" Source {cfgFile} file successfully saved as {prefixName} ");
            return true;
        }
        catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }
    }

    private bool RestoreFile(string cfgFile, string selectedFileName, string cfgFileBackup)
    {
        string targetPath = Path.Combine(FlexRadioFolder, cfgFile);
        string backupSiblingPath = Path.Combine(FlexRadioFolder, cfgFileBackup);

        if (File.Exists(targetPath))
        {
            try { File.SetAttributes(targetPath, FileAttributes.Normal); }
            catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }
        }

        try { File.Copy(selectedFileName, targetPath, true); }
        catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }

        if (File.Exists(backupSiblingPath))
        {
            try
            {
                File.SetAttributes(backupSiblingPath, FileAttributes.Normal);
                File.Delete(backupSiblingPath);
            }
            catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }
        }

        SetStatus($" Backup {Path.GetFileName(selectedFileName)} file successfully restored as {cfgFile} ");
        return true;
    }

    private bool ResetFile(string cfgFile, string cfgFileBackup)
    {
        string filePath = Path.Combine(FlexRadioFolder, cfgFile);
        string backupSiblingPath = Path.Combine(FlexRadioFolder, cfgFileBackup);

        if (!File.Exists(filePath))
        {
            SetStatus($" The {cfgFile} was previously reset ", 1);
            ShowRestartDialog(cfgFile);
            return true;
        }

        if (!ConfirmFileDelete(cfgFile, false))
        {
            SetStatus($" The {cfgFile} file reset was terminated by the user ", 1);
            return false;
        }

        try
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
            File.Delete(filePath);

            if (File.Exists(backupSiblingPath))
            {
                File.SetAttributes(backupSiblingPath, FileAttributes.Normal);
                File.Delete(backupSiblingPath);
            }

            SetStatus($" The {cfgFile} file was successfully reset ", 1);
            ShowRestartDialog(cfgFile);
            return true;
        }
        catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }
    }

    private bool RemoveBackupFiles()
    {
        if (!Directory.Exists(BackupFolder))
        {
            SetStatus(" Warning: Backup Folder does not exist ", 1);
            return false;
        }

        if (!ConfirmFileDelete(null, true))
        {
            SetStatus(" Clean Up terminated by the user ", 1);
            return false;
        }

        foreach (string file in Directory.GetFiles(BackupFolder))
        {
            try
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }
        }

        SetStatus(" Backup Settings files were successfully removed ");
        return true;
    }

    private bool ExportBackupFiles(string targetFolder)
    {
        if (!Directory.Exists(BackupFolder))
        {
            SetStatus(" Warning: Backup Folder does not exist ", 1);
            return false;
        }

        if (BackupFolder.Equals(targetFolder, StringComparison.OrdinalIgnoreCase))
        {
            SetStatus(" ERROR: Export Folder can not be the Backup Folder! ", 2);
            return false;
        }

        if (Directory.GetFiles(BackupFolder, "*.settings").Length == 0)
        {
            SetStatus(" Warning: Backup Folder does not contain any files ", 1);
            return false;
        }

        if (new DirectoryInfo(targetFolder).Parent == null)
        {
            targetFolder = Path.Combine(targetFolder, "SmartSDR Settings File Export");
            try { Directory.CreateDirectory(targetFolder); }
            catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }
        }

        try
        {
            foreach (string file in Directory.GetFiles(BackupFolder))
            {
                File.SetAttributes(file, FileAttributes.Normal);

                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetFolder, fileName);

                if (File.Exists(destFile))
                {
                    string caption = "Backup Settings File Export Overwrite Confirmation";
                    string message = $" The exported {fileName} backup file will be overwritten!\n\nAre you certain you want to overwrite the exported backup settings file?";
                    SetStatus($" {caption}");

                    if (MessageBox.Show(message, caption, MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        SetStatus($" Export of {fileName} skipped", 1);
                        continue;
                    }

                    File.SetAttributes(destFile, FileAttributes.Normal);
                }

                File.Copy(file, destFile, true);
            }
        }
        catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }

        SetStatus($" Settings file Backup Folder Exported to: {targetFolder}");
        return true;
    }

    private bool ImportBackupFiles(string targetFolder)
    {
        try { Directory.CreateDirectory(BackupFolder); }
        catch (Exception e) { SetStatus($" ERROR Creating Settings Backup folder: {e.Message}", 2); return false; }

        if (BackupFolder.Equals(targetFolder, StringComparison.OrdinalIgnoreCase))
        {
            SetStatus(" ERROR: Import Folder can not be the Backup Folder! ", 2);
            return false;
        }

        if (Directory.GetFiles(targetFolder, "*.settings").Length == 0)
        {
            SetStatus(" Warning: Import Folder does not contain any settings files ", 1);
            return false;
        }

        try
        {
            foreach (string file in Directory.GetFiles(targetFolder))
            {
                if (Path.GetExtension(file) != ".settings") continue;

                File.SetAttributes(file, FileAttributes.Normal);

                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(BackupFolder, fileName);

                if (File.Exists(destFile))
                {
                    string caption = "Backup Settings File Import Overwrite Confirmation";
                    string message = $" The imported {fileName} backup file will be overwritten!\n\nAre you certain you want to overwrite the imported backup settings file?";
                    SetStatus($" {caption}");

                    if (MessageBox.Show(message, caption, MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        SetStatus($" Import of {fileName} skipped", 1);
                        continue;
                    }

                    File.SetAttributes(destFile, FileAttributes.Normal);
                }

                File.Copy(file, destFile, true);
            }
        }
        catch (Exception e) { SetStatus($" ERROR: {e.Message}", 2); return false; }

        SetStatus($" Settings file Backup Folder Imported to {targetFolder}");
        return true;
    }

    // =======================================================================================
    // Shared dialog helpers
    // =======================================================================================

    private bool ConfirmFileDelete(string? cfgFile, bool deleteAll)
    {
        string msgText1 = deleteAll ? "All backed up settings files" : $"The {cfgFile} file";
        string msgText2 = deleteAll ? "all of the backed up settings files?" : $"the {cfgFile} file?";
        string caption = "Settings File Delete Confirmation";
        string message = $"{msgText1} will be deleted!\n\nAre you certain you want to delete {msgText2}";

        SetStatus($" {caption} ");

        return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
            == MessageBoxResult.Yes;
    }

    private void ShowRestartDialog(string cfgFile)
    {
        string appName = FriendlyAppName(cfgFile);
        SetStatus($" Restart the {appName} application ");

        string caption = $"{cfgFile} Config File Reset: Final Step";
        string message = $"The {cfgFile} file has been reset.\n\nRestart the {appName} application to create a new default {cfgFile} file.";
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private bool IsAppRunning(AppConfig app, string fileOperation)
    {
        Process[] processes = Process.GetProcessesByName(app.ProcessName);
        bool running = processes.Length > 0;
        foreach (Process p in processes) p.Dispose();

        if (!running) return false;

        string appName = FriendlyAppName(app.CfgFile);
        string caption = $"{appName} is Running!";
        string message = $"{appName} is running.\n\nWARNING: You must close {appName} before {fileOperation} the {app.CfgFile}.";

        SetStatus($" {caption} ", 1);
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        return true;
    }

    private static string FriendlyAppName(string cfgFile) =>
        Path.GetFileNameWithoutExtension(cfgFile) switch
        {
            "SSDR" => "SmartSDR for Windows",
            var name => name
        };

    private void SetStatus(string message, int level = 0)
    {
        StatusMessage = message;
        StatusColor = level switch
        {
            1 => Brushes.Blue,
            2 => Brushes.Red,
            _ => Brushes.Black
        };
    }
}

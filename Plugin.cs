using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about;

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            try
            {
                about = new PluginInfo();
                mbApiInterface = new MusicBeeApiInterface();
                mbApiInterface.Initialise(apiInterfacePtr);

                about.PluginInfoVersion = 1;
                about.Name = "MusicBeeQuickTag";
                about.Description = Localization.Get("PluginDescription");
                about.Author = "Stargazer-cc";
                about.TargetApplication = "";
                about.Type = PluginType.General;
                about.VersionMajor = 2;
                about.VersionMinor = 1;
                about.Revision = 3;
                about.MinInterfaceVersion = 20;
                about.MinApiRevision = 35;
                about.ReceiveNotifications = (ReceiveNotificationFlags.StartupOnly);
                about.ConfigurationPanelHeight = 0;

                return about;
            }
            catch (Exception ex)
            {
                // Try to log to a file
                try
                {
                    string logPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MB_ErrorLog.txt");
                    System.IO.File.WriteAllText(logPath, "Initialise Error: " + ex.ToString());
                }
                catch { }
                throw; // Re-throw to show error in MB
            }
        }

        public bool Configure(IntPtr panelHandle)
        {
            // Show settings form
            string storagePath = mbApiInterface.Setting_GetPersistentStoragePath();
            List<MetaDataType> currentFields = SettingsForm.LoadSettings(storagePath);
            
            using (SettingsForm form = new SettingsForm(mbApiInterface, storagePath, currentFields))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show(Localization.Get("SettingsSaved"), 
                        Localization.Get("PluginTitle"), 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                    return true;
                }
            }
            return false;
        }

        public void SaveSettings()
        {
        }

        public void Close(PluginCloseReason reason)
        {
        }

        public void Uninstall()
        {
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.PluginStartup:
                    mbApiInterface.MB_AddMenuItem("mnuTools/MusicBeeQuickTag", null, OnQuickTagBrowser);
                    mbApiInterface.MB_RegisterCommand("MusicBeeQuickTag: Open Browser", OnQuickTagBrowser);
                    break;
            }
        }

        private void OnQuickTagBrowser(object sender, EventArgs e)
        {
            try
            {
                // Get selected files
                string[] selectedFiles;
                mbApiInterface.Library_QueryFilesEx("domain=SelectedFiles", out selectedFiles);

                // Load field settings
                string storagePath = mbApiInterface.Setting_GetPersistentStoragePath();
                List<MetaDataType> fieldsToScan = SettingsForm.LoadSettings(storagePath);
                string presetsPath = System.IO.Path.Combine(storagePath, "MusicBeeQuickTag_Presets.txt");
                List<PresetGroup> presets = PresetManager.Load(presetsPath);

                // Show browser form (modeless)
                TagBrowserForm form = new TagBrowserForm(mbApiInterface, selectedFiles, fieldsToScan, presets);
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localization.Get("Error") + ": " + ex.Message + "\n\n" + ex.StackTrace, 
                    Localization.Get("PluginTitle"), 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
}

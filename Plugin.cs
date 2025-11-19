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
        private PluginInfo about = new PluginInfo();

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);

            about.PluginInfoVersion = 1;
            about.Name = "Quick Tag Browser";
            about.Description = "浏览并快速应用音乐库中已存在的标签值";
            about.Author = "Antigravity";
            about.TargetApplication = "";
            about.Type = PluginType.General;
            about.VersionMajor = 1;
            about.VersionMinor = 3;
            about.Revision = 0;
            about.MinInterfaceVersion = 20;
            about.MinApiRevision = 25;
            about.ReceiveNotifications = (ReceiveNotificationFlags.StartupOnly);
            about.ConfigurationPanelHeight = 0;

            return about;
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
                    MessageBox.Show("设置已保存！下次打开 Tag Browser 时生效。", 
                        "Quick Tag Browser", 
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
                    mbApiInterface.MB_AddMenuItem("mnuTools/Quick Tag Browser", null, OnQuickTagBrowser);
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

                // Show browser form (modeless)
                TagBrowserForm form = new TagBrowserForm(mbApiInterface, selectedFiles, fieldsToScan);
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误: " + ex.Message + "\n\n" + ex.StackTrace, 
                    "Quick Tag Browser", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
}

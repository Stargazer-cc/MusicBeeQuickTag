using System;
using System.Collections.Generic;
using System.Globalization;

namespace MusicBeePlugin
{
    public static class Localization
    {
        private static string currentLanguage;
        private static Dictionary<string, Dictionary<string, string>> translations;

        static Localization()
        {
            // 检测系统语言
            var culture = CultureInfo.CurrentUICulture;
            currentLanguage = culture.TwoLetterISOLanguageName == "zh" ? "zh-CN" : "en-US";

            // 初始化翻译字典
            InitializeTranslations();
        }

        private static void InitializeTranslations()
        {
            translations = new Dictionary<string, Dictionary<string, string>>();
            
            // 中文翻译
            var zhCN = new Dictionary<string, string>();
            // Plugin.cs
            zhCN.Add("PluginDescription", "浏览并快速应用音乐库中已存在的标签值");
            zhCN.Add("SettingsSaved", "设置已保存！下次打开工具页面时生效。");
            zhCN.Add("PluginTitle", "MusicBeeQuickTag");
            zhCN.Add("Error", "错误");
            
            // SettingsForm.cs
            zhCN.Add("SettingsTitle", "MusicBeeQuickTag设置");
            zhCN.Add("FieldsSettings", "字段设置");
            zhCN.Add("SettingsInfo", "选择要在 Tag Browser 中显示的字段。右侧列表的顺序决定了显示时的列顺序。");
            zhCN.Add("Scanning", "正在扫描音乐库...");
            zhCN.Add("AvailableFields", "可用字段:");
            zhCN.Add("SelectedFields", "已选字段 (从上到下 = 从左到右):");
            zhCN.Add("MoveUp", "上移");
            zhCN.Add("MoveDown", "下移");
            zhCN.Add("OK", "确定");
            zhCN.Add("Cancel", "取消");
            zhCN.Add("SelectAtLeastOne", "请至少选择一个字段！");
            zhCN.Add("Warning", "提示");
            zhCN.Add("SaveFailed", "保存设置失败: ");
            zhCN.Add("PresetSettings", "预设标签设置");
            zhCN.Add("PresetInfo", "选择预设组，或者创建新组。在下方输入标签内容（字段:值）。");
            zhCN.Add("AddToPresets", "添加到当前组");
            zhCN.Add("Add", "添加");
            zhCN.Add("Group", "预设标签组:");
            zhCN.Add("NewGroup", "新建组");
            zhCN.Add("DeleteGroup", "删除组");
            zhCN.Add("SavePresets", "保存预设");
            zhCN.Add("DefaultGroup", "默认组");
            zhCN.Add("ConfirmDelete", "确定要删除预设组 '{0}' 吗？");
            zhCN.Add("EnterGroupName", "请输入新组名称:");
            zhCN.Add("GroupExists", "该组名已存在！");
            zhCN.Add("ApplyShort", "应用");

            
            // TagBrowserForm.cs
            zhCN.Add("BrowserTitle", "MusicBeeQuickTag V2.1.4");
            zhCN.Add("NoFileSelected", "未选择任何文件");
            zhCN.Add("FilesSelected", "已选中 {0} 个文件");
            zhCN.Add("NoFieldsFound", "没有找到任何标签数据\n\n请先在设置中选择要显示的字段");
            zhCN.Add("AppliedTag", "✓ 已应用 '{0}' 到 {1} 字段 ({2} 个文件)");
            zhCN.Add("SelectFilesFirst", "请先选择音乐文件");
            zhCN.Add("Info", "提示");
            zhCN.Add("ApplyPresets", "应用预设标签");
            zhCN.Add("PresetsApplied", "已应用预设标签到 {0} 个文件");
            zhCN.Add("TagRemoved", "已移除标签值: {0}");
            zhCN.Add("TagAdded", "已添加标签值: {0}");
            
            translations.Add("zh-CN", zhCN);

            // 英文翻译
            var enUS = new Dictionary<string, string>();
            // Plugin.cs
            enUS.Add("PluginDescription", "Browse and quickly apply existing tag values from your music library");
            enUS.Add("SettingsSaved", "Settings saved! Changes will take effect next time you open the tool.");
            enUS.Add("PluginTitle", "MusicBeeQuickTag");
            enUS.Add("Error", "Error");
            
            // SettingsForm.cs
            enUS.Add("SettingsTitle", "MusicBeeQuickTag Settings");
            enUS.Add("FieldsSettings", "Field Settings");
            enUS.Add("SettingsInfo", "Select fields to display in Tag Browser. The order in the right list determines the column order.");
            enUS.Add("Scanning", "Scanning music library...");
            enUS.Add("AvailableFields", "Available Fields:");
            enUS.Add("SelectedFields", "Selected Fields (T→B = L→R):");
            enUS.Add("MoveUp", "⬆");
            enUS.Add("MoveDown", "⬇");
            enUS.Add("OK", "OK");
            enUS.Add("Cancel", "Cancel");
            enUS.Add("SelectAtLeastOne", "Please select at least one field!");
            enUS.Add("Warning", "Warning");
            enUS.Add("SaveFailed", "Failed to save settings: ");
            enUS.Add("PresetSettings", "Preset Tag Settings");
            enUS.Add("PresetInfo", "Select a preset group or create a new one. Enter tags below (Field:Value).");
            enUS.Add("AddToPresets", "Add to Current Group");
            enUS.Add("Add", "Add");
            enUS.Add("Group", "Preset Tag Group:");
            enUS.Add("NewGroup", "New Group");
            enUS.Add("DeleteGroup", "Delete Group");
            enUS.Add("SavePresets", "Save Presets");
            enUS.Add("DefaultGroup", "Default");
            enUS.Add("ConfirmDelete", "Are you sure you want to delete group '{0}'?");
            enUS.Add("EnterGroupName", "Enter new group name:");
            enUS.Add("GroupExists", "Group name already exists!");
            enUS.Add("ApplyShort", "Apply");

            
            // TagBrowserForm.cs
            enUS.Add("BrowserTitle", "MusicBeeQuickTag V2.1.4");
            enUS.Add("NoFileSelected", "No files selected");
            enUS.Add("FilesSelected", "{0} files selected");
            enUS.Add("NoFieldsFound", "No tag data found\n\nPlease select fields to display in settings");
            enUS.Add("AppliedTag", "✓ Applied '{0}' to {1} field ({2} files)");
            enUS.Add("SelectFilesFirst", "Please select music files first");
            enUS.Add("Info", "Info");
            enUS.Add("ApplyPresets", "Apply Presets");
            enUS.Add("PresetsApplied", "Applied presets to {0} files");
            enUS.Add("TagRemoved", "Removed tag value: {0}");
            enUS.Add("TagAdded", "Added tag value: {0}");
            
            translations.Add("en-US", enUS);
        }

        /// <summary>
        /// 获取翻译文本
        /// </summary>
        public static string Get(string key)
        {
            if (translations.ContainsKey(currentLanguage) && 
                translations[currentLanguage].ContainsKey(key))
            {
                return translations[currentLanguage][key];
            }
            
            // 如果当前语言没有翻译，尝试使用英文
            if (currentLanguage != "en-US" && 
                translations["en-US"].ContainsKey(key))
            {
                return translations["en-US"][key];
            }
            
            // 如果都没有，返回 key 本身
            return key;
        }

        /// <summary>
        /// 获取带格式化参数的翻译文本
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            string format = Get(key);
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return format;
            }
        }

        /// <summary>
        /// 获取当前语言代码
        /// </summary>
        public static string CurrentLanguage
        {
            get { return currentLanguage; }
        }

        /// <summary>
        /// 手动设置语言（可选功能）
        /// </summary>
        public static void SetLanguage(string languageCode)
        {
            if (translations.ContainsKey(languageCode))
            {
                currentLanguage = languageCode;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.IO;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        public class SettingsForm : Form
        {
            private CheckedListBox checkedListBox;
            private Button btnOK;
            private Button btnCancel;
            private Button btnSelectAll;
            private Button btnDeselectAll;
            private Label lblInfo;
            private Label lblScanning;

            private MusicBeeApiInterface mbApi;
            private Dictionary<MetaDataType, string> fieldNames;
            private Dictionary<MetaDataType, int> fieldDataCounts;
            private List<MetaDataType> selectedFields;
            private string settingsPath;

            public List<MetaDataType> SelectedFields
            {
                get { return selectedFields; }
            }

            public SettingsForm(MusicBeeApiInterface api, string storagePath, List<MetaDataType> currentFields)
            {
                mbApi = api;
                settingsPath = Path.Combine(storagePath, "QuickTagBrowser_Fields.txt");
                selectedFields = new List<MetaDataType>(currentFields);
                fieldNames = new Dictionary<MetaDataType, string>();
                fieldDataCounts = new Dictionary<MetaDataType, int>();
                
                InitializeComponent();
                ScanLibraryFields();
                PopulateFieldList();
                LoadCurrentSelection();
            }

            private void InitializeComponent()
            {
                this.Text = "Quick Tag Browser - 字段设置";
                this.Size = new Size(600, 650);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                // Info label
                lblInfo = new Label();
                lblInfo.Text = "选择要在 Tag Browser 中显示的字段：";
                lblInfo.Dock = DockStyle.Top;
                lblInfo.Height = 30;
                lblInfo.TextAlign = ContentAlignment.MiddleLeft;
                lblInfo.Padding = new Padding(10, 0, 10, 0);
                this.Controls.Add(lblInfo);

                // Scanning label
                lblScanning = new Label();
                lblScanning.Text = "正在扫描音乐库...";
                lblScanning.Dock = DockStyle.Fill;
                lblScanning.TextAlign = ContentAlignment.MiddleCenter;
                lblScanning.Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold);
                lblScanning.ForeColor = Color.DarkBlue;
                this.Controls.Add(lblScanning);

                // Button panel
                Panel buttonPanel = new Panel();
                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 80;
                this.Controls.Add(buttonPanel);

                // Select/Deselect buttons
                btnSelectAll = new Button();
                btnSelectAll.Text = "全选";
                btnSelectAll.Location = new Point(10, 10);
                btnSelectAll.Size = new Size(100, 30);
                btnSelectAll.Click += (s, e) =>
                {
                    for (int i = 0; i < checkedListBox.Items.Count; i++)
                        checkedListBox.SetItemChecked(i, true);
                };
                buttonPanel.Controls.Add(btnSelectAll);

                btnDeselectAll = new Button();
                btnDeselectAll.Text = "全不选";
                btnDeselectAll.Location = new Point(120, 10);
                btnDeselectAll.Size = new Size(100, 30);
                btnDeselectAll.Click += (s, e) =>
                {
                    for (int i = 0; i < checkedListBox.Items.Count; i++)
                        checkedListBox.SetItemChecked(i, false);
                };
                buttonPanel.Controls.Add(btnDeselectAll);

                // OK/Cancel buttons
                btnOK = new Button();
                btnOK.Text = "确定";
                btnOK.Location = new Point(370, 45);
                btnOK.Size = new Size(100, 30);
                btnOK.Click += BtnOK_Click;
                buttonPanel.Controls.Add(btnOK);

                btnCancel = new Button();
                btnCancel.Text = "取消";
                btnCancel.Location = new Point(480, 45);
                btnCancel.Size = new Size(100, 30);
                btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
                buttonPanel.Controls.Add(btnCancel);

                // Checked list box (will be created after scanning)
                checkedListBox = new CheckedListBox();
                checkedListBox.Dock = DockStyle.Fill;
                checkedListBox.CheckOnClick = true;
                checkedListBox.Font = new Font(this.Font.FontFamily, 10);
                checkedListBox.Margin = new Padding(0, 50, 0, 0);  // Add top margin
                checkedListBox.Visible = false;
                this.Controls.Add(checkedListBox);
            }

            private void ScanLibraryFields()
            {
                Application.DoEvents(); // Update UI

                // All possible fields to check
                List<MetaDataType> allFields = new List<MetaDataType>();
                
                // Add Custom fields
                for (int i = 1; i <= 16; i++)
                {
                    allFields.Add((MetaDataType)Enum.Parse(typeof(MetaDataType), "Custom" + i));
                }
                
                // Add Virtual fields
                for (int i = 1; i <= 25; i++)
                {
                    allFields.Add((MetaDataType)Enum.Parse(typeof(MetaDataType), "Virtual" + i));
                }
                
                // Add standard fields
                allFields.Add(MetaDataType.Genre);
                allFields.Add(MetaDataType.Mood);
                allFields.Add(MetaDataType.Grouping);
                allFields.Add(MetaDataType.Publisher);
                allFields.Add(MetaDataType.Keywords);
                allFields.Add(MetaDataType.Occasion);
                allFields.Add(MetaDataType.Quality);
                allFields.Add(MetaDataType.Tempo);
                allFields.Add(MetaDataType.Origin);
                allFields.Add(MetaDataType.Lyricist);
                allFields.Add(MetaDataType.Composer);
                allFields.Add(MetaDataType.Conductor);
                allFields.Add(MetaDataType.Comment);

                // Scan library
                string[] allFiles;
                if (mbApi.Library_QueryFilesEx("", out allFiles))
                {
                    foreach (var field in allFields)
                    {
                        HashSet<string> uniqueValues = new HashSet<string>();
                        
                        foreach (string file in allFiles)
                        {
                            string value = mbApi.Library_GetFileTag(file, field);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                uniqueValues.Add(value);
                            }
                        }

                        if (uniqueValues.Count > 0)
                        {
                            string fieldName = mbApi.Setting_GetFieldName(field);
                            if (string.IsNullOrEmpty(fieldName))
                            {
                                fieldName = field.ToString();
                            }
                            
                            fieldNames[field] = fieldName;
                            fieldDataCounts[field] = uniqueValues.Count;
                        }
                    }
                }
            }

            private void PopulateFieldList()
            {
                lblScanning.Visible = false;
                checkedListBox.Visible = true;
                this.Controls.Add(checkedListBox);

                if (fieldNames.Count == 0)
                {
                    checkedListBox.Items.Add("未找到任何包含数据的字段");
                    return;
                }

                // Sort by field name
                var sortedFields = fieldNames.OrderBy(kvp => kvp.Value).ToList();

                foreach (var kvp in sortedFields)
                {
                    MetaDataType field = kvp.Key;
                    string fieldName = kvp.Value;
                    int count = fieldDataCounts[field];
                    
                    string displayText = string.Format("{0} ({1} 个不同值)", fieldName, count);
                    checkedListBox.Items.Add(new FieldItem { Field = field, DisplayText = displayText });
                }
            }

            private void LoadCurrentSelection()
            {
                for (int i = 0; i < checkedListBox.Items.Count; i++)
                {
                    FieldItem item = checkedListBox.Items[i] as FieldItem;
                    if (item != null && selectedFields.Contains(item.Field))
                    {
                        checkedListBox.SetItemChecked(i, true);
                    }
                }
            }

            private void BtnOK_Click(object sender, EventArgs e)
            {
                selectedFields.Clear();
                
                foreach (var item in checkedListBox.CheckedItems)
                {
                    FieldItem fieldItem = item as FieldItem;
                    if (fieldItem != null)
                    {
                        selectedFields.Add(fieldItem.Field);
                    }
                }

                if (selectedFields.Count == 0)
                {
                    MessageBox.Show("请至少选择一个字段！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save settings
                try
                {
                    List<string> lines = new List<string>();
                    foreach (var field in selectedFields)
                    {
                        lines.Add(((int)field).ToString());
                    }
                    File.WriteAllLines(settingsPath, lines);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存设置失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            private class FieldItem
            {
                public MetaDataType Field { get; set; }
                public string DisplayText { get; set; }

                public override string ToString()
                {
                    return DisplayText;
                }
            }

            public static List<MetaDataType> LoadSettings(string storagePath)
            {
                string settingsPath = Path.Combine(storagePath, "QuickTagBrowser_Fields.txt");
                List<MetaDataType> fields = new List<MetaDataType>();

                if (File.Exists(settingsPath))
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(settingsPath);
                        foreach (string line in lines)
                        {
                            int fieldValue;
                            if (int.TryParse(line.Trim(), out fieldValue))
                            {
                                fields.Add((MetaDataType)fieldValue);
                            }
                        }
                    }
                    catch { }
                }

                // Default fields if no settings found
                if (fields.Count == 0)
                {
                    fields.Add(MetaDataType.Grouping);
                    fields.Add(MetaDataType.Genre);
                    fields.Add(MetaDataType.Mood);
                }

                return fields;
            }
        }
    }
}

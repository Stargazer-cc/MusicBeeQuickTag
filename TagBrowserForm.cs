using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        public class TagBrowserForm : Form
        {
            private MusicBeeApiInterface mbApi;
            private string[] selectedFiles;
            private Dictionary<MetaDataType, HashSet<string>> tagValues;
            private List<MetaDataType> fieldsToScan;

            private Label lblSelectedTrack;
            private TableLayoutPanel tableLayout;
            private Timer selectionTimer;

            public TagBrowserForm(MusicBeeApiInterface api, string[] files, List<MetaDataType> fields)
            {
                mbApi = api;
                selectedFiles = files;
                fieldsToScan = fields;
                tagValues = new Dictionary<MetaDataType, HashSet<string>>();

                InitializeComponent();
                ScanLibraryTags();
                PopulateColumns();
                
                selectionTimer = new Timer();
                selectionTimer.Interval = 500;
                selectionTimer.Tick += SelectionTimer_Tick;
                selectionTimer.Start();
            }

            private void SelectionTimer_Tick(object sender, EventArgs e)
            {
                string[] currentSelection;
                if (mbApi.Library_QueryFilesEx("domain=SelectedFiles", out currentSelection))
                {
                    if (!ArraysEqual(selectedFiles, currentSelection))
                    {
                        selectedFiles = currentSelection;
                        UpdateSelectedTrackLabel();
                    }
                }
            }

            private bool ArraysEqual(string[] a1, string[] a2)
            {
                if (a1 == null && a2 == null) return true;
                if (a1 == null || a2 == null) return false;
                if (a1.Length != a2.Length) return false;
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i]) return false;
                }
                return true;
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                if (selectionTimer != null)
                {
                    selectionTimer.Stop();
                    selectionTimer.Dispose();
                }
                base.OnFormClosing(e);
            }

            private void InitializeComponent()
            {
                this.Text = "Quick Tag Browser v1.5";
                this.Size = new Size(1200, 700);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MinimumSize = new Size(900, 500);
                this.BackColor = Color.FromArgb(245, 245, 245);

                lblSelectedTrack = new Label();
                lblSelectedTrack.Dock = DockStyle.Top;
                lblSelectedTrack.Height = 50;
                lblSelectedTrack.Font = new Font("Microsoft YaHei", 11, FontStyle.Bold);
                lblSelectedTrack.TextAlign = ContentAlignment.MiddleLeft;
                lblSelectedTrack.Padding = new Padding(20, 0, 20, 0);
                lblSelectedTrack.BackColor = Color.FromArgb(50, 50, 50);
                lblSelectedTrack.ForeColor = Color.White;
                this.Controls.Add(lblSelectedTrack);

                tableLayout = new TableLayoutPanel();
                tableLayout.Dock = DockStyle.Fill;
                tableLayout.AutoScroll = true;
                tableLayout.Padding = new Padding(10, 0, 10, 10);  // Left, Top, Right, Bottom
                tableLayout.Margin = new Padding(0);
                tableLayout.BackColor = Color.FromArgb(245, 245, 245);
                this.Controls.Add(tableLayout);

                UpdateSelectedTrackLabel();
            }

            private void UpdateSelectedTrackLabel()
            {
                if (selectedFiles == null || selectedFiles.Length == 0)
                {
                    lblSelectedTrack.Text = "未选择任何文件";
                    lblSelectedTrack.ForeColor = Color.FromArgb(255, 100, 100);
                }
                else if (selectedFiles.Length == 1)
                {
                    string title = mbApi.Library_GetFileTag(selectedFiles[0], MetaDataType.TrackTitle);
                    string artist = mbApi.Library_GetFileTag(selectedFiles[0], MetaDataType.Artist);
                    lblSelectedTrack.Text = string.Format("♪ {0} - {1}", artist, title);
                    lblSelectedTrack.ForeColor = Color.FromArgb(100, 200, 255);
                }
                else
                {
                    lblSelectedTrack.Text = string.Format("已选中 {0} 个文件", selectedFiles.Length);
                    lblSelectedTrack.ForeColor = Color.FromArgb(150, 255, 150);
                }
            }

            private void ScanLibraryTags()
            {
                foreach (var field in fieldsToScan)
                {
                    tagValues[field] = new HashSet<string>();
                }

                string[] allFiles;
                if (mbApi.Library_QueryFilesEx("", out allFiles))
                {
                    foreach (string file in allFiles)
                    {
                        foreach (var field in fieldsToScan)
                        {
                            string value = mbApi.Library_GetFileTag(file, field);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                string[] values = value.Split(new char[] { ';', '\0' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string v in values)
                                {
                                    string trimmed = v.Trim();
                                    if (!string.IsNullOrEmpty(trimmed))
                                    {
                                        tagValues[field].Add(trimmed);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private void PopulateColumns()
            {
                List<MetaDataType> fieldsWithData = new List<MetaDataType>();
                foreach (var field in fieldsToScan)
                {
                    if (tagValues[field].Count > 0)
                    {
                        fieldsWithData.Add(field);
                    }
                }

                if (fieldsWithData.Count == 0)
                {
                    Label emptyLabel = new Label();
                    emptyLabel.Text = "没有找到任何标签数据\n\n请先在设置中选择要显示的字段";
                    emptyLabel.Dock = DockStyle.Fill;
                    emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
                    emptyLabel.Font = new Font("Microsoft YaHei", 12);
                    emptyLabel.ForeColor = Color.Gray;
                    tableLayout.Controls.Add(emptyLabel);
                    return;
                }

                tableLayout.RowCount = 1;
                tableLayout.ColumnCount = fieldsWithData.Count;
                for (int i = 0; i < fieldsWithData.Count; i++)
                {
                    tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / fieldsWithData.Count));
                }

                Color[] headerColors = new Color[]
                {
                    Color.FromArgb(66, 133, 244),
                    Color.FromArgb(52, 168, 83),
                    Color.FromArgb(251, 188, 5),
                    Color.FromArgb(234, 67, 53),
                    Color.FromArgb(156, 39, 176),
                };

                for (int col = 0; col < fieldsWithData.Count; col++)
                {
                    MetaDataType field = fieldsWithData[col];
                    string fieldName = mbApi.Setting_GetFieldName(field);
                    if (string.IsNullOrEmpty(fieldName))
                    {
                        fieldName = field.ToString();
                    }

                    Panel containerPanel = new Panel();
                    containerPanel.Dock = DockStyle.Fill;
                    containerPanel.Margin = new Padding(5, 70, 5, 5);  // Left, Top, Right, Bottom
                    containerPanel.BackColor = Color.White;

                    Color headerColor = headerColors[col % headerColors.Length];

                    // Footer label at bottom
                    Label footerLabel = new Label();
                    footerLabel.Text = string.Format("{0} ({1})", fieldName, tagValues[field].Count);
                    footerLabel.Dock = DockStyle.Bottom;
                    footerLabel.Height = 45;
                    footerLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 11, FontStyle.Bold);
                    footerLabel.TextAlign = ContentAlignment.MiddleCenter;
                    footerLabel.ForeColor = Color.White;
                    footerLabel.BackColor = headerColor;
                    containerPanel.Controls.Add(footerLabel);

                    // ListBox fills remaining space
                    ListBox listBox = new ListBox();
                    listBox.Dock = DockStyle.Fill;
                    listBox.Font = new Font(SystemFonts.DefaultFont.FontFamily, 10);
                    listBox.IntegralHeight = false;
                    listBox.BorderStyle = BorderStyle.None;
                    listBox.BackColor = Color.White;
                    listBox.ForeColor = Color.FromArgb(60, 60, 60);
                    listBox.ItemHeight = 28;
                    listBox.DrawMode = DrawMode.OwnerDrawFixed;
                    listBox.Padding = new Padding(0, 20, 0, 0);  // Add top padding
                    
                    listBox.DrawItem += (s, e) =>
                    {
                        if (e.Index < 0) return;
                        
                        bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                        Color bgColor = isSelected ? Color.FromArgb(230, 240, 255) : Color.White;
                        Color textColor = isSelected ? Color.FromArgb(30, 30, 30) : Color.FromArgb(60, 60, 60);
                        
                        e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds);
                        
                        string text = listBox.Items[e.Index].ToString();
                        e.Graphics.DrawString(text, e.Font, new SolidBrush(textColor), 
                            e.Bounds.Left + 12, e.Bounds.Top + 6);
                    };

                    var sortedValues = tagValues[field].OrderBy(v => v).ToList();
                    foreach (string value in sortedValues)
                    {
                        listBox.Items.Add(value);
                    }

                    MetaDataType currentField = field;
                    listBox.DoubleClick += (s, e) =>
                    {
                        if (listBox.SelectedItem != null)
                        {
                            ApplyTag(currentField, listBox.SelectedItem.ToString());
                        }
                    };

                    containerPanel.Controls.Add(listBox);
                    tableLayout.Controls.Add(containerPanel, col, 0);
                }
            }

            private void ApplyTag(MetaDataType field, string value)
            {
                if (selectedFiles == null || selectedFiles.Length == 0)
                {
                    MessageBox.Show("请先选择音乐文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (string file in selectedFiles)
                {
                    mbApi.Library_SetFileTag(file, field, value);
                    mbApi.Library_CommitTagsToFile(file);
                }
                mbApi.MB_RefreshPanels();
                
                string fieldName = mbApi.Setting_GetFieldName(field);
                if (string.IsNullOrEmpty(fieldName))
                    fieldName = field.ToString();
                    
                lblSelectedTrack.Text = string.Format("✓ 已应用 '{0}' 到 {1} 字段 ({2} 个文件)", 
                    value, fieldName, selectedFiles.Length);
                lblSelectedTrack.ForeColor = Color.FromArgb(150, 255, 150);
                
                Timer resetTimer = new Timer();
                resetTimer.Interval = 2000;
                resetTimer.Tick += (s, e) =>
                {
                    UpdateSelectedTrackLabel();
                    resetTimer.Stop();
                    resetTimer.Dispose();
                };
                resetTimer.Start();
            }
        }
    }
}

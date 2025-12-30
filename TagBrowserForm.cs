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
            private Dictionary<MetaDataType, HashSet<string>> selectedFileTags; // Feature 2: Track selected file tags
            private List<MetaDataType> fieldsToScan;
            private List<PresetGroup> presetGroups; // Feature 5
            private PresetGroup currentPresetGroup; // Feature 5

            private Label lblSelectedTrack;
            private TableLayoutPanel tableLayout;
            private Timer selectionTimer;

            public TagBrowserForm(MusicBeeApiInterface api, string[] files, List<MetaDataType> fields, List<PresetGroup> presets)
            {
                mbApi = api;
                selectedFiles = files;
                fieldsToScan = fields;
                presetGroups = presets; // Feature 5
                if (presetGroups != null && presetGroups.Count > 0) currentPresetGroup = presetGroups[0];
                tagValues = new Dictionary<MetaDataType, HashSet<string>>();
                selectedFileTags = new Dictionary<MetaDataType, HashSet<string>>();

                InitializeComponent();
                ScanLibraryTags();
                ScanSelectedTags(); // Init selected tags
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
                        ScanSelectedTags(); // Refresh selected tags
                        RefreshListBoxes(); // Force redraw of all listboxes
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
                this.Text = Localization.Get("BrowserTitle");
                this.Size = new Size(1200, 700);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MinimumSize = new Size(900, 500);
                this.BackColor = Color.FromArgb(245, 245, 245);

                // Top Panel Container
                Panel topPanel = new Panel();
                topPanel.Dock = DockStyle.Top;
                topPanel.Height = 50;
                topPanel.BackColor = Color.FromArgb(50, 50, 50);
                this.Controls.Add(topPanel);

                // Label
                lblSelectedTrack = new Label();
                lblSelectedTrack.Dock = DockStyle.Fill;
                lblSelectedTrack.Font = new Font("Microsoft YaHei", 11, FontStyle.Bold);
                lblSelectedTrack.TextAlign = ContentAlignment.MiddleLeft;
                lblSelectedTrack.Padding = new Padding(20, 0, 20, 0);
                lblSelectedTrack.BackColor = Color.Transparent; // Using panel BG
                lblSelectedTrack.ForeColor = Color.White;
                topPanel.Controls.Add(lblSelectedTrack);

                // Feature 5: Group Dropdown & Button
                if (presetGroups != null && presetGroups.Count > 0)
                {
                    // Container Right
                    FlowLayoutPanel flowRight = new FlowLayoutPanel();
                    flowRight.Dock = DockStyle.Right;
                    flowRight.AutoSize = true;
                    flowRight.FlowDirection = FlowDirection.LeftToRight;
                    flowRight.WrapContents = false;
                    flowRight.Padding = new Padding(10, 8, 10, 0); // Top padding to center vertically
                    flowRight.BackColor = Color.Transparent;

                    Label lblGroup = new Label();
                    lblGroup.Text = Localization.Get("Group"); // "Group:"
                    lblGroup.ForeColor = Color.White;
                    lblGroup.AutoSize = true;
                    lblGroup.Margin = new Padding(0, 6, 5, 0); // Align text with combo
                    flowRight.Controls.Add(lblGroup);

                    ComboBox cbBrowserGroups = new ComboBox();
                    cbBrowserGroups.DropDownStyle = ComboBoxStyle.DropDownList;
                    cbBrowserGroups.Width = 100;
                    cbBrowserGroups.FlatStyle = FlatStyle.Flat;
                    foreach(var g in presetGroups) cbBrowserGroups.Items.Add(g.Name);
                    cbBrowserGroups.SelectedIndex = 0;
                    cbBrowserGroups.SelectedIndexChanged += (s, ev) => 
                    {
                        if (cbBrowserGroups.SelectedIndex >= 0) 
                            currentPresetGroup = presetGroups[cbBrowserGroups.SelectedIndex];
                    };
                    flowRight.Controls.Add(cbBrowserGroups);

                    Button btnApplyPresets = new Button();
                    btnApplyPresets.Text = Localization.Get("ApplyShort"); // "Apply" - Shortened
                    btnApplyPresets.AutoSize = true;
                    btnApplyPresets.BackColor = Color.FromArgb(70, 70, 70);
                    btnApplyPresets.ForeColor = Color.White;
                    btnApplyPresets.FlatStyle = FlatStyle.Flat;
                    btnApplyPresets.FlatAppearance.BorderSize = 0;
                    btnApplyPresets.Click += BtnApplyPresets_Click;
                    btnApplyPresets.Margin = new Padding(5, 0, 0, 0);
                    
                    flowRight.Controls.Add(btnApplyPresets);
                    
                    topPanel.Controls.Add(flowRight);
                    flowRight.BringToFront();
                    lblSelectedTrack.SendToBack();
                }



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
                    lblSelectedTrack.Text = Localization.Get("NoFileSelected");
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
                    lblSelectedTrack.Text = Localization.Get("FilesSelected", selectedFiles.Length);
                    lblSelectedTrack.ForeColor = Color.FromArgb(150, 255, 150);
                }
            }

            private void ScanSelectedTags()
            {
                selectedFileTags.Clear();
                foreach (var field in fieldsToScan)
                {
                    selectedFileTags[field] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                if (selectedFiles != null && selectedFiles.Length > 0)
                {
                    // 1. Get tags for first file
                    Dictionary<MetaDataType, HashSet<string>> firstFileTags = new Dictionary<MetaDataType, HashSet<string>>();
                    foreach (var field in fieldsToScan)
                    {
                        firstFileTags[field] = GetTagsForFile(selectedFiles[0], field);
                    }

                    // 2. Intersect with remaining files
                    for (int i = 1; i < selectedFiles.Length; i++)
                    {
                        foreach (var field in fieldsToScan)
                        {
                            HashSet<string> currentTags = GetTagsForFile(selectedFiles[i], field);
                            firstFileTags[field].IntersectWith(currentTags);
                        }
                    }

                    // 3. Assign to result
                    selectedFileTags = firstFileTags;
                }
            }

            private HashSet<string> GetTagsForFile(string file, MetaDataType field)
            {
                HashSet<string> tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string value = mbApi.Library_GetFileTag(file, field);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    string[] values = value.Split(new char[] { ';', '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string v in values)
                    {
                         tags.Add(v.Trim());
                    }
                }
                return tags;
            }

            private void RefreshListBoxes()
            {
                foreach (Control c in tableLayout.Controls)
                {
                    if (c is Panel)
                    {
                        foreach (Control inner in c.Controls)
                        {
                            if (inner is ListBox)
                            {
                                inner.Invalidate();
                            }
                        }
                    }
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
                    emptyLabel.Text = Localization.Get("NoFieldsFound");
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
                        
                        string text = listBox.Items[e.Index].ToString();
                        bool isTagPresent = selectedFileTags[field].Contains(text);

                        bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                        Color bgColor = isSelected ? Color.FromArgb(230, 240, 255) : Color.White;
                        Color textColor = isSelected ? Color.FromArgb(30, 30, 30) : Color.FromArgb(60, 60, 60);
                        
                        // Highlight logic
                        if (isTagPresent)
                        {
                            textColor = Color.Blue; 
                        }

                        e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds);
                        
                        Font font = isTagPresent ? new Font(e.Font, FontStyle.Bold) : e.Font;
                        e.Graphics.DrawString(text, font, new SolidBrush(textColor), 
                            e.Bounds.Left + 12, e.Bounds.Top + 6);
                    };

                    var sortedValues = tagValues[field].OrderBy(v => v).ToList();
                    foreach (string value in sortedValues)
                    {
                        listBox.Items.Add(value);
                    }

                    MetaDataType currentField = field;
                    listBox.MouseClick += (s, e) =>
                    {
                        int index = listBox.IndexFromPoint(e.Location);
                        if (index != ListBox.NoMatches)
                        {
                            string value = listBox.Items[index].ToString();
                            bool isPresent = selectedFileTags[currentField].Contains(value);
                            
                            if (isPresent)
                            {
                                RemoveTag(currentField, value);
                            }
                            else
                            {
                                ApplyTag(currentField, value, true);
                            }
                            
                            // Immediately refresh highlights
                            ScanSelectedTags();
                            listBox.Invalidate(listBox.GetItemRectangle(index));
                        }
                    };

                    containerPanel.Controls.Add(listBox);
                    tableLayout.Controls.Add(containerPanel, col, 0);
                }
            }

            private void ApplyTag(MetaDataType field, string value, bool append)
            {
                if (selectedFiles == null || selectedFiles.Length == 0)
                {
                    MessageBox.Show(Localization.Get("SelectFilesFirst"), Localization.Get("Info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (string file in selectedFiles)
                {
                    if (append)
                    {
                        string currentValue = mbApi.Library_GetFileTag(file, field);
                        if (!string.IsNullOrWhiteSpace(currentValue))
                        {
                            // Check if value already exists to avoid duplicates
                            string[] existingValues = currentValue.Split(new char[] { ';', '\0' }, StringSplitOptions.RemoveEmptyEntries);
                            bool exists = false;
                            foreach (string existing in existingValues)
                            {
                                if (existing.Trim().Equals(value, StringComparison.OrdinalIgnoreCase))
                                {
                                    exists = true;
                                    break;
                                }
                            }

                            if (!exists)
                            {
                                value = currentValue + "; " + value;
                            }
                            else
                            {
                                // Value already exists, skip this file or just continue to next
                                continue; 
                            }
                        }
                    }

                    mbApi.Library_SetFileTag(file, field, value);
                    mbApi.Library_CommitTagsToFile(file);
                }
                mbApi.MB_RefreshPanels();
                
                string fieldName = mbApi.Setting_GetFieldName(field);
                if (string.IsNullOrEmpty(fieldName))
                    fieldName = field.ToString();
                    
                string actionText = append ? Localization.Get("AddedTag", value, fieldName, selectedFiles.Length) : Localization.Get("AppliedTag", value, fieldName, selectedFiles.Length);
                // Fallback if localization key doesn't exist yet, though we should probably add it. 
                // For now, let's reuse AppliedTag format or construct a string if needed, 
                // but ideally we should update Localization. 
                // Since I cannot see Localization class, I will assume I can format the string here or use a generic message.
                
                // Actually, let's just use a hardcoded string format for now if we can't easily update Localization resource file (which is likely compiled or in another file I haven't seen).
                // Wait, I see `Localization.Get` calls. I should probably check if I can add keys.
                // But for now, I'll just change the message logic slightly.
                
                lblSelectedTrack.Text = string.Format("{0}: {1} -> {2} ({3})", 
                    append ? "Added" : "Applied", 
                    value, 
                    fieldName, 
                    selectedFiles.Length);

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


            private void RemoveTag(MetaDataType field, string valueToRemove)
            {
                 if (selectedFiles == null || selectedFiles.Length == 0) return;

                 foreach (string file in selectedFiles)
                 {
                     string currentValue = mbApi.Library_GetFileTag(file, field);
                     if (!string.IsNullOrWhiteSpace(currentValue))
                     {
                         var values = currentValue.Split(new char[] { ';', '\0' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToList();
                         if (values.RemoveAll(v => v.Equals(valueToRemove, StringComparison.OrdinalIgnoreCase)) > 0)
                         {
                             string newValue = string.Join("; ", values);
                             mbApi.Library_SetFileTag(file, field, newValue);
                             mbApi.Library_CommitTagsToFile(file);
                         }
                     }
                 }
                 mbApi.MB_RefreshPanels();
                 lblSelectedTrack.Text = Localization.Get("TagRemoved", valueToRemove);
                 // Note: ScanSelectedTags updated by caller
            }

            private void BtnApplyPresets_Click(object sender, EventArgs e)
            {
                if (selectedFiles == null || selectedFiles.Length == 0)
                {
                    MessageBox.Show(Localization.Get("SelectFilesFirst"), Localization.Get("Info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Use currentPresetGroup
                if (currentPresetGroup == null || currentPresetGroup.Tags.Count == 0) return;

                foreach (string preset in currentPresetGroup.Tags)
                {
                    string[] parts = preset.Split(new char[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        string fieldName = parts[0].Trim();
                        string value = parts[1].Trim();
                        
                        // Find MetaDataType from name
                        MetaDataType field = MetaDataType.Comment; // Default
                        bool found = false;
                        foreach(MetaDataType type in Enum.GetValues(typeof(MetaDataType)))
                        {
                             if (type.ToString().Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                             {
                                 field = type;
                                 found = true;
                                 break;
                             }
                        }

                        if (found)
                        {
                            ApplyTag(field, value, true);
                        }
                    }
                }
                
                lblSelectedTrack.Text = Localization.Get("PresetsApplied", selectedFiles.Length);
                ScanSelectedTags();
                RefreshListBoxes();
            }

        }
    }
}

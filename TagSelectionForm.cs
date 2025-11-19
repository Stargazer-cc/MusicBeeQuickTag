using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class TagSelectionForm : Form
    {
        public string SelectedTagValue
        {
            get
            {
                if (listBoxTags.SelectedItem == null)
                    return null;
                return listBoxTags.SelectedItem.ToString();
            }
        }

        public TagSelectionForm(List<string> tags)
        {
            InitializeComponent();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        listBoxTags.Items.Add(tag);
                    }
                }
            }
        }

        private void listBoxTags_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxTags.SelectedItem != null)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}

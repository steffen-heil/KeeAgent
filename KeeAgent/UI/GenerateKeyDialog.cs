// SPDX-License-Identifier: GPL-2.0-only
using System;
using System.Windows.Forms;

namespace KeeAgent.UI
{
  public partial class GenerateKeyDialog : Form
  {
    public string Comment { get { return commentTextBox.Text; } }

    public GenerateKeyDialog(string defaultComment)
    {
      InitializeComponent();
      if (Type.GetType("Mono.Runtime") == null) {
        Icon = Properties.Resources.KeeAgent_icon;
      } else {
        Icon = Properties.Resources.KeeAgent_icon_mono;
      }
      commentTextBox.Text = defaultComment;
    }

    void generateButton_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }
  }
}

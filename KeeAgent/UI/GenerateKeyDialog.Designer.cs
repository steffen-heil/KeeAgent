namespace KeeAgent.UI
{
  partial class GenerateKeyDialog
  {
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
      this.typeLabel      = new System.Windows.Forms.Label();
      this.commentLabel   = new System.Windows.Forms.Label();
      this.commentTextBox = new System.Windows.Forms.TextBox();
      this.generateButton = new System.Windows.Forms.Button();
      this.cancelButton   = new System.Windows.Forms.Button();
      this.SuspendLayout();

      // typeLabel
      this.typeLabel.AutoSize = true;
      this.typeLabel.Location = new System.Drawing.Point(15, 15);
      this.typeLabel.Name = "typeLabel";
      this.typeLabel.Text = "Type: Ed25519";

      // commentLabel
      this.commentLabel.AutoSize = true;
      this.commentLabel.Location = new System.Drawing.Point(15, 45);
      this.commentLabel.Name = "commentLabel";
      this.commentLabel.Text = "&Comment:";

      // commentTextBox
      this.commentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(
        System.Windows.Forms.AnchorStyles.Top |
        System.Windows.Forms.AnchorStyles.Left |
        System.Windows.Forms.AnchorStyles.Right));
      this.commentTextBox.Location = new System.Drawing.Point(15, 65);
      this.commentTextBox.Name = "commentTextBox";
      this.commentTextBox.Size = new System.Drawing.Size(370, 23);
      this.commentTextBox.TabIndex = 0;

      // generateButton
      this.generateButton.Anchor = ((System.Windows.Forms.AnchorStyles)(
        System.Windows.Forms.AnchorStyles.Bottom |
        System.Windows.Forms.AnchorStyles.Right));
      this.generateButton.Location = new System.Drawing.Point(229, 110);
      this.generateButton.Name = "generateButton";
      this.generateButton.Size = new System.Drawing.Size(75, 23);
      this.generateButton.TabIndex = 1;
      this.generateButton.Text = "&Generate";
      this.generateButton.UseVisualStyleBackColor = true;
      this.generateButton.Click += new System.EventHandler(this.generateButton_Click);

      // cancelButton
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(
        System.Windows.Forms.AnchorStyles.Bottom |
        System.Windows.Forms.AnchorStyles.Right));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(310, 110);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 2;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;

      // GenerateKeyDialog
      this.AcceptButton = this.generateButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(400, 150);
      this.Controls.Add(this.typeLabel);
      this.Controls.Add(this.commentLabel);
      this.Controls.Add(this.commentTextBox);
      this.Controls.Add(this.generateButton);
      this.Controls.Add(this.cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "GenerateKeyDialog";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Generate Ed25519 SSH Key";
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label typeLabel;
    private System.Windows.Forms.Label commentLabel;
    private System.Windows.Forms.TextBox commentTextBox;
    private System.Windows.Forms.Button generateButton;
    private System.Windows.Forms.Button cancelButton;
  }
}

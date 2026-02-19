namespace CoreOne.Winforms.Controls
{
    partial class ModelControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            PnlControls = new Panel();
            PnlView = new Panel();
            SuspendLayout();
            // 
            // PnlControls
            // 
            PnlControls.Dock = DockStyle.Bottom;
            PnlControls.Location = new Point(0, 340);
            PnlControls.Name = "PnlControls";
            PnlControls.Size = new Size(500, 40);
            PnlControls.TabIndex = 0;
            // 
            // PnlView
            // 
            PnlView.AutoScroll = true;
            PnlView.Dock = DockStyle.Fill;
            PnlView.Location = new Point(0, 0);
            PnlView.Name = "PnlView";
            PnlView.Size = new Size(500, 340);
            PnlView.TabIndex = 1;
            // 
            // ModelControl
            //  
            Controls.Add(PnlView);
            Controls.Add(PnlControls);
            Name = "ModelControl";
            Size = new Size(500, 380);
            ResumeLayout(false);
        }

        #endregion

        private Panel PnlControls;
        private Panel PnlView;
    }
}

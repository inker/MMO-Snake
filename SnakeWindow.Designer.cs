using SharpGL;
using System.Windows.Forms;

namespace Snake
{
    partial class SnakeWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Canvas = new System.Windows.Forms.PictureBox();
            this.GLControl = new SharpGL.OpenGLControl();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.ChangeModeButton = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.Canvas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GLControl)).BeginInit();
            this.StatusStrip.SuspendLayout();
            this.MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Canvas
            // 
            this.Canvas.BackColor = System.Drawing.Color.Black;
            this.Canvas.Location = new System.Drawing.Point(13, 13);
            this.Canvas.Margin = new System.Windows.Forms.Padding(4);
            this.Canvas.Name = "Canvas";
            this.Canvas.Size = new System.Drawing.Size(104, 65);
            this.Canvas.TabIndex = 0;
            this.Canvas.TabStop = false;
            // 
            // GLControl
            // 
            this.GLControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GLControl.DrawFPS = true;
            this.GLControl.FrameRate = 40;
            this.GLControl.Location = new System.Drawing.Point(0, 0);
            this.GLControl.Name = "GLControl";
            this.GLControl.OpenGLVersion = SharpGL.Version.OpenGLVersion.OpenGL2_1;
            this.GLControl.RenderContextType = SharpGL.RenderContextType.FBO;
            this.GLControl.RenderTrigger = SharpGL.RenderTrigger.TimerBased;
            this.GLControl.Size = new System.Drawing.Size(568, 347);
            this.GLControl.TabIndex = 0;
            this.GLControl.OpenGLInitialized += new System.EventHandler(this.GLInitialized);
            this.GLControl.OpenGLDraw += new SharpGL.RenderEventHandler(this.GLDraw);
            this.GLControl.Resized += new System.EventHandler(this.GLResized);
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.StatusStrip.Location = new System.Drawing.Point(0, 325);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(568, 22);
            this.StatusStrip.TabIndex = 1;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(118, 17);
            this.StatusLabel.Text = "toolStripStatusLabel1";
            // 
            // MenuStrip
            // 
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ChangeModeButton});
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(568, 24);
            this.MenuStrip.TabIndex = 2;
            this.MenuStrip.Text = "menuStrip1";
            // 
            // ChangeModeButton
            // 
            this.ChangeModeButton.Name = "ChangeModeButton";
            this.ChangeModeButton.Size = new System.Drawing.Size(120, 20);
            this.ChangeModeButton.Text = "Change to OpenGL";
            this.ChangeModeButton.Click += new System.EventHandler(this.OnChangeModeButtonClick);
            // 
            // SnakeWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Verdana", 9, System.Drawing.FontStyle.Regular);
            this.ClientSize = new System.Drawing.Size(568, 347);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.MenuStrip);
            this.Controls.Add(this.Canvas);
            this.Controls.Add(this.GLControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.MenuStrip;
            this.MaximizeBox = false;
            this.Name = "SnakeWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Snake";
            ((System.ComponentModel.ISupportInitialize)(this.Canvas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GLControl)).EndInit();
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private OpenGLControl GLControl;
        private PictureBox Canvas;
        private StatusStrip StatusStrip;
        private ToolStripStatusLabel StatusLabel;
        private MenuStrip MenuStrip;
        private ToolStripMenuItem ChangeModeButton;
    }
}


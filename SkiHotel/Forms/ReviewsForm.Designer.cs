namespace SkiHotel.Forms
{
    partial class ReviewsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ReviewsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 500);
            this.Name = "ReviewsForm";
            this.Text = "ReviewsForm";
            this.Load += new System.EventHandler(this.ReviewsForm_Load);
            this.ResumeLayout(false);

        }
    }
}
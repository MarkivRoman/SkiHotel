using System.Drawing;
using System.Windows.Forms;

namespace SkiHotel.Forms
{
    partial class OrderForm
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
            // OrderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(950, 620); // Збільшена ширина (880) та компактна висота (620)
            this.MinimumSize = new Size(950, 620);
            this.Name = "OrderForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "New Order — SKI HOTEL";
            this.Load += new System.EventHandler(this.OrderForm_Load);
            this.ResumeLayout(false);
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAnCuoiKy_Dijkstra.UI
{
    public class Welcome : Form
    {
        public Welcome()
        {
            InitializeUI();
        }
        private void InitializeUI()
        {
            this.BackgroundImage = Dijkstra.Properties.Resources.hong_tim;//hình ảnh được lấy từ máy chủ, global để tìm kiếm trong tất cả các file
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "Đồ án Dijkstra";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = Color.FromArgb(45, 45, 48);

            Label lblTitle = new Label()
            {
                Text = "Thuật toán Dijkstra \nỨng dụng tìm đường",
                Font = new Font("Segoe UI", 40, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 250,
                BackColor = Color.Transparent
            };

            Button btnStart = new Button()
            {
                Text = "Bắt Đầu",
                Size = new Size(300, 100),
                Location = new Point(615, 350),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += (s, e) => {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };//đóng Form và trả về kết quả OK
            Button btnExit = new Button()
            {
                Text = "Thoát",
                Size = new Size(300, 100),
                Location = new Point(615, 500),
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
            btnExit.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnStart);
            this.Controls.Add(btnExit);
            this.Controls.Add(lblTitle);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen pen = new Pen(Color.FromArgb(0, 122, 204), 5);
            e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
        }
    }
}

using Dijkstra.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAnCuoiKy_Dijkstra.UI
{
    public partial class MainForm
    {
        private SplitContainer splitContainer;
        private DoubleBufferedPanel mapPanel;
        private Panel controlPanel;

        // Cài đặt cho bản đồ: kéo, thả, zoom
        private float offsetX = 50f;
        private float offsetY = 50f;
        private float scale = 1.0f;
        private float coordScale = 3.0f;
        private bool isDragging = false;
        private Point lastMousePos;

        // Điểm đang chọn để xem thông tin
        private Vertex selectedVertex = null;
        private string selectedEdgeKey = null; // Lưu id cạnh đang chọn
        private double lastPathWeight = -1; // Lưu tổng trọng số

        // Các nút điều khiển trên giao diện
        private Label lblVertexCount;

        // Thêm điểm
        private TextBox txtPointId, txtPointName, txtPointX, txtPointY;
        private Button btnAddPoint;

        // Thêm cạnh
        private TextBox txtEdgeId1, txtEdgeId2;
        private CheckBox chkDirected;
        private Button btnAddEdge;

        // Xóa điểm
        private TextBox txtDeletePointId;
        private Button btnDeletePoint;

        // Xóa cạnh
        private TextBox txtDeleteEdgeId1, txtDeleteEdgeId2;
        private Button btnDeleteEdge;

        // File CSV
        private Button btnLoadCSV;
        private ProgressBar progressBar;

        // Tìm đường
        private TextBox txtFindSource, txtFindDest;
        private Button btnFindPath;

        // Các pop up hiển thị: Lỗi, read info..
        private Panel errorPanel;
        private Label lblErrorMessage;
        private Button btnCloseError;

        private Panel infoPanel;
        private Label lblInfo;
        private Button btnCloseInfo;

        private void InitializeUI()
        {
            this.SuspendLayout();
            this.Text = "Đồ Án Dijkstra";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Tạo splitcontainer chia đôi màn hình
            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterDistance = 700;
            splitContainer.FixedPanel = FixedPanel.Panel2; 
            this.Controls.Add(splitContainer);

            // Tạo Panel chứa Bản đồ (Trái)
            mapPanel = new DoubleBufferedPanel();
            mapPanel.Dock = DockStyle.Fill;
            mapPanel.BackColor = Color.White;
            // Thiết lập các sự kiện chuột và vẽ đồ họa
            mapPanel.Paint += MapPanel_Paint;
            mapPanel.MouseDown += MapPanel_MouseDown;
            mapPanel.MouseMove += MapPanel_MouseMove;
            mapPanel.MouseUp += MapPanel_MouseUp;
            mapPanel.MouseClick += MapPanel_MouseClick;
            splitContainer.Panel1.Controls.Add(mapPanel);

            // Tạo Panel chứa Bảng điều khiển (Phải)
            controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Fill;
            controlPanel.BackColor = Color.LightGray;
            controlPanel.AutoScroll = true;
            splitContainer.Panel2.Controls.Add(controlPanel);

            int currentY = 10;
            int marginX = 10;
            int width = 310;

            // --- Nơi hiển thị Số lượng đỉnh ---
            lblVertexCount = new Label() { Text = "Tổng số đỉnh: 0", Location = new Point(marginX, currentY), AutoSize = true, Font = new Font("Arial", 14, FontStyle.Bold) };
            controlPanel.Controls.Add(lblVertexCount);
            currentY += 40;

            // --- GroupBox Thêm Điểm ---
            GroupBox gbAddPoint = new GroupBox() { Text = "Nhập Điểm", Location = new Point(marginX, currentY), Size = new Size(width, 150), Font = new Font("Arial", 10) };

            gbAddPoint.Controls.Add(new Label() { Text = "ID:", Location = new Point(10, 35), AutoSize = true });
            txtPointId = new TextBox() { Location = new Point(50, 30), Width = 100 };
            gbAddPoint.Controls.Add(txtPointId);

            gbAddPoint.Controls.Add(new Label() { Text = "X:", Location = new Point(160, 35), AutoSize = true });
            txtPointX = new TextBox() { Location = new Point(190, 30), Width = 100 };
            gbAddPoint.Controls.Add(txtPointX);

            gbAddPoint.Controls.Add(new Label() { Text = "Tên:", Location = new Point(10, 75), AutoSize = true });
            txtPointName = new TextBox() { Location = new Point(50, 70), Width = 100 };
            gbAddPoint.Controls.Add(txtPointName);

            gbAddPoint.Controls.Add(new Label() { Text = "Y:", Location = new Point(160, 75), AutoSize = true });
            txtPointY = new TextBox() { Location = new Point(190, 70), Width = 100 };
            gbAddPoint.Controls.Add(txtPointY);

            btnAddPoint = new Button() { Text = "Nhập Điểm", Location = new Point(100, 110), Width = 110, BackColor = Color.LightCyan };
            btnAddPoint.Click += BtnAddPoint_Click;
            gbAddPoint.Controls.Add(btnAddPoint);
            controlPanel.Controls.Add(gbAddPoint);
            currentY += 160;

            // --- GroupBox Thêm Cạnh ---
            GroupBox gbAddEdge = new GroupBox() { Text = "Nhập Cạnh", Location = new Point(marginX, currentY), Size = new Size(width, 120), Font = new Font("Arial", 10) };

            gbAddEdge.Controls.Add(new Label() { Text = "ID 1:", Location = new Point(10, 35), AutoSize = true });
            txtEdgeId1 = new TextBox() { Location = new Point(55, 30), Width = 80 };
            EnableAutoComplete(txtEdgeId1); // Gợi ý ID tự động
            gbAddEdge.Controls.Add(txtEdgeId1);

            gbAddEdge.Controls.Add(new Label() { Text = "ID 2:", Location = new Point(145, 35), AutoSize = true });
            txtEdgeId2 = new TextBox() { Location = new Point(190, 30), Width = 80 };
            EnableAutoComplete(txtEdgeId2); // Gợi ý ID tự động
            gbAddEdge.Controls.Add(txtEdgeId2);

            chkDirected = new CheckBox() { Text = "Có hướng?", Location = new Point(10, 75), AutoSize = true };
            gbAddEdge.Controls.Add(chkDirected);

            btnAddEdge = new Button() { Text = "Nhập Cạnh", Location = new Point(130, 70), Width = 110, BackColor = Color.LightCyan };
            btnAddEdge.Click += BtnAddEdge_Click;
            gbAddEdge.Controls.Add(btnAddEdge);
            controlPanel.Controls.Add(gbAddEdge);
            currentY += 130;

            // --- GroupBox Xóa Điểm ---
            GroupBox gbDeletePoint = new GroupBox() { Text = "Xóa Điểm", Location = new Point(marginX, currentY), Size = new Size(width, 70), Font = new Font("Arial", 10) };

            gbDeletePoint.Controls.Add(new Label() { Text = "ID:", Location = new Point(10, 30), AutoSize = true });
            txtDeletePointId = new TextBox() { Location = new Point(50, 25), Width = 100 };
            EnableAutoComplete(txtDeletePointId);
            gbDeletePoint.Controls.Add(txtDeletePointId);

            btnDeletePoint = new Button() { Text = "Xóa Điểm", Location = new Point(170, 22), Width = 100, BackColor = Color.LightPink };
            btnDeletePoint.Click += BtnDeletePoint_Click;
            gbDeletePoint.Controls.Add(btnDeletePoint);
            controlPanel.Controls.Add(gbDeletePoint);
            currentY += 80;

            // --- GroupBox Xóa Cạnh ---
            GroupBox gbDeleteEdge = new GroupBox() { Text = "Xóa Cạnh", Location = new Point(marginX, currentY), Size = new Size(width, 70), Font = new Font("Arial", 10) };

            gbDeleteEdge.Controls.Add(new Label() { Text = "ID 1:", Location = new Point(10, 30), AutoSize = true });
            txtDeleteEdgeId1 = new TextBox() { Location = new Point(50, 25), Width = 70 };
            EnableAutoComplete(txtDeleteEdgeId1);
            gbDeleteEdge.Controls.Add(txtDeleteEdgeId1);

            gbDeleteEdge.Controls.Add(new Label() { Text = "ID 2:", Location = new Point(125, 30), AutoSize = true });
            txtDeleteEdgeId2 = new TextBox() { Location = new Point(165, 25), Width = 70 };
            EnableAutoComplete(txtDeleteEdgeId2);
            gbDeleteEdge.Controls.Add(txtDeleteEdgeId2);

            btnDeleteEdge = new Button() { Text = "Xóa Cạnh", Location = new Point(240, 22), Width = 65, BackColor = Color.LightPink };
            btnDeleteEdge.Click += BtnDeleteEdge_Click;
            gbDeleteEdge.Controls.Add(btnDeleteEdge);
            controlPanel.Controls.Add(gbDeleteEdge);
            currentY += 80;

            // --- Load CSV ---
            btnLoadCSV = new Button() { Text = "CHÈN TOÀN BỘ ĐỒ THỊ", Location = new Point(marginX, currentY), Size = new Size(width, 50), BackColor = Color.SkyBlue, Font = new Font("Arial", 11, FontStyle.Bold) };
            btnLoadCSV.Click += BtnLoadCSV_Click;
            controlPanel.Controls.Add(btnLoadCSV);
            currentY += 55;

            // Khi load file thì hiển thị thanh Loading
            progressBar = new ProgressBar() { Location = new Point(marginX, currentY), Size = new Size(width, 15), Visible = false, Style = ProgressBarStyle.Marquee };
            controlPanel.Controls.Add(progressBar);
            currentY += 25;

            // --- GroupBox Tìm Đường ---
            GroupBox gbFindPath = new GroupBox() { Text = "Tìm Đường Đi", Location = new Point(marginX, currentY), Size = new Size(width, 140), Font = new Font("Arial", 10), BackColor = Color.FromArgb(220, 255, 220) };

            gbFindPath.Controls.Add(new Label() { Text = "Điểm đi (ID):", Location = new Point(10, 35), AutoSize = true });
            txtFindSource = new TextBox() { Location = new Point(100, 30), Width = 180 };
            EnableAutoComplete(txtFindSource);
            gbFindPath.Controls.Add(txtFindSource);

            gbFindPath.Controls.Add(new Label() { Text = "Điểm đến (ID):", Location = new Point(10, 75), AutoSize = true });
            txtFindDest = new TextBox() { Location = new Point(100, 70), Width = 180 };
            EnableAutoComplete(txtFindDest);
            gbFindPath.Controls.Add(txtFindDest);

            btnFindPath = new Button() { Text = "TÌM ĐƯỜNG", Location = new Point(80, 105), Width = 150, BackColor = Color.LightGreen, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnFindPath.Click += BtnFindPath_Click;
            gbFindPath.Controls.Add(btnFindPath);
            controlPanel.Controls.Add(gbFindPath);

            // Tạo Panel Thông báo Lỗi
            errorPanel = new Panel() { Size = new Size(450, 250), BackColor = Color.FromArgb(230, 80, 80), Visible = false };
            errorPanel.BorderStyle = BorderStyle.FixedSingle;
            Label lblErrorTitle = new Label() { Text = "CÓ LỖI XẢY RA!", Font = new Font("Arial", 20, FontStyle.Bold), ForeColor = Color.White, Location = new Point(120, 20), AutoSize = true };
            lblErrorMessage = new Label() { Text = "Chi tiết lỗi...", Font = new Font("Arial", 12), ForeColor = Color.White, Location = new Point(20, 80), Size = new Size(410, 100), TextAlign = ContentAlignment.TopCenter };
            btnCloseError = new Button() { Text = "ĐÃ HIỂU", Location = new Point(175, 190), Size = new Size(100, 40), BackColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnCloseError.Click += (s, e) => errorPanel.Visible = false;

            errorPanel.Controls.Add(lblErrorTitle);
            errorPanel.Controls.Add(lblErrorMessage);
            errorPanel.Controls.Add(btnCloseError);
            mapPanel.Controls.Add(errorPanel); // Thêm vào bản đồ để pop up lên giữa

            // Tạo Panel read thông tin khi bấm vào đỉnh
            infoPanel = new Panel() { Size = new Size(280, 180), BackColor = Color.LightYellow, Visible = false };
            infoPanel.BorderStyle = BorderStyle.FixedSingle;
            Label lblInfoTitle = new Label() { Text = "CHẾ ĐỘ ĐỌC", Font = new Font("Arial", 10, FontStyle.Bold), Location = new Point(30, 10), AutoSize = true };
            lblInfo = new Label() { Text = "Thông số...", Font = new Font("Arial", 10), Location = new Point(15, 40), Size = new Size(250, 100) };
            btnCloseInfo = new Button() { Text = "Đóng", Location = new Point(100, 145), Size = new Size(80, 25) };
            btnCloseInfo.Click += (s, e) => infoPanel.Visible = false;

            infoPanel.Controls.Add(lblInfoTitle);
            infoPanel.Controls.Add(lblInfo);
            infoPanel.Controls.Add(btnCloseInfo);
            mapPanel.Controls.Add(infoPanel);
            
            this.ResumeLayout(false);
        }
    }
}

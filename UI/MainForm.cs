using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAnCuoiKy_Dijkstra.UI
{
public class Welcome : Form
{
    public Welcome()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        this.BackgroundImage = Dijkstra.Properties.Resources.hong_tim;//hình ảnh được lấy từ máy chủ, global để tìm kiếm trong tất cả các file
        this.BackgroundImageLayout=System.Windows.Forms.ImageLayout.Stretch;
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
        btnStart.Click += (s, e) => { this.DialogResult = DialogResult.OK;
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
        e.Graphics.DrawRectangle(pen,0,0, this.Width-1, this.Height-1);
    }
}
    public class MainForm : Form
    {
        private SplitContainer splitContainer;
        private DoubleBufferedPanel mapPanel;
        private Panel controlPanel;

        // Dữ liệu đồ thị và thuật toán
        private Graph graph;
        private CustomLinkedList<Vertex> currentPath;

        // Cài đặt cho bản đồ: kéo, thả, zoom
        private float offsetX = 50f;
        private float offsetY = 50f;
        private float scale = 1.0f;
        private float coordScale = 3.0f; // Hệ số phân tán tọa độ
        private bool isDragging = false;
        private Point lastMousePos;

        // Điểm đang được chọn để xem thông tin
        private Vertex selectedVertex = null;
        private string selectedEdgeKey = null; // Thêm biến lưu id cạnh đang chọn
        private double lastPathWeight = -1; // Thêm biến lưu tổng trọng số

        // Các nút điều khiển trên giao diện
        private Label lblVertexCount;
        
        // Khu vực thêm điểm
        private TextBox txtPointId, txtPointName, txtPointX, txtPointY;
        private Button btnAddPoint;

        // Khu vực thêm cạnh
        private TextBox txtEdgeId1, txtEdgeId2;
        private CheckBox chkDirected;
        private Button btnAddEdge;

        // Khu vực xóa điểm
        private TextBox txtDeletePointId;
        private Button btnDeletePoint;

        // Khu vực xóa cạnh
        private TextBox txtDeleteEdgeId1, txtDeleteEdgeId2;
        private Button btnDeleteEdge;

        // Khu vực File CSV
        private Button btnLoadCSV;
        private ProgressBar progressBar;

        // Khu vực Tìm đường
        private TextBox txtFindSource, txtFindDest;
        private Button btnFindPath;

        // Các pop up hiển thị: Lỗi, read info..
        private Panel errorPanel;
        private Label lblErrorMessage;
        private Button btnCloseError;

        private Panel infoPanel;
        private Label lblInfo;
        private Button btnCloseInfo;

        public MainForm()
        {
            // Khởi tạo đồ thị rỗng
            graph = new Graph();
            
            // Xây dựng giao diện
            InitializeUI();
            
            // Thiết lập tính năng lăn chuột để phóng to và thu nhỏ map
            this.MouseWheel += MapPanel_MouseWheel; 
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Đảm bảo panel điều khiển bên phải luôn đủ rộng 350px kể cả khi full màn hình
            splitContainer.SplitterDistance = this.ClientSize.Width - 350;
        }

        // Thay vì kéo thả Designer thì thiết lập hàm khởi tạo tất cả các control bằng code
        private void InitializeUI()
        {
            this.Text = "Đồ Án Dijkstra - Bản Đồ MVP";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized; // Thêm dòng này để phóng to cửa sổ khi mở

            // Tạo splitcontainer chia đôi màn hình: Trái (Bản đồ) và Phải (Bảng điều khiển)
            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterDistance = 700; // Kích thước panel trái to hơn
            splitContainer.FixedPanel = FixedPanel.Panel2; // Panel phải giữ nguyên kích thước khi thay đổi size cửa sổ
            this.Controls.Add(splitContainer);

            // Bước 1. Khởi tạo Panel chứa Bản đồ (Trái)
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

            // Bước 2. Khởi tạo Panel chứa Bảng điều khiển (Phải)
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

            // --- Load CSV (Tối ưu hóa chạy nền bằng Task) ---
            btnLoadCSV = new Button() { Text = "CHÈN TOÀN BỘ ĐỒ THỊ", Location = new Point(marginX, currentY), Size = new Size(width, 50), BackColor = Color.SkyBlue, Font = new Font("Arial", 11, FontStyle.Bold) };
            btnLoadCSV.Click += BtnLoadCSV_Click;
            controlPanel.Controls.Add(btnLoadCSV);
            currentY += 55;

            // Khi load file thì hiển thị thanh Loading ra
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

            // Bước 3. Khởi tạo Panel Thông báo Lỗi
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

            // Bước 4. Khởi tạo Panel read thông tin khi bấm vào đỉnh
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
        }

        // #### CÁC HÀM XỬ LÝ SỰ KIỆN BẤM NÚT ####

        private void BtnAddPoint_Click(object sender, EventArgs e)
        {
            try
            {
                string id = txtPointId.Text;
                string name = txtPointName.Text;
                
                if (!double.TryParse(txtPointX.Text, out double x) || !double.TryParse(txtPointY.Text, out double y))
                    throw new Exception("Tọa độ X, Y không hợp lệ. Vui lòng nhập số.");
                
                graph.AddVertex(id, name, x, y);
                UpdateUIAfterGraphChange();
                
                // Xóa trống ô nhập
                txtPointId.Clear(); txtPointName.Clear(); txtPointX.Clear(); txtPointY.Clear();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void BtnAddEdge_Click(object sender, EventArgs e)
        {
            try
            {
                string id1 = txtEdgeId1.Text;
                string id2 = txtEdgeId2.Text;
                bool isDirected = chkDirected.Checked;

                if (isDirected)
                    graph.AddDirectedEdge(id1, id2);
                else
                    graph.AddUndirectedEdge(id1, id2);

                mapPanel.Invalidate(); // Vẽ lại bản đồ
                txtEdgeId1.Clear(); txtEdgeId2.Clear();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void BtnDeletePoint_Click(object sender, EventArgs e)
        {
            try
            {
                string id = txtDeletePointId.Text;
                graph.RemoveVertex(id);
                UpdateUIAfterGraphChange();
                
                // Nếu điểm đang chọn bị xóa, bỏ chọn
                if (selectedVertex != null && selectedVertex.Id == id)
                {
                    selectedVertex = null;
                    infoPanel.Visible = false;
                }
                
                txtDeletePointId.Clear();
                MessageBox.Show("Đã xóa điểm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void BtnDeleteEdge_Click(object sender, EventArgs e)
        {
            try
            {
                string id1 = txtDeleteEdgeId1.Text;
                string id2 = txtDeleteEdgeId2.Text;

                graph.RemoveEdge(id1, id2);
                mapPanel.Invalidate(); // Vẽ lại bản đồ
                
                // Bỏ chọn cạnh nếu đang chọn cạnh này
                if (selectedEdgeKey == id1 + "_" + id2 || selectedEdgeKey == id2 + "_" + id1)
                {
                    selectedEdgeKey = null;
                    infoPanel.Visible = false;
                }

                txtDeleteEdgeId1.Clear(); 
                txtDeleteEdgeId2.Clear();
                MessageBox.Show("Đã xóa cạnh thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        // Async chạy ngầm để giao diện không bị lag khi tải file lớn
        private async void BtnLoadCSV_Click(object sender, EventArgs e)
        {
            btnLoadCSV.Enabled = false;
            progressBar.Visible = true;
            try
            {
                // Mở luồng Task chạy ngầm để lấy Graph
                Graph loadedGraph = await Task.Run(() => 
                {
                    string locPath = "DS_Location.csv";
                    string edgePath = "DS_Edge.csv";

                    // Khi chạy Debug trong Visual Studio, thư mục gốc thường nằm ở bin/Debug
                    // Code tự động dò tìm lùi lại các thư mục cha để tìm file CSV.
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    if (!File.Exists(locPath))
                    {
                        if (File.Exists(Path.Combine(baseDir, @"..\..\", locPath))) // Cho .NET Framework
                        {
                            locPath = Path.Combine(baseDir, @"..\..\", locPath);
                            edgePath = Path.Combine(baseDir, @"..\..\", edgePath);
                        }
                        else if (File.Exists(Path.Combine(baseDir, @"..\..\..\", locPath))) // Cho .NET Core / .NET 5+
                        {
                            locPath = Path.Combine(baseDir, @"..\..\..\", locPath);
                            edgePath = Path.Combine(baseDir, @"..\..\..\", edgePath);
                        }
                    }

                    return GraphLoader.LoadGraph(locPath, edgePath);
                });

                this.graph = loadedGraph;
                currentPath = null;
                lastPathWeight = -1;
                selectedVertex = null;
                selectedEdgeKey = null;
                
                // Tự động căn chỉnh lại bản đồ về tọa độ chuẩn
                offsetX = 50f; offsetY = 50f; scale = 1.0f;
                
                UpdateUIAfterGraphChange();
            }
            catch (Exception ex)
            {
                ShowError("Đã có lỗi khi load CSV:\n" + ex.Message);
            }
            finally
            {
                btnLoadCSV.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void BtnFindPath_Click(object sender, EventArgs e)
        {
            try
            {
                string src = txtFindSource.Text.Trim();
                string dest = txtFindDest.Text.Trim();

                DijkstraEngine engine = new DijkstraEngine(graph);
                PathResult result = engine.FindShortestPath(src, dest);

                if (result == null)
                {
                    ShowError("Không thể tìm thấy đường đi nào kết nối giữa \n[" + src + "] và [" + dest + "].");
                    currentPath = null;
                    lastPathWeight = -1;
                }
                else
                {
                    currentPath = result.Path;
                    lastPathWeight = result.TotalDistance;
                    mapPanel.Invalidate(); // Báo cho Bản đồ vẽ lại để đường đi màu đỏ
                    MessageBox.Show($"Tìm đường thành công!\nTổng trọng số (khoảng cách): {Math.Round(result.TotalDistance, 2)}", "Kết quả Dijkstra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ShowError("Lỗi trong quá trình tìm đường:\n" + ex.Message);
            }
        }

        // #### CÁC HÀM MỞ RỘNG CHO GIAO DIỆN ####

        private void ShowError(string msg)
        {
            lblErrorMessage.Text = msg;
            // Giữa màn hình
            errorPanel.Location = new Point((mapPanel.Width - errorPanel.Width) / 2, (mapPanel.Height - errorPanel.Height) / 2);
            errorPanel.BringToFront();
            errorPanel.Visible = true;
        }

        private void ShowInfo(Vertex v)
        {
            int edgeCount = CountEdges(v);
            lblInfo.Text = $"ID Thành Phố: {v.Id}\nTên: {v.Name}\nTọa độ (X, Y): ({v.X}, {v.Y})\nTổng số đường đi ra: {edgeCount}";
            
            // Hiển thị panel kế bên cursor
            Point p = new Point((int)(v.X * coordScale * scale + offsetX) + 15, (int)(v.Y * coordScale * scale + offsetY) + 15);
            
            // Điều kiện check để panel không bị chìm ra ngoài góc màn hình
            if (p.X + infoPanel.Width > mapPanel.Width) p.X = mapPanel.Width - infoPanel.Width;
            if (p.Y + infoPanel.Height > mapPanel.Height) p.Y = mapPanel.Height - infoPanel.Height;

            infoPanel.Location = p;
            infoPanel.BringToFront();
            infoPanel.Visible = true;
        }

        private void ShowEdgeInfo(string id1, string id2, bool isDirected, double weight, Point mousePos)
        {
            lblInfo.Text = $"CẠNH\nID 1: {id1}\nID 2: {id2}\nCó hướng: {(isDirected ? "Có" : "Không")}\nTrọng số: {Math.Round(weight, 2)}";
            
            // Hiển thị panel cạnh con chuột
            Point p = new Point(mousePos.X + 15, mousePos.Y + 15);
            
            // Đảm bảo panel không bị chìm ra ngoài góc màn hình
            if (p.X + infoPanel.Width > mapPanel.Width) p.X = mapPanel.Width - infoPanel.Width;
            if (p.Y + infoPanel.Height > mapPanel.Height) p.Y = mapPanel.Height - infoPanel.Height;

            infoPanel.Location = p;
            infoPanel.BringToFront();
            infoPanel.Visible = true;
        }

        private double PointToSegmentDistance(double px, double py, double x1, double y1, double x2, double y2)
        {
            double l2 = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            if (l2 == 0) return Math.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1));
            
            double t = ((px - x1) * (x2 - x1) + (py - y1) * (y2 - y1)) / l2;
            t = Math.Max(0, Math.Min(1, t));
            
            double projX = x1 + t * (x2 - x1);
            double projY = y1 + t * (y2 - y1);
            
            return Math.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));
        }

        // Hàm đếm số cạnh của 1 đỉnh
        private int CountEdges(Vertex v)
        {
            int count = 0;
            ListNode<Edge> cur = v.Edges.Head;
            while(cur != null) { count++; cur = cur.Next; }
            return count;
        }

        // Cập nhật lại Winform khi dữ liệu thay đổi
        private void UpdateUIAfterGraphChange()
        {
            lblVertexCount.Text = "Tổng số đỉnh: " + graph.VertexCount;
            
            // Tính năng auto hiện ID khi nhập ra
            // Cập nhật lại danh sách thành phố vào bộ nhớ Gợi ý
            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
            foreach (Vertex v in graph.GetAllVertices())
            {
                collection.Add(v.Id); // Gợi ý nhập nhanh ID
            }
            
            txtEdgeId1.AutoCompleteCustomSource = collection;
            txtEdgeId2.AutoCompleteCustomSource = collection;
            txtFindSource.AutoCompleteCustomSource = collection;
            txtFindDest.AutoCompleteCustomSource = collection;
            txtDeletePointId.AutoCompleteCustomSource = collection;
            txtDeleteEdgeId1.AutoCompleteCustomSource = collection;
            txtDeleteEdgeId2.AutoCompleteCustomSource = collection;

            mapPanel.Invalidate(); // Vẽ lại
        }

        private void EnableAutoComplete(TextBox txt)
        {
            txt.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txt.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        // #### XỬ LÝ CHUỘT VÀ VẼ BẢN ĐỒ ####

        private void MapPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // Zoom in / out khi lăn bi chuột
            if (e.Delta > 0) scale *= 1.1f;
            else scale /= 1.1f;
            
            mapPanel.Invalidate();
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastMousePos = e.Location;
            }
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Di chuyển Bản đồ theo độ di chuyển của chuột
                offsetX += (e.X - lastMousePos.X);
                offsetY += (e.Y - lastMousePos.Y);
                lastMousePos = e.Location;
                mapPanel.Invalidate(); // Vẽ lại liên tục
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void MapPanel_MouseClick(object sender, MouseEventArgs e)
        {
            infoPanel.Visible = false; // Tắt info cũ (nếu có)

            // Đưa tọa độ màn hình lại thành tọa độ thực của thuật toán (0-1000)
            float worldX = (e.X - offsetX) / scale / coordScale;
            float worldY = (e.Y - offsetY) / scale / coordScale;

            // Phạm vi sai số khi click
            double hitRadius = 15 / scale / coordScale; 
            
            foreach (Vertex v in graph.GetAllVertices())
            {
                // Dùng công thức Pythagore tính khoảng cách chuột -> đỉnh
                double dist = Math.Sqrt(Math.Pow(v.X - worldX, 2) + Math.Pow(v.Y - worldY, 2));
                if (dist <= hitRadius)
                {
                    selectedVertex = v; // Ghi nhận đỉnh đang chọn
                    selectedEdgeKey = null; // Bỏ chọn cạnh
                    mapPanel.Invalidate();
                    ShowInfo(v); // Hiện panel Read-mode
                    return;
                }
            }
            
            // Nếu không click vào đỉnh, kiểm tra xem có click vào cạnh không
            foreach (Vertex v in graph.GetAllVertices())
            {
                ListNode<Edge> edgeNode = v.Edges.Head;
                while (edgeNode != null)
                {
                    Vertex dest = edgeNode.Data.Destination;
                    
                    // Khoảng cách từ điểm click đến đoạn thẳng cạnh
                    double edgeDist = PointToSegmentDistance(worldX, worldY, v.X, v.Y, dest.X, dest.Y);
                    
                    // Phạm vi sai số cho đường kẻ mỏng hơn 1 xíu so với đỉnh
                    if (edgeDist <= hitRadius * 0.8) 
                    {
                        selectedVertex = null; // Bỏ chọn đỉnh
                        selectedEdgeKey = v.Id + "_" + dest.Id; // Ghi nhận cạnh đang chọn
                        mapPanel.Invalidate();
                        
                        bool isDirected = !graph.HasEdge(dest, v);
                        ShowEdgeInfo(v.Id, dest.Id, isDirected, edgeNode.Data.Weight, e.Location);
                        return;
                    }
                    
                    edgeNode = edgeNode.Next;
                }
            }
            
            // Nếu click ra ngoài khoảng trống thì bỏ chọn
            selectedVertex = null;
            selectedEdgeKey = null;
            mapPanel.Invalidate();
        }

        // Hàm này được WinForms gọi mỗi khi cần vẽ lại giao diện
        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias; // Bật khử răng cưa cho nét vẽ mượt

            // 1. VẼ LƯỚI TỌA ĐỘ GRIDLINE
            Pen gridPen = new Pen(Color.FromArgb(230, 230, 230), 1);
            int gridSize = 50; // Kích thước mỗi ô vuông
            
            // Vẽ các hoành độ tung độ lấp đầy panel hiện tại
            for (int i = -100; i < 200; i++)
            {
                float x = i * gridSize * scale + offsetX % (gridSize * scale);
                g.DrawLine(gridPen, x, 0, x, mapPanel.Height);

                float y = i * gridSize * scale + offsetY % (gridSize * scale);
                g.DrawLine(gridPen, 0, y, mapPanel.Width, y);
            }

            // Dịch chuyển ma trận gốc của Graphics theo thông số Kéo thả & Zoom
            g.TranslateTransform(offsetX, offsetY);
            g.ScaleTransform(scale, scale);

            // Các công cụ vẽ
            Pen normalEdgePen = new Pen(Color.FromArgb(90, 150, 150, 150), 1); // Cạnh bình thường mờ đi (Alpha 90) để đỡ rối
            Pen highlightEdgePen = new Pen(Color.Red, 3); // Đường đi tô đậm màu đỏ
            
            Pen normalDirectedPen = new Pen(Color.FromArgb(90, 150, 150, 150), 1);
            normalDirectedPen.CustomEndCap = new AdjustableArrowCap(4, 4);

            Pen highlightDirectedPen = new Pen(Color.Red, 3);
            highlightDirectedPen.CustomEndCap = new AdjustableArrowCap(4, 4);

            Pen selectedEdgePen = new Pen(Color.DodgerBlue, 3); // Bút vẽ cạnh đang chọn
            Pen selectedDirectedPen = new Pen(Color.DodgerBlue, 3);
            selectedDirectedPen.CustomEndCap = new AdjustableArrowCap(4, 4);
            
            Brush normalNodeBrush = Brushes.Black;
            Brush selectedNodeBrush = Brushes.DodgerBlue;
            Brush highlightedNodeBrush = Brushes.Red;
            Font nameFont = new Font("Arial", 8, FontStyle.Bold);

            // Ghi nhớ các cạnh thuộc Đường đi ngắn nhất để sau đó tô màu đỏ
            HashSet<string> highlightedEdges = new HashSet<string>();
            HashSet<string> highlightedNodes = new HashSet<string>();
            
            if (currentPath != null && currentPath.Head != null)
            {
                ListNode<Vertex> cur = currentPath.Head;
                while (cur != null)
                {
                    highlightedNodes.Add(cur.Data.Id); // Lấy danh sách đỉnh màu đỏ
                    
                    if (cur.Next != null)
                    {
                        // Highlight cạnh từ điểm bắt đầu và kết thúc
                        highlightedEdges.Add(cur.Data.Id + "_" + cur.Next.Data.Id);
                        highlightedEdges.Add(cur.Next.Data.Id + "_" + cur.Data.Id);
                    }
                    cur = cur.Next;
                }
            }

            // 2. VẼ TOÀN BỘ CÁC CẠNH
            foreach (Vertex v in graph.GetAllVertices())
            {
                ListNode<Edge> edgeNode = v.Edges.Head;
                while (edgeNode != null)
                {
                    Vertex dest = edgeNode.Data.Destination;
                    string edgeKey = v.Id + "_" + dest.Id;

                    bool isHighlighted = highlightedEdges.Contains(edgeKey);
                    bool isSelected = (selectedEdgeKey == edgeKey || selectedEdgeKey == dest.Id + "_" + v.Id);
                    
                    // Kiểm tra xem đây có phải là cạnh có hướng không (nếu đỉnh kia không có cạnh ngược lại)
                    bool isDirected = !graph.HasEdge(dest, v);

                    float startX = (float)(v.X * coordScale);
                    float startY = (float)(v.Y * coordScale);
                    float endX = (float)(dest.X * coordScale);
                    float endY = (float)(dest.Y * coordScale);

                    if (isDirected)
                    {
                        // Thu ngắn điểm kết thúc một chút để mũi tên không bị đỉnh (hình tròn) đè lên
                        double dx = dest.X - v.X;
                        double dy = dest.Y - v.Y;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        if (dist > 0)
                        {
                            float offset = 5f; // Rút ngắn lại bằng bán kính của đỉnh
                            endX = (float)(endX - (dx / dist) * offset);
                            endY = (float)(endY - (dy / dist) * offset);
                        }

                        if (isHighlighted)
                        {
                            // Nếu cạnh này nằm trong đường đi Dijkstra thì vẽ màu đỏ
                            g.DrawLine(highlightDirectedPen, startX, startY, endX, endY);
                        }
                        else if (isSelected)
                            g.DrawLine(selectedDirectedPen, startX, startY, endX, endY);
                        else
                            g.DrawLine(normalDirectedPen, startX, startY, endX, endY);
                    }
                    else
                    {
                        // Vẽ cạnh vô hướng bình thường
                        if (isHighlighted)
                        {
                            // Nếu cạnh này nằm trong đường đi Dijkstra thì vẽ màu đỏ
                            g.DrawLine(highlightEdgePen, startX, startY, endX, endY);
                        }
                        else if (isSelected)
                            g.DrawLine(selectedEdgePen, startX, startY, endX, endY);
                        else
                            g.DrawLine(normalEdgePen, startX, startY, endX, endY);
                    }

                    edgeNode = edgeNode.Next;
                }
            }

            // 3. VẼ CÁC ĐỈNH VÀ TÊN THÀNH PHỐ
            float r = 5f; // Bán kính vòng tròn của đỉnh
            foreach (Vertex v in graph.GetAllVertices())
            {
                float drawX = (float)(v.X * coordScale);
                float drawY = (float)(v.Y * coordScale);

                // Tô màu dựa theo trạng thái: Đang chọn, Nằm trong đường đi Dijkstra, Bình thường
                Brush currentBrush = normalNodeBrush;
                if (highlightedNodes.Contains(v.Id)) currentBrush = highlightedNodeBrush;
                if (v == selectedVertex) currentBrush = selectedNodeBrush; // Ưu tiên màu xanh đang chọn nhất

                // Vẽ cục tròn
                g.FillEllipse(currentBrush, drawX - r, drawY - r, r * 2, r * 2);
                
                // Vẽ text là tên thành phố
                // Chỉ vẽ chữ khi Zoom to (scale >= 1.5) HOẶC đỉnh đang được chọn HOẶC đỉnh nằm trong đường đi
                if (scale >= 1f || v == selectedVertex || highlightedNodes.Contains(v.Id))
                {
                    string text = v.Name;
                    float textX = drawX - r;
                    float textY = drawY + r + 3;

                    // Vẽ một lớp nền trắng mờ dưới gầm chữ để chữ luôn nổi bật, không bị đường kẻ đâm xuyên
                    SizeF textSize = g.MeasureString(text, nameFont);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), textX, textY, textSize.Width, textSize.Height);

                    g.DrawString(text, nameFont, Brushes.Black, textX, textY);
                }
            }

            // 4. VẼ TỔNG TRỌNG SỐ LÊN BẢN ĐỒ
            if (currentPath != null && currentPath.Head != null && lastPathWeight >= 0)
            {
                Vertex dest = null;
                ListNode<Vertex> cur = currentPath.Head;
                while (cur != null) { dest = cur.Data; cur = cur.Next; }

                if (dest != null)
                {
                    string weightText = $"TỔNG TRỌNG SỐ: {Math.Round(lastPathWeight, 2)}";
                    Font weightFont = new Font("Arial", 11, FontStyle.Bold);
                    float drawX = (float)(dest.X * coordScale);
                    float drawY = (float)(dest.Y * coordScale);

                    SizeF size = g.MeasureString(weightText, weightFont);
                    float boxX = drawX - size.Width / 2;
                    float boxY = drawY - 30; // Nổi phía trên

                    g.FillRectangle(new SolidBrush(Color.FromArgb(240, 255, 255, 200)), boxX, boxY, size.Width, size.Height);
                    g.DrawRectangle(Pens.Orange, boxX, boxY, size.Width, size.Height);
                    g.DrawString(weightText, weightFont, Brushes.Red, boxX, boxY);
                }
            }
        }
    }

    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}

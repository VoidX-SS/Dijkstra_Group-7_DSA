using System;
using System.Collections.Generic;

namespace DoAnCuoiKy_Dijkstra
{
    public class Graph
    {
        // Danh sách các đỉnh trong đồ thị (lưu bằng LinkedList tự cài đặt)
        private MyLinkedList<Vertex> vertices;

        // Constructor: khởi tạo danh sách đỉnh rỗng
        public Graph()
        {
            vertices = new MyLinkedList<Vertex>();
        }

        // Trả về danh sách đỉnh
        public MyLinkedList<Vertex> Vertices
        {
            get { return vertices; }
        }

        // Trả về số lượng đỉnh trong đồ thị
        public int VertexCount()
        {
            return vertices.Count;
        }

        // Thêm một đỉnh mới vào đồ thị
        public void AddVertex(string id, string name, double x, double y)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("Mã địa điểm không hợp lệ.");

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Tên địa điểm không hợp lệ.");

            // Chuẩn hóa chuỗi (xóa khoảng trắng dư)
            id = id.Trim();
            name = name.Trim();

            // Kiểm tra trùng ID (không cho phép trùng đỉnh)
            if (FindVertexById(id) != null)
                throw new Exception("Mã địa điểm đã tồn tại.");

            // Tạo đỉnh mới và thêm vào cuối danh sách
            Vertex vertex = new Vertex(id, name, x, y);
            vertices.AddLast(vertex);
        }

        // Tìm đỉnh theo ID (duyệt tuyến tính LinkedList - O(n))
        public Vertex FindVertexById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            id = id.Trim();

            ListNode<Vertex> current = vertices.Head;

            // Duyệt từng node trong danh sách
            while (current != null)
            {
                // So sánh theo Id
                if (current.Data != null && current.Data.Id == id)
                    return current.Data;

                current = current.Next;
            }

            // Không tìm thấy
            return null;
        }

        // Lấy đỉnh theo vị trí index
        public Vertex GetVertexAt(int index)
        {
            // Kiểm tra index hợp lệ
            if (index < 0 || index >= vertices.Count)
                throw new Exception("Chỉ số đỉnh không hợp lệ.");

            return vertices.GetAt(index);
        }

        // Lấy vị trí (index) của một đỉnh trong danh sách
        public int GetVertexIndex(Vertex vertex)
        {
            if (vertex == null)
                return -1;

            ListNode<Vertex> current = vertices.Head;
            int index = 0;

            // Duyệt danh sách để tìm vị trí
            while (current != null)
            {
                // So sánh theo Id
                if (current.Data != null && current.Data.Id == vertex.Id)
                    return index;

                current = current.Next;
                index++;
            }

            return -1; // Không tìm thấy
        }

        // Kiểm tra xem có cạnh nối từ 'from' đến 'to' hay không
        public bool HasEdge(Vertex from, Vertex to)
        {
            if (from == null || to == null)
                return false;

            // Nếu danh sách cạnh rỗng thì chắc chắn không có
            if (from.Edges == null || from.Edges.Head == null)
                return false;

            ListNode<Edge> current = from.Edges.Head;

            // Duyệt danh sách cạnh của đỉnh 'from'
            while (current != null)
            {
                // Kiểm tra đỉnh đích có trùng không
                if (current.Data != null &&
                    current.Data.Destination != null &&
                    current.Data.Destination.Id == to.Id)
                {
                    return true;
                }

                current = current.Next;
            }

            return false;
        }

        // Tính khoảng cách Euclid giữa 2 đỉnh (trọng số cạnh)
        public double CalculateEuclideanDistance(Vertex a, Vertex b)
        {
            if (a == null || b == null)
                throw new Exception("Đỉnh không hợp lệ.");

            double dx = a.X - b.X;
            double dy = a.Y - b.Y;

            // Công thức khoảng cách: sqrt(dx^2 + dy^2)
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Thêm cạnh vô hướng (2 chiều)
        public void AddUndirectedEdge(string id1, string id2)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(id1) || string.IsNullOrWhiteSpace(id2))
                throw new Exception("Mã địa điểm không hợp lệ.");

            id1 = id1.Trim();
            id2 = id2.Trim();

            // Không cho nối chính nó
            if (id1 == id2)
                throw new Exception("Không thể nối một địa điểm với chính nó.");

            // Tìm 2 đỉnh
            Vertex v1 = FindVertexById(id1);
            Vertex v2 = FindVertexById(id2);

            if (v1 == null || v2 == null)
                throw new Exception("Một trong hai địa điểm không tồn tại.");

            // Kiểm tra cạnh đã tồn tại chưa
            if (HasEdge(v1, v2) || HasEdge(v2, v1))
                throw new Exception("Cạnh đã tồn tại.");

            // Tính khoảng cách (trọng số)
            double distance = CalculateEuclideanDistance(v1, v2);

            // Thêm cạnh 2 chiều
            v1.Edges.AddLast(new Edge(v2, distance));
            v2.Edges.AddLast(new Edge(v1, distance));
        }

        // Thêm cạnh có hướng (1 chiều)
        public void AddDirectedEdge(string fromId, string toId)
        {
            if (string.IsNullOrWhiteSpace(fromId) || string.IsNullOrWhiteSpace(toId))
                throw new Exception("Mã địa điểm không hợp lệ.");

            fromId = fromId.Trim();
            toId = toId.Trim();

            if (fromId == toId)
                throw new Exception("Không thể nối một địa điểm với chính nó.");

            // Tìm đỉnh nguồn và đích
            Vertex from = FindVertexById(fromId);
            Vertex to = FindVertexById(toId);

            if (from == null || to == null)
                throw new Exception("Một trong hai địa điểm không tồn tại.");

            // Kiểm tra cạnh đã tồn tại
            if (HasEdge(from, to))
                throw new Exception("Cạnh đã tồn tại.");

            // Tính trọng số
            double distance = CalculateEuclideanDistance(from, to);

            // Thêm cạnh một chiều
            from.Edges.AddLast(new Edge(to, distance));
        }
    }
}

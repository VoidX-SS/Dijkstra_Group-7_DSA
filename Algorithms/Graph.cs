using System;

namespace DoAnCuoiKy_Dijkstra
{
    // Cấu trúc đồ thị quản lý toàn bộ các điểm và các cạnh nối
    public class Graph
    {
        // Sử dụng CustomDictionary (HashTable) để tìm đỉnh theo ID
        private CustomDictionary<string, Vertex> vertexMap;

        public Graph()
        {
            vertexMap = new CustomDictionary<string, Vertex>();
        }

        // Lấy danh sách tất cả các đỉnh
        public CustomList<Vertex> GetAllVertices()
        {
            return vertexMap.Values;
        }

        // Trả về số lượng đỉnh
        public int VertexCount 
        { 
            get { return vertexMap.Count; } 
        }

        // Thêm một đỉnh mới vào đồ thị
        public void AddVertex(string id, string name, double x, double y)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("ID và Name không được để trống.");

            id = id.Trim();
            name = name.Trim();

            if (vertexMap.ContainsKey(id))
                throw new ArgumentException("Đỉnh có mã đã tồn tại.");

            Vertex vertex = new Vertex(id, name, x, y);
            vertexMap.Add(id, vertex); // Lưu vào Dictionary
        }

        // Tìm đỉnh theo ID
        public Vertex GetVertex(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            id = id.Trim();

            if (vertexMap.ContainsKey(id))
                return vertexMap[id];
                
            return null;
        }

        // Kiểm tra xem có cạnh nối từ from tới to không
        public bool HasEdge(Vertex from, Vertex to)
        {
            if (from == null || to == null) return false;

            ListNode<Edge> current = from.Edges.Head;
            while (current != null)
            {
                Edge edge = current.Data;
                if (edge.Destination.Id == to.Id)
                    return true;
                    
                current = current.Next;
            }
            return false;
        }

        // Tính khoảng cách Euclid giữa 2 đỉnh
        private double CalculateEuclideanDistance(Vertex a, Vertex b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Thêm cạnh 2 chiều (Undirected Edge)
        public void AddUndirectedEdge(string id1, string id2)
        {
            id1 = id1.Trim();
            id2 = id2.Trim();

            if (id1 == id2)
                throw new ArgumentException("Không thể nối đỉnh với chính nó.");

            Vertex v1 = GetVertex(id1);
            Vertex v2 = GetVertex(id2);

            if (v1 == null || v2 == null)
                throw new ArgumentException("Một trong hai đỉnh không tồn tại.");

            if (HasEdge(v1, v2))
                return; // Đã tồn tại cạnh

            double distance = CalculateEuclideanDistance(v1, v2);

            v1.Edges.AddLast(new Edge(v2, distance));
            v2.Edges.AddLast(new Edge(v1, distance));
        }

        // Thêm cạnh 1 chiều (Directed Edge) - Dùng nếu có đường một chiều
        public void AddDirectedEdge(string fromId, string toId)
        {
            fromId = fromId.Trim();
            toId = toId.Trim();

            if (fromId == toId)
                throw new ArgumentException("Không thể nối đỉnh với chính nó.");

            Vertex from = GetVertex(fromId);
            Vertex to = GetVertex(toId);

            if (from == null || to == null)
                throw new ArgumentException("Một trong hai đỉnh không tồn tại.");

            if (HasEdge(from, to))
                return;

            double distance = CalculateEuclideanDistance(from, to);
            from.Edges.AddLast(new Edge(to, distance));
        }

        // Xóa đỉnh khỏi đồ thị
        public void RemoveVertex(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Lỗi: ID không được để trống.");
            id = id.Trim();

            if (!vertexMap.ContainsKey(id))
                throw new ArgumentException("Lỗi: Đỉnh không tồn tại trong đồ thị.");

            // 1. Xóa tất cả các cạnh từ các đỉnh khác trỏ tới đỉnh này
            // Sử dụng vòng lặp duyệt qua tất cả các đỉnh
            CustomList<Vertex> allVertices = GetAllVertices();
            for (int i = 0; i < allVertices.Count; i++)
            {
                Vertex v = allVertices[i];
                ListNode<Edge> current = v.Edges.Head;
                while (current != null)
                {
                    ListNode<Edge> next = current.Next; // Lưu lại Next vì có thể current bị xóa
                    if (current.Data.Destination.Id == id)
                        v.Edges.Remove(current.Data);
                    current = next;
                }
            }

            // 2. Xóa đỉnh khỏi từ điển đồ thị
            vertexMap.Remove(id);
        }

        // Xóa cạnh khỏi đồ thị (xóa mọi chiều)
        public void RemoveEdge(string id1, string id2)
        {
            if (string.IsNullOrWhiteSpace(id1) || string.IsNullOrWhiteSpace(id2)) 
                throw new ArgumentException("Lỗi: ID không được để trống.");

            id1 = id1.Trim();
            id2 = id2.Trim();

            Vertex v1 = GetVertex(id1);
            Vertex v2 = GetVertex(id2);

            if (v1 == null || v2 == null)
                throw new ArgumentException("Lỗi: Một trong hai đỉnh không tồn tại.");

            // Thử xóa ở cả 2 đầu, nếu có ít nhất 1 đầu xóa thành công là OK
            bool removed1 = RemoveEdgeFromList(v1, id2);
            bool removed2 = RemoveEdgeFromList(v2, id1);

            if (!removed1 && !removed2)
                throw new ArgumentException("Lỗi: Không tìm thấy cạnh nối giữa hai đỉnh này.");
        }

        // Hàm phụ trợ để tìm và xóa cạnh trong danh sách liên kết của 1 đỉnh
        private bool RemoveEdgeFromList(Vertex from, string toId)
        {
            ListNode<Edge> current = from.Edges.Head;
            while (current != null)
            {
                if (current.Data.Destination.Id == toId)
                {
                    from.Edges.Remove(current.Data);
                    return true;
                }
                current = current.Next;
            }
            return false;
        }
    }
}

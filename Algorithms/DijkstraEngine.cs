using System;
using System.Collections.Generic;

namespace DoAnCuoiKy_Dijkstra
{
    public class DijkstraEngine
    {
        // Đồ thị cần xử lý
        private Graph graph;

        // Constructor: nhận vào đồ thị đã được tạo sẵn
        public DijkstraEngine(Graph graph)
        {
            if (graph == null)
                throw new ArgumentNullException("graph", "Đồ thị không được null.");

            this.graph = graph;
        }

        // Tìm chỉ số của đỉnh chưa xét có khoảng cách nhỏ nhất
        private int GetMinDistanceVertexIndex(double[] distances, bool[] visited)
        {
            double minDistance = double.MaxValue;
            int minIndex = -1;

            for (int i = 0; i < distances.Length; i++)
            {
                if (!visited[i] && distances[i] < minDistance)
                {
                    minDistance = distances[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }

        // Dựng lại đường đi ngắn nhất từ mảng previous
        // previous[i] lưu chỉ số đỉnh đứng trước đỉnh i trên đường đi ngắn nhất
        private CustomLinkedList<Vertex> BuildPath(int[] previous, int startIndex, int endIndex)
        {
            CustomLinkedList<Vertex> path = new CustomLinkedList<Vertex>();
            int currentIndex = endIndex;

            // Truy vết ngược từ đỉnh đích về đỉnh nguồn
            while (currentIndex != -1)
            {
                Vertex currentVertex = graph.GetVertexAt(currentIndex);

                if (currentVertex == null)
                    return null;

                // Thêm vào đầu danh sách để có đúng thứ tự từ nguồn đến đích
                path.AddFirst(currentVertex);

                if (currentIndex == startIndex)
                    break;

                currentIndex = previous[currentIndex];
            }

            // Kiểm tra đường đi có hợp lệ hay không
            Vertex startVertex = graph.GetVertexAt(startIndex);

            if (path.Head == null || startVertex == null || path.Head.Data.Id != startVertex.Id)
                return null;

            return path;
        }

        // Hàm chính: tìm đường đi ngắn nhất từ startId đến endId bằng thuật toán Dijkstra
        public PathResult FindShortestPath(string startId, string endId)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(startId))
                throw new ArgumentException("Mã địa điểm bắt đầu không hợp lệ.", "startId");

            if (string.IsNullOrWhiteSpace(endId))
                throw new ArgumentException("Mã địa điểm kết thúc không hợp lệ.", "endId");

            // Chuẩn hóa dữ liệu
            startId = startId.Trim();
            endId = endId.Trim();

            // Kiểm tra đồ thị
            int n = graph.VertexCount();
            if (n <= 0)
                throw new InvalidOperationException("Đồ thị rỗng, không thể tìm đường đi.");

            // Tìm đỉnh bắt đầu và đỉnh kết thúc
            Vertex startVertex = graph.FindVertexById(startId);
            Vertex endVertex = graph.FindVertexById(endId);

            if (startVertex == null)
                throw new Exception("Không tìm thấy địa điểm bắt đầu.");

            if (endVertex == null)
                throw new Exception("Không tìm thấy địa điểm kết thúc.");

            // Lấy vị trí của hai đỉnh trong đồ thị
            int startIndex = graph.GetVertexIndex(startVertex);
            int endIndex = graph.GetVertexIndex(endVertex);

            if (startIndex == -1 || endIndex == -1)
                throw new Exception("Không xác định được vị trí đỉnh trong đồ thị.");

            // Trường hợp điểm bắt đầu trùng điểm kết thúc
            if (startIndex == endIndex)
            {
                CustomLinkedList<Vertex> samePath = new CustomLinkedList<Vertex>();
                samePath.AddFirst(startVertex);

                return new PathResult(samePath, 0);
            }

            // distances[i]: khoảng cách ngắn nhất tạm thời từ đỉnh bắt đầu đến đỉnh i
            double[] distances = new double[n];

            // visited[i]: đánh dấu đỉnh i đã được xét hay chưa
            bool[] visited = new bool[n];

            // previous[i]: lưu chỉ số đỉnh đứng trước i trên đường đi ngắn nhất
            int[] previous = new int[n];

            // Khởi tạo ban đầu
            for (int i = 0; i < n; i++)
            {
                distances[i] = double.MaxValue;
                visited[i] = false;
                previous[i] = -1;
            }

            // Khoảng cách từ đỉnh bắt đầu đến chính nó bằng 0
            distances[startIndex] = 0;

            // Vòng lặp chính của thuật toán Dijkstra
            for (int count = 0; count < n; count++)
            {
                // Chọn đỉnh chưa xét có khoảng cách nhỏ nhất
                int currentIndex = GetMinDistanceVertexIndex(distances, visited);

                if (currentIndex == -1)
                    break;

                // Nếu khoảng cách vẫn là vô cực thì các đỉnh còn lại không thể đi tới
                if (distances[currentIndex] == double.MaxValue)
                    break;

                visited[currentIndex] = true;

                // Nếu đã tới đích thì dừng sớm
                if (currentIndex == endIndex)
                    break;

                Vertex currentVertex = graph.GetVertexAt(currentIndex);

                if (currentVertex == null || currentVertex.Edges == null)
                    continue;

                // Duyệt các cạnh kề của đỉnh hiện tại
                ListNode<Edge> edgeNode = currentVertex.Edges.Head;

                while (edgeNode != null)
                {
                    Edge edge = edgeNode.Data;

                    if (edge != null && edge.Destination != null)
                    {
                        // Dijkstra không hỗ trợ cạnh có trọng số âm
                        if (edge.Weight < 0)
                            throw new Exception("Thuật toán Dijkstra không hỗ trợ cạnh có trọng số âm.");

                        int neighborIndex = graph.GetVertexIndex(edge.Destination);

                        // Chỉ xét đỉnh kề chưa được duyệt
                        if (neighborIndex != -1 && !visited[neighborIndex])
                        {
                            double newDistance = distances[currentIndex] + edge.Weight;

                            // Nếu tìm được đường đi ngắn hơn thì cập nhật
                            if (newDistance < distances[neighborIndex])
                            {
                                distances[neighborIndex] = newDistance;
                                previous[neighborIndex] = currentIndex;
                            }
                        }
                    }

                    edgeNode = edgeNode.Next;
                }
            }

            // Nếu khoảng cách tới đích vẫn là vô cực thì không tồn tại đường đi
            if (distances[endIndex] == double.MaxValue)
                return null;

            // Dựng lại đường đi ngắn nhất
            CustomLinkedList<Vertex> path = BuildPath(previous, startIndex, endIndex);

            if (path == null)
                return null;

            // Trả về kết quả cuối cùng
            return new PathResult(path, distances[endIndex]);
        }
    }
}

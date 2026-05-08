using System;

namespace DoAnCuoiKy_Dijkstra
{
    public class DijkstraEngine
    {
        private Graph graph;

        public DijkstraEngine(Graph graph)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            this.graph = graph;
        }

        // Dựng lại đường đi từ điển lưu vết
        private CustomLinkedList<Vertex> BuildPath(CustomDictionary<Vertex, Vertex> previous, Vertex startVertex, Vertex endVertex)
        {
            CustomLinkedList<Vertex> path = new CustomLinkedList<Vertex>();
            Vertex current = endVertex;

            while (current != null)
            {
                path.AddFirst(current); // Thêm vào đầu để đúng thứ tự Nguồn -> Đích

                if (current == startVertex)
                    break;

                // Nếu không có vết để đi lùi và chưa đến điểm đầu thì không có đường đi
                if (!previous.ContainsKey(current))
                    return null;

                current = previous[current];
            }

            return path;
        }

        // Tìm đường đi ngắn nhất bằng MinHeap
        public PathResult FindShortestPath(string startId, string endId)
        {
            if (string.IsNullOrWhiteSpace(startId) || string.IsNullOrWhiteSpace(endId))
                throw new ArgumentException("ID không hợp lệ.");

            Vertex startVertex = graph.GetVertex(startId);
            Vertex endVertex = graph.GetVertex(endId);

            if (startVertex == null || endVertex == null)
                throw new ArgumentException("Điểm bắt đầu hoặc kết thúc không tồn tại trong đồ thị.");

            // Trường hợp bắt đầu trùng kết thúc
            if (startVertex == endVertex)
            {
                CustomLinkedList<Vertex> samePath = new CustomLinkedList<Vertex>();
                samePath.AddLast(startVertex);
                return new PathResult(samePath, 0);
            }

            // distances: Bản đồ lưu khoảng cách ngắn nhất tạm thời từ đỉnh xuất phát tới các đỉnh khác
            CustomDictionary<Vertex, double> distances = new CustomDictionary<Vertex, double>();
            // previous: Bản đồ lưu đỉnh liền trước trên đường đi ngắn nhất (để truy vết)
            CustomDictionary<Vertex, Vertex> previous = new CustomDictionary<Vertex, Vertex>();
            // minHeap: Hàng đợi ưu tiên lấy đỉnh có khoảng cách ngắn nhất
            MinHeap<Vertex> minHeap = new MinHeap<Vertex>();

            // Khởi tạo trạng thái ban đầu: Khoảng cách tới tất cả đỉnh là vô cực
            CustomList<Vertex> allVertices = graph.GetAllVertices();
            for (int i = 0; i < allVertices.Count; i++)
            {
                Vertex v = allVertices[i];
                distances.Add(v, double.MaxValue);
            }

            // Khoảng cách tới điểm xuất phát = 0
            distances[startVertex] = 0;
            minHeap.Insert(startVertex, 0);

            // Vòng lặp chính của Dijkstra
            while (!minHeap.IsEmpty())
            {
                // Bước 1: Lấy đỉnh có khoảng cách nhỏ nhất trong các đỉnh chưa xét xong
                Vertex currentVertex = minHeap.ExtractMin();
                double currentDistance = distances[currentVertex];

                // Nếu đã đến đích thì có thể dừng
                if (currentVertex == endVertex)
                    break;

                // Bước 2: Duyệt qua các cạnh kề của đỉnh hiện tại bằng vòng lặp while với con trỏ
                ListNode<Edge> edgeNode = currentVertex.Edges.Head;
                while (edgeNode != null)
                {
                    Edge edge = edgeNode.Data;
                    Vertex neighbor = edge.Destination;
                    double weight = edge.Weight;

                    if (weight < 0)
                        throw new InvalidOperationException("Dijkstra không hỗ trợ cạnh có trọng số âm.");

                    double newDistance = currentDistance + weight;

                    // Bước 3: Nếu tìm được đường ngắn hơn thì cập nhật lại
                    if (newDistance < distances[neighbor])
                    {
                        distances[neighbor] = newDistance;
                        
                        if (previous.ContainsKey(neighbor))
                            previous[neighbor] = currentVertex; 
                        else
                            previous.Add(neighbor, currentVertex); 

                        // Nếu đỉnh đã có trong heap thì cập nhật độ ưu tiên, nếu chưa thì thêm vào
                        if (minHeap.Contains(neighbor))
                            minHeap.DecreaseKey(neighbor, newDistance);
                        else
                            minHeap.Insert(neighbor, newDistance);
                    }
                    
                    edgeNode = edgeNode.Next;
                }
            }

            // Nếu khoảng cách đến đích vẫn là vô cực, tức là không có đường đi nối giữa 2 đỉnh
            if (distances[endVertex] == double.MaxValue)
                return null;

            // Xây dựng lại danh sách đường đi từ vết lưu
            CustomLinkedList<Vertex> path = BuildPath(previous, startVertex, endVertex);
            if (path == null) 
                return null;

            return new PathResult(path, distances[endVertex]);
        }
    }
}

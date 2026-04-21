using System;
using System.Collections.Generic;

namespace DoAnCuoiKy_Dijkstra
{
    public class PathResult
    {
        // Danh sách các đỉnh thuộc đường đi ngắn nhất
        // Dùng CustomLinkedList để thống nhất với cấu trúc dữ liệu của đồ án
        public CustomLinkedList<Vertex> Path { get; private set; }

        // Tổng khoảng cách của đường đi ngắn nhất
        public double TotalDistance { get; private set; }

        // Constructor: khởi tạo kết quả đường đi
        public PathResult(CustomLinkedList<Vertex> path, double totalDistance)
        {
            // Kiểm tra danh sách đường đi hợp lệ
            if (path == null)
                throw new ArgumentNullException("path", "Danh sách đường đi không được null.");

            // Kiểm tra tổng khoảng cách hợp lệ
            if (totalDistance < 0)
                throw new ArgumentOutOfRangeException("totalDistance", "Tổng khoảng cách không được âm.");

            // Gán giá trị cho thuộc tính
            Path = path;
            TotalDistance = totalDistance;
        }
    }
}

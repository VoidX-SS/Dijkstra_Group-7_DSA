using System;
using System.Collections.Generic;

namespace DoAnCuoiKy_Dijkstra
{
    public class Vertex
    {
        // Mã địa điểm (duy nhất)
        public string Id { get; private set; }

        // Tên địa điểm
        public string Name { get; private set; }

        // Tọa độ X (dùng để tính khoảng cách)
        public double X { get; private set; }

        // Tọa độ Y (dùng để tính khoảng cách)
        public double Y { get; private set; }

        // Danh sách các cạnh kề (adjacency list)
        // Mỗi cạnh nối đến một đỉnh khác kèm theo trọng số
        public CustomLinkedList<Edge> Edges { get; private set; }

        // Constructor: khởi tạo một đỉnh
        public Vertex(string id, string name, double x, double y)
        {
            // Kiểm tra dữ liệu đầu vào
            // Không cho phép null, rỗng hoặc chỉ chứa khoảng trắng
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("Mã địa điểm không hợp lệ.");

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Tên địa điểm không hợp lệ.");

            // Chuẩn hóa dữ liệu (loại bỏ khoảng trắng dư)
            Id = id.Trim();
            Name = name.Trim();

            // Gán tọa độ
            X = x;
            Y = y;

            // Khởi tạo danh sách cạnh kề rỗng
            // Dùng LinkedList theo yêu cầu đề bài
            Edges = new CustomLinkedList<Edge>();
        }

        // Ghi đè phương thức ToString để hiển thị thông tin đỉnh
        // Dùng khi đưa dữ liệu lên giao diện (ListBox, ComboBox, ...)
        public override string ToString()
        {
            // Format số thực gọn lại (tối đa 2 chữ số thập phân)
            return Id + " - " + Name + " (" + X.ToString("0.##") + ", " + Y.ToString("0.##") + ")";
        }
    }
}

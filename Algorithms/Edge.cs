using System;

namespace DoAnCuoiKy_Dijkstra
{
    public class Edge
    {
        // Đỉnh đích mà cạnh này trỏ tới
        public Vertex Destination { get; private set; }

        // Trọng số của cạnh (khoảng cách)
        public double Weight { get; private set; }

        // Constructor: khởi tạo cạnh
        public Edge(Vertex destination, double weight)
        {
            // Kiểm tra dữ liệu đầu vào
            if (destination == null)
                throw new Exception("Đỉnh đích không hợp lệ.");

            if (weight < 0)
                throw new Exception("Trọng số không hợp lệ (không được âm).");

            Destination = destination;
            Weight = weight;
        }

        // Hiển thị thông tin cạnh
        public override string ToString()
        {
            return Destination.Name + " - " + Weight.ToString("0.00");
        }
    }
}

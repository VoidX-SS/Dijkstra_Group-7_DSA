**1. Đồ án cuối kì:** Tìm đường đi ngắn nhất bằng thuật toán Dijkstra  
**2. Môn**: Cấu trúc dữ liệu và giải thuật  
**3. Tên thành viên**
  + Nguyễn Nguyễn Minh Nhân (31251021644)
  + Bùi Thị Huyền Trang (31251028194)
  + Nguyễn An Khang (31251027441)
  + Vương Nguyễn Minh Nhật (31251022055)
  
**4. Chi tiết:**
Hệ thống là một giải pháp tìm đường đi ngắn nhất trong mạng lưới giao thông đô thị, được tối ưu hóa sâu về cả thời gian thực thi lẫn không gian lưu trữ bộ nhớ bằng MinHeap và cấu trúc LinkedList.

Tính năng:
- Thuật toán: Triển khai thuật toán Dijkstra trên không gian đồ thị 2D sử dụng khoảng cách Euclid làm trọng số.
- Động cơ hiệu năng cao: Thay thế mảng tuần tự bằng cấu trúc MinHeap, kết hợp cùng kỹ thuật DecreaseKey giúp tối ưu hóa thời gian tìm kiếm đỉnh nhỏ nhất. Độ phức tạp thuật toán giảm từ O(V²) xuống O((V+E) ⋅ logV).
- Quản lý bộ nhớ linh hoạt: Xây dựng CustomLinkedList từ con số 0 để quản lý danh sách kề thay vì dùng ma trận kề, giúp không gian lưu trữ chỉ tiêu tốn O(V+E), tránh phân mảnh bộ nhớ. Ngoài ra còn xây dựng lại cấu trúc CustomList và CustomDictionary phù hợp.
- Giao diện trực quan (WinForms): Giao diện tương tác người dùng tích hợp thuật toán chọn điểm trên bản đồ. Có các nút chức năng để vận hành tìm đường đi ngắn nhất trong thành phố.
- Bảo vệ khỏi kịch bản ngoại lệ: Xử lý an toàn các kịch bản thực tế như đồ thị không liên thông, ID đỉnh không tồn tại, hoặc nhập sai định dạng dữ liệu.

Đánh giá hiệu năng:

Hệ thống đã trải qua kiểm thử sức chịu tải khắc nghiệt bằng hàm sinh dữ liệu tự động với mật độ cạnh E ≈ 3V.
- Tốc độ xử lý: Xử lý đồ thị lên đến 150,000 đỉnh với thời gian trung bình là 1346.79 ms.
- Mức tiêu thụ RAM: Ở mốc cực đại 5,000,000 đỉnh, bộ nhớ lưu trữ cấp phát thêm chỉ tốn 1.13 MB.

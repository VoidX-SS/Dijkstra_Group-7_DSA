using System;

namespace DoAnCuoiKy_Dijkstra
{
    // Tự cài đặt lại danh sách mảng động để thay thế List<T> của C#, 
    public class CustomList<T> : System.Collections.Generic.IEnumerable<T>
    {
        // Mảng tĩnh bên dưới để chứa dữ liệu
        private T[] items;
        // Biến đếm số lượng phần tử hiện có
        private int count;

        // Trả về số lượng phần tử
        public int Count
        {
            get { return count; }
        }

        // Khởi tạo sức chứa mặc định là 4 ô
        public CustomList(int capacity = 4)
        {
            items = new T[capacity];
            count = 0;
        }

        // Hàm thêm phần tử vào cuối mảng
        public void Add(T item)
        {
            // Nếu mảng đầy thì tự động nhân đôi kích thước
            if (count == items.Length) 
                Resize();
                
            items[count] = item;
            count++;
        }

        // Hàm xóa phần tử tại một vị trí
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException("Lỗi: Vị trí xóa không hợp lệ.");

            // Kéo các phần tử phía sau lùi lên 1 bước để lấp chỗ trống
            for (int i = index; i < count - 1; i++)
                items[i] = items[i + 1];
                
            items[count - 1] = default(T); // Xóa rác
            count--;
        }

        // Hàm tăng sức chứa của mảng lên gấp đôi
        private void Resize()
        {
            int newCapacity = items.Length == 0 ? 4 : items.Length * 2;
            T[] newItems = new T[newCapacity];
            
            for (int i = 0; i < count; i++)
                newItems[i] = items[i];
                
            items = newItems;
        }

        // Indexer để truy cập các phần tử giống như mảng bình thường (vd: list[0])
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                    throw new ArgumentOutOfRangeException("Lỗi: Vị trí không hợp lệ.");
                return items[index];
            }
            set
            {
                if (index < 0 || index >= count)
                    throw new ArgumentOutOfRangeException("Lỗi: Vị trí không hợp lệ.");
                items[index] = value;
            }
        }
        
        // Hỗ trợ vòng lặp foreach ở bên Winform
        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
                yield return items[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

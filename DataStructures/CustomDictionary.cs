using System;

namespace DoAnCuoiKy_Dijkstra
{
    // Lớp phụ để chứa một cặp Key và Value
    public class CustomKeyValue<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }

        public CustomKeyValue(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }

    // Tự code lại cấu trúc HashTable (Bảng băm) bằng phương pháp mảng 
    // chứa các danh sách liên kết để thay thế Dictionary có sẵn
    public class CustomDictionary<K, V>
    {
        // Mảng chứa các danh sách liên kết, dùng để giải quyết đụng độ collision
        private CustomLinkedList<CustomKeyValue<K, V>>[] buckets;
        
        // Số lượng phần tử đã được thêm vào
        private int count;

        public CustomDictionary(int capacity = 100)
        {
            buckets = new CustomLinkedList<CustomKeyValue<K, V>>[capacity];
            count = 0;
        }

        // Trả về số lượng
        public int Count
        {
            get { return count; }
        }

        // Hàm băm: Đổi key thành con số vị trí index trong mảng
        private int GetBucketIndex(K key)
        {
            if (key == null) 
                throw new ArgumentNullException("Lỗi: Key không được để trống.");
                
            int hash = key.GetHashCode();
            return Math.Abs(hash) % buckets.Length;
        }

        // Hàm thêm một cặp Key-Value mới vào từ điển
        public void Add(K key, V value)
        {
            // Nếu mảng sắp đầy thì tạo mảng mới lớn gấp đôi để tránh đụng độ collision nhiều
            if (count >= buckets.Length * 0.75)
                Resize(buckets.Length * 2);

            int index = GetBucketIndex(key);
            
            if (buckets[index] == null)
                buckets[index] = new CustomLinkedList<CustomKeyValue<K, V>>();

            // Duyệt danh sách liên kết tại vị trí đó để kiểm tra xem có trùng key không
            ListNode<CustomKeyValue<K, V>> current = buckets[index].Head;
            while (current != null)
            {
                if (current.Data.Key.Equals(key))
                    throw new ArgumentException("Lỗi: Key này đã tồn tại rồi.");
                    
                current = current.Next;
            }

            buckets[index].AddLast(new CustomKeyValue<K, V>(key, value));
            count++;
        }

        // Hàm tìm xem có key này không
        public bool ContainsKey(K key)
        {
            int index = GetBucketIndex(key);
            if (buckets[index] == null) 
                return false;

            ListNode<CustomKeyValue<K, V>> current = buckets[index].Head;
            while (current != null)
            {
                if (current.Data.Key.Equals(key)) 
                    return true;
                    
                current = current.Next;
            }
            return false;
        }

        // Hàm xóa một key
        public bool Remove(K key)
        {
            int index = GetBucketIndex(key);
            if (buckets[index] == null) 
                return false;

            ListNode<CustomKeyValue<K, V>> current = buckets[index].Head;
            while (current != null)
            {
                if (current.Data.Key.Equals(key))
                {
                    buckets[index].Remove(current.Data);
                    count--;
                    return true;
                }
                current = current.Next;
            }
            return false;
        }

        // Cài đặt ngoặc vuông [] để có thể lấy hoặc gán giá trị theo kiểu dict["A"]
        public V this[K key]
        {
            get
            {
                int index = GetBucketIndex(key);
                if (buckets[index] != null)
                {
                    ListNode<CustomKeyValue<K, V>> current = buckets[index].Head;
                    while (current != null)
                    {
                        if (current.Data.Key.Equals(key))
                            return current.Data.Value;
                        current = current.Next;
                    }
                }
                throw new Exception("Lỗi: Không tìm thấy Key.");
            }
            set
            {
                int index = GetBucketIndex(key);
                if (buckets[index] == null)
                    buckets[index] = new CustomLinkedList<CustomKeyValue<K, V>>();

                ListNode<CustomKeyValue<K, V>> current = buckets[index].Head;
                while (current != null)
                {
                    // Nếu tìm thấy key cũ thì cập nhật giá trị mới đè lên
                    if (current.Data.Key.Equals(key))
                    {
                        current.Data.Value = value; 
                        return;
                    }
                    current = current.Next;
                }

                // Nếu chưa có thì thêm vô đuôi
                buckets[index].AddLast(new CustomKeyValue<K, V>(key, value));
                count++;
            }
        }

        // Lấy danh sách chứa tất cả các Value 
        public CustomList<V> Values
        {
            get
            {
                CustomList<V> list = new CustomList<V>(count);
                for (int i = 0; i < buckets.Length; i++)
                {
                    if (buckets[i] != null)
                    {
                        ListNode<CustomKeyValue<K, V>> current = buckets[i].Head;
                        while (current != null)
                        {
                            list.Add(current.Data.Value);
                            current = current.Next;
                        }
                    }
                }
                return list;
            }
        }

        // Hàm thay đổi kích thước mảng băm
        private void Resize(int newCapacity)
        {
            CustomLinkedList<CustomKeyValue<K, V>>[] oldBuckets = buckets;
            buckets = new CustomLinkedList<CustomKeyValue<K, V>>[newCapacity];
            count = 0; // Sẽ đếm lại lúc gọi hàm Add

            // Dời dữ liệu cho từng phần tử từ mảng cũ sang mảng mới
            for (int i = 0; i < oldBuckets.Length; i++)
            {
                if (oldBuckets[i] != null)
                {
                    ListNode<CustomKeyValue<K, V>> current = oldBuckets[i].Head;
                    while (current != null)
                    {
                        Add(current.Data.Key, current.Data.Value);
                        current = current.Next;
                    }
                }
            }
        }
    }
}

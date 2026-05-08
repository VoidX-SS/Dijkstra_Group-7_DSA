using System;

namespace DoAnCuoiKy_Dijkstra
{
    // Cài đặt hàng đợi ưu tiên MinHeap bằng CustomList và CustomDictionary.
    // Giúp giảm độ phức tạp thuật toán Dijkstra xuống O((V+E)logV).
    public class MinHeap<T>
    {
        // Lớp nội bộ lưu trữ dữ liệu của đỉnh và độ ưu tiên (khoảng cách)
        private class HeapNode
        {
            public T Item { get; set; }
            public double Priority { get; set; }

            public HeapNode(T item, double priority)
            {
                Item = item;
                Priority = priority;
            }
        }

        // CustomList dùng để lưu trữ các phần tử của Heap theo cấu trúc cây nhị phân
        private CustomList<HeapNode> heap;
        
        // CustomDictionary dùng để lưu index (vị trí) của mỗi đỉnh trong List
        private CustomDictionary<T, int> indexMap;

        // Trả về số lượng phần tử hiện tại trong Heap
        public int Count 
        { 
            get { return heap.Count; } 
        }

        public MinHeap()
        {
            heap = new CustomList<HeapNode>();
            indexMap = new CustomDictionary<T, int>();
        }

        public bool IsEmpty()
        {
            return heap.Count == 0;
        }

        // Tìm vị trí của phần tử trong List.
        private int FindIndex(T item)
        {
            if (item != null && indexMap.ContainsKey(item))
                return indexMap[item];
            return -1;
        }

        public bool Contains(T item)
        {
            return FindIndex(item) != -1;
        }

        // Thêm một đỉnh mới vào Heap
        public void Insert(T item, double priority)
        {
            int index = FindIndex(item);
            if (index != -1)
            {
                // Đỉnh đã tồn tại -> cập nhật lại khoảng cách (ưu tiên)
                DecreaseKey(item, priority);
                return;
            }

            // Tạo node mới và đưa vào cuối mảng List
            HeapNode newNode = new HeapNode(item, priority);
            heap.Add(newNode);
            
            // Cập nhật vị trí của đỉnh vừa thêm vào Dictionary vào vị trí cuối cùng
            indexMap.Add(item, heap.Count - 1);
            
            // Sift Up để đảm bảo phần tử nhỏ nhất luôn ở trên cùng (index 0)
            HeapifyUp(heap.Count - 1);
        }

        // Lấy ra đỉnh có khoảng cách nhỏ nhất (ưu tiên cao nhất)
        public T ExtractMin()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Hàng đợi rỗng.");

            // Phần tử nhỏ nhất luôn nằm ở vị trí 0
            T minItem = heap[0].Item;
            int lastIndex = heap.Count - 1;
            
            // Đưa phần tử cuối cùng lên đầu để lấp chỗ trống
            heap[0] = heap[lastIndex];
            
            // Cập nhật lại vị trí mới của phần tử này trong Dictionary
            indexMap[heap[0].Item] = 0;
            
            // Xóa phần tử ở cuối (vừa được lấy lên đầu)
            heap.RemoveAt(lastIndex);
            
            // Xóa phần tử min khỏi Dictionary
            indexMap.Remove(minItem);

            // Sift Down để khôi phục cấu trúc Min Heap
            if (!IsEmpty())
                HeapifyDown(0);

            return minItem;
        }

        // Cập nhật lại độ ưu tiên (khoảng cách) của một đỉnh nếu tìm được đường đi ngắn hơn
        public void DecreaseKey(T item, double newPriority)
        {
            int index = FindIndex(item);
            if (index == -1)
                return;

            // Chỉ cập nhật nếu khoảng cách mới nhỏ hơn khoảng cách hiện tại
            if (newPriority < heap[index].Priority)
            {
                heap[index].Priority = newPriority;
                
                // Do khoảng cách nhỏ đi, đỉnh có xu hướng nổi lên trên, dùng Sift Up
                HeapifyUp(index);
            }
        }

        // Đẩy phần tử lên trên nếu nó nhỏ hơn phần tử cha
        private void HeapifyUp(int index)
        {
            int parentIndex = (index - 1) / 2;

            while (index > 0 && heap[index].Priority < heap[parentIndex].Priority)
            {
                Swap(index, parentIndex);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }
        }

        // Đẩy phần tử xuống dưới nếu nó lớn hơn phần tử con
        private void HeapifyDown(int index)
        {
            int lastIndex = heap.Count - 1;
            while (true)
            {
                int leftChildIndex = 2 * index + 1;
                int rightChildIndex = 2 * index + 2;
                int smallestIndex = index;

                // Tìm con nhỏ nhất trong 2 con (trái và phải)
                if (leftChildIndex <= lastIndex && heap[leftChildIndex].Priority < heap[smallestIndex].Priority)
                    smallestIndex = leftChildIndex;

                if (rightChildIndex <= lastIndex && heap[rightChildIndex].Priority < heap[smallestIndex].Priority)
                    smallestIndex = rightChildIndex;

                // Nếu có con nhỏ hơn, thực hiện hoán đổi
                if (smallestIndex != index)
                {
                    Swap(index, smallestIndex);
                    index = smallestIndex;
                }
                else
                {
                    break; // Đã cân bằng
                }
            }
        }

        // Hoán đổi 2 phần tử trong List và Dictionary
        private void Swap(int i, int j)
        {
            // Đổi chỗ trong List
            HeapNode temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;

            // Cập nhật lại Index trong Dictionary
            indexMap[heap[i].Item] = i;
            indexMap[heap[j].Item] = j;
        }
    }
}

using System;

namespace DoAnCuoiKy_Dijkstra
{
    public class CustomLinkedList<T>
    {
        public ListNode<T> flink { get; private set; }
        public ListNode<T> blink { get; private set; }
        public int Count { get; private set; }

        public MyLinkedList()
        {
            flink = null;
            blink = null;
            Count = 0;
        }

        // Hàm thêm thành phố
        public void AddLast(T data)
        {
            ListNode<T> newNode = new ListNode<T>(data);

            if (flink == null)
            {
                flink = blink = newNode;
            }
            else
            {
                blink.Next = newNode;
                newNode.Prev = blink;
                blink = newNode;
            }
            Count++;
        }

        // Lấy thành phố tại một vị trí index
        public T GetAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException("Vị trí không hợp lệ.");

            ListNode<T> current = flink;
            for (int i = 0; i < index; i++)
            {
                current = current.Next;
            }
            return current.Data;
        }

        // Hàm Remove
        public void Remove(ListNode<T> nodeToRemove)
        {
            if (nodeToRemove == null || flink == null) return;

            // Nếu là flink
            if (nodeToRemove == flink)
            {
                flink = nodeToRemove.Next;
                if (flink != null) flink.Prev = null;
                else blink = null; // List có 1 phần tử
            }
            // Nếu là blink
            else if (nodeToRemove == blink)
            {
                blink = nodeToRemove.Prev;
                if (blink != null) blink.Next = null;
            }
            // Nếu nằm giữa
            else
            {
                nodeToRemove.Prev.Next = nodeToRemove.Next;
                nodeToRemove.Next.Prev = nodeToRemove.Prev;
            }

            Count--;
        }
    }
}

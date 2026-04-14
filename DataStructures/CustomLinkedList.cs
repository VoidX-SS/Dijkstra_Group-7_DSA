using System;

namespace DoAnCuoiKy_Dijkstra
{
    public class CustomLinkedList<T>
    {
        public ListNode<T> Head { get; private set; }
        public ListNode<T> Tail { get; private set; }
        public int Count { get; private set; }

        public CustomLinkedList()
        {
            Head = null;
            Tail = null;
            Count = 0;
        }

        // Hàm thêm thành phố
        public void AddLast(T data)
        {
            ListNode<T> newNode = new ListNode<T>(data);

            if (Head == null)
            {
                Head = Tail = newNode;
            }
            else
            {
                Tail.Next = newNode;
                newNode.Prev = Tail;
                Tail = newNode;
            }
            Count++;
        }

        // Lấy thành phố tại một vị trí index
        public T GetAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException("Vị trí không hợp lệ.");

            ListNode<T> current = Head;
            for (int i = 0; i < index; i++)
            {
                current = current.Next;
            }
            return current.Data;
        }

        // Hàm Remove
        public void Remove(ListNode<T> nodeToRemove)
        {
            if (nodeToRemove == null || Head == null) return;

            // Nếu là Head
            if (nodeToRemove == Head)
            {
                Head = nodeToRemove.Next;
                if (Head != null) Head.Prev = null;
                else Tail = null; // List có 1 phần tử
            }
            // Nếu là Tail
            else if (nodeToRemove == Tail)
            {
                Tail = nodeToRemove.Prev;
                if (Tail != null) Tail.Next = null;
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

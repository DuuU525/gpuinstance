using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 3 A星节点
/// 接下来要实现的就是图的节点……欸？不是说状态就是节点吗，怎么还要定义节点类呢？这是为了方便寻找「路径」，GOAP会采用启发式搜索，就像A星寻路所用的那样。所谓「启发式搜索」就是有按照一定 「启发值」 进行的搜索，它的反面就是「盲目搜索」，如深度优先搜索、广度优先搜索。启发式搜索需要设计 「启发函数」 来计算「启发值」。
/// 在A星寻路中，我们通过计算「当前位置离起点的距离 + 当前位置离终点的距离」做为启发值来寻找最短路径；类似的，在我们实现的这个GOAP中，我们会通过计算「起点状态至当前状态 累计的动作代价 + 当前状态 与目标状态的相关度」作为启发值。
/// 累计代价，也相当于与起始状态的「距离」；与目标状态的相关度，在世界状态类中已经说明了，就是比较当前状态与目标状态的有效位的值有多少是相同的，通常相同的越多就越接近。
/// PS：在寻路时，常需要选取已探索过的节点中具有最小启发值的节点。用遍历倒也能做到，但总归效率不高，故可以用「堆」，也就是 「优先队列」 ：
/// </summary>
namespace GOAP
{

    //堆属于常用数据结构中的一种，我默认大家都会了，原理就不加以注释说明了
    public interface IMyHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }
    public class MyHeap<T> where T : IMyHeapItem<T>
    {
        public int NowLength { get; private set; }
        public int MaxLength { get; private set; }
        public T Top => heap[0];
        public bool IsEmpty => NowLength == 0;
        public bool IsFull => NowLength >= MaxLength - 1;
        private readonly bool isReverse;
        private readonly T[] heap;

        public MyHeap(int maxLength, bool isReverse = false)
        {
            NowLength = 0;
            MaxLength = maxLength;
            heap = new T[MaxLength + 1];
            this.isReverse = isReverse;
        }
        public T this[int index]
        {
            get => heap[index];
        }
        public void PushHeap(T value)
        {
            if (NowLength < MaxLength)
            {
                value.HeapIndex = NowLength;
                heap[NowLength] = value;
                Swim(NowLength);
                ++NowLength;
            }
        }
        public void PopHeap()
        {
            if (NowLength > 0)
            {
                heap[0] = heap[--NowLength];
                heap[0].HeapIndex = 0;
                Sink(0);
            }
        }
        public bool Contains(T value)
        {
            foreach (var v in heap)
            {
                if (Equals(v, value))
                {
                    return true;
                }
            }
            return false;
        }
        public T Find(T value)
        {
            if (Contains(value))
                return heap[value.HeapIndex];
            return default;
        }
        public void Clear()
        {
            for (int i = 0; i < NowLength; ++i)
            {
                heap[i].HeapIndex = 0;
            }
            NowLength = 0;
        }
        private void SwapValue(T a, T b)
        {
            heap[a.HeapIndex] = b;
            heap[b.HeapIndex] = a;
            (b.HeapIndex, a.HeapIndex) = (a.HeapIndex, b.HeapIndex);
        }

        private void Swim(int index)
        {
            int father;
            while (index > 0)
            {
                father = (index - 1) >> 1;
                if (IsBetter(heap[index], heap[father]))
                {
                    SwapValue(heap[father], heap[index]);
                    index = father;
                }
                else return;
            }
        }

        private void Sink(int index)
        {
            int largest, left = (index << 1) + 1;
            while (left < NowLength)
            {
                largest = left + 1 < NowLength && IsBetter(heap[left + 1], heap[left]) ? left + 1 : left;
                if (IsBetter(heap[index], heap[largest]))
                    largest = index;
                if (largest == index) return;
                SwapValue(heap[largest], heap[index]);
                index = largest;
                left = (index << 1) + 1;
            }
        }
        private bool IsBetter(T v1, T v2)
        {
            return isReverse ? (v2.CompareTo(v1) < 0) : (v1.CompareTo(v2) < 0);
        }
    }
}
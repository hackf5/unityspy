// SEE: http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis.Workspaces/ObjectPool%25601.cs

namespace HackF5.UnitySpy.Util
{
    using System;
    using System.Threading;

    public class ByteArrayPool
    {
        private const int BufferSize = 0x10;

        private const int MaxBufferCount = 0x10;

        private static readonly byte[] Empty = new byte[ByteArrayPool.BufferSize];

        private readonly Element[] items;

        private byte[] firstItem;

        private ByteArrayPool()
        {
            this.items = new Element[ByteArrayPool.MaxBufferCount];
        }

        public static ByteArrayPool Instance { get; } = new ByteArrayPool();

        public byte[] Rent(int size)
        {
            if (size > ByteArrayPool.BufferSize)
            {
                return new byte[size];
            }

            var item = this.firstItem;
            if ((item == null) || (item != Interlocked.CompareExchange(ref this.firstItem, null, item)))
            {
                var itemsCopy = this.items;
                for (var i = 0; i < itemsCopy.Length; i++)
                {
                    // Note that the initial read is optimistically not synchronized. That is intentional.
                    // We will interlock only when we have a candidate. in a worst case we may miss some
                    // recently returned objects. Not a big deal.
                    item = itemsCopy[i].Value;
                    if (item != null)
                    {
                        if (item == Interlocked.CompareExchange(ref itemsCopy[i].Value, null, item))
                        {
                            break;
                        }
                    }
                }
            }

            return item ?? new byte[ByteArrayPool.BufferSize];
        }

        public void Return(byte[] buffer)
        {
            if (buffer.Length > ByteArrayPool.BufferSize)
            {
                return;
            }

            // optimistically clear buffer.
            Buffer.BlockCopy(ByteArrayPool.Empty, 0, buffer, 0, ByteArrayPool.BufferSize);

            // Intentionally not using interlocked here.
            // In a worst case scenario two objects may be stored into same slot.
            // It is very unlikely to happen and will only mean that one of the objects will get collected.
            if (this.firstItem == null)
            {
                this.firstItem = buffer;
            }
            else
            {
                var itemsCopy = this.items;
                for (var i = 0; i < itemsCopy.Length; i++)
                {
                    if (itemsCopy[i].Value == null)
                    {
                        itemsCopy[i].Value = buffer;
                        break;
                    }
                }
            }
        }

        private struct Element
        {
            internal byte[] Value;
        }
    }
}
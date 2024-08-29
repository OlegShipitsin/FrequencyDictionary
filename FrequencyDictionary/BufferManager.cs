using System.Collections.Concurrent;

namespace FrequencyDictionary
{
    public interface IBuffer
    {
        char[] Block { get; set; }
    }

    /// <summary>
    /// Class for reusing buffers to save memory
    /// </summary>
    public class BufferManager: IDisposable
    {
        private class Buffer : IBuffer
        {
            private char[]? _block;

            public Buffer(int size)
            {
                Block = new char[size];
                Reusable = false;
            }
            public Buffer(Guid groupUid)
            {
                Reusable = true;
                GroupUid = groupUid;
            }

            public Guid GroupUid { get; private set; }


            public char[] Block
            {
                get => _block ??= new char[Config.BufferSize];
                set => _block = value;
            }

            public bool Reusable { get; private set; }
        }

        private readonly Buffer?[] _buffers = new Buffer[Config.BufferPoolSize];

        private int _count;
        private Guid _groupUid = Guid.NewGuid();

        private readonly ConcurrentQueue<Buffer> _freeBuffers = new();
        private readonly object _lock = new();

        public IBuffer GetTempBuffer(int size)
        {
            return new Buffer(size);
        }

        public async Task<IBuffer> GetBuffer()
        {
            if (_freeBuffers.TryDequeue(out var freeBuffer))
            {
                return freeBuffer;
            }

            lock (_lock)
            {
                if (_count < Config.BufferPoolSize)
                {
                    return _buffers[_count++] = new Buffer(_groupUid);
                }
            }

            while (!_freeBuffers.TryDequeue(out freeBuffer))
            {
                await Task.Delay(1);
            }

            return freeBuffer;
        }

        public void ReleaseBuffer(IBuffer buffer)
        {
            if (buffer is Buffer buff && buff.Reusable && buff.GroupUid == _groupUid)
            {
                _freeBuffers.Enqueue(buff);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _groupUid = Guid.NewGuid();
                for (var index = 0; index < _buffers.Length; index++)
                {
                    _buffers[index] = null;
                }

                _count = 0;

                _freeBuffers.Clear();
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }
}

using System.Collections.Concurrent;
using System.Text;

namespace FrequencyDictionary.DictionaryBuilder
{
    public class BufferedAsyncWithQueueDictionaryBuilder : IDictionaryBuilder
    {
        private class Tails
        {
            public Tails(int index)
            {
                Index = index;
                Tail = null;
                Head = null;
                WholeBody = false;
            }

            public int Index { get; }

            public string? Head { get; set; }
            public string? Tail { get; set; }
            public bool WholeBody { get; set; }
        }

        private class TextBlock
        {
            public TextBlock(int index, IBuffer buffer, int realLen)
            {
                Index = index;
                Buffer = buffer;
                RealLen = realLen;
            }

            public IBuffer Buffer { get; }
            public int Index { get; }
            public int RealLen { get; }
        }

        private readonly ConcurrentDictionary<string, int> _dictionary = new();
        private readonly ConcurrentBag<Tails> _tails = new();
        private readonly ConcurrentQueue<TextBlock> _queue = new();
        private BufferManager _bufferManager;

        private readonly object _eowLock = new();
        private bool _endOfWork;


        private bool CheckIfEndOfWord(char ch)
        {
            //return !char.IsLetterOrDigit(ch);
            return ch == ' ' || ch == '\n' || ch == '\r';
        }

        private void ParseBlock(TextBlock block)
        {
            var tails = new Tails(block.Index);
            var sb = new StringBuilder();

            var gotHead = false;

            for (var index = 0; index < block.RealLen; index++)
            {
                var ch = block.Buffer.Block[index];

                // If we found the end of the word
                if (CheckIfEndOfWord(ch))
                {
                    // And we have something in stringBuilder
                    if (sb.Length > 0)
                    {
                        // Get the word
                        var word = sb.ToString();

                        // If it's head
                        if (!gotHead)
                        {
                            gotHead = true;
                            tails.Head = word;
                        }
                        else
                        {
                            _dictionary.AddOrUpdate(word, 1, (_, i) => i + 1);
                        }
                    }
                    // If no word at beginning then no head 
                    else if (!gotHead)
                        gotHead = true;


                    sb.Clear();
                }
                else
                {
                    sb.Append(char.ToLower(ch));
                }
            }

            if (sb.Length > 0)
            {
                tails.Tail = sb.ToString();
                if (!gotHead)
                {
                    tails.WholeBody = true;
                }
            }

            _tails.Add(tails);


            _bufferManager.ReleaseBuffer(block.Buffer);
        }



        private void ProcessTails()
        {
            var sb = new StringBuilder(" ");

            foreach (var tail in _tails.OrderBy(x => x.Index))
            {
                if (!string.IsNullOrEmpty(tail.Head))
                {
                    sb.Append(tail.Head);
                }

                if (!tail.WholeBody)
                    sb.Append(" ");

                if (!string.IsNullOrEmpty(tail.Tail))
                {
                    sb.Append(tail.Tail);
                }
                else
                {
                    sb.Append(" ");
                }
            }

            sb.Append(" ");

            var lastBlock = _bufferManager.GetTempBuffer(sb.Length);
            sb.CopyTo(0, lastBlock.Block, sb.Length);

            ParseBlock(new TextBlock(-1, lastBlock, sb.Length));
        }


        private async Task ProcessQueue()
        {
            var tasks = new Task[Config.ParallelTaskCount];
            var i = 0;

            while (true)
            {
                bool eow;
                lock (_eowLock)
                {
                    eow = _endOfWork;
                }

                if (eow && _queue.IsEmpty)
                {
                    await Task.WhenAll(tasks.Where(x => x != null));
                    return;
                }


                // Fill all the task spots
                while (i < tasks.Length && _queue.TryDequeue(out var block1))
                {
                    tasks[i++] = Task.Run(() => ParseBlock(block1));
                }

                // When all spots are occupied getting completed tasks
                if (i == tasks.Length)
                {
                    // Waiting for the free spot
                    int freeTaskIndex = Task.WaitAny(tasks);

                    // if there is any block to process
                    if (_queue.TryDequeue(out var block))
                    {
                        tasks[freeTaskIndex] = Task.Run(() => ParseBlock(block));
                    }
                }
            }
        }

        private async Task ReadBlocksAsync(Stream inStream)
        {
            using (var reader = new StreamReader(inStream, Config.DefaultInputEncoding, true))
            {
                var buffer = await _bufferManager.GetBuffer();

                var i = 0;
                int len;

                while ((len = await reader.ReadBlockAsync(buffer.Block)) > 0)
                {
                    _queue.Enqueue(new TextBlock(i++, buffer, len));
                    buffer = await _bufferManager.GetBuffer();
                }
            }

            lock (_eowLock)
            {
                _endOfWork = true;
            }
        }

        private async Task IterateStream(Stream inStream)
        {
            var tasks = new List<Task>
            {
                Task.Run(ProcessQueue),
                Task.Run(() => ReadBlocksAsync(inStream))
            };

            await Task.WhenAll(tasks);

            ProcessTails();
        }

        public Dictionary<string, int> GetDictionary(Stream inStream)
        {
            using (_bufferManager = new BufferManager())
            {
                Task.Run(async () => await IterateStream(inStream)).Wait();

                Dictionary<string, int> result = _dictionary.ToDictionary(x => x.Key, pair => pair.Value);

                _queue.Clear();
                _dictionary.Clear();
                _tails.Clear();

                return result;
            }
        }
    }
}

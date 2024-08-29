using FrequencyDictionary;
using FrequencyDictionary.DictionaryBuilder;
using NUnit.Framework;

namespace FrequencyDictionaryTests
{
    public class Tests
    {
        private void CompareDictionary(Dictionary<string, int> original, Dictionary<string, int> built)
        {
            Assert.That(built.Count, Is.EqualTo(original.Count));

            foreach (var p in original)
            {
                Assert.That(built[p.Key], Is.EqualTo(p.Value));
            }
        }

        [SetUp]
        public void SetUp()
        {
            Config.BufferSize = 1024 * 1024;
            Config.BufferPoolSize = 25;
            Config.ParallelTaskCount = 30;
        }

        [Test, Timeout(1000)]
        public void DeadlockCheck()
        {
            var dict = new Dictionary<string, int>()
            {
                {new string('a', 1), 1}
            };

            Dictionary<string, int> result;
            using (var stream = TestHelpers.DictionaryToStream(dict))
            {
                var builder = new BufferedAsyncWithQueueDictionaryBuilder();
                result = builder.GetDictionary(stream);
            }

            CompareDictionary(dict, result);
        }

        [Test]
        public void GeneratedDictionary()
        {
            var dict = TestHelpers.GenerateDictionary(1000, 10);

            Dictionary<string, int> result;
            using (var stream = TestHelpers.DictionaryToStream(dict))
            {
                var builder = new BufferedAsyncWithQueueDictionaryBuilder();
                result = builder.GetDictionary(stream);
            }

            CompareDictionary(dict, result);
        }

        [Test]
        public void EmptyDictionary()
        {
            var dict = new Dictionary<string, int>();

            Dictionary<string, int> result;
            using (var stream = TestHelpers.DictionaryToStream(dict))
            {
                var builder = new BufferedAsyncWithQueueDictionaryBuilder();
                result = builder.GetDictionary(stream);
            }

            CompareDictionary(dict, result);
        }

        [Test]
        public void SmallBufferSize()
        {
            var dict = TestHelpers.GenerateDictionary(1000, 100);

            Config.BufferSize = 1024;

            Dictionary<string, int> result;
            using (var stream = TestHelpers.DictionaryToStream(dict))
            {
                var builder = new BufferedAsyncWithQueueDictionaryBuilder();
                result = builder.GetDictionary(stream);
            }

            CompareDictionary(dict, result);
        }



        [Test]
        public void OneBufferInPool()
        {
            var dict = TestHelpers.GenerateDictionary(1000, 100);

            Config.BufferPoolSize = 1;

            Dictionary<string, int> result;
            using (var stream = TestHelpers.DictionaryToStream(dict))
            {
                var builder = new BufferedAsyncWithQueueDictionaryBuilder();
                result = builder.GetDictionary(stream);
            }

            CompareDictionary(dict, result);
        }

        [Test]
        public void OneTask()
        {
            var dict = TestHelpers.GenerateDictionary(1000, 100);

            Config.ParallelTaskCount = 1;

            Dictionary<string, int> result;
            using (var stream = TestHelpers.DictionaryToStream(dict))
            {
                var builder = new BufferedAsyncWithQueueDictionaryBuilder();
                result = builder.GetDictionary(stream);
            }

            CompareDictionary(dict, result);
        }


        [Test]
        //[Ignore("Ignore huge test")]
        public void GeneratedHugeDictionary()
        {
            var fileName = "huge_sample.txt";

            try
            {
                var dict = TestHelpers.GenerateDictionary(300000, 500, 1000);

                TestHelpers.SaveToFile(dict, fileName);

                Dictionary<string, int> result;
                using (var stream = File.OpenRead(fileName))
                {
                    Console.WriteLine($"Size of huge test: {Utils.ReadableSize(stream.Length)}");
                    var builder = new BufferedAsyncWithQueueDictionaryBuilder();
                    result = builder.GetDictionary(stream);
                }

                CompareDictionary(dict, result);
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [Test]
        public void OneLongWord()
        {
            var dict = new Dictionary<string, int>()
            {
                {new string('a', 100000000), 1}
            };

            Dictionary<string, int> result;
            using (var stream = TestHelpers.DictionaryToStream(dict))
            {
                Console.WriteLine($"Size of stream: {Utils.ReadableSize(stream.Length)}");
                var builder = new BufferedAsyncWithQueueDictionaryBuilder();
                result = builder.GetDictionary(stream);
            }

            CompareDictionary(dict, result);
        }
    }
}
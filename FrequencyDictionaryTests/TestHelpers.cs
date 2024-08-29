using FrequencyDictionary;

namespace FrequencyDictionaryTests;

public static class TestHelpers
{
    public static Dictionary<string, int> GenerateDictionary(int uniqueWordsCount, int maxWordCount)
    {
        return GenerateDictionary(uniqueWordsCount, 1, maxWordCount);
    }

    public static Dictionary<string, int> GenerateDictionary(int uniqueWordsCount, int minWordCount, int maxWordCount)
    {
        if (minWordCount > maxWordCount)
        {
            (minWordCount, maxWordCount) = (maxWordCount, minWordCount);
        }

        minWordCount = Math.Max(minWordCount, 1);
        maxWordCount = Math.Max(maxWordCount, 1);

        var rnd = new Random();
        var result = new Dictionary<string, int>(uniqueWordsCount);
        for (int i = 0; i < uniqueWordsCount; i++)
        {
            var word = Utils.GenerateWord(rnd.Next(1, 15));

            while (result.ContainsKey(word))
            {
                word = Utils.GenerateWord(rnd.Next(1, 15));
            }

            result.Add(word, rnd.Next(minWordCount, maxWordCount + 1));
        }

        return result;
    }

    public static void SaveToFile(Dictionary<string, int> dictionary, string filename)
    {
        using (var fs = File.OpenWrite(filename))
        {
            using (var writer = new StreamWriter(fs, Config.DefaultInputEncoding))
            {
                foreach (var pair in dictionary)
                {
                    for (int i = 0; i < pair.Value; i++)
                    {
                        writer.Write(pair.Key);
                        writer.Write(" ");
                    }
                    writer.WriteLine();
                    writer.Flush();
                }
            }
        }
    }

    private class Pair
    {
        public Pair(string key, int value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public int Value { get; set; }
    }

    public static Stream DictionaryToStream(Dictionary<string, int> dictionary)
    {
        var totalWords = dictionary.Sum(x => x.Value);

        var list = dictionary.Select(x => new Pair(x.Key, x.Value)).ToList();
        var rnd = new Random();
        var result = new MemoryStream();

        StreamWriter writer = new StreamWriter(result, Config.DefaultInputEncoding);

        for (int i = 0; i < totalWords; i++)
        {
            var index = rnd.Next(list.Count);
            var word = list[index];

            writer.Write(word.Key);

            if (rnd.Next(1000) == 0)
                writer.WriteLine();
            else
                writer.Write(" ");

            if (word.Value == 1)
            {
                list.Remove(word);
            }
            else
            {
                word.Value--;
            }
        }

        writer.Flush();
        result.Position = 0;

        return result;
    }
}
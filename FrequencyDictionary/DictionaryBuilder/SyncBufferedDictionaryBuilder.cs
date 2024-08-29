using System.Text;

namespace FrequencyDictionary.DictionaryBuilder;

[Obsolete("Just for performance comparing")]
public class SyncBufferedDictionaryBuilder : IDictionaryBuilder
{
    public Dictionary<string, int> GetDictionary(Stream inStream)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        var sb = new StringBuilder();

        var buffer = new char[Config.BufferSize].AsSpan();

        using (var reader = new StreamReader(inStream, Config.DefaultInputEncoding, true))
        {
            while (reader.ReadBlock(buffer) > 0)
            {
                for (var i = 0; i < buffer.Length; i++)
                {
                    var ch = buffer[i];
                    if (char.IsLetterOrDigit(ch))
                    {
                        sb.Append(char.ToLower(ch));
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            var word = sb.ToString();

                            if (result.ContainsKey(word))
                                result[word] += 1;
                            else
                            {
                                result.Add(word, 1);
                            }
                        }

                        sb = new StringBuilder();
                    }
                }

                buffer = new char[Config.BufferSize].AsSpan();
            }
        }

        return result;
    }
}
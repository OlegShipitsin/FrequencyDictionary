using System.Text;

namespace FrequencyDictionary;

public static class Utils
{
    private const string Letters = "abcdefghijklmnopqrstuvwxyz";
    private static readonly Random Rnd = new();
    
    public static void Convert(InOutStreams streams)
    {
        // Convert in to out;
        using (var reader = new StreamReader(streams.InStream))
        {
            streams.OutStream.SetLength(0);
            using (var writer = new StreamWriter(streams.OutStream, Config.DefaultOutputEncoding))
            {
                writer.Write(reader.ReadToEnd());
            }
        }
    }

    public static string ReadableSize(double size)
    {
        int unit = 0;
        string[] units = { "B", "KB", "MB", "GB", "TB", "Oops" };

        while (size >= 1024 && unit < units.Length-1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.#} {units[unit]}";
    }
    
    public static string GenerateWord(int len)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < len; i++)
        {
            sb.Append(Letters[Rnd.Next(0, Letters.Length - 1)]);
        }
        return sb.ToString();
    }
}
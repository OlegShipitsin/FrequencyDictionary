using System.Diagnostics;
using System.Text;

namespace FrequencyDictionary;

public static class Config
{
    private static readonly Encoding Win1252;

    static Config()
    {
        // Add support Win-1252 encoding
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Win1252 = Encoding.GetEncoding("Windows-1252");
    }

    public static Encoding DefaultInputEncoding { get; set; } = Win1252;
    public static Encoding DefaultOutputEncoding { get; set; } = Win1252;


    // Max pool size in bytes: 2(size of char) * BufferSize * BufferPoolSize 
    // By default:  2 * 1024 * 1024 * 25 => 50MB
    private static int _bufferPoolSize = 25;
    private static int _bufferSize = 1024 * 1024 * 1;

    public static int BufferSize
    {
        get => _bufferSize;
        set => _bufferSize = Math.Max(value, 1);
    }

    public static int BufferPoolSize
    {
        get => _bufferPoolSize;
        set => _bufferPoolSize = Math.Max(value, 1);
    }


    public static int ParallelTaskCount { get; set; } = 30;

    public static readonly Stopwatch GlobalStopwatch = new();

}
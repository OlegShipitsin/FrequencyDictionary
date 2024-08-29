
namespace FrequencyDictionary;

public class InOutStreams
{
    public Stream InStream { get; }
    public Stream OutStream { get; }

    public InOutStreams(Stream inStream, Stream outStream)
    {
        InStream = inStream;
        OutStream = outStream;
    }
}

public static class FileOperations
{
    public static InOutStreams OpenFiles(string inFileName, string outFileName)
    {
        var inStream = File.Open(inFileName, FileMode.Open, FileAccess.Read);

        // Open the target file before the real work and keep it opened to avoid any problems in the end 
        // Not modify the file when open, it will be cleared before writing the result
        var outStream = File.Open(outFileName, FileMode.OpenOrCreate, FileAccess.Write);

        return new InOutStreams(inStream, outStream);
    }

    public static void OrderAndSaveToFile(Dictionary<string, int> result, Stream outStream)
    {
        using (var writer = new StreamWriter(outStream, Config.DefaultOutputEncoding))
        {
            var ordered = result.OrderByDescending(x => x.Value)
                //.ThenBy(x => x.Key)  // Not mentioned in the task
                ;

            foreach (var x in ordered)
            {
                writer.WriteLine($"{x.Key},{x.Value}");
                writer.Flush();
            }
        }
    }
}
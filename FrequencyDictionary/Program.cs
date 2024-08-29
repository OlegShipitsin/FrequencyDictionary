using System.Diagnostics;
using FrequencyDictionary.DictionaryBuilder;

namespace FrequencyDictionary
{
    internal class Program
    {
        static void Main(string[] args)
        {
            InOutStreams? streams = null;

            Config.GlobalStopwatch.Start();

            try
            {
                if (args.Length == 0)
                    throw new ArgumentException("Input file not set!");

                if (args.Length < 2)
                    throw new ArgumentException("Output file not set!");

                if (args[0] == args[1])
                    throw new ArgumentException("Input and output files cannot be the same!");

                streams = FileOperations.OpenFiles(args[0], args[1]);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(1);
            }

            Console.WriteLine($"Input file: {args[0]}; size: {Utils.ReadableSize(streams.InStream.Length)}");
            Console.WriteLine($"Output file: {args[1]}");


            //var builder = new SyncBufferedDictionaryBuilder(); // Performance comparision
            var builder = new BufferedAsyncWithQueueDictionaryBuilder();

            var dict = builder.GetDictionary(streams.InStream);

   
            // Clear the output file;
            streams.OutStream.SetLength(0);
   
            FileOperations.OrderAndSaveToFile(dict, streams.OutStream);

            Config.GlobalStopwatch.Stop();

            Console.WriteLine($"Done by: {Config.GlobalStopwatch.Elapsed};\n" +
                              $"Peak memory usage: {Utils.ReadableSize(Process.GetCurrentProcess().PeakWorkingSet64)}");
        }
    }
}
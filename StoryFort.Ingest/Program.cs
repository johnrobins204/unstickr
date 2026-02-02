using System;
using System.Threading.Tasks;

namespace StoryFort.Ingest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Console.WriteLine("StoryFort.Ingest pipeline started.");
            await IngestAlice.RunAsync();
            System.Console.WriteLine("StoryFort.Ingest pipeline completed.");
        }
    }
}
namespace Uploader;
class Program
{
    static async Task Main(string[] args)
    {
        await Uploader.Upload();
        
        Console.WriteLine("Press Enter to exit...");
        
        Console.ReadLine();
    }
}
namespace Uploader;
class Program
{
    static async Task Main(string[] args)
    {
        var uploader =  new Uploader();

        await uploader.Upload();
        
        Console.WriteLine("Press Enter to exit...");
        
        Console.ReadLine();
    }
}
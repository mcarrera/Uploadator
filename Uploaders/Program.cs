namespace Uploader;
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter Filename: ");
        var filename = Console.ReadLine();
        var uploader = new Uploader();

        await uploader.Upload(filename);

        Console.WriteLine("Press Enter to exit...");

        Console.ReadLine();
    }
}
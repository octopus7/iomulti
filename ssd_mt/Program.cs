using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

Console.WriteLine("'c' : create files");
Console.WriteLine("'s' : single thread read");
Console.WriteLine("'m' : multi thread read");
string? command = Console.ReadLine();
Console.WriteLine($"command : {command}");

string path = "files";

if (!Directory.Exists(path)) Directory.CreateDirectory(path);

Stopwatch stopwatch = new Stopwatch();

if (command == "c")
{
    stopwatch.Start();
    int count = 50000;
    Console.WriteLine($"write {count}");
    for(int i = 0; i < count; i++)
    {

        Random random = new Random();

        byte[] bytes = new byte[random.NextInt64() % 65536 + 4096]; 
        // range 4096 ~ (4096+65536)

        random.NextBytes(bytes);

        File.WriteAllBytes(path + "/" + i.ToString("D5") + ".bin", bytes);
    }
    stopwatch.Stop();
}
else
{
    ConcurrentBag<byte[]> bytes = new ConcurrentBag<byte[]>();
    StringBuilder stringBuilder = new StringBuilder();
    var files = Directory.GetFiles(path);

    
    if (command == "s")
    {
        Console.WriteLine("read Single");
        stopwatch.Start();
        foreach(String file in files)
        {
            readFile(bytes, stringBuilder, file);
        }
    }
    else
    {
        Console.WriteLine("thread count ?");
        string? count = Console.ReadLine();

        int threadCount = int.TryParse(count, out threadCount) ? threadCount : 64;
        ThreadPool.SetMinThreads(threadCount, threadCount);

        Console.WriteLine($"read MT ({threadCount})");
        stopwatch.Start();
        Parallel.ForEach(files, (file) =>
        {
            readFile(bytes, stringBuilder, file);
        });
    }
    stopwatch.Stop();

    // Console.WriteLine(stringBuilder.ToString());
}

Console.WriteLine($"Elapsed time :  {stopwatch.Elapsed.TotalSeconds}");

static void readFile(ConcurrentBag<byte[]> bytes, StringBuilder stringBuilder, string file)
{
    var bin = File.ReadAllBytes(file);
    bytes.Add(bin);
    stringBuilder.AppendLine(file + " " + bin.Length);
}
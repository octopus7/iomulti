using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

Console.WriteLine("'c' : create files");
Console.WriteLine("'s' : single thread read");
Console.WriteLine("'m' : multi thread read");
Console.WriteLine("'a' : async read");
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
    Console.WriteLine(DateTime.Now.ToString());

    for (int i = 0; i < count; i++)
    {

        Random random = new Random();

        byte[] bytes = new byte[random.NextInt64() % (65536-2048) + 2048]; // range 2048 ~ 65536

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

    if (command == "a")
    {
        Console.WriteLine("read Async");
        Console.WriteLine(DateTime.Now.ToString());

        List<Task<byte[]>> tasks = new List<Task<byte[]>>();

        stopwatch.Start();
        foreach (String file in files)
        {
            Task<byte[]> task = Task.Run(() => File.ReadAllBytesAsync(file));
            tasks.Add(task);
        }   
        Console.WriteLine("began : " + DateTime.Now.ToString());

        while (tasks.Count > 0)
        {
            tasks.RemoveAll(t => t.IsCompleted);
            Thread.Sleep(1);
        }
    }
    else if (command == "s")
    {
        Console.WriteLine("read Single");
        Console.WriteLine(DateTime.Now.ToString());

        stopwatch.Start();
        foreach (String file in files)
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
        ThreadPool.SetMaxThreads(threadCount, threadCount);

        Console.WriteLine($"read MT ({threadCount})");
        Console.WriteLine(DateTime.Now.ToString());

        stopwatch.Start();
        Parallel.ForEach(files, (file) =>
        {
            readFile(bytes, stringBuilder, file);
        });
    }
    stopwatch.Stop();

    // Console.WriteLine(stringBuilder.ToString());
}
Console.WriteLine(DateTime.Now.ToString());
Console.WriteLine($"Elapsed time :  {stopwatch.Elapsed.TotalSeconds}");

static void readFile(ConcurrentBag<byte[]> bytes, StringBuilder stringBuilder, string file)
{
    var bin = File.ReadAllBytes(file);
    bytes.Add(bin);
    stringBuilder.AppendLine(file + " " + bin.Length);
}

//static async Task<byte[]> readFileAsync(ConcurrentBag<byte[]> bytes, StringBuilder stringBuilder, string file)
//{
//    await File.ReadAllBytesAsync(file);
//}
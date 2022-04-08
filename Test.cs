using DownloadQueue;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = NetTask.MakeDefault("https://github.com/project-violet/database/releases/download/rd2020.06.07/ex-hentai-archive.json");
            task.DownloadString = true;
            task.StartCallback = () => Console.WriteLine("Download Start!");
            task.SizeCallback = (size) => Console.WriteLine("Total File Size: " + size);
            task.DownloadCallback = (size) => Console.WriteLine("Receive: " + size);
            task.Filename = "test";
            var dq = new DownloadQueue.DownloadQueue();
            dq.DownloadFile(task);
        }
    }
}

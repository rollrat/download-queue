# Download Queue

## Usage

### 1. Download Html

``` cs
static void Main(string[] args)
{
    var dq = new DownloadQueue.DownloadQueue();
    var html = dq.DownloadString("https://www.google.com/");
    Console.WriteLine(html);
}
```

### 2. Download File with Progress

``` cs
static void Main(string[] args)
{
    var dq = new DownloadQueue.DownloadQueue();
    var task = dq.MakeDefault("https://speed.hetzner.de/100MB.bin");
    task.StartCallback = () => Console.WriteLine("Download Start!");
    long total = 0;
    long current = 0;
    task.SizeCallback = (size) => { Console.WriteLine("Total File Size: " + size); total = size; };
    task.DownloadCallback = (size) => { current += size; Console.WriteLine("Receive: " + size + " " + (current / (double)total)); };
    task.Filename = "test";
    dq.DownloadFile(task);
}
```

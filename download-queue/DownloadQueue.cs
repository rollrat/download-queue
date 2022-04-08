// C# download queue library
// Copyright (C) 2020-2022. rollrat. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadQueue
{
    public class DownloadQueue : IDisposable
    {
        internal bool TimeoutInfinite { get; private set; }
        internal int TimeoutMillisecond { get; private set; }
        internal int RetryCount { get; set; }
        internal int DownloadBufferSize { get; private set; }
        internal IWebProxy DefaultProxy { get; private set; }
        internal int Capacity { get; private set; }

        SemaphoreSlim semaphore;

        private bool disposedValue;

        public DownloadQueue(
            int Capacity = 0,
            bool TimeoutInfinite = false, 
            int TimeoutMillisecond = 10000, 
            int RetryCount = 10, 
            int DownloadBufferSize = 131072, 
            IWebProxy DefaultProxy = null)
        {
            this.Capacity = Capacity;

            if (this.Capacity == 0)
                this.Capacity = Environment.ProcessorCount;

            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            this.TimeoutInfinite = TimeoutInfinite;
            this.TimeoutMillisecond = TimeoutMillisecond;
            this.RetryCount = RetryCount;
            this.DownloadBufferSize = DownloadBufferSize;
            this.DefaultProxy = DefaultProxy;

            ThreadPool.SetMinThreads(Capacity, Capacity);
            semaphore = new SemaphoreSlim(Capacity, Capacity);
        }

        /// <summary>
        /// Append task to download queue.
        /// </summary>
        /// <param name="task"></param>
        public Task AppendTask(NetTask task)
        {
            return Task.Run(async () =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                _ = Task.Run(() =>
                {
                    NetField.Do(task);
                    semaphore.Release();
                }).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Create new downlaod task with pc user-agent.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="cookie">Cookie message</param>
        /// <returns></returns>
        public NetTask MakeDefault(string url, string cookie = "")
            => new NetTask
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36",
                TimeoutInfinite = TimeoutInfinite,
                TimeoutMillisecond = TimeoutMillisecond,
                AutoRedirection = true,
                RetryWhenFail = true,
                RetryCount = RetryCount,
                DownloadBufferSize = DownloadBufferSize,
                Proxy = DefaultProxy,
                Cookie = cookie,
                Url = url
            };

        /// <summary>
        /// Create new download task with mobile user-agent.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="cookie">Cookie message</param>
        /// <returns></returns>
        public NetTask MakeDefaultMobile(string url, string cookie = "")
            => new NetTask
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Android 7.0; Mobile; rv:54.0) Gecko/54.0 Firefox/54.0 AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.125 Mobile Safari/603.2.4",
                TimeoutInfinite = TimeoutInfinite,
                TimeoutMillisecond = TimeoutMillisecond,
                AutoRedirection = true,
                RetryWhenFail = true,
                RetryCount = RetryCount,
                DownloadBufferSize = DownloadBufferSize,
                Proxy = DefaultProxy,
                Cookie = cookie,
                Url = url
            };

        /// <summary>
        /// Download multiple strings at once and return the download result.
        /// </summary>
        /// <param name="urls">URLs</param>
        /// <param name="cookie">Cookie message</param>
        /// <param name="complete">Callback for completed.</param>
        /// <param name="error">Callback for error occurred.</param>
        /// <returns></returns>
        public async Task<List<string>> DownloadStrings(List<string> urls, string cookie = "", Action complete = null, Action error = null)
        {
            var interrupt = new ManualResetEvent(false);
            var result = new string[urls.Count];
            var count = urls.Count;
            int iter = 0;

            foreach (var url in urls)
            {
                var itertmp = iter;
                var task = MakeDefault(url);
                task.DownloadString = true;
                task.CompleteCallbackString = (str) =>
                {
                    result[itertmp] = str;
                    if (Interlocked.Decrement(ref count) == 0)
                        interrupt.Set();
                    complete?.Invoke();
                };
                task.ErrorCallback = (code, e) =>
                {
                    if (Interlocked.Decrement(ref count) == 0)
                        interrupt.Set();
                    error?.Invoke();
                };
                task.Cookie = cookie;
                await AppendTask(task).ConfigureAwait(false);
                iter++;
            }

            interrupt.WaitOne();

            return result.ToList();
        }

        /// <summary>
        /// Download multiple strings at once and return the download result.
        /// </summary>
        /// <param name="tasks">Tasks</param>
        /// <returns></returns>
        public async Task<List<string>> DownloadStrings(List<NetTask> tasks)
        {
            var interrupt = new ManualResetEvent(false);
            var result = new string[tasks.Count];
            var count = tasks.Count;
            int iter = 0;

            foreach (var task in tasks)
            {
                var itertmp = iter;
                task.DownloadString = true;
                task.CompleteCallbackString = (str) =>
                {
                    result[itertmp] = str;
                    if (Interlocked.Decrement(ref count) == 0)
                        interrupt.Set();
                };
                task.ErrorCallback = (code, e) =>
                {
                    if (Interlocked.Decrement(ref count) == 0)
                        interrupt.Set();
                };
                await AppendTask(task).ConfigureAwait(false);
                iter++;
            }

            interrupt.WaitOne();

            return result.ToList();
        }

        /// <summary>
        /// Download string from url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string DownloadString(string url)
        {
            return DownloadStringAsync(MakeDefault(url)).Result;
        }

        /// <summary>
        /// Download string from download task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public string DownloadString(NetTask task)
        {
            return DownloadStringAsync(task).Result;
        }

        /// <summary>
        /// Download string from download task.
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns></returns>
        public async Task<string> DownloadStringAsync(NetTask task)
        {
            return await Task.Run(async () =>
            {
                var interrupt = new ManualResetEvent(false);
                string result = null;

                task.DownloadString = true;
                task.CompleteCallbackString = (string str) =>
                {
                    result = str;
                    interrupt.Set();
                };

                task.ErrorCallback = (code, e) =>
                {
                    task.ErrorCallback = null;
                    interrupt.Set();
                };

                await AppendTask(task).ConfigureAwait(false);

                interrupt.WaitOne();

                return result;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Download multiple files at once and wait for download complete.
        /// </summary>
        /// <param name="url_path">(URL, Filename) pair list.</param>
        /// <param name="cookie">Cookie message</param>
        /// <param name="download">Callback for downloaded block size.</param>
        /// <param name="complete">Callback for completed.</param>
        /// <param name="error">Callback for error occurred.</param>
        /// <returns></returns>
        public async Task DownloadFiles(List<(string, string)> url_path, string cookie = "", Action<long> download = null, Action complete = null, Action error = null)
        {
            var interrupt = new ManualResetEvent(false);
            var count = url_path.Count;
            int iter = 0;

            foreach (var up in url_path)
            {
                var itertmp = iter;
                var task = MakeDefault(up.Item1);
                task.SaveFile = true;
                task.Filename = up.Item2;
                task.DownloadCallback = (sz) =>
                {
                    download?.Invoke(sz);
                };
                task.CompleteCallback = () =>
                {
                    if (Interlocked.Decrement(ref count) == 0)
                        interrupt.Set();
                    complete?.Invoke();
                };
                task.ErrorCallback = (code, e) =>
                {
                    if (Interlocked.Decrement(ref count) == 0)
                        interrupt.Set();
                    error?.Invoke();
                };
                task.Cookie = cookie;
                await AppendTask(task).ConfigureAwait(false);
                iter++;
            }

            interrupt.WaitOne();
        }

        /// <summary>
        /// Download file from url.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="filename">Filename</param>
        public void DownloadFile(string url, string filename)
        {
            var task = MakeDefault(url);
            task.SaveFile = true;
            task.Filename = filename;
            DownloadFileAsync(task).Wait();
        }

        /// <summary>
        /// Download file from download task.
        /// </summary>
        /// <param name="task">Task</param>
        public void DownloadFile(NetTask task)
        {
            DownloadFileAsync(task).Wait();
        }

        /// <summary>
        /// Download file from downlaod task.
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns></returns>
        public async Task DownloadFileAsync(NetTask task)
        {
            await Task.Run(async () =>
            {
                var interrupt = new ManualResetEvent(false);

                task.SaveFile = true;
                task.CompleteCallback = () =>
                {
                    interrupt.Set();
                };

                task.ErrorCallback = (code, e) =>
                {
                    task.ErrorCallback = null;
                    interrupt.Set();
                };

                await AppendTask(task).ConfigureAwait(false);

                interrupt.WaitOne();
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Download bytes from url.
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
        public byte[] DownloadData(string url)
        {
            return DownloadDataAsync(MakeDefault(url)).Result;
        }

        /// <summary>
        /// Download bytes from url.
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
        public byte[] DownloadData(NetTask task)
        {
            return DownloadDataAsync(task).Result;
        }

        /// <summary>
        /// Download bytes from url.
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
        public async Task<byte[]> DownloadDataAsync(NetTask task)
        {
            return await Task.Run(async () =>
            {
                var interrupt = new ManualResetEvent(false);
                byte[] result = null;

                task.MemoryCache = true;
                task.CompleteCallbackBytes = (byte[] bytes) =>
                {
                    result = bytes;
                    interrupt.Set();
                };

                task.ErrorCallback = (code, e) =>
                {
                    task.ErrorCallback = null;
                    interrupt.Set();
                };

                await AppendTask(task).ConfigureAwait(false);

                interrupt.WaitOne();

                return result;
            }).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

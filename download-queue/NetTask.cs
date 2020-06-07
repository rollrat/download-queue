// C# download queue library
// Copyright (C) 2020. rollrat. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace download_queue
{
    /// <summary>
    /// Information of what download for
    /// </summary>
    public class NetTask : ISchedulerContents<NetTask, NetPriority>
    {
        public static NetTask MakeDefault(string url, string cookie = "")
            => new NetTask
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36",
                TimeoutInfinite = AppProvider.TimeoutInfinite,
                TimeoutMillisecond = AppProvider.TimeoutMillisecond,
                AutoRedirection = true,
                RetryWhenFail = true,
                RetryCount = AppProvider.RetryCount,
                DownloadBufferSize = AppProvider.DownloadBufferSize,
                Priority = new NetPriority() { Type = NetPriorityType.Trivial },
                Proxy = AppProvider.DefaultProxy,
                Cookie = cookie,
                Url = url
            };

        public static NetTask MakeDefaultMobile(string url, string cookie = "")
            => new NetTask
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Android 7.0; Mobile; rv:54.0) Gecko/54.0 Firefox/54.0 AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.125 Mobile Safari/603.2.4",
                TimeoutInfinite = AppProvider.TimeoutInfinite,
                TimeoutMillisecond = AppProvider.TimeoutMillisecond,
                AutoRedirection = true,
                RetryWhenFail = true,
                RetryCount = AppProvider.RetryCount,
                DownloadBufferSize = AppProvider.DownloadBufferSize,
                Priority = new NetPriority() { Type = NetPriorityType.Trivial },
                Proxy = AppProvider.DefaultProxy,
                Cookie = cookie,
                Url = url
            };

        public enum NetError
        {
            Unhandled = 0,
            CannotContinueByCriticalError,
            UnknowError, // Check DPI Blocker
            UriFormatError,
            Aborted,
            ManyRetry,
        }

        /* Task Information */

        public int Index { get; set; }

        /* Http Information */

        public string Url { get; set; }
        public List<string> FailUrls { get; set; }
        public string Accept { get; set; }
        public string Referer { get; set; }
        public string UserAgent { get; set; }
        public string Cookie { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, string> Query { get; set; }
        public IWebProxy Proxy { get; set; }

        /* Detail Information */

        /// <summary>
        /// Text Encoding Information
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Set if you want to download and save file to your own device.
        /// </summary>
        public bool SaveFile { get; set; }
        public string Filename { get; set; }

        /// <summary>
        /// Set if needing only string datas.
        /// </summary>
        public bool DownloadString { get; set; }

        /// <summary>
        /// Download data to temporary directory on your device.
        /// </summary>
        public bool DriveCache { get; set; }

        /// <summary>
        /// Download data to memory.
        /// </summary>
        public bool MemoryCache { get; set; }

        /// <summary>
        /// Retry download when fail to download.
        /// </summary>
        public bool RetryWhenFail { get; set; }
        public int RetryCount { get; set; }

        /// <summary>
        /// Timeout settings
        /// </summary>
        public bool TimeoutInfinite { get; set; }
        public int TimeoutMillisecond { get; set; }

        public int DownloadBufferSize { get; set; }

        public bool AutoRedirection { get; set; }

        public bool NotifyOnlySize { get; set; }

        /* Callback Functions */

        public Action<long> SizeCallback;
        public Action<long> DownloadCallback;
        public Action StartCallback;
        public Action CompleteCallback;
        public Action<string> CompleteCallbackString;
        public Action<byte[]> CompleteCallbackBytes;
        public Action<CookieCollection> CookieReceive;
        public Action<string> HeaderReceive;
        public Action CancleCallback;

        /// <summary>
        /// Return total downloaded size
        /// </summary>
        public Action<int> RetryCallback;

        // Execption or HttpStatusCode
        public Action<NetError, object> ErrorCallback;

        /* For NetField */

        public bool Aborted;
        public HttpWebRequest Request;
        public CancellationToken Cancel;
    }
}

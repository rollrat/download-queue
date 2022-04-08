// C# download queue library
// Copyright (C) 2020-2022. rollrat. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace DownloadQueue
{
    /// <summary>
    /// Information of what download for
    /// </summary>
    public class NetTask
    {
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
        public string RequestBody { get; set; }
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

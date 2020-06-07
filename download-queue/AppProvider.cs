// C# download queue library
// Copyright (C) 2020. rollrat. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace download_queue
{
    public class AppProvider
    {
        internal static NetScheduler Scheduler { get; set; }

        internal static bool TimeoutInfinite { get; set; }
        internal static int TimeoutMillisecond { get; set; }
        internal static int RetryCount { get; set; }
        internal static int DownloadBufferSize { get; set; }
        internal static IWebProxy DefaultProxy { get; set; }

        public static void Init(int threadcount = -1, 
            bool TimeoutInfinite = false, 
            int TimeoutMillisecond = 10000, 
            int RetryCount = 10, 
            int DownloadBufferSize = 131072, 
            IWebProxy DefaultProxy = null)
        {
            if (threadcount == -1)
                threadcount = Environment.ProcessorCount;

            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            Scheduler = new NetScheduler(threadcount);

            AppProvider.TimeoutInfinite = TimeoutInfinite;
            AppProvider.TimeoutMillisecond = TimeoutMillisecond;
            AppProvider.RetryCount = RetryCount;
            AppProvider.DownloadBufferSize = DownloadBufferSize;
            AppProvider.DefaultProxy = DefaultProxy;
        }
    }
}

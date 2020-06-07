// C# download queue library
// Copyright (C) 2020. rollrat. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;

namespace DownloadQueue
{
    public abstract class NetTaskPass
    {
        public static List<NetTaskPass> Passes = new List<NetTaskPass>();

        public static void RunOnField(ref NetTask content)
        {
            foreach (var pass in Passes)
                pass.RunOnPass(ref content);
        }

        public static void RemoveFromPasses<T>() where T : NetTaskPass, new()
        {
            lock (Passes)
            {
                var class_name = (new T()).GetType().Name;
                for (int i = 0; i < Passes.Count; i++)
                {
                    if (Passes[i].GetType().Name == class_name)
                    {
                        Passes.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public abstract void RunOnPass(ref NetTask content);
    }
}

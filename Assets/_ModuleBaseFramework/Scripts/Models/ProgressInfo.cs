using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ModuleBased.Models
{
    public struct ProgressInfo
    {
        public float Progress { get; private set; }
        public string Message { get; private set; }

        public ProgressInfo(float p, string msg = "")
        {
            if (p < 0)
                p = 0;
            if (p > 1)
                p = 1;
            Progress = p;
            if (msg == null)
                msg = string.Empty;
            Message = msg;
        }
    }

    

    
    public static class ProgressExtension
    {
        public static void Report(this IProgress<ProgressInfo> progress, float p, string msg = "")
        {
            progress.Report(new ProgressInfo(p, msg));
        }
    }
}

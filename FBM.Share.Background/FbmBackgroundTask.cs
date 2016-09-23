using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using System.Diagnostics;

namespace FBM.Background
{
    public sealed class FbmBackgroundTask : IBackgroundTask
    {
        void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("FbmBackgroundTask.Run()");
        }
    }
}

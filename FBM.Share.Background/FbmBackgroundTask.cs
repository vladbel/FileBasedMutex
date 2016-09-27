using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using System.Diagnostics;
using FBM.Kit.Services;
using FBM.Services;

namespace FBM.Background
{
    public sealed class FbmBackgroundTask : IBackgroundTask
    {
        void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("FbmBackgroundTask.Run(): Start");

            IMutex mutex = new RefreshTokenMutex();

            try
            {
                Debug.WriteLine("FbmBackgroundTask.Run(): enter mutex");
                mutex.Aquire();
                //Do work
                Debug.WriteLine("FbmBackgroundTask.Run(): do work");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FbmBackgroundTask.Run(): exception {0}", ex.ToString());
            }
            finally
            {
                Debug.WriteLine("FbmBackgroundTask.Run(): release mutex");
                mutex.Release();
            }
        }
    }
}

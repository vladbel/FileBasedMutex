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
            bool mutexAquired = false;

            try
            {
                Debug.WriteLine("FbmBackgroundTask.Run(): enter mutex");
                mutexAquired = mutex.Aquire();
                if ( mutexAquired)
                {
                    //Do work
                    Debug.WriteLine("FbmBackgroundTask.Run(): do work");
                }
                else
                {
                    //unable aquire mutex
                    Debug.WriteLine("FbmBackgroundTask.Run(): unable aquire mutex");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("FbmBackgroundTask.Run(): exception {0}", ex.ToString());
            }
            finally
            {
                if (mutexAquired)
                {
                    Debug.WriteLine("FbmBackgroundTask.Run(): release mutex");
                    //mutex.Release();
                }

            }
        }
    }
}

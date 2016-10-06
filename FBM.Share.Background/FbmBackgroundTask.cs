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
            Debug.WriteLine("--------------------FbmBackgroundTask.Run(): Start");

            //IMutex mutex = new RefreshTokenMutex();
            IMutex mutex = new FileMutex();
            MutexOperationResult mutexAquired = null;
            MutexOperationResult mutexReleased = null;

            try
            {
                Debug.WriteLine("--------------------FbmBackgroundTask.Run(): enter mutex");
                mutexAquired = mutex.Aquire(5000);
                Debug.WriteLine("--------------------FbmBackgroundTask.Run():" + mutexAquired.ToString());

                if ( (mutexAquired.Result & MutexOperationResultEnum.Aquired) != MutexOperationResultEnum.NoValue )
                {
                    // Do work here
                    for (var i = 0; i < 5; i++)
                    {
                        Task.Delay(1000);
                        Debug.WriteLine("--------------------FbmBackgroundTask.Run():" + "doing some work");
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("--------------------FbmBackgroundTask.Run(): exception {0}", ex.ToString());
            }
            finally
            {
                if (mutexAquired != null && (mutexAquired.Result & MutexOperationResultEnum.Aquired) != MutexOperationResultEnum.NoValue)
                {
                    mutexReleased = mutex.Release();
                    Debug.WriteLine("--------------------FbmBackgroundTask.Run(): " + mutexReleased.ToString());
                }

                if ( mutexReleased == null || ( mutexReleased.Result & MutexOperationResultEnum.Released) == MutexOperationResultEnum.NoValue)
                {
                    mutexReleased = mutex.Release(forceDisposeIfNotReleased: true);
                    Debug.WriteLine("--------------------FbmBackgroundTask.Run(): " + "forceDispose: " + mutexReleased.ToString());
                }

            }
        }
    }
}

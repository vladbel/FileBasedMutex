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

            IMutex mutex = new RefreshTokenMutex();
            MutexOperationResult mutexAquired = MutexOperationResult.NoValue;
            MutexOperationResult mutexReleased = MutexOperationResult.NoValue;

            try
            {
                Debug.WriteLine("--------------------FbmBackgroundTask.Run(): enter mutex");
                mutexAquired = mutex.Aquire(5000);
                Debug.WriteLine("--------------------FbmBackgroundTask.Run():" + mutexAquired.ToString());

                if ( (mutexAquired & MutexOperationResult.Aquired) != MutexOperationResult.NoValue )
                {
                    // Do work here
                    for (var i = 0; i < 5; i++)
                    {
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
                if ( (mutexAquired & MutexOperationResult.Aquired) != MutexOperationResult.NoValue)
                {
                    mutexReleased = mutex.Release();
                    Debug.WriteLine("--------------------FbmBackgroundTask.Run(): " + mutexReleased.ToString());
                }

                if ( MutexOperationResult.NoValue == ( mutexReleased & MutexOperationResult.Released))
                {
                    mutexReleased = mutex.Release(forceDisposeIfNotReleased: true);
                    Debug.WriteLine("--------------------FbmBackgroundTask.Run(): " + "forceDispose: " + mutexReleased.ToString());
                }

            }
        }
    }
}

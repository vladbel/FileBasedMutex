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
            var task = Task.Run(async () => await Run());
            task.Wait();
        }

        private async Task Run()
        {
            Debug.WriteLine("--------------------FbmBackgroundTask.Run(): Start");

            //IMutex mutex = new RefreshTokenMutex();
            IMutexService mutex = new MutexService();
            MutexOperationResult mutexAcquired = null;
            MutexOperationResult mutexReleased = null;

            try
            {
                Debug.WriteLine("--------------------FbmBackgroundTask.Run(): enter mutex");
                mutexAcquired = await mutex.AcquireAsync(5000);
                Debug.WriteLine("--------------------FbmBackgroundTask.Run():" + mutexAcquired.ToString());

                if ((mutexAcquired.Result & MutexOperationResultEnum.Acquired) != MutexOperationResultEnum.NoValue)
                {
                    // Do work here
                    for (var i = 0; i < 10; i++)
                    {
                        await Task.Delay(1000);
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
                if (mutexAcquired != null && mutexAcquired.ResultIs(MutexOperationResultEnum.Acquired))
                {
                    mutexReleased = await mutex.ReleaseAsync(mutexAcquired.AcquisitionKey);
                    Debug.WriteLine("--------------------FbmBackgroundTask.Run(): " + mutexReleased.ToString());

                    if (mutexReleased == null || !mutexReleased.ResultIs(MutexOperationResultEnum.Released))
                    {
                        var mutexCleared = await mutex.ClearAsync();
                        Debug.WriteLine("--------------------FbmBackgroundTask.Run(): " + "Clear: " + mutexCleared.ToString());
                    }
                }
            }
        }
    }
}

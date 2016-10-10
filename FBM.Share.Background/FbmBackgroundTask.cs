﻿using System.Threading;
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
            MutexOperationResult mutexAquired = null;
            MutexOperationResult mutexReleased = null;

            try
            {
                Debug.WriteLine("--------------------FbmBackgroundTask.Run(): enter mutex");
                mutexAquired = await mutex.AquireAsync(5000);
                Debug.WriteLine("--------------------FbmBackgroundTask.Run():" + mutexAquired.ToString());

                if ((mutexAquired.Result & MutexOperationResultEnum.Aquired) != MutexOperationResultEnum.NoValue)
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
                if (mutexAquired != null && mutexAquired.ResultIs(MutexOperationResultEnum.Aquired))
                {
                    mutexReleased = await mutex.ReleaseAsync(mutexAquired.AcquisitionKey);
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

using FBM.Kit.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace FBM.Services
{
    internal class MutexService : IMutexService
    {
        private const string FILE = "mutex.txt";
        private const int TIMEOUT_MSEC = 15000;

        private StorageFolder Folder
        {
            get { return ApplicationData.Current.TemporaryFolder; }
        }

        public async Task<MutexOperationResult> AquireAsync ( int milliseconds = 0)
        {
            var result = new MutexOperationResult();
            StorageFile file = null;

            try
            {
                file = await GetFile();
            }
            catch
            {
                result.Result = MutexOperationResultEnum.ExceptionFileSystem;
                return result;
            }


            if (file == null)
            {
                try
                {
                    result.Result = MutexOperationResultEnum.Free;
                    var key = Guid.NewGuid().ToString();
                    file = await Folder.CreateFileAsync(FILE);
                    await FileIO.WriteTextAsync(file, key);
                    result.AcquisitionKey = key;
                    result.Result = MutexOperationResultEnum.Aquired;
                }
                catch (Exception)
                {
                    result.Result = MutexOperationResultEnum.ExceptionFileSystem;
                }

            }
            else
            {
                // First attempt to intercept mutex: force clear if mutex aquired longer then TIMEOUT
                var forcedCleared = await ForceClearIfTimeoutExpired();
                if (forcedCleared.ResultIs(MutexOperationResultEnum.Cleared))
                {
                    var forcedAquired = await AquireAsync();
                    result.Combine(forcedCleared);
                    result.Combine(forcedAquired);
                    return result;
                }

                // Second attempt to intercept mutex: await specifyed delay
                if (milliseconds >= 1000)
                {
                    for (var i = 0; i < Math.Min( milliseconds, TIMEOUT_MSEC) / 1000; i++) // There is no need to wait longer then TIMEOUT,
                                                                                           // if mutex not released it indicates greater failure
                    {
                        await Task.Delay(1000);
                        var delayedAquired = await AquireAsync();
                        if (delayedAquired.ResultIs( MutexOperationResultEnum.Aquired))
                        {
                            return delayedAquired;
                        }
                    }
                    result.Result = MutexOperationResultEnum.FailToAquire;
                }
            }
            SetDeferredMutexRelease(result); // Will launch task which release mutex when timeout

            return result;
        }



        public async Task<MutexOperationResult>ReleaseAsync (string key)
        {
            var released = new MutexOperationResult();

            var file = await GetFile();

            if (file == null)
            {
                released.Result = MutexOperationResultEnum.Free;
            }
            else
            {
                var mutexKey = await FileIO.ReadTextAsync(file);
                if (mutexKey == key)
                {
                    await file.DeleteAsync();
                    released.AcquisitionKey = key;
                    released.Result = MutexOperationResultEnum.Released;

                    CancelDeferredMutexRelease();
                }
                else
                {
                    released.Result = MutexOperationResultEnum.FailToRelease;
                }
            }

            return released;
        }

        public async Task<MutexOperationResult> ClearAsync()
        {
            var cleared = new MutexOperationResult();
            try
            {
                var file = await GetFile();
                if (file != null)
                {
                    await file.DeleteAsync();
                    cleared.Result = MutexOperationResultEnum.Cleared;
                    CancelDeferredMutexRelease();
                }
                else
                {
                    cleared.Result = MutexOperationResultEnum.Free;
                }
            }
            catch
            {
                cleared.Result = MutexOperationResultEnum.ExceptionFileSystem | MutexOperationResultEnum.FailToClear;
            }
            return cleared;
        }

        private async Task<MutexOperationResult> ForceClearIfTimeoutExpired()
        {
            var result = new MutexOperationResult();

            try
            {
                var file = await GetFile();
                if (file != null)
                {
                    var currentTime = DateTime.Now;
                    var fileTimeStamp = file.DateCreated;

                    if ((currentTime - fileTimeStamp).TotalMilliseconds > TIMEOUT_MSEC * 2)
                    {
                        var cleared = await ClearAsync();
                        result.Combine(cleared);
                        Debug.WriteLine("-------Force clear: " + cleared.ToString());
                    }
                }
                else
                {
                    result.Result = MutexOperationResultEnum.Free;
                }
            }
            catch
            {
                result.Result = MutexOperationResultEnum.ExceptionFileSystem | MutexOperationResultEnum.FailToClear;
            }

            return result;
        }

        private async Task<StorageFile> GetFile()
        {
            var files = await Folder.GetFilesAsync();
            var file = files.FirstOrDefault(f => f.Name.StartsWith(FILE));
            return file;
        }

        #region Deferred Release

        private Task _deferredRelease;
        private CancellationTokenSource _deferredReleaseCancellationSource;
        private void SetDeferredMutexRelease(MutexOperationResult result)
        {
            if ((result.Result & MutexOperationResultEnum.Aquired) == MutexOperationResultEnum.Aquired)
            {
                _deferredReleaseCancellationSource = new CancellationTokenSource();
                var deferredReleaseCancellationToken = _deferredReleaseCancellationSource.Token;
                _deferredRelease = Task.Run(
                    async () =>
                    {
                        var checkIfCancelledPeriod = 1000;
                        for (var i = 0; i < TIMEOUT_MSEC/ checkIfCancelledPeriod; i++)
                        {
                            await Task.Delay(checkIfCancelledPeriod);
                            if (deferredReleaseCancellationToken.IsCancellationRequested)
                            {
                                Debug.WriteLine("-------Deferred release: cancel");
                                deferredReleaseCancellationToken.ThrowIfCancellationRequested();
                            }
                            else
                            {
                                Debug.WriteLine("-------Deferred release: waiting " + (i * 1000).ToString());
                            }
                        }

                        var released = await ReleaseAsync(result.AcquisitionKey);
                        Debug.WriteLine("-------Deferred release: " + released.ToString());
                    }
                    , deferredReleaseCancellationToken
                    );
            }
        }

        private void CancelDeferredMutexRelease()
        {
            // Cancel pending deferrred release task
            if (_deferredRelease != null && !_deferredRelease.IsCompleted)
            {
                _deferredReleaseCancellationSource.Cancel();
                _deferredReleaseCancellationSource.Dispose();
            }
        }
        #endregion
    }
}

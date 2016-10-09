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

        private Task _deferredRelease;
        private CancellationTokenSource _cancellationSource;

        private StorageFolder Folder
        {
            get { return ApplicationData.Current.TemporaryFolder; }
        }

        public async Task<MutexOperationResult> Aquire ( int msec = 0)
        {
            var result = new MutexOperationResult();

            var files = await Folder.GetFilesAsync();
            var file = files.FirstOrDefault(f => f.Name.StartsWith(FILE));

            if (file == null)
            {
                try
                {
                    var key = Guid.NewGuid().ToString();
                    file = await Folder.CreateFileAsync(FILE);
                    await FileIO.WriteTextAsync(file, key);
                    result.AcquisitionKey = key;
                    result.Result = MutexOperationResultEnum.Aquired;
                }
                catch (Exception)
                {
                    result.Result = MutexOperationResultEnum.UnknownException;
                }

            }
            else if (msec > 1)
            {
                for ( var i = 0; i < 5; i++ )
                {
                    await Task.Delay(msec / 5);
                    result = await Aquire();
                    if ((result.Result & MutexOperationResultEnum.Aquired) == MutexOperationResultEnum.Aquired)
                    {
                        return result;
                    }
                }
                result.Result = MutexOperationResultEnum.FailToAquire;
            }

            if ((result.Result & MutexOperationResultEnum.Aquired) == MutexOperationResultEnum.Aquired)
            {
                _cancellationSource = new CancellationTokenSource();
                var deferredReleaseCancellationToken = _cancellationSource.Token;
                _deferredRelease = Task.Run(
                    async () =>
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            await Task.Delay(1000);
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
                        Debug.WriteLine("-------Deferred release: release ");
                        await Release(result.AcquisitionKey);
                    }
                    , deferredReleaseCancellationToken
                    );
            }
            return result;
        }


        public async Task<MutexOperationResult>Release (string key)
        {
            var released = new MutexOperationResult();

            var files = await Folder.GetFilesAsync();
            var file = files.FirstOrDefault(f => f.Name.StartsWith(FILE));

            if (file == null)
            {
                released.Result = MutexOperationResultEnum.FailToRelease;
            }
            else
            {
                var mutexKey = await FileIO.ReadTextAsync(file);
                if (mutexKey == key)
                {
                    await file.DeleteAsync();
                    released.AcquisitionKey = key;
                    released.Result = MutexOperationResultEnum.Released;

                    // Cancel pending deferrred release task
                    if (_deferredRelease != null && !_deferredRelease.IsCompleted)
                    {
                        _cancellationSource.Cancel();
                        _cancellationSource.Dispose();
                    }
                }
                else
                {
                    released.Result = MutexOperationResultEnum.FailToRelease;
                }
            }

            return released;
        }

        public async Task<MutexOperationResult> Dispose()
        {
            var disposed = new MutexOperationResult();
            try
            {
                var files = await Folder.GetFilesAsync();
                var file = files.FirstOrDefault(f => f.Name.StartsWith(FILE));

                if (file != null)
                {
                    await file.DeleteAsync();
                    disposed.Result = MutexOperationResultEnum.Disposed;
                }
                else
                {
                    disposed.Result = MutexOperationResultEnum.FailToDispose;
                }
            }
            catch
            {
                disposed.Result = MutexOperationResultEnum.UnknownException;
            }
            return disposed;
        }
    }
}

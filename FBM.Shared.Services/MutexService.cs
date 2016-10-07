using FBM.Kit.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

namespace FBM.Services
{
    internal class MutexService : IMutexService
    {
        private const string FILE = "mutex.txt";

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

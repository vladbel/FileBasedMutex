using FBM.Kit.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

namespace FBM.Services
{
    internal class FileMutexService : IMutexService
    {
        private const string FILE = "mutex.txt";

        private StorageFolder Folder
        {
            get { return ApplicationData.Current.TemporaryFolder; }
        }
        public MutexOperationResult Aquire(int milliseconds = 0)
        {
            var result = new MutexOperationResult();
            try
            {
                result = AquireAsync(milliseconds).Result;
            }
            catch (Exception)
            {
                result.Result = MutexOperationResultEnum.UnknownException;
            }
            return result;
        }

        private async Task<MutexOperationResult> AquireAsync ( int msec = 0)
        {
            var result = new MutexOperationResult();

            var files = await Folder.GetFilesAsync();
            var file = files.FirstOrDefault(f => f.Name.StartsWith(FILE));

            if (file == null)
            {
                file = await Folder.CreateFileAsync(FILE);
                await FileIO.WriteTextAsync(file, "aquired");
                result.Result =  MutexOperationResultEnum.Aquired;
            }
            else if (msec > 1)
            {
                for ( var i = 0; i < 5; i++ )
                {
                    await Task.Delay(msec / 5);
                    result = await AquireAsync();
                    if ((result.Result & MutexOperationResultEnum.Aquired) == MutexOperationResultEnum.Aquired)
                    {
                        return result;
                    }
                }
                result.Result = MutexOperationResultEnum.FailToAquire;
            }
            return result;
        }

       public MutexOperationResult Release(bool forceDisposeIfNotReleased = false)
        {
            var result = new MutexOperationResult();
            try
            {
                result = ReleaseAsync(forceDisposeIfNotReleased).Result;
            }
            catch (Exception)
            {
                result.Result = MutexOperationResultEnum.UnknownException;
            }
            return result;
        }

        private async Task<MutexOperationResult>ReleaseAsync (bool forceDisposeIfNotReleased = false)
        {
            var released = new MutexOperationResult();
            var file = await Folder.GetFileAsync(FILE);
            await file.DeleteAsync();
            released.Result = MutexOperationResultEnum.Released;
            return released;
        }

    }
}

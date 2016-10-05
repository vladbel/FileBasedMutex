using FBM.Kit.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

namespace FBM.Services
{
    internal class FileMutex : IMutex
    {
        private const string FILE = "mutex.txt";

        private StorageFolder Folder
        {
            get { return ApplicationData.Current.TemporaryFolder; }
        }
        public MutexOperationResult Aquire(int milliseconds = 0)
        {
            var result = MutexOperationResult.NoValue;
            try
            {
                result = AquireAsync(milliseconds).Result;
            }
            catch (Exception)
            {
                result = MutexOperationResult.UnknownException;
            }
            return result;
        }

        private async Task<MutexOperationResult> AquireAsync ( int msec = 0)
        {
            var result = MutexOperationResult.NoValue;
            var file = await Folder.GetFileAsync(FILE);

            if (file == null)
            {
                file = await Folder.CreateFileAsync(FILE);
                await FileIO.WriteTextAsync(file, "aquired");
                result =  MutexOperationResult.Aquired;
            }
            else
            {
                result = MutexOperationResult.FailToAquire;
            }
            return result;
        }

       public MutexOperationResult Release(bool forceDisposeIfNotReleased = false)
        {
            var result = MutexOperationResult.NoValue;
            try
            {
                result = ReleaseAsync(forceDisposeIfNotReleased).Result;
            }
            catch (Exception)
            {
                result = MutexOperationResult.UnknownException;
            }
            return result;
        }

        private async Task<MutexOperationResult>ReleaseAsync (bool forceDisposeIfNotReleased = false)
        {
            var file = await Folder.GetFileAsync(FILE);
            await file.DeleteAsync();
            return MutexOperationResult.Released;
        }

    }
}

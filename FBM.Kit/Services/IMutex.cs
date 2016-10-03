using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBM.Kit.Services
{
    [Flags]
    public enum  MutexOperationResult
    {
        Unknown = 0,
        Opened = 1,
        Created = 2,
        Aquired = 4,
        Released = 8,
        Disposed = 16,
        AbadonedException = 32,
        UnknownException = 64,
        FailToCreate = 128,
        FailToAquire = 256,
        FailToOpen = 512,
        FailToDispose = 1024
    }
    public interface IMutex
    {
        MutexOperationResult Aquire( int milliseconds = 0);
        Task<MutexOperationResult> AquireAsync(int milliseconds);
        MutexOperationResult Release();
    }
}

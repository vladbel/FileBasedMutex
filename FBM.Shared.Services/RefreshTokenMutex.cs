using System;
using System.Threading;
using System.Diagnostics;
using FBM.Kit.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FBM.Services
{
    internal class RefreshTokenMutex : IMutexService
    {
        private const string REFRESH_TOKEN_MUTEX_NAME = "REFRESH_TOKEN_MUTEX_NAME";
        private const int SYNCHRONIZATION_EX_HRESULT = -2146233088;


        private Mutex OpenExisting( )
        {
            Mutex mutex = null;
            try
            {
                mutex = Mutex.OpenExisting(REFRESH_TOKEN_MUTEX_NAME);
            }
            catch (WaitHandleCannotBeOpenedException ex)
            {
                // mutex not exist ex
                Debug.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                // unexpected condition
                Debug.WriteLine(ex.ToString());
            }
            return mutex;
        }
        public MutexOperationResult Aquire( int milliseconds = 0)
        {
            Mutex mutex = null;
            var result = new MutexOperationResult();
            mutex = OpenExisting();

            if (mutex == null)
            {
                mutex = CreateMutex();
                if (mutex == null)
                {
                    result.Result = MutexOperationResultEnum.FailToCreate;
                }
                else
                {
                    result.Result = MutexOperationResultEnum.Created;
                }
            }
            else
            {
                result.Result = MutexOperationResultEnum.Opened;
            }

             result.Result = result.Result | Aquire(mutex, milliseconds).Result;
            return result;
        }

        private MutexOperationResult Aquire( Mutex mutex, int milliseconds = 0)
        {
            MutexOperationResult mutexAquired = new MutexOperationResult();
            if (mutex != null)
            {
                try
                {
                    if ( mutex.WaitOne(milliseconds))
                    {
                        mutexAquired.Result = MutexOperationResultEnum.Aquired;
                    }
                    else
                    {
                        mutexAquired.Result = MutexOperationResultEnum.FailToAquire;
                    }
                }
                catch (AbandonedMutexException)
                {
                    // we may try to recover
                    mutexAquired.Result = MutexOperationResultEnum.AbadonedException;
                    mutex.Dispose();
                    mutexAquired.Result = mutexAquired.Result | MutexOperationResultEnum.Disposed;
                    mutex = CreateMutex();
                    
                    if ( mutex != null)
                    {
                        mutexAquired.Result = mutexAquired.Result | MutexOperationResultEnum.Created;
                        if (mutex.WaitOne(milliseconds))
                            {
                            mutexAquired.Result = mutexAquired.Result | MutexOperationResultEnum.Aquired;
                        }
                        else
                        {
                            mutexAquired.Result = mutexAquired.Result | MutexOperationResultEnum.FailToAquire;
                        }
                    }
                    else
                    {
                        mutexAquired.Result = mutexAquired.Result | MutexOperationResultEnum.FailToCreate;
                    }
                }

            }
            return mutexAquired;
        }

        public MutexOperationResult Release(bool forceDisposeIfNotReleased = false)
        {
            var mutex = OpenExisting();
            if(mutex == null)
            {
                var result = new MutexOperationResult();
                    result.Result = MutexOperationResultEnum.FailToOpen;
                return result;
            }
            return Release(mutex, forceDisposeIfNotReleased);
        }

        private MutexOperationResult Release(Mutex mutex, bool forceDisposeIfNotReleased = false)
        {
            var mutexReleased = new MutexOperationResult();

            if (mutex != null)
            {
                try
                {
                    mutex.ReleaseMutex();
                    mutexReleased.Result = MutexOperationResultEnum.Released;
                }
                catch (Exception ex)
                {
                    // getting here following ex:
                    // "object synchronization method was called from an unsynchronized block of code"
                    if ( ex.HResult == SYNCHRONIZATION_EX_HRESULT)
                    {
                        mutexReleased.Result = MutexOperationResultEnum.SynchronizationException;
                    }
                    else
                    {
                        mutexReleased.Result = MutexOperationResultEnum.UnknownException;
                    }
                    
                }

                try
                {
                    if ( mutexReleased.Result != MutexOperationResultEnum.Released 
                        && forceDisposeIfNotReleased)
                    {
                        mutex.Dispose();
                        mutexReleased.Result = MutexOperationResultEnum.Disposed;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    mutexReleased.Result = MutexOperationResultEnum.FailToDispose;
                }
            }
            else
            {
                //attempt to release unexisting mutex
                mutexReleased.Result = MutexOperationResultEnum.FailToDispose;
            }

            return mutexReleased;
        }

        private Mutex CreateMutex()
        {
            bool mutexCreated = false;
            Mutex mutex = null;
            try
            {
                mutex = new Mutex(false, REFRESH_TOKEN_MUTEX_NAME, out mutexCreated);
            }
            catch (Exception ex)
            {
                // can' create mutex
                Debug.WriteLine(ex.ToString());
            }

            return mutex;
        }

        public void Dispose()
        {
            var mutex = OpenExisting();

            if (mutex != null)
            {
                mutex.Dispose();
            }
        }

        public Task<MutexOperationResult> AquireAsync(int milliseconds)
        {
            return Task.Run(() => Aquire(milliseconds));
        }
    }
}

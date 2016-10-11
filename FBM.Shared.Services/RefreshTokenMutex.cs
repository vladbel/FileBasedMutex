using System;
using System.Threading;
using System.Diagnostics;
using FBM.Kit.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FBM.Services
{
    internal class RefreshTokenMutex
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
        public MutexOperationResult Acquire( int milliseconds = 0)
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
                    result.Result = MutexOperationResultEnum.Acquired;
                }
            }
            else
            {
                result.Result = MutexOperationResultEnum.Acquired;
            }

             result.Result = result.Result | Acquire(mutex, milliseconds).Result;
            return result;
        }

        private MutexOperationResult Acquire( Mutex mutex, int milliseconds = 0)
        {
            MutexOperationResult mutexAcquired = new MutexOperationResult();
            if (mutex != null)
            {
                try
                {
                    if ( mutex.WaitOne(milliseconds))
                    {
                        mutexAcquired.Result = MutexOperationResultEnum.Acquired;
                    }
                    else
                    {
                        mutexAcquired.Result = MutexOperationResultEnum.FailToAcquire;
                    }
                }
                catch (AbandonedMutexException)
                {
                    // we may try to recover
                    mutexAcquired.Result = MutexOperationResultEnum.ExceptionUnknown;
                    mutex.Dispose();
                    mutexAcquired.Result = mutexAcquired.Result | MutexOperationResultEnum.Cleared;
                    mutex = CreateMutex();
                    
                    if ( mutex != null)
                    {
                        mutexAcquired.Result = mutexAcquired.Result | MutexOperationResultEnum.Acquired;
                        if (mutex.WaitOne(milliseconds))
                            {
                            mutexAcquired.Result = mutexAcquired.Result | MutexOperationResultEnum.Acquired;
                        }
                        else
                        {
                            mutexAcquired.Result = mutexAcquired.Result | MutexOperationResultEnum.FailToAcquire;
                        }
                    }
                    else
                    {
                        mutexAcquired.Result = mutexAcquired.Result | MutexOperationResultEnum.FailToCreate;
                    }
                }

            }
            return mutexAcquired;
        }

        public MutexOperationResult Release(bool forceDisposeIfNotReleased = false)
        {
            var mutex = OpenExisting();
            if(mutex == null)
            {
                var result = new MutexOperationResult();
                    result.Result = MutexOperationResultEnum.FailToRelease;
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
                        mutexReleased.Result = MutexOperationResultEnum.ExceptionUnknown;
                    }
                    else
                    {
                        mutexReleased.Result = MutexOperationResultEnum.ExceptionUnknown;
                    }
                    
                }

                try
                {
                    if ( mutexReleased.Result != MutexOperationResultEnum.Released 
                        && forceDisposeIfNotReleased)
                    {
                        mutex.Dispose();
                        mutexReleased.Result = MutexOperationResultEnum.Cleared;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    mutexReleased.Result = MutexOperationResultEnum.FailToClear;
                }
            }
            else
            {
                //attempt to release unexisting mutex
                mutexReleased.Result = MutexOperationResultEnum.FailToClear;
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

        public Task<MutexOperationResult> AcquireAsync(int milliseconds)
        {
            return Task.Run(() => Acquire(milliseconds));
        }
    }
}

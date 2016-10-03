using System;
using System.Threading;
using System.Diagnostics;
using FBM.Kit.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FBM.Services
{
    internal class RefreshTokenMutex : IMutex
    {
        public const string REFRESH_TOKEN_MUTEX_NAME = "REFRESH_TOKEN_MUTEX_NAME";


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
            var result = MutexOperationResult.Unknown;
            mutex = OpenExisting();

            if (mutex == null)
            {
                mutex = CreateMutex();
                if (mutex == null)
                {
                    result = MutexOperationResult.FailToCreate;
                }
                else
                {
                    result = MutexOperationResult.Created;
                }
            }
            else
            {
                result = MutexOperationResult.Opened;
            }

            return result | Aquire(mutex, milliseconds);
        }

        private MutexOperationResult Aquire( Mutex mutex, int milliseconds = 0)
        {
            MutexOperationResult mutexAquired = MutexOperationResult.Unknown;
            if (mutex != null)
            {
                try
                {
                    if ( mutex.WaitOne(milliseconds))
                    {
                        mutexAquired = MutexOperationResult.Aquired;
                    }
                    else
                    {
                        mutexAquired = MutexOperationResult.FailToAquire;
                    }
                }
                catch (AbandonedMutexException)
                {
                    // we may try to recover
                    mutexAquired = MutexOperationResult.AbadonedException;
                    mutex.Dispose();
                    mutexAquired = mutexAquired | MutexOperationResult.Disposed;
                    mutex = CreateMutex();
                    
                    if ( mutex != null)
                    {
                        mutexAquired = mutexAquired | MutexOperationResult.Created;
                        if (mutex.WaitOne(milliseconds))
                            {
                            mutexAquired = mutexAquired | MutexOperationResult.Aquired;
                        }
                        else
                        {
                            mutexAquired = mutexAquired | MutexOperationResult.FailToAquire;
                        }
                    }
                    else
                    {
                        mutexAquired = mutexAquired | MutexOperationResult.FailToCreate;
                    }
                }

            }
            return mutexAquired;
        }

        public MutexOperationResult Release()
        {
            var mutex = OpenExisting();
            if(mutex == null)
            {
                return MutexOperationResult.FailToOpen;
            }
            return Release(mutex);
        }

        private MutexOperationResult Release(Mutex mutex)
        {
            var mutexReleased = MutexOperationResult.Unknown;

            if (mutex != null)
            {
                try
                {
                    mutex.ReleaseMutex();
                    mutexReleased = MutexOperationResult.Released;
                }
                catch (Exception ex)
                {
                    // getting here following ex:
                    // "object synchronization method was called from an unsynchronized block of code"
                    Debug.WriteLine(ex.ToString());
                    mutexReleased = MutexOperationResult.UnknownException;
                }

                try
                {
                    if ( mutexReleased != MutexOperationResult.Released)
                    {
                        mutex.Dispose();
                        mutexReleased = MutexOperationResult.Disposed;
                    }
                }
                catch (Exception ex)
                {
                    // getting here following ex:
                    // "object synchronization method was called from an unsynchronized block of code"
                    Debug.WriteLine(ex.ToString());
                    mutexReleased = MutexOperationResult.FailToDispose;
                }
            }
            else
            {
                //attempt to release unexisting mutex
                mutexReleased = MutexOperationResult.FailToDispose;
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

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


        private Mutex OpenExisting()
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
        public bool Aquire( int milliseconds = 0)
        {
            Mutex mutex = null;
            mutex = OpenExisting();

            if (mutex == null)
            {
                    mutex = CreateMutex();
            }

            return Aquire(mutex, milliseconds);
        }

        private bool Aquire( Mutex mutex, int milliseconds = 0)
        {
            bool mutexAquired = false;
            if (mutex != null)
            {
                try
                {
                    mutexAquired = mutex.WaitOne(milliseconds);
                }
                catch (AbandonedMutexException)
                {
                    // we may try to recover
                    mutex.Dispose();
                    mutex = CreateMutex();
                    if ( mutex != null)
                    {
                        mutexAquired = mutex.WaitOne(milliseconds);
                    }
                }

            }
            return mutexAquired;
        }

        public bool Release()
        {
            var mutex = OpenExisting();
            if(mutex == null)
            {
                return true;
            }
            return Release(mutex);
        }

        private bool Release(Mutex mutex)
        {
            var mutexReleased = false;

            if (mutex != null)
            {
                try
                {
                    mutex.ReleaseMutex();
                    mutexReleased = true;
                }
                catch (Exception ex)
                {
                    // getting here following ex:
                    // "object synchronization method was called from an unsynchronized block of code"
                    Debug.WriteLine(ex.ToString());
                    mutexReleased = false;
                }

                try
                {
                    if ( !mutexReleased)
                    {
                        mutex.Dispose();
                        mutexReleased = true;
                    }
                }
                catch (Exception ex)
                {
                    // getting here following ex:
                    // "object synchronization method was called from an unsynchronized block of code"
                    Debug.WriteLine(ex.ToString());
                    mutexReleased = false;
                }
            }
            else
            {
                //attempt to release unexisting mutex
                mutexReleased = false;
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
                var aquired = Aquire(mutex);
                var released = Release(mutex);
                if ( aquired && released)
                {
                    mutex.Dispose();
                }
                else if (!aquired)
                {
                    // unable to aquire mutex
                }
                else
                {
                    // can aquire, but unable to release
                }
            }
        }

        public Task<bool> AquireAsync(int milliseconds)
        {
            return Task.Run(() => Aquire(milliseconds));
        }
    }
}

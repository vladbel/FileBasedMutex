﻿using System;
using System.Threading;
using System.Diagnostics;
using FBM.Kit.Services;
using System.Collections.Generic;
using System.Text;

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
        public bool Aquire()
        {

            Mutex mutex = OpenExisting();

            if (mutex == null)
            {
                    mutex = CreateMutex();
            }

            return Aquire(mutex);
        }

        private bool Aquire( Mutex mutex)
        {
            bool mutexAquired = false;
            if (mutex != null)
            {
                mutexAquired = mutex.WaitOne(0);
            }
            return mutexAquired;
        }

        public bool Release()
        {
            var mutex = OpenExisting();
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
    }
}

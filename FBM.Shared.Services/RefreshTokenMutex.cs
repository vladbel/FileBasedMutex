using System;
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
        private string _name;
        private Mutex _mutex = null;

        public RefreshTokenMutex ()
        {
            _name = REFRESH_TOKEN_MUTEX_NAME;
        }
        public bool Aquire()
        {
            bool mutexAquired = false;
            bool mutexNotExists = false;

            try
            {
                _mutex = Mutex.OpenExisting(_name);
            }
            catch (WaitHandleCannotBeOpenedException ex)
            {
                // mutex not exist ex
                Debug.WriteLine(ex.ToString());
                mutexNotExists = true;
            }
            catch (Exception ex)
            {
                // unexpected condition
                Debug.WriteLine(ex.ToString());
            }

            if (mutexNotExists)
            {
                try
                {
                    CreateMutex();
                }
                catch ( Exception ex)
                {
                    // can' create mutex
                    Debug.WriteLine(ex.ToString());
                }
            }

            if ( _mutex != null )
            {
                mutexAquired = _mutex.WaitOne(0);
            }

            return mutexAquired;
        }

        public bool Release()
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
            }

            return true;
        }

        private bool CreateMutex()
        {
            bool mutexCreated = false;
            _mutex = new Mutex(false , _name, out mutexCreated);

            return mutexCreated;
        }

        public void Dispose()
        {
            if (_mutex != null)
            {
                _mutex.Dispose();
                _mutex = null;
            }

        }
    }
}

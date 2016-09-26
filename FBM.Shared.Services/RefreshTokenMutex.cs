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
        public void Enter()
        {
            try
            {
                _mutex = Mutex.OpenExisting(_name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            if (_mutex == null)
            {
                CreateMutex();
            }
            else
            {
                _mutex.WaitOne();
            }
        }

        public void Release()
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
                _mutex = null;
            }

        }

        private void CreateMutex()
        {
            bool mutexCreated = false;
            _mutex = new Mutex(true , _name, out mutexCreated);

            if (!mutexCreated)
            {
                throw new Exception("Can't create named  mutex");
            }
        }
    }
}

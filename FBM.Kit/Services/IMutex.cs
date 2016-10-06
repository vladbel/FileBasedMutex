using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBM.Kit.Services
{
    [Flags]
    public enum  MutexOperationResultEnum
    {
        NoValue = 0,
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
        FailToDispose = 1024,
        SynchronizationException = 2048
    }

    public class MutexOperationResult
    {
        public MutexOperationResult()
        {
            Result = MutexOperationResultEnum.NoValue;
            _history = new List<string>();
        }

        private List<String> _history;
        MutexOperationResultEnum _result;
        public MutexOperationResultEnum Result
        {
            get
            {
                return _result;
            }
            set
            {
                if ( _history == null)
                {
                    _history = new List<string>();
                }

                _history.Add(((_result ^ value) & value).ToString());
                _result = value;
            }
        }

        public void Combine ( MutexOperationResult anotherResult)
        {
            _history.AddRange(anotherResult._history);
            _result = _result | anotherResult._result;
        }

        public override string ToString()
        {
            if (_history?.Count > 0)
            {
                return _history.Aggregate((i, j) => i.ToString() + "-" + j.ToString());
            }
            return Result.ToString();
        }
    }
    public interface IMutexService
    {
        MutexOperationResult Aquire( int milliseconds = 0);
        MutexOperationResult Release(bool forceDisposeIfNotReleased = false);
    }
}

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
        Free = 1,
        Acquired = 2,
        Released = 4,
        Cleared = 8,
        FailToCreate =16,
        FailToAcquire = 32,
        FailToRelease = 64,
        FailToClear = 128,
        ExceptionUnknown = 256,
        ExceptionFileSystem = 512
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

                _history.Add(value.ToString());
                _result = _result | value;
            }
        }

        public bool ResultIs( MutexOperationResultEnum resultToCheck)
        {
            return (_result & resultToCheck) == resultToCheck;
        }

        public string AcquisitionKey { get; set; }

        public void Combine ( MutexOperationResult anotherResult)
        {
            _history.AddRange(anotherResult._history);
            _result = _result | anotherResult._result;

            if ( anotherResult.ResultIs(MutexOperationResultEnum.Acquired))
            {
                this.AcquisitionKey = anotherResult.AcquisitionKey;
            }
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
        Task<MutexOperationResult> AcquireAsync( int milliseconds = 0);
        Task<MutexOperationResult> ReleaseAsync(string key);
        Task<MutexOperationResult> ClearAsync();
    }
}

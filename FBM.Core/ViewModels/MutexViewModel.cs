using FBM.Kit.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace FBM.Core.ViewModels
{
    public class MutexViewModel : ViewModels.ViewModelBase
    {
        private IMutexService _mutex;
        public MutexViewModel(IMutexService mutex)
        {
            _mutex = mutex;

            AcquireMutex = new RelayCommand(async () =>
                                                {
                                                    await AcquireMutexAsync();
                                                });

            ReleaseMutex = new RelayCommand(async () =>
            {
                await ReleaseMutexAsync();
            });

            DoWork = new RelayCommand(async () =>
           {
               await DoWorkAsync();
           });
        }


        private int _msec = 1000;
        public int Milliseconds
        {
            get
            {
                return _msec;
            }
            set
            {
                _msec = value;
                OnPropertyChanged();
            }
        }

        private bool _forceDispose = false;
        public bool ForceDispose
        {
            get
            {
                return _forceDispose;
            }
            set
            {
                _forceDispose = value;
                OnPropertyChanged();
            }
        }

        private string _mutexStatus = "Unknown";
        public string MutexStatus
        {
            get
            {
                return _mutexStatus;
            }
            set
            {
                _mutexStatus = value;
                OnPropertyChanged();
            }
        }

        public ICommand DoWork { get; set; }

        private async Task DoWorkAsync()
        {
            MutexStatus = "";
            MutexOperationResult AcquiredResult;
            MutexOperationResult releaseResult;
            var result = "Started";

            await Task.Run(async () =>
                     {
                         AcquiredResult = await _mutex.AcquireAsync(_msec);

                         if ((AcquiredResult.Result & MutexOperationResultEnum.Acquired) != MutexOperationResultEnum.NoValue)
                         {
                             for (var i = 1; i < 6; i++)
                             {
                                 await Task.Delay(_msec);
                                 Debug.WriteLine("Running: " + (i * Milliseconds).ToString());
                             }
                             releaseResult = await _mutex.ReleaseAsync(AcquiredResult.AcquisitionKey);
                             result = (AcquiredResult.Result | releaseResult.Result).ToString();
                         }

                     });

            MutexStatus = result;

        }


        private MutexOperationResult  _acquisitionResult;

        public ICommand AcquireMutex { get; set; }
        private async Task AcquireMutexAsync()
        {
            _acquisitionResult = await Task.Run( () => 
                {
                    Debug.WriteLine("-----------------------ThreadId = " + Environment.CurrentManagedThreadId);
                    return _mutex.AcquireAsync(_msec);
                });
            MutexStatus = _acquisitionResult.ToString();
        }

        public ICommand ReleaseMutex { get; set; }

        private async Task ReleaseMutexAsync()
        {
            if (ForceDispose)
            {
                var disposed = await _mutex.ClearAsync();
                MutexStatus = disposed.ToString();
                return;
            }

            if (_acquisitionResult == null)
            {
                MutexStatus = "Mutex not Acquired yet";
                return;
            }
            var result = await Task.Run( () => 
                    {
                        return _mutex.ReleaseAsync(_acquisitionResult.AcquisitionKey);
                    });

            MutexStatus = result.ToString();
        }

    }
}

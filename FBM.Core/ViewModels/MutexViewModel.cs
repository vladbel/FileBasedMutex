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
        private IMutex _mutex;
        public MutexViewModel(IMutex mutex)
        {
            _mutex = mutex;

            AquireMutex = new RelayCommand(async () =>
                                                {
                                                    await AquireMutexAsync();
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
            MutexOperationResult aquiredResult;
            MutexOperationResult releaseResult;
            var result = "Started";

            await Task.Run(async () =>
                     {
                         aquiredResult = _mutex.Aquire(_msec);

                         if ((aquiredResult.Result & MutexOperationResultEnum.Aquired) != MutexOperationResultEnum.NoValue)
                         {
                             for (var i = 1; i < 6; i++)
                             {
                                 await Task.Delay(_msec);
                                 Debug.WriteLine("Running: " + (i * Milliseconds).ToString());
                             }
                             releaseResult = _mutex.Release();
                             result = (aquiredResult.Result | releaseResult.Result).ToString();
                         }

                     });

            MutexStatus = result;

        }


        public ICommand AquireMutex { get; set; }
        private async Task AquireMutexAsync()
        {
            var aquired = await Task.Run( () => 
                {
                    Debug.WriteLine("-----------------------ThreadId = " + Environment.CurrentManagedThreadId);
                    return _mutex.Aquire(_msec);
                });
            MutexStatus = aquired.ToString();
        }

        public ICommand ReleaseMutex { get; set; }

        private async Task ReleaseMutexAsync()
        {
            var result = await Task.Run( () => 
                    {
                        return _mutex.Release(_forceDispose);
                    });
            MutexStatus = result.ToString();
        }

    }
}

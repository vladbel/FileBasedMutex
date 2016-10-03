using FBM.Kit.Services;
using System;
using System.Collections.Generic;
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
        }

        public ICommand AquireMutex { get; set; }

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

        private async Task AquireMutexAsync(int milliseconds = 0)
        {
            var aquired = await _mutex.AquireAsync(milliseconds);

            MutexStatus = aquired.ToString();

        }

        public ICommand ReleaseMutex { get; set; }

        private async Task ReleaseMutexAsync()
        {
            var result = await Task.FromResult(_mutex.Release());
            MutexStatus = result.ToString();
        }

    }
}

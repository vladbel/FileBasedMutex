﻿using FBM.Core.ViewModels;
using FBM.Kit.Services;
using FBM.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FBM.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = new MutexViewModel(new RefreshTokenMutex());
        }


        private void ReleaseMutexButton_Click(object sender, RoutedEventArgs e)
        {
            var mutex = new RefreshTokenMutex();
            var mutexReleased = mutex.Release();


            if (mutexReleased == MutexOperationResult.Released)
            {
                Debug.WriteLine("--------------------- Released -------------------------");
            }
            else
            {
                Debug.WriteLine("--------------------- NOT released-------------------------");
            }
        }


        private async void AquireMutexButton_Click(object sender, RoutedEventArgs e)
        {
            var mutex = new RefreshTokenMutex();
            var mutexAquired = await mutex.AquireAsync(5000);

            if (mutexAquired == MutexOperationResult.Aquired)
            {
                Debug.WriteLine("********************** Aquired********************************");
            }
            else
            {
                Debug.WriteLine("********************** NOT Aquired********************************");
            }
        }
    }
}

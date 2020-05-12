using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Dialogs;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;
using SkiaSharp;

namespace ControlCatalog.ViewModels
{
    class MainWindowViewModel : ReactiveObject
    {
        private IManagedNotificationManager _notificationManager;

        private bool _isMenuItemChecked = true;
        private WindowState _windowState;
        private WindowState[] _windowStates;
        private ObservableCollection<Bitmap> _covers;

        public MainWindowViewModel(IManagedNotificationManager notificationManager)
        {            
            _notificationManager = notificationManager;

            ShowCustomManagedNotificationCommand = ReactiveCommand.Create(() =>
            {
                NotificationManager.Show(new NotificationViewModel(NotificationManager) { Title = "Hey There!", Message = "Did you know that Avalonia now supports Custom In-Window Notifications?" });
            });

            ShowManagedNotificationCommand = ReactiveCommand.Create(() =>
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Welcome", "Avalonia now supports Notifications.", NotificationType.Information));
            });

            ShowNativeNotificationCommand = ReactiveCommand.Create(() =>
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", "Native Notifications are not quite ready. Coming soon.", NotificationType.Error));
            });

            AboutCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new AboutAvaloniaDialog();

                var mainWindow = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

                await dialog.ShowDialog(mainWindow);
            });

            ExitCommand = ReactiveCommand.Create(() =>
            {
                (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
            });

            ToggleMenuItemCheckedCommand = ReactiveCommand.Create(() =>
            {
                IsMenuItemChecked = !IsMenuItemChecked;
            });

            WindowState = WindowState.Normal;

            WindowStates = new WindowState[]
            {
                WindowState.Minimized,
                WindowState.Normal,
                WindowState.Maximized,
                WindowState.FullScreen,
            };

            Covers = new ObservableCollection<Bitmap>();

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

                var assets = assetLoader.GetAssets(new System.Uri("avares://ControlCatalog/Assets/Albums"), new System.Uri("avares://ControlCatalog/"));

                int x = 0;
                foreach (var uri in assets)
                {
                    var (stream, assembly) = assetLoader.OpenAndGetAssembly(uri);

                    using (stream)
                    {
                        var bitmap = CreateBitmap(stream);

                        Covers.Add(bitmap);
                        
                    }

                    if(x++ == 100)
                    {
                        break;
                    }
                }
            });
        }

        private List<Bitmap> holdRefs = new List<Bitmap>();

        private Bitmap CreateBitmap(Stream stream)
        {
            //return new Bitmap(stream).CreateScaledBitmap(new PixelSize(200, 200));
            var bmp = Bitmap.DecodeToHeight(stream, 275, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            {                
                var result =  bmp.CreateScaledBitmap(new PixelSize(100, 400), Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
                holdRefs.Add(bmp);
                //return bmp;
                bmp.Dispose();
                return result;
            }
        }


        public ObservableCollection<Bitmap> Covers
        {
            get { return _covers; }
            set { this.RaiseAndSetIfChanged(ref _covers, value); }
        }

        public WindowState WindowState
        {
            get { return _windowState; }
            set { this.RaiseAndSetIfChanged(ref _windowState, value); }
        }

        public WindowState[] WindowStates
        {
            get { return _windowStates; }
            set { this.RaiseAndSetIfChanged(ref _windowStates, value); }
        }

        public IManagedNotificationManager NotificationManager
        {
            get { return _notificationManager; }
            set { this.RaiseAndSetIfChanged(ref _notificationManager, value); }
        }

        public bool IsMenuItemChecked
        {
            get { return _isMenuItemChecked; }
            set { this.RaiseAndSetIfChanged(ref _isMenuItemChecked, value); }
        }

        public ReactiveCommand<Unit, Unit> ShowCustomManagedNotificationCommand { get; }

        public ReactiveCommand<Unit, Unit> ShowManagedNotificationCommand { get; }

        public ReactiveCommand<Unit, Unit> ShowNativeNotificationCommand { get; }

        public ReactiveCommand<Unit, Unit> AboutCommand { get; }

        public ReactiveCommand<Unit, Unit> ExitCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleMenuItemCheckedCommand { get; }
    }
}

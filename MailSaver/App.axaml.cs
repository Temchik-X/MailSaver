using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using MailSaver.ViewModels;
using MailSaver.Views;
using ReactiveUI;
using System;

namespace MailSaver;

public partial class App : Application
{
    private TrayIcon _trayIcon;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            // Создаем и настраиваем TrayIcon
            _trayIcon = new TrayIcon { 
            Icon = new WindowIcon("assets/logo-tray.ico"),
            ToolTipText = "EmailSaver"
            };
            // Настраиваем события
            _trayIcon.Clicked += (sender, args) => OpenApp();

            desktop.Exit += OnExit;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    private void OpenApp()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.Show();
            desktop.MainWindow.WindowState = WindowState.Normal;
        }
    }

    private void ExitApp()
    {
        _trayIcon.Dispose();
        Environment.Exit(0);
    }

    private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _trayIcon.Dispose();
    }
}

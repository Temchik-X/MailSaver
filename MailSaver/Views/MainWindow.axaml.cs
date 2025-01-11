using Avalonia.Controls;
using Avalonia.Interactivity;
using MailSaver.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MailSaver.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    public async void ClickHandler(object sender, RoutedEventArgs args)
    {
        message.Text = "Loading...";
        await ChooseProtocol();
    }
    public async Task ChooseProtocol()
    {
        string info;
        button.IsEnabled = false;
        EmailSaver emailSaver = new EmailSaver();
        
        if (imap.IsChecked == true)
        {
            info = await emailSaver.Save(2);
        }
        else if (pop.IsChecked == true)
        {
            info = await emailSaver.Save(1);
        }
        else
        {
            info = "Choose protocol";
        }
        message.Text = info;
        button.IsEnabled = true;
    }
    private void MinimizeToTray(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }
}
